using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Ensconce.Update
{
    public class UpdateFile
    {
        public class Substitution
        {
            public string XPath;
            public string ReplacementContent;
            public bool HasReplacementContent;
            public bool RemoveCurrentAttributes;
            public List<(string attributeName, string newValue)> ChangeAttributes;
            public string AppendAfter;
            public bool HasAppendAfter;
            public string AddChildContent;
            public string AddChildContentIfNotExists;
            public bool HasAddChildContent;
            public bool Execute;

            public Substitution()
            {
                XPath = "";
                ReplacementContent = "";
                HasReplacementContent = false;
                RemoveCurrentAttributes = false;
                ChangeAttributes = new List<(string attributeName, string newValue)>();
                AppendAfter = "";
                HasAppendAfter = false;
                AddChildContent = "";
                AddChildContentIfNotExists = "";
                HasAddChildContent = false;
                Execute = true;
            }
        }

        public struct Namespace
        {
            public string Uri;
            public string Prefix;
        }

        public static void UpdateFiles(string substitutionFile, Lazy<TagDictionary> tagValues, bool outputFailureContext)
        {
            var (subsXml, nsm) = LoadAndValidateSubstitutionDoc(substitutionFile);

            var files = subsXml.XPathSelectElements("/s:Root/s:Files/s:File", nsm)
                               .Select(el => el.Attribute("Filename")?.Value)
                               .Select(path => path.RenderTemplate(tagValues))
                               .ToList();

            var exceptions = new ConcurrentQueue<Exception>();

            Parallel.ForEach(files, file =>
            {
                try
                {
                    File.WriteAllText(file, Update(subsXml, nsm, file, tagValues, outputFailureContext));
                }
                catch (Exception e)
                {
                    exceptions.Enqueue(e);
                }
            });

            if (exceptions.Count > 0) throw new AggregateException(exceptions);
        }

        public static string Update(string substitutionFile, string baseFile, Lazy<TagDictionary> tagValues = null, bool outputFailureContext = false)
        {
            var (subsXml, nsm) = LoadAndValidateSubstitutionDoc(substitutionFile);

            return Update(subsXml, nsm, baseFile, tagValues, outputFailureContext);
        }

        private static (XDocument subsXml, XmlNamespaceManager nsm) LoadAndValidateSubstitutionDoc(string substitutionFile)
        {
            var subsXml = XDocument.Load(substitutionFile);
            var nsm = new XmlNamespaceManager(new NameTable());
            nsm.AddNamespace("s", "http://15below.com/Substitutions.xsd");

            var schemas = new XmlSchemaSet();
            var assembly = Assembly.GetExecutingAssembly();

            schemas.Add(null, XmlReader.Create(assembly.GetManifestResourceStream("Ensconce.Update.Substitutions.xsd")));

            subsXml.Validate(schemas, (sender, args) => throw args.Exception);

            return (subsXml, nsm);
        }

        private static string Update(XDocument subsXml, XmlNamespaceManager nsm, string baseFile, Lazy<TagDictionary> tagValues = null, bool outputFailureContext = false)
        {
            Logging.Log($"Updating file {baseFile}");

            tagValues = tagValues ?? new Lazy<TagDictionary>(TagDictionary.Empty);

            var fileElement = subsXml.XPathSelectElements("/s:Root/s:Files/s:File", nsm)
                                     .SingleOrDefault(el => ((string)el.Attribute("Filename")).RenderTemplate(tagValues) == baseFile);

            if (fileElement == null) return File.ReadAllText(baseFile);

            string baseData = null;

            var replacementTemplateElement = fileElement.XPathSelectElement("s:ReplacementTemplate", nsm);
            if (replacementTemplateElement != null)
            {
                var replacementTemplate = replacementTemplateElement.Value.RenderTemplate(tagValues);
                baseData = File.ReadAllText(replacementTemplate).RenderTemplate(tagValues);
            }

            var subs = fileElement.XPathSelectElements("s:Changes/s:Change", nsm)
                                  .Select(change => BuildSubstitions(change, nsm, tagValues))
                                  .ToList();
            if (subs.Any())
            {
                if (baseData == null) baseData = File.ReadAllText(baseFile);
                var baseXml = XDocument.Parse(baseData);
                try
                {
                    return UpdateXml(tagValues, subs, baseXml, nsm, subsXml);
                }
                catch (Exception)
                {
                    if (outputFailureContext)
                    {
                        var partialFilename = $"{baseFile}_partial";
                        baseXml.Save(partialFilename);
                    }
                    throw;
                }
            }

            return baseData;
        }

        private static string UpdateXml(Lazy<TagDictionary> tagValues, IEnumerable<Substitution> subs, XNode baseXml, IXmlNamespaceResolver nsm, XNode subsXml)
        {
            var nss = subsXml.XPathSelectElements("/s:Root/s:Namespaces/s:Namespace", nsm)
                             .Select(ns => new Namespace
                             {
                                 Prefix = ns.Attribute("Prefix")?.Value,
                                 Uri = ns.Value
                             });

            var baseNsm = new XmlNamespaceManager(new NameTable());

            foreach (var ns in nss)
            {
                baseNsm.AddNamespace(ns.Prefix, ns.Uri);
            }

            foreach (var sub in subs.Where(x => x.Execute))
            {
                Logging.Log($"Updating xpath {sub.XPath}");

                var activeNode = baseXml.XPathSelectElement(sub.XPath.RenderTemplate(tagValues), baseNsm);

                if (activeNode == null)
                {
                    throw new ApplicationException($"XPath select of {sub.XPath} returned null");
                }

                if (sub.HasAddChildContent) AddChildContentToActive(tagValues, activeNode, sub);
                if (sub.HasReplacementContent) ReplaceChildNodes(tagValues, activeNode, sub);
                if (sub.HasAppendAfter) AppendAfterActive(tagValues, activeNode, sub);
                if (sub.RemoveCurrentAttributes) activeNode.RemoveAttributes();

                foreach (var (atttibute, value) in sub.ChangeAttributes)
                {
                    activeNode.SetAttributeValue(atttibute, value.RenderTemplate(tagValues));
                }
            }

            return baseXml.ToString();
        }

        private static void AppendAfterActive(Lazy<TagDictionary> tagValues, XNode activeNode, Substitution sub)
        {
            var fakeRoot = XElement.Parse("<fakeRoot>" + sub.AppendAfter.RenderXmlTemplate(tagValues) + "</fakeRoot>");
            activeNode.AddAfterSelf(fakeRoot.Elements());
        }

        private static void AddChildContentToActive(Lazy<TagDictionary> tagValues, XContainer activeNode, Substitution sub)
        {
            if (sub.AddChildContentIfNotExists == null || activeNode.Document?.XPathSelectElement(sub.AddChildContentIfNotExists.RenderTemplate(tagValues)) == null)
            {
                var fakeRoot = XElement.Parse("<fakeRoot>" + sub.AddChildContent.RenderXmlTemplate(tagValues) + "</fakeRoot>");
                activeNode.Add(fakeRoot.Elements());
            }
        }

        private static void ReplaceChildNodes(Lazy<TagDictionary> tagValues, XContainer activeNode, Substitution sub)
        {
            var replacementValue = sub.ReplacementContent.RenderXmlTemplate(tagValues);
            // Ugly hack to stop XElement.SetValue escaping text...
            var tempEl = XElement.Parse("<fakeRoot>" + replacementValue + "</fakeRoot>");
            var children = tempEl.DescendantNodes().Where(el => el.Parent == tempEl);
            activeNode.ReplaceNodes(children);
        }

        private static Substitution BuildSubstitions(XElement change, IXmlNamespaceResolver nsm, Lazy<TagDictionary> tagValues)
        {
            //Default everything off
            var sub = new Substitution();

            if (change.Attribute("type") == null)
            {
                sub.ReplacementContent = change.XPathSelectElement("s:ReplacementContent", nsm)?.Value;
                sub.HasReplacementContent = sub.ReplacementContent != null;

                sub.AddChildContent = change.XPathSelectElement("s:AddChildContent", nsm)?.Value;
                sub.HasAddChildContent = sub.AddChildContent != null;
                sub.AddChildContentIfNotExists = change.XPathSelectElement("s:AddChildContent", nsm)?.Attribute("ifNotExists")?.Value;

                sub.AppendAfter = change.XPathSelectElement("s:AppendAfter", nsm)?.Value;
                sub.HasAppendAfter = sub.AppendAfter != null;

                sub.XPath = (change.Attribute("xPath")?.Value ?? change.XPathSelectElement("s:XPath", nsm)?.Value).RenderTemplate(tagValues);

                sub.RemoveCurrentAttributes = XmlConvert.ToBoolean(change.TryXPathValueWithDefault("s:RemoveCurrentAttributes", nsm, "false"));

                foreach (var ca in change.XPathSelectElements("s:ChangeAttribute", nsm))
                {
                    sub.ChangeAttributes.Add((ca.Attribute("attributeName")?.Value, ca.Attribute("value")?.Value ?? ca.Value));
                }
            }
            else
            {
                switch (change.Attribute("type")?.Value.ToLower())
                {
                    case "replacementcontent":
                        sub.ReplacementContent = change.Value;
                        sub.HasReplacementContent = true;
                        break;

                    case "addchildcontent":
                        sub.AddChildContent = change.Value;
                        sub.HasAddChildContent = true;
                        sub.AddChildContentIfNotExists = change.Attribute("ifNotExists")?.Value;
                        break;

                    case "appendafter":
                        sub.AppendAfter = change.Value;
                        sub.HasAppendAfter = true;
                        break;

                    case "removecurrentattributes":
                        sub.RemoveCurrentAttributes = true;
                        break;

                    case "changeattribute":
                        sub.ChangeAttributes.Add((change.Attribute("attributeName")?.Value, change.Attribute("value")?.Value));
                        break;

                    default:
                        throw new Exception($"Unknown change type '{change.Attribute("type")?.Value}'");
                }

                sub.XPath = change.Attribute("xPath")?.Value.RenderTemplate(tagValues);

                if (change.Attribute("if") != null)
                {
                    sub.Execute = bool.Parse($"{{% if {change.Attribute("if")?.Value} %}}true{{% else %}}false{{% endif %}}".RenderTemplate(tagValues));
                }
            }

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
