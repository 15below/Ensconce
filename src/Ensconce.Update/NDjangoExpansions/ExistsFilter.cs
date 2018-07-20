using NDjango.Interfaces;

namespace Ensconce.Update.NDjangoExpansions
{
    [Name("exists")]
    public class ExistsFilter : ISimpleFilter
    {
        public object Perform(object value)
        {
            return !(value is NDjangoWrapper.ErrorTemplate);
        }
    }
}
