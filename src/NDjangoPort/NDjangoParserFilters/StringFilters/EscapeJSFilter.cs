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
    ///     Hex encodes characters for use in JavaScript strings.
    ///     This does not make the string safe for use in HTML, but does protect you from syntax errors 
    ///     when using templates to generate JavaScript/JSON.
    /// </summary>
    [NDjango.Interfaces.Name("escapejs")]
    public class EscapeJSFilter : NDjango.Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members
        private static IDictionary<string, string> jsEscapes;

        public object Perform(object __p1)
        {
            if (jsEscapes == null)
            {
                lock (this)
                {
                    jsEscapes = new Dictionary<string, string>();
                    jsEscapes.Add("\\", "\\x5C");
                    jsEscapes.Add("\'", "\\x27");
                    jsEscapes.Add("\"", "\\x22");
                    jsEscapes.Add(">", "\\x3E");
                    jsEscapes.Add("<", "\\x3C");
                    jsEscapes.Add("&", "\\x26");
                    jsEscapes.Add("=", "\\x3D");
                    jsEscapes.Add("-", "\\x2D");
                    jsEscapes.Add(";", "\\x3B");
                    for (int i = 0; i < 32; i++)
                    {
                        jsEscapes.Add(new string((char)i, 1), string.Format("\\x{0:X2}", i));
                    }
                }
            }

            string strRetValue = Convert.ToString(__p1);

            foreach (KeyValuePair<string, string> kvPair in jsEscapes)
            {
                strRetValue = strRetValue.Replace(kvPair.Key, kvPair.Value);
            }
            return strRetValue;
        }

        #endregion
    }
}
