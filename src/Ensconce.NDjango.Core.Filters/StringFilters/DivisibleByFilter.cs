using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Returns True if the value is divisible by the argument.
    ///
    ///     For example:
    ///     {{ value|divisibleby:"3" }}
    ///     If value is 21, the output would be True.
    /// </summary>
    [Interfaces.Name("divisibleby")]
    public class DivisibleByFilter : Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return false; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            var p1 = Utilities.get_int(__p1);
            var p2 = Utilities.get_int(__p2);

            if (p1.Item1 && p2.Item1)
                return (p1.Item2 % p2.Item2 == 0);
            else
                return false;
        }

        #endregion

        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
