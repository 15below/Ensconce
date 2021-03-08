using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Converts a string into all lowercase.
    ///
    ///     For example:
    ///     {{ value|lower }}
    ///     If value is "Still MAD At Yoko", the output will be "still mad at yoko".
    /// </summary>
    [Interfaces.Name("lower")]
    public class LowerFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            return Convert.ToString(__p1).ToLower();
        }

        #endregion ISimpleFilter Members
    }
}
