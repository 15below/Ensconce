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
    ///     Removes all values of arg from the given string.
    ///     
    ///     For example:
    ///     {{ value|cut:" "}}
    ///     If value is "String with spaces", the output will be "Stringwithspaces".
    /// </summary>
    [NDjango.Interfaces.Name("cut")]
    public class CutFilter: NDjango.Interfaces.IFilter
    {

        #region IFilter Members

        public object DefaultValue
        {
            get { return null; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            return Convert.ToString(__p1).Replace(Convert.ToString(__p2), string.Empty);
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
