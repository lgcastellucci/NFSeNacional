using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace NFSeNacional.Services
{
    public class NfseServiceGinfes
    {
        private readonly X509Certificate2 _certificado;
        public class retornoEnviarLote
        {
            public bool sucesso { get; set; }
            public string mensagem { get; set; }
            public int tipoAmbiente { get; set; }
            public string versaoAplicativo { get; set; }
            public string dataHoraProcessamento { get; set; }
            public string idDps { get; set; }
            public string chaveAcesso { get; set; }
            public string nfseXmlGZipB64 { get; set; }
            public string alertas { get; set; }

            public retornoEnviarLote()
            {
                sucesso = false;
                mensagem = "";
                tipoAmbiente = 0;
                versaoAplicativo = "";
                dataHoraProcessamento = "";
                idDps = "";
                chaveAcesso = "";
                nfseXmlGZipB64 = "";
                alertas = "";
            }
        }

        public NfseServiceGinfes(string caminhoCertificado)
        {

            if (!string.IsNullOrEmpty(caminhoCertificado))
                _certificado = Certificado.Buscar(caminhoCertificado);

            ConfigurarTls12();

        }

        public retornoEnviarLote EnviarLote(string xmlDpsAssinado)
        {
            return EnviarLoteAsync(xmlDpsAssinado).Result;
        }

        private async Task<retornoEnviarLote> EnviarLoteAsync(string xmlAssinado)
        {
            var retorno = new retornoEnviarLote();

            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(_certificado);

            using (var client = new HttpClient(handler))
            {
                string arq0 = "<ns2:cabecalho xmlns:ns2=\"http://www.ginfes.com.br/cabecalho_v03.xsd\" versao=\"3\"><versaoDados>3</versaoDados></ns2:cabecalho>";

                // Configura Headers para JSON
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
                client.DefaultRequestHeaders.Add("SOAPAction", "http://producao.ginfes.com.br/RecepcionarLoteRpsV3");

                // 1. Compacta e Codifica (GZip + Base64)
                string soapEnvelope = "";
                soapEnvelope += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
                soapEnvelope += "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://producao.ginfes.com.br\">";
                soapEnvelope += "  <soap:Body>";
                soapEnvelope += "    <ns1:RecepcionarLoteRpsV3>";
                soapEnvelope += "      <arg0>"+ WebUtility.HtmlEncode(arq0) + "</arg0>";
                soapEnvelope += "      <arg1>" + WebUtility.HtmlEncode(xmlAssinado) + "</arg1>";
                soapEnvelope += "    </ns1:RecepcionarLoteRpsV3>";
                soapEnvelope += "  </soap:Body>";
                soapEnvelope += "</soap:Envelope>";

                //Gravar o XML enviado para o Ginfes em um arquivo para debug
                File.WriteAllText("C:\\Projetos\\Castellucci\\GitHub\\NFSeNacional\\bin\\Debug\\net10.0\\Ginfes\\envio_ginfes.xml", soapEnvelope);
                File.WriteAllText("C:\\Projetos\\Castellucci\\GitHub\\NFSeNacional\\bin\\Debug\\net10.0\\Ginfes\\xmlAssinado.xml", xmlAssinado);


                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

                string urlAbsoluta = "https://producao.ginfes.com.br/ServiceGinfesImpl";

                var response = await client.PostAsync(urlAbsoluta, content);
                string responseString = await response.Content.ReadAsStringAsync();
                File.WriteAllText("C:\\Projetos\\Castellucci\\GitHub\\NFSeNacional\\bin\\Debug\\net10.0\\Ginfes\\resposta_ginfes.xml", responseString);


                if (!response.IsSuccessStatusCode)
                {
                    retorno.mensagem = "Erro HTTP: " + response.StatusCode;
                    return retorno;
                }

                // 1. Extrair o conteúdo da tag <return>
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseString);

                var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("env", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("ser", "http://producao.ginfes.com.br");

                var returnNode = xmlDoc.SelectSingleNode("//ser:RecepcionarLoteRpsV3Response/return", nsmgr);
                if (returnNode == null)
                {
                    retorno.mensagem = "Resposta SOAP sem tag <return>";
                    return retorno;
                }

                // 2. Decodificar HTML
                string innerXml = WebUtility.HtmlDecode(returnNode.InnerText);

                // 3. Carregar o XML de resposta
                var respostaDoc = new XmlDocument();
                respostaDoc.LoadXml(innerXml);

                // 4. Verificar se há erro
                var msgRetorno = respostaDoc.GetElementsByTagName("ns2:MensagemRetorno");
                if (msgRetorno.Count > 0)
                {
                    var codigo = msgRetorno[0]["ns3:Codigo"]?.InnerText;
                    var mensagem = msgRetorno[0]["ns3:Mensagem"]?.InnerText;
                    var correcao = msgRetorno[0]["ns3:Correcao"]?.InnerText;
                    retorno.mensagem = $"Erro {codigo}: {mensagem} - {correcao}";
                    return retorno;
                }

                // Aqui você pode extrair outros dados conforme o schema de sucesso
                //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><ns3:EnviarLoteRpsResposta xmlns:ns2="http://www.ginfes.com.br/tipos_v03.xsd" xmlns:ns3="http://www.ginfes.com.br/servico_enviar_lote_rps_resposta_v03.xsd"><ns3:NumeroLote>5346</ns3:NumeroLote><ns3:DataRecebimento>2026-02-13T16:34:43</ns3:DataRecebimento><ns3:Protocolo>634180966</ns3:Protocolo></ns3:EnviarLoteRpsResposta>
                var enviarLoteRpsResposta = respostaDoc.GetElementsByTagName("ns3:EnviarLoteRpsResposta");
                var numeroDoLote = enviarLoteRpsResposta[0]["ns3:NumeroLote"]?.InnerText;
                var DataRecebimento = enviarLoteRpsResposta[0]["ns3:DataRecebimento"]?.InnerText;
                var Protocolo = enviarLoteRpsResposta[0]["ns3:Protocolo"]?.InnerText;

                retorno.sucesso = true;
                retorno.mensagem = "Lote enviado com sucesso (ajuste para extrair dados do XML de sucesso)";
                return retorno;
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

    }
}
