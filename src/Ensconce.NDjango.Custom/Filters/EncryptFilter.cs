using Ensconce.NDjango.Core;
using Ensconce.NDjango.Custom.Helpers;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Ensconce.NDjango.Custom.Filters
{
    [Interfaces.Name("encrypt")]
    public class EncryptFilter : Interfaces.IFilter
    {
        public object Perform(object value)
        {
            throw new Exception("Missing certificate name as a parameter");
        }

        public object PerformWithParam(object value, object parameter)
        {
            if (!(parameter is string s) || String.IsNullOrEmpty(s))
            {
                throw new Exception($"encrypt parameter must be a non-empty string");
            }
            else if (value is ErrorTemplate)
            {
                throw new Exception("The value to be encrypted is a ndjango error");
            }

            return EncryptionHelpers.Encrypt(StoreLocation.LocalMachine, StoreName.My, (string)parameter, (string)value);
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
