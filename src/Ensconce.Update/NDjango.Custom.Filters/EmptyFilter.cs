using Ensconce.NDjango.Core;

namespace Ensconce.Update.NDjango.Custom.Filters
{
    [Interfaces.Name("empty")]
    public class EmptyFilter : Interfaces.ISimpleFilter
    {
        public object Perform(object value)
        {
            return value is NDjangoWrapper.ErrorTemplate || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}
