using Ensconce.NDjango.Core;

namespace Ensconce.NDjango.Custom.Filters
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
            return value is ErrorTemplate ? parameter : value;
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
