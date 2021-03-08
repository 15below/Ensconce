using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Adds the arg to the value. Argument and Value must be integers.
    ///
    ///     For example:
    ///     {{ value|add:"2" }}
    ///     If value is 4, then the output will be 6.
    /// </summary>
    [Interfaces.Name("add")]
    public class AddFilter : Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return null; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            var p1 = Utilities.get_int(__p1);
            var p2 = Utilities.get_int(__p2);

            if (p1.Item1 && p2.Item1)
            {
                return p1.Item2 + p2.Item2;
            }
            else
            {
                return __p1;
            }
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
