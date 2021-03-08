using System;
using System.Collections;
using System.Linq;

namespace Ensconce.NDjango.Core.Filters.List
{
    /// <summary>
    ///     Returns True if the value's length is the argument, or False otherwise.
    ///
    ///     For example:
    ///     {{ value|length_is:"4" }}
    ///     If value is ['a', 'b', 'c', 'd'], the output will be True.
    /// </summary>
    [Interfaces.Name("length_is")]
    public class LengthIsFilter : Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return null; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            object toConvert = (__p1 is IEnumerable) ? __p1 : Convert.ToString(__p1);
            return Convert.ToInt32(__p2) == ((IEnumerable)toConvert).OfType<object>().Count();
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
