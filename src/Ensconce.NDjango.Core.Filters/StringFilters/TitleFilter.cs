using System;
using System.Text.RegularExpressions;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Converts a string into Titlecase.
    /// </summary>
    [Interfaces.Name("title")]
    public class TitleFilter : Interfaces.ISimpleFilter
    {
        private static string capString(Match matchString)
        {
            string strTemp = matchString.ToString();
            return char.ToUpper(strTemp[0]) + strTemp.Substring(1).ToLower();
        }

        #region ISimpleFilter Members

        public object Perform(object __p1)
        {
            string str = Convert.ToString(__p1);
            return Regex.Replace(str, @"[\w\']+", new MatchEvaluator(capString));
        }

        #endregion ISimpleFilter Members
    }
}
