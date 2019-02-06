using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Ensconce.Update
{
    public class TagDictionary : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> innerDictionary;

        private TagDictionary() : this(new Dictionary<string, object>()) { }

        private TagDictionary(IDictionary<string, object> baseDictionary)
        {
            innerDictionary = baseDictionary;
        }

        public static TagDictionary Empty()
        {
            var tagDictionary = new TagDictionary();
            tagDictionary.LoadDictionary(string.Empty, new Dictionary<TagSource, string>());
            return tagDictionary;
        }

        public static TagDictionary FromIdentifier(string identifier)
        {
            var tagDictionary = new TagDictionary();
            tagDictionary.LoadDictionary(identifier, new Dictionary<TagSource, string> { { TagSource.Environment, "" } });
            return tagDictionary;
        }

        public static TagDictionary FromXml(string identifier, string xmlData)
        {
            var tagDictionary = new TagDictionary();
            tagDictionary.LoadDictionary(identifier, new Dictionary<TagSource, string> { { TagSource.Environment, "" }, { TagSource.XmlData, xmlData } });
            return tagDictionary;
        }

        public static TagDictionary FromSources(string identifier, Dictionary<TagSource, string> sources)
        {
            if (sources.Count == 0)
            {
                sources = new Dictionary<TagSource, string> { { TagSource.Environment, "" } };
            }

            var tagDictionary = new TagDictionary();
            tagDictionary.LoadDictionary(identifier, sources);
            return tagDictionary;
        }

        public static TagDictionary FromDictionary(IDictionary<string, object> raw)
        {
            return new TagDictionary(raw);
        }

        private void LoadDictionary(string identifier, Dictionary<TagSource, string> sources)
        {
            if (!string.IsNullOrEmpty(identifier)) this.AddOrDiscard("identity", identifier);

            //Load environment 1st
            if (sources.ContainsKey(TagSource.Environment))
            {
                LoadEnvironment();
            }

            //Load by xml data 2nd
            if (sources.ContainsKey(TagSource.XmlData))
            {
                PropertiesFromXml(identifier, sources[TagSource.XmlData]);
            }

            //Load xml file name 3rd
            if (sources.ContainsKey(TagSource.XmlFileName))
            {
                PropertiesFromXml(identifier, File.ReadAllText(sources[TagSource.XmlFileName]));
            }

            if (ContainsKey("Environment") && ((string)this["Environment"]).StartsWith("DR-"))
            {
                if (ContainsKey("IsDRMachine") && ((string)this["IsDRMachine"]).ToLower() == "true")
                {
                    this["Environment"] = ((string)this["Environment"]).Substring(3);
                }
            }

            ExpandDictionaryValues(this);
        }

        private void LoadEnvironment()
        {
            var env = Environment.GetEnvironmentVariables();
            foreach (string key in env.Keys)
            {
                var wantedKey = key;
                if (wantedKey.StartsWith("Octopus"))
                {
                    if (wantedKey.Contains("."))
                    {
                        wantedKey = wantedKey.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries).Last();
                    }
                    else
                    {
                        wantedKey = wantedKey.Split(new[] { "Octopus" }, 2, StringSplitOptions.RemoveEmptyEntries).Last();
                    }

                    if (wantedKey.EndsWith("Name"))
                    {
                        wantedKey = wantedKey.Substring(0, wantedKey.Length - 4);
                    }
                }
                this.AddOrDiscard(wantedKey.Replace(".", String.Empty), env[key].ToString());
            }
        }

        private void PropertiesFromXml(string identifier, string xmlData)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Parse(xmlData);
            }
            catch (Exception)
            {
                return;
            }

            ValidateStructureFile(doc);
            BasePropertiesFromXml(doc);
            IdentityPropertyGroupsFromXml(identifier, doc);
            GeneralPropertiesFromXml(doc);
            LabelPropertiesFromXml(doc);
            DbLoginPropertiesFromXml(doc);
        }

        private static void ValidateStructureFile(XDocument doc)
        {
            var schemas = new XmlSchemaSet();
            var assembly = Assembly.GetExecutingAssembly();

            schemas.Add(null, XmlReader.Create(assembly.GetManifestResourceStream("Ensconce.Update.FixedStructure.xsd")));

            doc.Validate(schemas, (sender, args) => { throw args.Exception; });
        }

        private void BasePropertiesFromXml(XDocument doc)
        {
            var environmentNode = doc.XPathSelectElement("/Structure/Environment");
            if (environmentNode != null) this.AddOrDiscard("Environment", environmentNode.Value);

            var clientCode = doc.XPathSelectElement("/Structure/ClientCode");
            if (clientCode != null) this.AddOrDiscard("ClientCode", clientCode.Value);
        }

        private void IdentityPropertyGroupsFromXml(string identifier, XDocument doc)
        {
            var matchingGroupProperties = doc.XPathSelectElements("/Structure/PropertyGroups/PropertyGroup")
                .Where(p => p.Name == "PropertyGroup")
                .Where(p => p.Attribute("identity").Value == identifier)
                .SelectMany(pg => pg.XPathSelectElements("Properties/Property"));

            foreach (var prop in matchingGroupProperties)
            {
                var key = prop.Attribute("name").Value;
                var value = prop.Value.Trim();
                this[key] = value;
            }
        }

        private void GeneralPropertiesFromXml(XDocument doc)
        {
            var generalProperties = doc.XPathSelectElements("/Structure/Properties/Property");
            foreach (var prop in generalProperties)
            {
                var key = prop.Attribute("name").Value;
                var value = prop.Value.Trim();
                this.AddOrDiscard(key, value);
            }
        }

        private void LabelPropertiesFromXml(XDocument doc)
        {
            foreach (var propertyGroup in doc.XPathSelectElements("/Structure/PropertyGroups/PropertyGroup"))
            {
                var labels = new List<string>();
                if (propertyGroup.Attribute("label") != null)
                {
                    labels.Add(propertyGroup.Attribute("label").Value);
                }
                else if (propertyGroup.XPathSelectElements(".//Label").Any())
                {
                    labels.AddRange(propertyGroup.XPathSelectElements(".//Label").Select(x => x.Value));
                }
                else
                {
                    throw new InvalidDataException("PropertyGroup found with no label attribute or nodes");
                }

                var identity = propertyGroup.Attribute("identity").Value;
                foreach (var label in labels)
                {
                    SubTagDictionary labelDic;
                    if (ContainsKey(label))
                    {
                        labelDic = this[label] as SubTagDictionary;
                        if (labelDic == null)
                        {
                            throw new InvalidDataException(string.Format("A label for a method group has been specified that clashes with a property name. The conflicting label is {0}", label));
                        }
                    }
                    else
                    {
                        labelDic = new SubTagDictionary();
                        Add(label, labelDic);
                    }

                    Dictionary<string, object> instanceDic;
                    if (labelDic.ContainsKey(identity))
                    {
                        instanceDic = labelDic[identity] as Dictionary<string, object>;
                    }
                    else
                    {
                        instanceDic = new Dictionary<string, object> { { "identity", identity } };
                        labelDic.Add(identity, instanceDic);
                    }

                    foreach (var prop in propertyGroup.XPathSelectElements(".//Property"))
                    {
                        var key = prop.Attribute("name").Value;
                        var value = prop.Value.Trim();
                        instanceDic.AddOrDiscard(key, value);
                    }
                }
            }
        }

        private void DbLoginPropertiesFromXml(XDocument doc)
        {
            var dbLoginElements = doc.XPathSelectElements("/Structure/DbLogins/DbLogin");
            foreach (var dbLoginElement in dbLoginElements)
            {
                var username = dbLoginElement.XPathSelectElement("Name").Value;
                string dbKey;
                if (dbLoginElement.XPathSelectElement("Key") != null && !string.IsNullOrWhiteSpace(dbLoginElement.XPathSelectElement("Key").Value))
                {
                    dbKey = dbLoginElement.XPathSelectElement("Key").Value;
                }
                else
                {
                    dbKey = username.StartsWith("tagClientCode-tagEnvironment-") ? username.Substring(29) : username;
                }

                SubTagDictionary dbLogins;
                if (ContainsKey("DbLogins"))
                {
                    dbLogins = this["DbLogins"] as SubTagDictionary;
                }
                else
                {
                    dbLogins = new SubTagDictionary();
                    Add("DbLogins", dbLogins);
                }

                if (!dbLogins.ContainsKey(dbKey))
                {
                    var dbLoginDic = new Dictionary<string, object> { { "Username", username } };
                    if (string.IsNullOrWhiteSpace(dbLoginElement.TryXPathValueWithDefault("ConnectionString", "")))
                    {
                        dbLoginDic.Add("Password", dbLoginElement.XPathSelectElement("Password").Value);
                        dbLoginDic.Add("DefaultDb", dbLoginElement.XPathSelectElement("DefaultDb").Value);
                        dbLoginDic.Add("ConnectionString", string.Format("Data Source={{{{ DbServer }}}}; Initial Catalog={0}; User ID={1}; Password={2};", dbLoginElement.XPathSelectElement("DefaultDb").Value, username, dbLoginElement.XPathSelectElement("Password").Value));
                    }
                    else
                    {
                        dbLoginDic.Add("ConnectionString", dbLoginElement.XPathSelectElement("ConnectionString").Value);
                    }
                    dbLogins.Add(dbKey, dbLoginDic);
                }
            }
        }

        private void ExpandDictionaryValues(IDictionary<string, object> dictionaryToExpand)
        {
            foreach (var key in dictionaryToExpand.Keys.ToList())
            {
                var valueAsString = dictionaryToExpand[key] as string;
                if (valueAsString != null)
                {
                    dictionaryToExpand[key] = GetExpandedPropertyValue(valueAsString);
                }
                else
                {
                    var valueAsSubTagDictionary = dictionaryToExpand[key] as SubTagDictionary;
                    if (valueAsSubTagDictionary != null)
                    {
                        foreach (var subTagDictionary in valueAsSubTagDictionary.Values)
                        {
                            ExpandDictionaryValues(subTagDictionary);
                        }
                    }
                }
            }
        }

        private string GetExpandedPropertyValue(string baseValue)
        {
            // TODO: Remove this replacement set once move to delimetered tags complete
            var djangoStyleValue = baseValue.Replace("tagDbServer", "{{ DbServer }}")
                                            .Replace("tagClientCode", "{{ ClientCode }}")
                                            .Replace("tagEnvironment", "{{ Environment }}");

            return djangoStyleValue.RenderTemplate(new Lazy<TagDictionary>(() => this));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return innerDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)innerDictionary).GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            innerDictionary.Add(item);
        }

        public void Clear()
        {
            innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return innerDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            innerDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return innerDictionary.Remove(item);
        }

        public int Count => innerDictionary.Count;

        public bool IsReadOnly => innerDictionary.IsReadOnly;

        public bool ContainsKey(string key)
        {
            return innerDictionary.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            innerDictionary.Add(key, value);
        }

        public bool Remove(string key)
        {
            return innerDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return innerDictionary.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get => innerDictionary[key];
            set => innerDictionary[key] = value;
        }

        public ICollection<string> Keys => innerDictionary.Keys;

        public ICollection<object> Values => innerDictionary.Values;
    }
}