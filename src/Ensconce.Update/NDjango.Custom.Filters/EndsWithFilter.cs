using Ensconce.NDjango.Core;
using System;

namespace Ensconce.Update.NDjango.Custom.Filters
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
            else if (value is NDjangoWrapper.ErrorTemplate)
            {
                throw new Exception($"Value does not exist when calling endsWith");
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
