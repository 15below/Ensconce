using NDjango.Interfaces;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
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
