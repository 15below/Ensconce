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
using System.Globalization;

namespace NDjango.FiltersCS
{
    /// <summary>
    ///     Class used to register filters.
    /// </summary>
    public static class FilterManager
    {
        public static IEnumerable<Filter> GetFilters()
        {
            Filter[] filters = 
            {
                new Filter("add", new NDjango.FiltersCS.AddFilter()),
                new Filter("get_digit", new NDjango.FiltersCS.GetDigit()),
                new Filter("default", new NDjango.FiltersCS.DefaultFilter()),
                new Filter("divisibleby", new NDjango.FiltersCS.DivisibleByFilter()),
                new Filter("addslashes", new NDjango.FiltersCS.AddSlashesFilter()),
                new Filter("capfirst", new NDjango.FiltersCS.CapFirstFilter()),
                new Filter("escapejs", new NDjango.FiltersCS.EscapeJSFilter()),
                new Filter("fix_ampersands", new NDjango.FiltersCS.FixAmpersandsFilter()),
                new Filter("floatformat", new NDjango.FiltersCS.FloatFormatFilter()),
                new Filter("linenumbers", new NDjango.FiltersCS.LineNumbersFilter()),
                new Filter("lower", new NDjango.FiltersCS.LowerFilter()),
                new Filter("upper", new NDjango.FiltersCS.UpperFilter()),
                new Filter("make_list", new NDjango.FiltersCS.MakeListFilter()),
                new Filter("wordcount", new NDjango.FiltersCS.WordCountFilter()),
                new Filter("ljust", new NDjango.FiltersCS.LJustFilter()),
                new Filter("rjust", new NDjango.FiltersCS.RJustFilter()),
                new Filter("center", new NDjango.FiltersCS.CenterFilter()),
                new Filter("cut", new NDjango.FiltersCS.CutFilter()),
                new Filter("title", new NDjango.FiltersCS.TitleFilter()),
                new Filter("removetags", new NDjango.FiltersCS.RemoveTagsFilter()),
                new Filter("first", new NDjango.FiltersCS.FirstFilter()),
                new Filter("last", new NDjango.FiltersCS.LastFilter()),
                new Filter("length", new NDjango.FiltersCS.LengthFilter()),
                new Filter("length_is", new NDjango.FiltersCS.LengthIsFilter()),
                new Filter("random", new NDjango.FiltersCS.RandomFilter()),
                new Filter("slice", new NDjango.FiltersCS.SliceFilter())
            };

            return filters;
        }
    }
}
