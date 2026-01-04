using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace NFSeNacional.Services
{


    public class AssinadorDigital
    {
        private readonly X509Certificate2 _certificado;
        public AssinadorDigital(string caminhoCertificado)
        {

            if (!string.IsNullOrEmpty(caminhoCertificado))
                _certificado = Certificado.Buscar(caminhoCertificado);
        }

        public string AssinarDps(string xmlTexto)
        {
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(xmlTexto);

            var listaInfDps = doc.GetElementsByTagName("infDPS");
            if (listaInfDps.Count == 0) throw new Exception("infDPS não encontrado");

            var infDpsElement = (XmlElement)listaInfDps[0];
            string idTarget = infDpsElement.GetAttribute("Id");

            var signedXml = new SignedXml(doc);
            signedXml.SigningKey = _certificado.GetRSAPrivateKey();

            // Configurações SHA-256 (Requer que a classe RSAPKCS1SHA256... esteja registrada no Program.cs)
            signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;

            var reference = new Reference("#" + idTarget);
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform());
            reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";

            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(_certificado)); // Adiciona o certificado ao KeyInfo
            signedXml.KeyInfo = keyInfo;

            // Calcula a assinatura
            signedXml.ComputeSignature();

            var xmlDigitalSignature = signedXml.GetXml();
            doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return doc.OuterXml;
        }

    }
}
