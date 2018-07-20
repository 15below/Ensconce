/****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Parser.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDjango.FiltersCS
{
    /// <summary>
    ///     Given a whole number, returns the requested digit of it, where 1 is the
    ///     right-most digit, 2 is the second-right-most digit, etc. Returns the
    ///     original value for invalid input (if input or argument is not an integer,
    ///     or if argument is less than 1). Otherwise, output is always an integer.
    ///     
    ///     For example:
    ///     {{ value|get_digit:"2" }}
    ///     If value is 123456789, the output will be 8.
    /// </summary>
    [NDjango.Interfaces.Name("get_digit")]
    public class GetDigit : NDjango.Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return null; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            var p1 = NDjango.Utilities.get_int(__p1);
            var p2 = NDjango.Utilities.get_int(__p2);
            if( !p1.Item1 || !p2.Item1 || (p2.Item2 < 1))
                return __p1;

            string valStr = (p1.Item2 > 0) ? (p1.Item2.ToString()) : ((p1.Item2 * (-1)).ToString());
            if (valStr.Length >= p2.Item2) 
                return valStr[valStr.Length - p2.Item2];
            else 
                return 0;
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
