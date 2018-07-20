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
    ///     Displays text with line numbers. Calculates maximum line number width 
    ///     to align text with correct padding.
    /// </summary>
    [NDjango.Interfaces.Name("linenumbers")]
    public class LineNumbersFilter : NDjango.Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            string[] lines = Convert.ToString(__p1).Split('\n');
            int width = lines.Length.ToString().Length;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = string.Format("{0:d" + width + "}. {1}", i + 1, lines[i]);
            }
            return string.Join("\n", lines);
        }

        #endregion
    }
}
