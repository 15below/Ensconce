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
    ///     Returns True if the value is divisible by the argument.
    ///
    ///     For example:
    ///     {{ value|divisibleby:"3" }}
    ///     If value is 21, the output would be True.
    /// </summary>
    [NDjango.Interfaces.Name("divisibleby")]
    public class DivisibleByFilter : NDjango.Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return false; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            var p1 = NDjango.Utilities.get_int(__p1);
            var p2 = NDjango.Utilities.get_int(__p2);

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
