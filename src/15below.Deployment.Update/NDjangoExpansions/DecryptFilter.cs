using NDjango.Interfaces;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
{
    [Name("decrypt")]
    public class DecryptFilter : IFilter
    {
        public object Perform(object value)
        {
            throw new System.NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            var cert = LoadCertificate(StoreName.My, StoreLocation.LocalMachine, $"CN={(string)parameter}");

            if (cert == null)
            {
                return new NDjangoWrapper.ErrorTemplate();
            }
            else
            {
                var data = Convert.FromBase64String((string)value);
                var decrypted = DecryptData(cert, data);
                return Encoding.UTF8.GetString(decrypted);
            }
        }

        public object DefaultValue
        {
            get { return null; }
        }

        private X509Certificate2 LoadCertificate(StoreName storeName, StoreLocation location, string subjectName)
        {
            X509Store store = new X509Store(storeName, location);

            try
            {
                store.Open(OpenFlags.ReadOnly);

                foreach (X509Certificate2 certificate in store.Certificates)
                {
                    if (certificate.SubjectName.Name == subjectName)
                    {
                        return certificate;
                    }
                }

                return null;
            }
            finally
            {
                store.Close();
            }
        }

        private byte[] DecryptData(X509Certificate2 cert, byte[] data)
        {
            var rsa = (RSACryptoServiceProvider)cert.PrivateKey;
            return rsa.Decrypt(data, false);
        }
    }
}
