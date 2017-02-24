using NDjango.Interfaces;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
{
    [Name("empty")]
    public class EmptyFilter : ISimpleFilter
    {
        private readonly string templateStringIfInvalid;

        public EmptyFilter(string templateStringIfInvalid)
        {
            this.templateStringIfInvalid = templateStringIfInvalid;
        }

        public object Perform(object value)
        {
            return string.IsNullOrWhiteSpace(value.ToString()) || value.ToString() == templateStringIfInvalid;
        }
    }
}
