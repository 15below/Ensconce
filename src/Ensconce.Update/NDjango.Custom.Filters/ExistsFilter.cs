using Ensconce.NDjango.Core;

namespace Ensconce.Update.NDjango.Custom.Filters
{
    [Interfaces.Name("exists")]
    public class ExistsFilter : Interfaces.ISimpleFilter
    {
        public object Perform(object value)
        {
            return !(value is NDjangoWrapper.ErrorTemplate);
        }
    }
}
