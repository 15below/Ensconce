using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Replaces ampersands with ``&amp;`` entities.
    /// </summary>
    [Interfaces.Name("fix_ampersands")]
    public class FixAmpersandsFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            return Convert.ToString(__p1).Replace("&", "&amp;");
        }

        #endregion ISimpleFilter Members
    }
}
