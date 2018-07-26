using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Capitalizes the first character of the value.
    /// </summary>
    [Interfaces.Name("capfirst")]
    public class CapFirstFilter : Interfaces.ISimpleFilter
    {

        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            string strVal = Convert.ToString(__p1);
            return strVal.Substring(0, 1).ToUpper() + strVal.Substring(1);
        }

        #endregion
    }
}
