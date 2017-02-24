using NDjango.Interfaces;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
{
    [Name("default")]
    public class DefaultFilter : IFilter
    {
        private readonly string templateStringIfInvalid;

        public DefaultFilter(string templateStringIfInvalid)
        {
            this.templateStringIfInvalid = templateStringIfInvalid;
        }

        public object Perform(object value)
        {
            throw new System.NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            return value.ToString() == templateStringIfInvalid ? parameter : value;
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
