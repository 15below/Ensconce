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
    ///     Returns a slice of the list. In NDjango case slice is not a special subset of array items, but just 
    ///     another array created from selected items.
    ///     
    ///     Uses the same syntax as Python's list slicing; see
    ///     http://diveintopython.org/native_data_types/lists.html#odbchelper.list.slice
    ///     for an introduction.
    ///     Another link:
    ///     http://www.python.org/doc/2.3.5/whatsnew/section-slices.html 
    /// </summary>
    [NDjango.Interfaces.Name("slice")]
    public class SliceFilter : NDjango.Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return null; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            string[] sliceParams = Convert.ToString(__p2).Split(new String[] {":"},StringSplitOptions.None);

            object toConvert = (__p1 is IEnumerable) ? __p1 : Convert.ToString(__p1);
            IEnumerable<object> enumToProcess = ((IEnumerable)toConvert).OfType<object>();
            int count = enumToProcess.Count();

            int startId = (sliceParams.Length >= 2 && sliceParams[0] != string.Empty) ? Convert.ToInt32(sliceParams[0]) : 0;
            int stopId = (sliceParams.Length >=2 && sliceParams[1] != string.Empty) ? Convert.ToInt32(sliceParams[1]) : (count);
            int stepCount = (sliceParams.Length >=3 && sliceParams[2] != string.Empty) ? Convert.ToInt32(sliceParams[2]) : 1;
            
            if (sliceParams.Length == 1 && sliceParams[0] != string.Empty)
                stopId = Convert.ToInt32(sliceParams[0]);

            if (count == 0 || stepCount == 0)
                return string.Empty;

            if (stopId < 0)
                stopId = count + stopId;

            List<object> retObjList = new List<object>();
            if (stepCount < 0)
            {
                for (int i = (stopId - 1); i >= startId; i += stepCount)
                    retObjList.Add(enumToProcess.ElementAt(i));
            }
            else
            {
                for (int i = startId ; i < stopId; i += stepCount)
                    retObjList.Add(enumToProcess.ElementAt(i));
            }

            if (toConvert is string)
            {
                return new string(retObjList.OfType<char>().ToArray<char>());
            }
            else
            {
                return retObjList;
            }
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
