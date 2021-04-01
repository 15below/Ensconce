using Ensconce.NDjango.Core;

namespace Ensconce.NDjango.Custom.Filters
{
    [Interfaces.Name("exists")]
    public class ExistsFilter : Interfaces.ISimpleFilter
    {
        public object Perform(object value)
        {
            return !(value is ErrorTemplate);
        }
    }
}
