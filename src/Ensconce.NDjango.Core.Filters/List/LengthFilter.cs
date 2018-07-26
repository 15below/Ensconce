using System;
using System.Collections;
using System.Linq;

namespace Ensconce.NDjango.Core.Filters.List
{
    /// <summary>
    ///     Returns the length of the value. This works for both strings and lists.
    ///
    ///     For example:
    ///     {{ value|length }}
    ///     If value is ['a', 'b', 'c', 'd'], the output will be 4.
    /// </summary>
    [Interfaces.Name("length")]
    public class LengthFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            object toConvert = (__p1 is IEnumerable) ? __p1 : Convert.ToString(__p1);
            return ((IEnumerable)toConvert).OfType<object>().Count();
        }

        #endregion
    }
}
