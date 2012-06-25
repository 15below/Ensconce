using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FifteenBelow.Deployment.Update
{

    public class UpdateFile
    {
        public struct Substitution
        {
            public string XPath;
            public string ReplacementContent;
            public bool HasReplacementContent;
            public bool RemoveCurrentAttributes;
            public List<Tuple<string, string>> ChangeAttributes;
            public string AppendAfter;
            public bool HasAppendAfter;
            public string AddChildContent;
            public bool HasAddChildContent;
        }

        public struct Namespace
        {
            public string Uri;
            public string Prefix;
        }

        public static IEnumerable<Tuple<string, string>> UpdateAll(string substitutionFile, IDictionary<string, object> tagValues)
        {
            var subsXml = XDocument.Load(substitutionFile);
            var nsm = new XmlNamespaceManager(new NameTable());
            nsm.AddNamespace("s", "http://15below.com/Substitutions.xsd");
            var files =
                subsXml
                    .XPathSelectElements("/s:Root/s:Files/s:File", nsm)
                    .Select(el => el.Attribute("Filename").Value)
                    .Select(path => path.RenderTemplate(tagValues));
            return files.Select(file => Tuple.Create(file, Update(substitutionFile, file, tagValues)));
        }

        public static string Update(string substitutionFile, string baseFile, IDictionary<string, object> tagValues = null)
        {
            tagValues = tagValues ?? new Dictionary<string, object>();
            var subsXml = XDocument.Load(substitutionFile);
            var nsm = new XmlNamespaceManager(new NameTable());
            nsm.AddNamespace("s", "http://15below.com/Substitutions.xsd");
            var fileElement =
                subsXml.XPathSelectElements("/s:Root/s:Files/s:File", nsm).SingleOrDefault(
                    el => ((string) el.Attribute("Filename")).RenderTemplate(tagValues) == baseFile);
                 
            if (fileElement == null) return File.ReadAllText(baseFile);
            var subs =
                fileElement
                    .XPathSelectElements(
                        string.Format("s:Changes/s:Change", baseFile), nsm)
                    .Select(
                        change => BuildSubstitions(change, nsm)
                    );
            if (subs.Any())
            {
                var baseXml = XDocument.Load(baseFile);
                return UpdateXml(tagValues, subs, baseXml, nsm, subsXml);
            }
            var replacementTemplate =
                fileElement.XPathSelectElement("s:ReplacementTemplate", nsm).Value;
            return File.ReadAllText(replacementTemplate).RenderTemplate(tagValues);
        }

        private static string UpdateXml(IDictionary<string, object> tagValues, IEnumerable<Substitution> subs, XDocument baseXml, XmlNamespaceManager nsm,
                                        XDocument subsXml)
        {
            var nss =
                subsXml
                    .XPathSelectElements("/s:Root/s:Namespaces/s:Namespace", nsm)
                    .Select(ns => new Namespace
                                      {
                                          Prefix = ns.Attribute("Prefix").Value,
                                          Uri = ns.Value
                                      });
            var baseNsm = new XmlNamespaceManager(new NameTable());
            foreach (var ns in nss)
            {
                baseNsm.AddNamespace(ns.Prefix, ns.Uri);
            }
            foreach (var sub in subs)
            {
                var activeNode = baseXml.XPathSelectElement(sub.XPath, baseNsm);
                if (sub.HasReplacementContent)
                    ReplaceChildNodes(tagValues, activeNode, sub);
                if(sub.HasAddChildContent)
                    AddChildContentToActive(tagValues, activeNode, sub);
                if (sub.HasAppendAfter)
                    AppendAfterActive(tagValues, activeNode, sub);
                if (sub.RemoveCurrentAttributes)
                    activeNode.RemoveAttributes();
                foreach (var ca in sub.ChangeAttributes)
                {
                    activeNode.SetAttributeValue(ca.Item1,
                                                 ca.Item2.RenderTemplate(tagValues));
                }
            }
            return baseXml.ToString();
        }

        private static void AppendAfterActive(IDictionary<string, object> tagValues, XElement activeNode, Substitution sub)
        {
            activeNode.AddAfterSelf(XElement.Parse(sub.AppendAfter.RenderTemplate(tagValues)));
        }

        private static void AddChildContentToActive(IDictionary<string, object> tagValues, XElement activeNode, Substitution sub)
        {
            activeNode.Add(XElement.Parse(sub.AddChildContent.RenderTemplate(tagValues)));
        }

        private static void ReplaceChildNodes(IDictionary<string, object> tagValues, XElement activeNode,
                                              Substitution sub)
        {
            var replacementValue = sub.ReplacementContent.RenderTemplate(tagValues);
            // Ugly hack to stop XElement.SetValue escaping text...
            var tempEl = XElement.Parse("<fakeRoot>" + replacementValue + "</fakeRoot>");
            var children = tempEl.DescendantNodes().Where(el => el.Parent == tempEl);
            activeNode.ReplaceNodes(children);
        }

        private static Substitution BuildSubstitions(XElement change, XmlNamespaceManager nsm)
        {
            var sub = new Substitution();
            var replacementContent = change.XPathSelectElement("s:ReplacementContent", nsm);
            if(replacementContent == null)
            {
                sub.ReplacementContent = "";
                sub.HasReplacementContent = false;
            }
            else
            {
                sub.ReplacementContent = replacementContent.Value;
                sub.HasReplacementContent = true;
            }
            var addChildContent = change.XPathSelectElement("s:AddChildContent", nsm);
            if(addChildContent == null)
            {
                sub.AddChildContent = "";
                sub.HasAddChildContent = false;
            }
            else
            {
                sub.AddChildContent = addChildContent.Value;
                sub.HasAddChildContent = true;
            }
            var appendAfter = change.XPathSelectElement("s:AppendAfter", nsm);
            if(appendAfter == null)
            {
                sub.AppendAfter = "";
                sub.HasAppendAfter = false;
            }
            else
            {
                sub.AppendAfter = appendAfter.Value;
                sub.HasAppendAfter = true;
            }
            sub.XPath = change.XPathSelectElement("s:XPath", nsm).Value;
            sub.RemoveCurrentAttributes = 
                XmlConvert.ToBoolean(change.TryXPathValueWithDefault("s:RemoveCurrentAttributes", nsm, "false"));
            sub.ChangeAttributes = change.XPathSelectElements("s:ChangeAttribute", nsm)
                .Select(ca => new Tuple<string, string>(ca.Attribute("attributeName").Value, ca.Value)).ToList();
            return sub;
        }

    }

    public static class ElementExtensions
    {
        public static string TryXPathValueWithDefault(this XElement element, string xPath, string defaultValue)
        {
            var xPathResult = element.XPathSelectElement(xPath);
            return xPathResult == null ? defaultValue : xPathResult.Value;
        }

        public static string TryXPathValueWithDefault(this XElement element, string xPath, IXmlNamespaceResolver nsm, string defaultValue)
        {
            var xPathResult = element.XPathSelectElement(xPath, nsm);
            return xPathResult == null ? defaultValue : xPathResult.Value;
        }
    }
}