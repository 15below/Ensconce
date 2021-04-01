using Ensconce.NDjango.Core;

namespace Ensconce.NDjango.Custom.Filters
{
    [Interfaces.Name("concat")]
    public class ConcatFilter : Interfaces.IFilter
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
