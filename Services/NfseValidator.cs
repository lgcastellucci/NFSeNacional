using System.Xml;
using System.Xml.Schema;

namespace NFSeNacional.Services
{
    public class NfseValidator
    {
        private XmlSchemaSet _schemas;
        public List<string> Erros { get; private set; }

        public NfseValidator(string caminhoPastaXsd)
        {
            _schemas = new XmlSchemaSet();
            Erros = new List<string>();

            // Configuração para PERMITIR DTD ao ler os arquivos XSD
            // Isso resolve o erro "DTD é proibido" ao carregar o xmldsig-core-schema.xsd
            XmlReaderSettings settingsXsd = new XmlReaderSettings();
            settingsXsd.DtdProcessing = DtdProcessing.Parse;

            // Carregamos os XSDs usando um Reader configurado, não direto pelo caminho
            AdicionarSchema("http://www.w3.org/2000/09/xmldsig#", Path.Combine(caminhoPastaXsd, "xmldsig-core-schema.xsd"), settingsXsd);
            AdicionarSchema("http://www.sped.fazenda.gov.br/nfse", Path.Combine(caminhoPastaXsd, "tiposSimples_v1.01.xsd"), settingsXsd);
            AdicionarSchema("http://www.sped.fazenda.gov.br/nfse", Path.Combine(caminhoPastaXsd, "tiposComplexos_v1.01.xsd"), settingsXsd);
            AdicionarSchema("http://www.sped.fazenda.gov.br/nfse", Path.Combine(caminhoPastaXsd, "DPS_v1.01.xsd"), settingsXsd);

            _schemas.Compile();
        }

        // Método auxiliar para carregar o XSD com as configurações de DTD ativadas
        private void AdicionarSchema(string ns, string caminho, XmlReaderSettings settings)
        {
            if (File.Exists(caminho))
            {
                using (var reader = XmlReader.Create(caminho, settings))
                {
                    _schemas.Add(ns, reader);
                }
            }
            else
            {
                throw new FileNotFoundException("Arquivo XSD não encontrado: " + caminho);
            }
        }

        public bool ValidarXml(string xmlContent)
        {
            Erros.Clear();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas = _schemas;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);

            // IMPORTANTE: Habilita DTD também para a leitura do XML final (caso necessário)
            settings.DtdProcessing = DtdProcessing.Parse;

            using (StringReader sr = new StringReader(xmlContent))
            using (XmlReader reader = XmlReader.Create(sr, settings))
            {
                try
                {
                    while (reader.Read()) { } // Ler todo o arquivo para validar
                }
                catch (XmlException ex)
                {
                    Erros.Add("Erro de XML Malformado: " + ex.Message);
                    return false;
                }
            }

            return Erros.Count == 0;
        }

        private void ValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Erros.Add("Aviso: " + args.Message);
            else
                Erros.Add("Erro: " + args.Message);
        }
    }
}
