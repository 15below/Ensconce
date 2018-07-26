using System;
using Ensconce.NDjango.Core;

namespace Ensconce.Update.NDjango.Custom.Filters
{
    [Interfaces.Name("startsWith")]
    public class StartsWithFilter : Interfaces.IFilter
    {
        public object Perform(object value)
        {
            throw new NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            if (!(parameter is string s) || String.IsNullOrEmpty(s))
            {
                throw new Exception($"startsWith parameter must be a non-empty string");
            }
            else if (value is NDjangoWrapper.ErrorTemplate)
            {
                throw new Exception($"Value does not exist when calling startsWith");
            }
            else
            {
                return (value ?? "").ToString().StartsWith((parameter ?? "").ToString(), StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
