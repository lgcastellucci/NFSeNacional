using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace NFSeNacional.Services
{
    public class NfseService
    {
        private readonly X509Certificate2 _certificado;
        public NfseService(string caminhoCertificado)
        {

            if (!string.IsNullOrEmpty(caminhoCertificado))
                _certificado = BuscarCertificado(caminhoCertificado);

            ConfigurarTls12();

        }

        public string ConsultarNFSe(string chave)
        {
            return ConsultarNFSeAsync(chave).Result;
        }
        public byte[] ConsultarDanfse(string chave)
        {
            return ConsultarDanfseAsync(chave).Result;
        }

        private async Task<string> EnviarDpsAsync(string xmlDpsAssinado, string serialCertificado)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(_certificado);

            using (var client = new HttpClient(handler))
            {
                // Configura Headers para JSON
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                // 1. Compacta e Codifica (GZip + Base64)
                string dpsBase64 = CompactarEnviar(xmlDpsAssinado);

                // 2. Monta o JSON
                string jsonBody = string.Format("{{ \"dpsXmlGZipB64\": \"{0}\" }}", dpsBase64);

                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                string urlAbsoluta = "https://sefin.producaorestrita.nfse.gov.br/SefinNacional/nfse"; //versão 2

                HttpResponseMessage response = await client.PostAsync(urlAbsoluta, content);

                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Erro API (" + response.StatusCode + "): " + responseString);
                }

                return responseString;
            }

        }
        private async Task<string> ConsultarNFSeAsync(string chave)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(_certificado);

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("https://adn.nfse.gov.br/"); // Base correta
                client.DefaultRequestHeaders.Accept.Clear();

                // Rota padrão para consultar uma nfse específica por chave
                string endpoint = string.Format("https://sefin.nfse.gov.br/sefinnacional/nfse/{0}/", chave);

                var response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Erro na Consulta: " + response.StatusCode);

                var retornoStr = await response.Content.ReadAsStringAsync();

                try
                {
                    //Ler o Json
                    var jsonDoc = JsonDocument.Parse(retornoStr);
                    string nfseXmlGZipB64 = jsonDoc.RootElement.GetProperty("nfseXmlGZipB64").GetString();

                    retornoStr = DescompactarNfseXmlGZipB64(nfseXmlGZipB64);
                }
                catch
                {
                    throw new Exception("Erro ao processar o retorno da consulta.");
                }

                return retornoStr; // Aqui deve vir o XML da NFS-e se ela existir
            }
        }
        private async Task<byte[]> ConsultarDanfseAsync(string chave)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(_certificado);

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("https://adn.nfse.gov.br/"); // Base correta
                client.DefaultRequestHeaders.Accept.Clear();

                // Rota padrão para consultar uma nfse específica por chave
                string endpoint = string.Format("https://adn.nfse.gov.br/danfse/{0}/", chave);

                var response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Erro na Consulta: " + response.StatusCode);

                byte[] retornoBytes = await response.Content.ReadAsByteArrayAsync();

                return retornoBytes; // Aqui deve vir o PDF da NFS-e se ela existir
            }
        }

        private void ConfigurarTls12()
        {
            // TRUQUE PARA VS2012 / .NET 4.5:
            // O Enum Tls12 não existe nativamente no 4.5 sem patch, então usamos o cast numérico (3072).
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            }
            catch
            {
                // Fallback se o cast falhar (mas geralmente é necessário para gov.br)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            }

            // Ignorar erros de validação de certificado do servidor (útil em homologação, perigoso em produção)
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
        private X509Certificate2 BuscarCertificado(string caminhoCertificado)
        {
            //Se o arquivo não existir então é porque passou so o nome e o mesmo esta no diretório da aplicação
            if (!File.Exists(caminhoCertificado))
                caminhoCertificado = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, caminhoCertificado);

            if (File.Exists(caminhoCertificado))
            {
                try
                {
                    // Carrega o certificado e a chave privada do arquivo PEM
                    var certificado = X509Certificate2.CreateFromPemFile(caminhoCertificado);

                    // O CreateFromPemFile NÃO importa a chave privada para o repositório de chaves do Windows de forma utilizável para TLS automaticamente.
                    certificado = new X509Certificate2(certificado.Export(X509ContentType.Pfx));
                    return certificado;
                }
                catch (Exception ex)
                {
                    LogService.Log("Erro ao carregar o certificado: " + ex.Message);
                }
            }
            else
            {
                LogService.Log("Arquivo de certificado PEM não encontrado: " + caminhoCertificado);
            }

            return null;
        }

        private static string CompactarEnviar(string xmlTexto)
        {
            // 1. Converte a string XML para bytes (UTF-8)
            byte[] buffer = Encoding.UTF8.GetBytes(xmlTexto);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gZipStream.Write(buffer, 0, buffer.Length);
                }
                // Importante: O stream precisa ser fechado/flushado antes de pegar o array

                // 2. Pega os bytes compactados e converte para Base64
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
        private static string DescompactarNfseXmlGZipB64(string nfseXmlGZipB64)
        {
            // 1. Decodifica de Base64 para bytes
            byte[] gzipBytes = Convert.FromBase64String(nfseXmlGZipB64);

            // 2. Descompacta usando GZipStream
            using (var compressedStream = new MemoryStream(gzipBytes))
            {
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(resultStream);
                        // 3. Converte para string UTF-8
                        return Encoding.UTF8.GetString(resultStream.ToArray());
                    }
                }
            }
        }

    }
}
