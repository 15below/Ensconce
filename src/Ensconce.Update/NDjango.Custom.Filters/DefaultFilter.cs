using Ensconce.NDjango.Core;

namespace Ensconce.Update.NDjango.Custom.Filters
{
    [Interfaces.Name("default")]
    public class DefaultFilter : Interfaces.IFilter
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
