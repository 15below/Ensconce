using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ensconce.NDjango.Core.Filters.List
{
    /// <summary>
    ///     Returns a random item from the given list.
    ///
    ///     For example:
    ///     {{ value|random }}
    ///     If value is the list ['a', 'b', 'c', 'd'], the output could be "b".
    /// </summary>
    [Interfaces.Name("random")]
    public class RandomFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        private static readonly Random rnd = new Random();

        public object Perform(object __p1)
        {
            object toConvert = (__p1 is IEnumerable) ? __p1 : Convert.ToString(__p1);
            IEnumerable<object> enumToProcess = ((IEnumerable)toConvert).OfType<object>();
            int count = enumToProcess.Count();
            return (count == 0) ? string.Empty : enumToProcess.ElementAt<object>(rnd.Next(count));
        }

        #endregion ISimpleFilter Members
    }
}
