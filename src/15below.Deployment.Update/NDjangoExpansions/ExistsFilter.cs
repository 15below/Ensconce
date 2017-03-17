using NDjango.Interfaces;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
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
