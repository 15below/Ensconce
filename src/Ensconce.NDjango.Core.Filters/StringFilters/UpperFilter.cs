using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Converts a string into all uppercase.
    ///
    ///     For example:
    ///     {{ value|upper }}
    ///     If value is "Joel is a slug", the output will be "JOEL IS A SLUG".
    /// </summary>
    [Interfaces.Name("upper")]
    public class UpperFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            return Convert.ToString(__p1).ToUpper();
        }

        #endregion ISimpleFilter Members
    }
}
