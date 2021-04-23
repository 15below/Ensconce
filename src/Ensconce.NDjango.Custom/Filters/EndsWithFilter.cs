using Ensconce.NDjango.Core;
using System;

namespace Ensconce.NDjango.Custom.Filters
{
    [Interfaces.Name("endsWith")]
    public class EndsWithFilter : Interfaces.IFilter
    {
        public object Perform(object value)
        {
            throw new NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            if (!(parameter is string s) || String.IsNullOrEmpty(s))
            {
                throw new Exception($"endsWith parameter must be a non-empty string");
            }
            else if (value is ErrorTemplate)
            {
                throw new Exception($"The value to be checked by 'endsWith' is an ndjango error");
            }
            else
            {
                return (value ?? "").ToString().EndsWith((parameter ?? "").ToString(), StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
