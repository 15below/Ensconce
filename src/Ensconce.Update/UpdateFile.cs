using Newtonsoft.Json.Linq;
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
using Formatting = Newtonsoft.Json.Formatting;

namespace Ensconce.Update
{
    public class UpdateFile
    {
        public class Substitution
        {
            public string Path;
            public bool PathMatchAll;
            public string ReplacementContent;
            public bool HasReplacementContent;
            public bool RemoveCurrentAttributes;
            public List<(string attributeName, string newValue)> AddAttributes;
            public List<(string attributeName, string newValue)> ChangeAttributes;
            public string ChangeValue;
            public bool HasChangeValue;
            public string AppendAfter;
            public bool HasAppendAfter;
            public string AddChildContent;
            public string AddChildContentIfNotExists;
            public bool HasAddChildContent;
            public bool Execute;

            public Substitution()
            {
                Path = "";
                ReplacementContent = "";
                HasReplacementContent = false;
                RemoveCurrentAttributes = false;
                AddAttributes = new List<(string attributeName, string newValue)>();
                ChangeAttributes = new List<(string attributeName, string newValue)>();
                ChangeValue = "";
                HasChangeValue = false;
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

        private enum FileType
        {
            Xml,
            Json
        }

        public static void UpdateFiles(string substitutionFile, Lazy<TagDictionary> tagValues, bool outputFailureContext)
        {
            var (subsXml, nsm) = LoadAndValidateSubstitutionDoc(substitutionFile);

            var files = subsXml.XPathSelectElements("/s:Root/s:Files/s:File", nsm)
                               .Select(el => el.Attribute("Filename")?.Value)
                               .Select(path => path.RenderTemplate(tagValues))
                               .ToList();

            var exceptions = new ConcurrentBag<Exception>();

            Parallel.ForEach(files, file =>
            {
                try
                {
                    File.WriteAllText(file, Update(subsXml, nsm, file, tagValues, outputFailureContext));
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            });

            if (exceptions.Count == 1) throw exceptions.First();
            if (exceptions.Count > 1) throw new AggregateException(exceptions);
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

            var replacementTemplateValue = fileElement.XPathSelectElement("s:ReplacementTemplate", nsm)?.Value ?? fileElement.Attribute("ReplacementTemplate")?.Value;
            if (!string.IsNullOrWhiteSpace(replacementTemplateValue))
            {
                var replacementTemplate = replacementTemplateValue.RenderTemplate(tagValues);
                baseData = File.ReadAllText(replacementTemplate).RenderTemplate(tagValues);
            }

            if (!Enum.TryParse(fileElement.Attribute("FileType")?.Value, true, out FileType fileType))
            {
                if (Path.GetExtension(baseFile).Equals(".json", StringComparison.InvariantCultureIgnoreCase))
                {
                    fileType = FileType.Json;
                }
                else
                {
                    fileType = FileType.Xml;
                }
            }

            var subs = fileElement.XPathSelectElements("s:Changes/s:Change", nsm)
                                  .Select(change => BuildSubstitions(change, nsm, tagValues, fileType))
                                  .ToList();
            if (subs.Any())
            {
                if (baseData == null) baseData = File.ReadAllText(baseFile);

                switch (fileType)
                {
                    case FileType.Xml:
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

                    case FileType.Json:
                        var baseJson = JObject.Parse(baseData);
                        try
                        {
                            return UpdateJson(tagValues, subs, baseJson, nsm, subsXml);
                        }
                        catch (Exception)
                        {
                            if (outputFailureContext)
                            {
                                var partialFilename = $"{baseFile}_partial";
                                File.WriteAllText(baseJson.ToString(Formatting.Indented), partialFilename);
                            }
                            throw;
                        }

                    default:
                        throw new ApplicationException("Unknown file type");
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
                Logging.Log($"Updating xpath {sub.Path}");

                var xPathMatches = baseXml.XPathSelectElements(sub.Path.RenderTemplate(tagValues), baseNsm).ToList();

                if (xPathMatches.Count == 0)
                {
                    throw new ApplicationException($"XPath select of {sub.Path} returned no matches.");
                }

                if (!sub.PathMatchAll && xPathMatches.Count > 1)
                {
                    throw new ApplicationException($"XPath select of {sub.Path} returned multiple matches. If the intention was to update all matches, use the attribute matchAll=\"true\" on the XPath or Change node.");
                }

                foreach (var activeNode in sub.PathMatchAll ? xPathMatches : xPathMatches.Take(1))
                {
                    if (sub.HasAddChildContent) AddChildContentToActive(tagValues, activeNode, sub);
                    if (sub.HasReplacementContent) ReplaceChildNodes(tagValues, activeNode, sub);
                    if (sub.HasAppendAfter) AppendAfterActive(tagValues, activeNode, sub);
                    if (sub.RemoveCurrentAttributes) activeNode.RemoveAttributes();
                    if (sub.HasChangeValue) activeNode.Value = sub.ChangeValue.RenderTemplate(tagValues);

                    foreach (var (attribute, value) in sub.AddAttributes)
                    {
                        if (activeNode.Attribute(attribute) == null)
                        {
                            activeNode.SetAttributeValue(attribute, value.RenderTemplate(tagValues));
                        }
                        else
                        {
                            throw new ApplicationException($"XPath of {sub.Path} with attribute {attribute} already exists, cannot add");
                        }
                    }

                    foreach (var (attribute, value) in sub.ChangeAttributes)
                    {
                        if (activeNode.Attribute(attribute) != null)
                        {
                            activeNode.SetAttributeValue(attribute, value.RenderTemplate(tagValues));
                        }
                        else
                        {
                            throw new ApplicationException($"XPath of {sub.Path} with attribute {attribute} does not exist, cannot change");
                        }
                    }
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

        private static string UpdateJson(Lazy<TagDictionary> tagValues, IEnumerable<Substitution> subs, JObject baseJson, IXmlNamespaceResolver nsm, XNode subsXml)
        {
            foreach (var sub in subs.Where(x => x.Execute))
            {
                Logging.Log($"Updating JsonPath {sub.Path}");

                var xPathMatches = baseJson.SelectTokens(sub.Path.RenderTemplate(tagValues)).ToList();

                if (xPathMatches.Count == 0)
                {
                    throw new ApplicationException($"JsonPath select of {sub.Path} returned no matches.");
                }

                if (!sub.PathMatchAll && xPathMatches.Count > 1)
                {
                    throw new ApplicationException($"JsonPath select of {sub.Path} returned multiple matches. If the intention was to update all matches, use the attribute matchAll=\"true\" on the XPath or Change node.");
                }

                foreach (var activeObject in sub.PathMatchAll ? xPathMatches : xPathMatches.Take(1))
                {
                    //Should never get to this, but just in case!
                    if (sub.RemoveCurrentAttributes) throw new ApplicationException("Remove attributes is not supported with json files");
                    if (sub.AddAttributes.Any()) throw new ApplicationException("Add attributes is not supported with json files");
                    if (sub.ChangeAttributes.Any()) throw new ApplicationException("Change attributes is not supported with json files");
                    if (sub.HasAppendAfter) throw new ApplicationException("Append after is not supported with json files");
                    if (sub.HasAddChildContent) throw new ApplicationException("Add child is not supported with json files");

                    if (sub.HasReplacementContent) activeObject.Replace(JObject.Parse($"{{ fakeRoot: {sub.ReplacementContent.RenderTemplate(tagValues)} }}").GetValue("fakeRoot"));
                    if (sub.HasChangeValue) activeObject.Replace(new JValue(sub.ChangeValue.RenderTemplate(tagValues)));
                }
            }

            return baseJson.ToString(Formatting.Indented);
        }

        private static Substitution BuildSubstitions(XElement change, IXmlNamespaceResolver nsm, Lazy<TagDictionary> tagValues, FileType fileType)
        {
            //Default everything off
            var sub = new Substitution();

            if (change.Attribute("type") == null)
            {
                //Old style long hand
                switch (fileType)
                {
                    case FileType.Xml:
                        sub.Path = (change.Attribute("xPath")?.Value ?? change.XPathSelectElement("s:XPath", nsm)?.Value).RenderTemplate(tagValues);
                        sub.PathMatchAll = (change.Attribute("matchAll")?.Value ?? change.XPathSelectElement("s:XPath", nsm)?.Attribute("matchAll")?.Value ?? "false").RenderTemplate(tagValues).Equals("true", StringComparison.CurrentCultureIgnoreCase);
                        break;

                    case FileType.Json:
                        sub.Path = (change.Attribute("jsonPath")?.Value ?? change.XPathSelectElement("s:JsonPath", nsm)?.Value).RenderTemplate(tagValues);
                        sub.PathMatchAll = (change.Attribute("matchAll")?.Value ?? change.XPathSelectElement("s:JsonPath", nsm)?.Attribute("matchAll")?.Value ?? "false").RenderTemplate(tagValues).Equals("true", StringComparison.CurrentCultureIgnoreCase);
                        break;

                    default:
                        throw new ApplicationException("Unknown file type");
                }

                if (fileType == FileType.Xml)
                {
                    sub.AddChildContent = change.XPathSelectElement("s:AddChildContent", nsm)?.Value;
                    sub.HasAddChildContent = sub.AddChildContent != null;
                    sub.AddChildContentIfNotExists = change.XPathSelectElement("s:AddChildContent", nsm)?.Attribute("ifNotExists")?.Value;

                    sub.AppendAfter = change.XPathSelectElement("s:AppendAfter", nsm)?.Value;
                    sub.HasAppendAfter = sub.AppendAfter != null;

                    sub.RemoveCurrentAttributes = XmlConvert.ToBoolean(change.TryXPathValueWithDefault("s:RemoveCurrentAttributes", nsm, "false"));

                    foreach (var ca in change.XPathSelectElements("s:AddAttribute", nsm))
                    {
                        sub.AddAttributes.Add((ca.Attribute("attributeName")?.Value, ca.Attribute("value")?.Value ?? ca.Value));
                    }

                    foreach (var ca in change.XPathSelectElements("s:ChangeAttribute", nsm))
                    {
                        sub.ChangeAttributes.Add((ca.Attribute("attributeName")?.Value, ca.Attribute("value")?.Value ?? ca.Value));
                    }

                    sub.ReplacementContent = change.XPathSelectElement("s:ReplacementContent", nsm)?.Value;
                    sub.HasReplacementContent = sub.ReplacementContent != null;
                }

                var changeValue = change.XPathSelectElement("s:ChangeValue", nsm);
                sub.ChangeValue = changeValue?.Attribute("value") != null ? changeValue?.Attribute("value")?.Value : changeValue?.Value;
                sub.HasChangeValue = sub.ChangeValue != null;

                if (change.Attribute("if") != null)
                {
                    sub.Execute = bool.Parse($"{{% if {change.Attribute("if")?.Value} %}}true{{% else %}}false{{% endif %}}".RenderTemplate(tagValues));
                }
            }
            else
            {
                //New style short hand
                switch (fileType)
                {
                    case FileType.Xml:
                        sub.Path = change.Attribute("xPath")?.Value.RenderTemplate(tagValues);
                        break;

                    case FileType.Json:
                        sub.Path = change.Attribute("jsonPath")?.Value.RenderTemplate(tagValues);
                        break;

                    default:
                        throw new ApplicationException("Unknown file type");
                }

                sub.PathMatchAll = change.Attribute("matchAll")?.Value.RenderTemplate(tagValues).Equals("true", StringComparison.CurrentCultureIgnoreCase) ?? false;

                switch (change.Attribute("type")?.Value.ToLower())
                {
                    case "replacementcontent":
                        sub.ReplacementContent = change.Value;
                        sub.HasReplacementContent = true;
                        break;

                    case "addchildcontent" when fileType == FileType.Xml:
                        sub.AddChildContent = change.Value;
                        sub.HasAddChildContent = true;
                        sub.AddChildContentIfNotExists = change.Attribute("ifNotExists")?.Value;
                        break;

                    case "appendafter" when fileType == FileType.Xml:
                        sub.AppendAfter = change.Value;
                        sub.HasAppendAfter = true;
                        break;

                    case "removecurrentattributes" when fileType == FileType.Xml:
                        sub.RemoveCurrentAttributes = true;
                        break;

                    case "addattribute" when fileType == FileType.Xml:
                        sub.AddAttributes.Add((change.Attribute("attributeName")?.Value, change.Attribute("value")?.Value));
                        break;

                    case "changeattribute" when fileType == FileType.Xml:
                        sub.ChangeAttributes.Add((change.Attribute("attributeName")?.Value, change.Attribute("value")?.Value));
                        break;

                    case "changevalue":
                        sub.ChangeValue = change.Attribute("value") != null ? change.Attribute("value")?.Value : change.Value;
                        sub.HasChangeValue = true;
                        break;

                    default:
                        throw new Exception($"Unknown change type '{change.Attribute("type")?.Value}'");
                }

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
