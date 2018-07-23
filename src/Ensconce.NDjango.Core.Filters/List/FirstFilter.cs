using System;
using System.Collections;

namespace Ensconce.NDjango.Core.Filters.List
{
    /// <summary>
    ///     Returns the first item in a list.
    ///
    ///     For example:
    ///     {{ value|first }}
    ///     If value is the list ['a', 'b', 'c'], the output will be 'a'.
    /// </summary>
    [Interfaces.Name("first")]
    public class FirstFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            object toConvert = (__p1 is IEnumerable) ? __p1 : Convert.ToString(__p1);

            IEnumerator enList = ((IEnumerable)toConvert).GetEnumerator();
            if (enList.MoveNext())
                return enList.Current;
            return string.Empty;
        }

        #endregion
    }
}
