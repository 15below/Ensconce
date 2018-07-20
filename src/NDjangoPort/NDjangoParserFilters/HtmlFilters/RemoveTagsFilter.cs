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
using System.Text.RegularExpressions;

namespace NDjango.FiltersCS
{
    /// <summary>
    ///     Removes a space-separated list of [X]HTML tags from the output.
    ///     
    ///     For example:
    ///     {{ value|removetags:"b span"|safe }}
    ///     If value is "<b>Joel</b> <button>is</button> a <span>slug</span>" the output will be "Joel <button>is</button> a slug".
    /// </summary>
    [NDjango.Interfaces.Name("removetags")]
    public class RemoveTagsFilter : NDjango.Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return null; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            string[] tags = Convert.ToString(__p2).Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tags.Length; i++)
            {
                tags[i] = Regex.Escape(tags[i]);
            }

            string tagsForRegx = string.Join("|", tags);
            Regex regxStartTag = new Regex(string.Format(@"<({0})(/?>|(\s+[^>]*>))", tagsForRegx), RegexOptions.Compiled);
            Regex regxEndTag = new Regex(string.Format(@"</({0})>", tagsForRegx), RegexOptions.Compiled);
            string retValue = regxStartTag.Replace(Convert.ToString(__p1), string.Empty);
            retValue = regxEndTag.Replace(retValue, string.Empty);
            return retValue;
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
