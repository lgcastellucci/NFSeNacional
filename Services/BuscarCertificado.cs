using System.Security.Cryptography.X509Certificates;

namespace NFSeNacional.Services
{
    public static class Certificado
    {
        public static X509Certificate2 Buscar(string caminhoCertificado)
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
    }
}
