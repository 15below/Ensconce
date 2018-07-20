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
using System.Collections;

namespace NDjango.FiltersCS
{
    /// <summary>
    ///     Returns the first item in a list.
    ///     
    ///     For example:
    ///     {{ value|first }}
    ///     If value is the list ['a', 'b', 'c'], the output will be 'a'.
    /// </summary>
    [NDjango.Interfaces.Name("first")]
    public class FirstFilter : NDjango.Interfaces.ISimpleFilter
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
