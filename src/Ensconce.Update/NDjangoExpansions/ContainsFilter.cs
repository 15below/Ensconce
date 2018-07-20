using NDjango.Interfaces;
using System;

namespace Ensconce.Update.NDjangoExpansions
{
    [Name("contains")]
    public class ContainsFilter : IFilter
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
            else if (value is NDjangoWrapper.ErrorTemplate)
            {
                throw new Exception($"Value does not exist when calling contains");
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
