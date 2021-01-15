using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Left-aligns the value in a field of a given width.
    ///     Argument: field size.
    /// </summary>
    [Interfaces.Name("ljust")]
    public class LJustFilter : Interfaces.IFilter
    {
        #region IFilter Members

        public object DefaultValue
        {
            get { return null; }
        }

        public object PerformWithParam(object __p1, object __p2)
        {
            var p2 = Utilities.get_int(__p2);
            int width = p2.Item1 ? p2.Item2 : 0;
            return Convert.ToString(__p1).PadRight(width);
        }

        #endregion IFilter Members

        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            throw new NotImplementedException();
        }

        #endregion ISimpleFilter Members
    }
}
