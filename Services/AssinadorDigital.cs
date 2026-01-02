using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace NFSeNacional.Services
{
    public class AssinadorDigital
    {
        public string AssinarDps(string xmlTexto, string serialCertificado)
        {
            return "";
        }

        private X509Certificate2 BuscarCertificadoPorSerial(string serial)
        {
            if (string.IsNullOrEmpty(serial)) return null;
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                string serialLimpo = serial.Replace(" ", "").ToUpper();
                foreach (var cert in store.Certificates)
                {
                    if (cert.SerialNumber.ToUpper() == serialLimpo) return cert;
                }
            }
            finally { store.Close(); }
            return null;
        }
    }
}
