using Ensconce.NDjango.Core;
using Ensconce.NDjango.Custom.Helpers;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Ensconce.NDjango.Custom.Filters
{
    [Interfaces.Name("decrypt")]
    public class DecryptFilter : Interfaces.IFilter
    {
        public object Perform(object value)
        {
            throw new Exception("Missing certificate name as a parameter");
        }

        public object PerformWithParam(object value, object parameter)
        {
            if (!(parameter is string s) || String.IsNullOrEmpty(s))
            {
                throw new Exception($"decrypt parameter must be a non-empty string");
            }
            else if (value is ErrorTemplate)
            {
                throw new Exception("The value to be decrypted is an ndjango error");
            }

            return EncryptionHelpers.Decrypt(StoreLocation.LocalMachine, StoreName.My, (string)parameter, (string)value);
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
