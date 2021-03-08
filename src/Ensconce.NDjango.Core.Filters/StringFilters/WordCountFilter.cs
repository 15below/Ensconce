using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Returns the number of words.
    /// </summary>
    [Interfaces.Name("wordcount")]
    public class WordCountFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            return Convert.ToString(__p1).Split((string[])null, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        #endregion ISimpleFilter Members
    }
}
