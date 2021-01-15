using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Adds slashes before quotes. Useful for escaping strings in CSV, for
    ///     example. Less useful for escaping JavaScript; use the ``escapejs``
    ///     filter instead.
    /// </summary>
    [Interfaces.Name("addslashes")]
    public class AddSlashesFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            return Convert.ToString(__p1).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'");
        }

        #endregion ISimpleFilter Members
    }
}
