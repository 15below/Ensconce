using NDjango.Interfaces;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
{
    [Name("concat")]
    public class ConcatFilter : IFilter
    {
        public object Perform(object value)
        {
            throw new System.NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            return value.ToString() + parameter.ToString();
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
