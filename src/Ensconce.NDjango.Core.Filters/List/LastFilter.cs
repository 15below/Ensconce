using System;
using System.Collections;
using System.Linq;

namespace Ensconce.NDjango.Core.Filters.List
{
    /// <summary>
    ///     Returns the last item in a list.
    ///
    ///     For example:
    ///     {{ value|last }}
    ///     If value is the list ['a', 'b', 'c', 'd'], the output will be the string "d".
    /// </summary>
    [Interfaces.Name("last")]
    public class LastFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            object toConvert = (__p1 is IEnumerable) ? __p1 : Convert.ToString(__p1);
            object retObject = ((IEnumerable)toConvert).OfType<object>().LastOrDefault<object>();
            return (retObject == null) ? string.Empty : retObject;
        }

        #endregion ISimpleFilter Members
    }
}
