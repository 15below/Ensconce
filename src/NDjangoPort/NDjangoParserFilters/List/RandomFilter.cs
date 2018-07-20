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
    ///     Returns a random item from the given list.
    ///     
    ///     For example:
    ///     {{ value|random }}
    ///     If value is the list ['a', 'b', 'c', 'd'], the output could be "b".
    /// </summary>
    [NDjango.Interfaces.Name("random")]
    public class RandomFilter : NDjango.Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        private static Random rnd = new Random();

        public object Perform(object __p1)
        {
            object toConvert = (__p1 is IEnumerable) ? __p1 : Convert.ToString(__p1);
            IEnumerable<object> enumToProcess = ((IEnumerable)toConvert).OfType<object>();
            int count = enumToProcess.Count();
            return (count == 0) ? string.Empty : enumToProcess.ElementAt<object>(rnd.Next(count));
        }

        #endregion
    }
}
