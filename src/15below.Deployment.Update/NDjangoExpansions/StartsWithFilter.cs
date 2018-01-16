using NDjango.Interfaces;
using System;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
{
    [Name("startsWith")]
    public class StartsWithFilter : IFilter
    {
        public object Perform(object value)
        {
            throw new System.NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            return value is NDjangoWrapper.ErrorTemplate ? false : (value ?? "").ToString().StartsWith((parameter ?? "").ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
