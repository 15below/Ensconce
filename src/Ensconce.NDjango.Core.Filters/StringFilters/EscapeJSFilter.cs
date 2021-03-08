using System;
using System.Collections.Generic;

namespace Ensconce.NDjango.Core.Filters.StringFilters
{
    /// <summary>
    ///     Hex encodes characters for use in JavaScript strings.
    ///     This does not make the string safe for use in HTML, but does protect you from syntax errors
    ///     when using templates to generate JavaScript/JSON.
    /// </summary>
    [Interfaces.Name("escapejs")]
    public class EscapeJSFilter : Interfaces.ISimpleFilter
    {
        #region ISimpleFilter Members

        private static IDictionary<string, string> jsEscapes;

        public object Perform(object __p1)
        {
            if (jsEscapes == null)
            {
                lock (this)
                {
                    jsEscapes = new Dictionary<string, string>();
                    jsEscapes.Add("\\", "\\x5C");
                    jsEscapes.Add("\'", "\\x27");
                    jsEscapes.Add("\"", "\\x22");
                    jsEscapes.Add(">", "\\x3E");
                    jsEscapes.Add("<", "\\x3C");
                    jsEscapes.Add("&", "\\x26");
                    jsEscapes.Add("=", "\\x3D");
                    jsEscapes.Add("-", "\\x2D");
                    jsEscapes.Add(";", "\\x3B");
                    for (int i = 0; i < 32; i++)
                    {
                        jsEscapes.Add(new string((char)i, 1), $"\\x{i:X2}");
                    }
                }
            }

            string strRetValue = Convert.ToString(__p1);

            foreach (KeyValuePair<string, string> kvPair in jsEscapes)
            {
                strRetValue = strRetValue.Replace(kvPair.Key, kvPair.Value);
            }
            return strRetValue;
        }

        #endregion ISimpleFilter Members
    }
}
