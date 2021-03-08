using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Displays text with line numbers. Calculates maximum line number width
    ///     to align text with correct padding.
    /// </summary>
    [Interfaces.Name("linenumbers")]
    public class LineNumbersFilter : Interfaces.ISimpleFilter
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

        #endregion ISimpleFilter Members
    }
}
