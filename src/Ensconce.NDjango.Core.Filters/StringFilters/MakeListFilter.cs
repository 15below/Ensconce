using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Returns the value turned into a list. For an integer, it's a list of digits.
    ///     For a string, it's a list of characters.
    ///
    ///     For example:
    ///     {{ value|make_list }}
    ///     If value is the string "Joel", the output would be the list
    ///     [u'J', u'o', u'e', u'l']. If value is 123, the output will be the list [1, 2, 3].
    ///
    /// </summary>
    [Interfaces.Name("make_list")]
    public class MakeListFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            object toConvert = (__p1 is IEnumerable) ? __p1 : Convert.ToString(__p1);
            return new List<object>(((IEnumerable)toConvert).OfType<object>());
        }

        #endregion ISimpleFilter Members
    }
}
