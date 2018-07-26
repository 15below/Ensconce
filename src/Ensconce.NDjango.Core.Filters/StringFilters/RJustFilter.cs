using System;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Right-aligns the value in a field of a given width.
    ///     Argument: field size.
    /// </summary>
    [Interfaces.Name("rjust")]
    public class RJustFilter : Interfaces.IFilter
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
            return Convert.ToString(__p1).PadLeft(width);
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
