using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Removes all values of arg from the given string.
    ///
    ///     For example:
    ///     {{ value|cut:" "}}
    ///     If value is "String with spaces", the output will be "Stringwithspaces".
    /// </summary>
    [Interfaces.Name("cut")]
    public class CutFilter : Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return null; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            return Convert.ToString(__p1).Replace(Convert.ToString(__p2), string.Empty);
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
