using System;
using System.Globalization;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Displays a float to a specified number of decimal places.

    ///     If called without an argument, it displays the floating point number with
    ///     one decimal place -- but only if there's a decimal place to be displayed:

    ///     * num1 = 34.23234
    ///     * num2 = 34.00000
    ///     * num3 = 34.26000
    ///     * {{ num1|floatformat }} displays "34.2"
    ///     * {{ num2|floatformat }} displays "34"
    ///     * {{ num3|floatformat }} displays "34.3"

    ///     If arg is positive, it will always display exactly arg number of decimal
    ///     places:

    ///     * {{ num1|floatformat:3 }} displays "34.232"
    ///     * {{ num2|floatformat:3 }} displays "34.000"
    ///     * {{ num3|floatformat:3 }} displays "34.260"

    ///     If arg is negative, it will display arg number of decimal places -- but
    ///     only if there are places to be displayed:

    ///     * {{ num1|floatformat:"-3" }} displays "34.232"
    ///     * {{ num2|floatformat:"-3" }} displays "34"
    ///     * {{ num3|floatformat:"-3" }} displays "34.260"

    ///     If the input float is infinity or NaN, the (platform-dependent) string
    ///     representation of that value will be displayed.
    /// </summary>
    [Interfaces.Name("floatformat")]
    public class FloatFormatFilter : Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return -1; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            string input_val = Convert.ToString(__p1);

            if (!Decimal.TryParse(input_val, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal d))
            {
                return string.Empty;
            }

            var p2 = Utilities.get_int(__p2);
            if (!p2.Item1)
            {
                return input_val;
            }

            int p = p2.Item2;

            bool isRound = Decimal.Truncate(d) - d == Decimal.Zero;
            if (isRound && (p < 0))
            {
                p = 0;
            }

            int exp = Math.Abs(p);
            return Decimal.Round(d, exp, MidpointRounding.AwayFromZero).ToString(string.Format("F{0}", exp));
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
