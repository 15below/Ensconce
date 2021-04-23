using Ensconce.NDjango.Core;

namespace Ensconce.NDjango.Custom.Filters
{
    [Interfaces.Name("empty")]
    public class EmptyFilter : Interfaces.ISimpleFilter
    {
        public object Perform(object value)
        {
            return value is ErrorTemplate || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}
