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
        // These values (and no others) can be used as tags in property values
        private readonly HashSet<string> idSpecificValues = new HashSet<string>();
        private readonly Dictionary<string, IEnumerable<string>> labelsAndIdentities = new Dictionary<string, IEnumerable<string>>();

        public TagDictionary(string identifier, params Tuple<string, TagSource>[] sources) : this(identifier, false, sources)
        { }

        public TagDictionary(string identifier, string xmlData) : this(identifier, Tuple.Create("", TagSource.Environment), Tuple.Create(xmlData, TagSource.XmlData))
        { }

        private TagDictionary(string identifier, bool isLabel, params Tuple<string, TagSource>[] sources)
        {
            sources = FixSources(sources);

            LoadDictionary(identifier, sources);

            ExpandDictionaryValues();

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

        private static Tuple<string, TagSource>[] FixSources(Tuple<string, TagSource>[] sources)
        {
            if (sources.Length == 0)
            {
                sources = new[] { Tuple.Create("", TagSource.Environment) };
            }
            else if (sources.Any(x => x.Item2 == TagSource.XmlFileName))
            {
                //convert xmlFileName to be xmlData
                var newSources = new List<Tuple<string, TagSource>>();
                newSources.AddRange(sources.Where(x => x.Item2 == TagSource.Environment || x.Item2 == TagSource.XmlData));

                foreach (var source in sources.Where(x => x.Item2 == TagSource.XmlFileName))
                {
                    XDocument doc;
                    try
                    {
                        doc = XDocument.Load(new FileStream(source.Item1, FileMode.Open, FileAccess.Read));
                    }
                    catch (Exception)
                    {
                        doc = new XDocument();
                    }

                    newSources.Add(new Tuple<string, TagSource>(doc.ToString(), TagSource.XmlData));
                }

                sources = newSources.ToArray();
            }
            return sources;
        }

        private void LoadDictionary(string identifier, Tuple<string, TagSource>[] sources)
        {
            if (!string.IsNullOrEmpty(identifier)) AddOrDiscard("identity", identifier);
            Add("DbLogins", DbLogins);

            //Load environment 1st
            if (sources.Any(x => x.Item2 == TagSource.Environment))
            {
                LoadEnvironment();
            }

            //Load by xml data 2nd
            foreach (var source in sources.Where(x => !string.IsNullOrEmpty(x.Item1) && x.Item2 == TagSource.XmlData))
            {
                XDocument doc = null;

                try
                {
                    doc = XDocument.Parse(source.Item1);
                }
                catch (Exception)
                {
                    break;
                }

                PropertiesFromXml(identifier, doc);
            }

            if (ContainsKey("Environment") && ((string) this["Environment"]).StartsWith("DR-"))
            {
                if (ContainsKey("IsDRMachine") && ((string) this["IsDRMachine"]).ToLower() == "true")
                {
                    this["Environment"] = ((string) this["Environment"]).Substring(3);
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

        private void PropertiesFromXml(string identifier, XDocument doc)
        {
            var environmentNode = doc.XPathSelectElement("/Structure/Environment");
            if(environmentNode != null) AddOrDiscard("Environment", environmentNode.Value);

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
                        labelsAndIdentities[label] = labelsAndIdentities[label].Concat(new List<string> { identity });
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

        private string GetExpandedPropertyValue(string baseValue)
        {
            // TODO: Remove this replacement set once move to delimetered tags complete
            var djangoStyleValue = baseValue.Replace("tagDbServer", "{{ DbServer }}")
                                            .Replace("tagClientCode", "{{ ClientCode }}")
                                            .Replace("tagEnvironment", "{{ Environment }}");

            return djangoStyleValue.Contains("{") ? djangoStyleValue.RenderTemplate(this) : baseValue;
        }

        private void LoadEnvironment()
        {
            var env = Environment.GetEnvironmentVariables();
            foreach (string key in env.Keys)
            {
                var wantedKey = key;
                if (wantedKey.StartsWith("Octopus"))
                {
                    wantedKey = GetOctopusVariable(wantedKey);
                }
                AddOrDiscard(wantedKey, env[key].ToString());
            }
        }

        private static string GetOctopusVariable(string variableName)
        {
            if (variableName.Contains("."))
            {
                variableName = variableName.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries).Last().Replace(".", String.Empty);
            }
            else
            {
                variableName = variableName.Split(new[] { "Octopus" }, 2, StringSplitOptions.RemoveEmptyEntries).Last();
            }

            if (variableName.EndsWith("Name"))
            {
                variableName = variableName.Substring(0, variableName.Length - "Name".Length);
            }

            return variableName;
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