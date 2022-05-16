using System;
using System.Collections;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ensconce.Update
{
    public static class XDocumentExtensions
    {
        public static bool XPathExists(this XDocument document, string rawXPath, Lazy<TagDictionary> tagValues, IXmlNamespaceResolver nsm)
        {
            if (document == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(rawXPath))
            {
                return false;
            }

            var xpathEvaluate = (IEnumerable)document.XPathEvaluate(rawXPath.RenderTemplate(tagValues), nsm);

            return xpathEvaluate.Cast<object>().Any();
        }
    }
}
