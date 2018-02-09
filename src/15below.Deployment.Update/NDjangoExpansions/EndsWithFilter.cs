using NDjango.Interfaces;
using System;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
{
    [Name("endsWith")]
    public class EndsWithFilter : IFilter
    {
        public object Perform(object value)
        {
            throw new NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            if (value is NDjangoWrapper.ErrorTemplate)
            {
                return value;
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
