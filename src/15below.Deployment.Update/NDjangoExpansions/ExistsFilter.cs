using NDjango.Interfaces;

namespace FifteenBelow.Deployment.Update.NDjangoExpansions
{
    [Name("exists")]
    public class ExistsFilter : ISimpleFilter
    {
        private readonly string templateStringIfInvalid;

        public ExistsFilter(string templateStringIfInvalid)
        {
            this.templateStringIfInvalid = templateStringIfInvalid;
        }

        public object Perform(object value)
        {
            return value.ToString() != templateStringIfInvalid;
            
        }
    }
}
