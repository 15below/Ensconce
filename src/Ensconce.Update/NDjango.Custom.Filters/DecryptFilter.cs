using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Ensconce.NDjango.Core;

namespace Ensconce.Update.NDjango.Custom.Filters
{
    [Interfaces.Name("decrypt")]
    public class DecryptFilter : Interfaces.IFilter
    {
        public object Perform(object value)
        {
            throw new NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            if (value is NDjangoWrapper.ErrorTemplate)
            {
                throw new Exception("Value does not exist when calling decrypt");
            }
            else
            {
                var cert = new CertificateDetails(StoreLocation.LocalMachine, StoreName.My, (string)parameter);
                var data = Convert.FromBase64String((string)value);
                var decrypted = Decrypt(cert, data);
                return Encoding.UTF8.GetString(decrypted);
            }
        }

        public object DefaultValue
        {
            get { return null; }
        }

        public byte[] Decrypt(CertificateDetails details, byte[] data)
        {
            Exception lastException = null;

            foreach (var certificate in FindCertificates(details))
            {
                try
                {
                    var rsa = (RSACryptoServiceProvider)certificate.PrivateKey;
                    return rsa.Decrypt(data, true);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw new CryptographicException($"Could not decrypt data using any found certificate with subject name '{details.CertificateSubjectName}' in the store {details.Location}/{details.Name}", lastException);
        }

        public IEnumerable<X509Certificate2> FindCertificates(CertificateDetails details)
        {
            var store = new X509Store(details.Name, details.Location);

            try
            {
                bool found = false;

                store.Open(OpenFlags.ReadOnly);

                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, details.CertificateSubjectName, false)
                                              .Cast<X509Certificate2>()
                                              .Where(cert => cert.NotBefore <= DateTime.Now && cert.NotAfter >= DateTime.Now)
                                              .OrderByDescending(cert => cert.NotBefore);

                foreach (var certificate in certs)
                {
                    found = true;
                    yield return certificate;
                }

                if (!found)
                {
                    throw new KeyNotFoundException($"Could not find certificate with subject name '{details.CertificateSubjectName}' in the store {details.Location}/{details.Name}");
                }
            }
            finally
            {
                store.Close();
            }
        }

        public class CertificateDetails
        {
            public StoreLocation Location { get; set; }
            public StoreName Name { get; set; }
            public string CertificateSubjectName { get; set; }

            public CertificateDetails(StoreLocation location, StoreName name, string certificateSubjectName)
            {
                Location = location;
                Name = name;
                CertificateSubjectName = certificateSubjectName ?? throw new ArgumentNullException(nameof(certificateSubjectName));
            }
        }
    }
}
