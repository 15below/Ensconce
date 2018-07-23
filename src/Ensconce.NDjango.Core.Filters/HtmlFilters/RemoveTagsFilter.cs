using System;
using System.Text.RegularExpressions;

namespace Ensconce.NDjango.Core.Filters.HtmlFilters
{
    /// <summary>
    ///     Removes a space-separated list of [X]HTML tags from the output.
    ///
    ///     For example:
    ///     {{ value|removetags:"b span"|safe }}
    ///     If value is "<b>Joel</b> <button>is</button> a <span>slug</span>" the output will be "Joel <button>is</button> a slug".
    /// </summary>
    [Interfaces.Name("removetags")]
    public class RemoveTagsFilter : Interfaces.IFilter
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
