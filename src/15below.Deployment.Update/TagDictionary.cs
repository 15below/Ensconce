using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FifteenBelow.Deployment.Update
{
    public class TagDictionary : Dictionary<string, object>
    {
        public Dictionary<string, DbLogin> DbLogins = new Dictionary<string, DbLogin>();
        private readonly HashSet<string> idSpecificValues = new HashSet<string>();
        private readonly Dictionary<string, List<string>> labelsAndIdentities = new Dictionary<string, List<string>>();

        public TagDictionary(string identifier) : this(identifier, false, new Dictionary<TagSource, string>()) { }

        public TagDictionary(string identifier, Dictionary<TagSource, string> sources) : this(identifier, false, sources) { }

        public TagDictionary(string identifier, string xmlData) : this(identifier, false, new Dictionary<TagSource, string> { { TagSource.Environment, "" }, { TagSource.XmlData, xmlData } }) { }

        private TagDictionary(string identifier, bool isLabel, Dictionary<TagSource, string> sources)
        {
            if (sources.Count == 0)
            {
                sources = new Dictionary<TagSource, string> { { TagSource.Environment, "" } };
            }

            LoadDictionary(identifier, sources);

            if (!isLabel)
            {
                foreach (var label in labelsAndIdentities.Keys)
                {
                    if (ContainsKey(label))
                    {
                        throw new InvalidDataException(string.Format("A label for a method group has been specified that clashes with a property name. The conflicting label is {0}", label));
                    }
                    var labelEnumerable = new LabelEnumeration();
                    foreach (var id in labelsAndIdentities[label])
                    {
                        if (!labelEnumerable.ContainsKey(id))
                            labelEnumerable.Add(id, new TagDictionary(id, true, sources));
                    }
                    this[label] = labelEnumerable;
                }
            }
        }

        private void LoadDictionary(string identifier, Dictionary<TagSource, string> sources)
        {
            if (!string.IsNullOrEmpty(identifier)) AddOrDiscard("identity", identifier);

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

            ExpandDictionaryValues();

            Add("DbLogins", DbLogins);
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
                        wantedKey = wantedKey.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries).Last().Replace(".", String.Empty);
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
                AddOrDiscard(wantedKey, env[key].ToString());
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

            var environmentNode = doc.XPathSelectElement("/Structure/Environment");
            if (environmentNode != null) AddOrDiscard("Environment", environmentNode.Value);

            var clientCode = doc.XPathSelectElement("/Structure/ClientCode");
            if (clientCode != null) AddOrDiscard("ClientCode", clientCode.Value);

            var matchingGroupProperties = doc.XPathSelectElements("/Structure/PropertyGroups/PropertyGroup")
                                             .Where(p => p.Name == "PropertyGroup")
                                             .Where(p => p.Attribute("identity").Value == identifier)
                                             .Select(pg => pg.XPathSelectElements("Properties/Property"))
                                             .Aggregate(new List<XElement>(), (list, elements) => list.Concat(elements).ToList());

            foreach (var prop in matchingGroupProperties)
            {
                var key = prop.Attribute("name").Value;
                var value = prop.Value.Trim();
                AddOrDiscard(key, value, true);
            }

            var generalProperties = doc.XPathSelectElements("/Structure/Properties/Property");

            foreach (var prop in generalProperties)
            {
                var key = prop.Attribute("name").Value;
                var value = prop.Value.Trim();
                AddOrDiscard(key, value);
            }

            var labelledGroups = doc.XPathSelectElements("/Structure/PropertyGroups/PropertyGroup").Where(pg => pg.Elements().Select(e => e.Name).Contains("Label"));

            foreach (var labelledGroup in labelledGroups)
            {
                var labels = labelledGroup.Elements("Label").Select(e => e.Value);
                var identity = labelledGroup.Attribute("identity").Value;
                foreach (var label in labels)
                {
                    if (labelsAndIdentities.Keys.Contains(label))
                    {
                        labelsAndIdentities[label].Add(identity);
                    }
                    else
                    {
                        labelsAndIdentities[label] = new List<string> { identity };
                    }
                }
            }

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

                if (!DbLogins.ContainsKey(dbKey))
                {
                    var password = string.Empty;
                    var defaultDb = string.Empty;
                    string connectionString;

                    if (string.IsNullOrWhiteSpace(dbLoginElement.TryXPathValueWithDefault("ConnectionString", "")))
                    {
                        password = dbLoginElement.XPathSelectElement("Password").Value;
                        defaultDb = dbLoginElement.XPathSelectElement("DefaultDb").Value;
                        connectionString = string.Format("Data Source={{{{ DbServer }}}}; Initial Catalog={0}; User ID={1}; Password={2};", dbLoginElement.XPathSelectElement("DefaultDb").Value, username, dbLoginElement.XPathSelectElement("Password").Value);
                    }
                    else
                    {
                        connectionString = dbLoginElement.XPathSelectElement("ConnectionString").Value;
                    }

                    DbLogins.Add(dbKey, new DbLogin
                    {
                        Username = username,
                        Password = password,
                        DefaultDb = defaultDb,
                        ConnectionString = connectionString
                    });
                }
            }
        }

        private void ExpandDictionaryValues()
        {
            this.ToList().ForEach(pair => this[pair.Key] = GetExpandedPropertyValue(pair.Value.ToString()));

            foreach (var dbLogin in DbLogins.Values)
            {
                dbLogin.DefaultDb = GetExpandedPropertyValue(dbLogin.DefaultDb);
                dbLogin.Username = GetExpandedPropertyValue(dbLogin.Username);
                dbLogin.ConnectionString = GetExpandedPropertyValue(dbLogin.ConnectionString);
            }
        }

        private string GetExpandedPropertyValue(string baseValue)
        {
            // TODO: Remove this replacement set once move to delimetered tags complete
            var djangoStyleValue = baseValue.Replace("tagDbServer", "{{ DbServer }}")
                                            .Replace("tagClientCode", "{{ ClientCode }}")
                                            .Replace("tagEnvironment", "{{ Environment }}");

            return djangoStyleValue.Contains("{") ? djangoStyleValue.RenderTemplate(this) : baseValue;
        }

        public void AddOrDiscard(string key, string value, bool idSpecific = false)
        {
            if (!Keys.Contains(key)) Add(key, value);
            if (Keys.Contains(key) && !idSpecificValues.Contains(key) && idSpecific) this[key] = value;
            if (idSpecific) idSpecificValues.Add(key);
        }

        public string GetDbPassword(string dbUserName)
        {
            return DbLogins[dbUserName].Password;
        }

        public class DbLogin
        {
            public string Username;
            public string Password;
            public string DefaultDb;
            public string ConnectionString;

            public override string ToString()
            {
                return Username;
            }
        }
    }

    public enum TagSource
    {
        Environment,
        XmlFileName,
        XmlData
    }
}