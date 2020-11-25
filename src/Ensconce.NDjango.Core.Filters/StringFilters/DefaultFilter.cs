using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     If value string representation evaluates to empty string(null is one of the cases) ,
    ///     use given default. Otherwise, use the value.
    ///
    ///     For example:
    ///     {{ value|default:"nothing" }}
    ///     If value is "" (the empty string), the output will be nothing.
    /// </summary>
    [Interfaces.Name("default")]
    public class DefaultFilter : Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return null; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            return (Convert.ToString(__p1) == string.Empty) ? __p2 : __p1;
        }

        #endregion IFilter Members

        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            throw new NotImplementedException();
        }

        #endregion ISimpleFilter Members
    }
}
