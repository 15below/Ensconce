using NDjango.Interfaces;

namespace Ensconce.Update.NDjangoExpansions
{
    [Name("empty")]
    public class EmptyFilter : ISimpleFilter
    {
        public object Perform(object value)
        {
            return value is NDjangoWrapper.ErrorTemplate || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}
