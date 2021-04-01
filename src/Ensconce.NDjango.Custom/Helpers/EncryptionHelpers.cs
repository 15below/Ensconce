using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Ensconce.NDjango.Custom.Helpers
{
    internal static class EncryptionHelpers
    {
        internal static string Encrypt(StoreLocation location, StoreName name, string certificateSubjectName, string value)
        {
            var cert = new CertificateDetails(location, name, certificateSubjectName);
            var data = Encoding.UTF8.GetBytes(value);
            var encrypted = Encrypt(cert, data);
            return Convert.ToBase64String(encrypted);
        }

        internal static string Decrypt(StoreLocation location, StoreName name, string certificateSubjectName, string value)
        {
            var cert = new CertificateDetails(location, name, certificateSubjectName);
            var data = Convert.FromBase64String(value);
            var encrypted = Decrypt(cert, data);
            return Encoding.UTF8.GetString(encrypted);
        }

        private static byte[] Encrypt(CertificateDetails details, byte[] data)
        {
            var cert = FindCertificates(details).First();
            using (var rsa = cert.GetRSAPublicKey())
            {
                return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA1);
            }
        }

        private static byte[] Decrypt(CertificateDetails details, byte[] data)
        {
            var exceptions = new List<Exception>();

            foreach (var certificate in FindCertificates(details))
            {
                try
                {
                    using (var rsa = certificate.GetRSAPrivateKey())
                    {
                        return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA1);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new CryptographicException($"Could not decrypt data using any found certificate with subject name '{details.CertificateSubjectName}' in the store {details.Location}/{details.Name}:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", exceptions.Select(e => e.Message))}");
        }

        private static List<X509Certificate2> FindCertificates(CertificateDetails details)
        {
            var results = new List<X509Certificate2>();
            var store = new X509Store(details.Name, details.Location);

            try
            {
                store.Open(OpenFlags.ReadOnly);

                var distinguishedName = details.CertificateSubjectName.StartsWith("CN=", StringComparison.InvariantCultureIgnoreCase) ? details.CertificateSubjectName : $"CN={details.CertificateSubjectName}";

                var certs = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, distinguishedName, false)
                    .Cast<X509Certificate2>()
                    .Where(cert => cert.NotBefore <= DateTime.Now && cert.NotAfter >= DateTime.Now)
                    .OrderByDescending(cert => cert.NotBefore);

                foreach (var certificate in certs)
                {
                    results.Add(certificate);
                }

                if (!results.Any())
                {
                    throw new KeyNotFoundException($"Could not find certificate with subject name '{details.CertificateSubjectName}' in the store {details.Location}/{details.Name}");
                }

                return results;
            }
            finally
            {
                store.Close();
            }
        }

        private class CertificateDetails
        {
            public StoreLocation Location { get; }
            public StoreName Name { get; }
            public string CertificateSubjectName { get; }

            public CertificateDetails(StoreLocation location, StoreName name, string certificateSubjectName)
            {
                Location = location;
                Name = name;
                CertificateSubjectName = certificateSubjectName ?? throw new ArgumentNullException(nameof(certificateSubjectName));
            }
        }
    }
}
