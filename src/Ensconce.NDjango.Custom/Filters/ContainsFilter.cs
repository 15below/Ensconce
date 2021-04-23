using Ensconce.NDjango.Core;
using System;

namespace Ensconce.NDjango.Custom.Filters
{
    [Interfaces.Name("contains")]
    public class ContainsFilter : Interfaces.IFilter
    {
        public object Perform(object value)
        {
            throw new NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            if (!(parameter is string s) || String.IsNullOrEmpty(s))
            {
                throw new Exception($"contains parameter must be a non-empty string");
            }
            else if (value is ErrorTemplate)
            {
                throw new Exception($"The value to be checked by 'contains' is an ndjango error");
            }
            else
            {
                return (value ?? "").ToString().IndexOf((parameter ?? "").ToString(), StringComparison.CurrentCultureIgnoreCase) >= 0;
            }
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
