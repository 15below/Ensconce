using NDjango.Interfaces;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
{
    [Name("default")]
    public class DefaultFilter : IFilter
    {
        public object Perform(object value)
        {
            throw new System.NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            return value is NDjangoWrapper.ErrorTemplate ? parameter : value;
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
