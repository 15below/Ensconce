using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
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
            ValidateSubstitutionDoc(subsXml);
            var fileElement =
                subsXml.XPathSelectElements("/s:Root/s:Files/s:File", nsm).SingleOrDefault(
                    el => ((string) el.Attribute("Filename")).RenderTemplate(tagValues) == baseFile);
                 
            if (fileElement == null) return File.ReadAllText(baseFile);
            string replacementTemplate;
            string baseData = null;
            var replacementTemplateElement = fileElement.XPathSelectElement("s:ReplacementTemplate", nsm);
            if (replacementTemplateElement != null)
            {
                replacementTemplate = replacementTemplateElement.Value;
                baseData = File.ReadAllText(replacementTemplate).RenderTemplate(tagValues);
            }
            var subs =
                fileElement
                    .XPathSelectElements(
                        string.Format("s:Changes/s:Change"), nsm)
                    .Select(
                        change => BuildSubstitions(change, nsm, tagValues)
                    );
            if (subs.Any())
            {
                if (baseData == null) baseData = File.ReadAllText(baseFile);
                var baseXml = XDocument.Parse(baseData);
                return UpdateXml(tagValues, subs, baseXml, nsm, subsXml);
            }
            return baseData;
        }

        private static void ValidateSubstitutionDoc(XDocument subXml)
        {
            var schemas = new XmlSchemaSet();
            var assembly = Assembly.GetExecutingAssembly();
            schemas.Add(null,
                        XmlReader.Create(
                            assembly.GetManifestResourceStream("FifteenBelow.Deployment.Update.Substitutions.xsd")));
            subXml.Validate(schemas, (sender, args) => { throw args.Exception; });
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
                var activeNode = baseXml.XPathSelectElement(sub.XPath.RenderTemplate(tagValues), baseNsm);

				if (activeNode == null)
				{
					throw new ApplicationException(String.Format("XPath select of {0} returned null", sub.XPath));
				}

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
            var fakeRoot = XElement.Parse("<fakeRoot>" + sub.AppendAfter.RenderTemplate(tagValues) + "</fakeRoot>");
            activeNode.AddAfterSelf(fakeRoot.Elements());
        }

        private static void AddChildContentToActive(IDictionary<string, object> tagValues, XElement activeNode, Substitution sub)
        {
            var fakeRoot = XElement.Parse("<fakeRoot>" + sub.AddChildContent.RenderTemplate(tagValues) + "</fakeRoot>");
            activeNode.Add(fakeRoot.Elements());
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

        private static Substitution BuildSubstitions(XElement change, XmlNamespaceManager nsm, IDictionary<string, object> tagValues)
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
            sub.XPath = change.XPathSelectElement("s:XPath", nsm).Value.RenderTemplate(tagValues);
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