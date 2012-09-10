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
        public struct DbLogin
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

        public struct User
        {
            public string Name;
            public string Role;
        }

        public Dictionary<string, DbLogin> DbLogins = new Dictionary<string, DbLogin>();
        // These values (and no others) can be used as tags in property values
        private static readonly string[] ValidTagsInProperties = new[] {"ClientCode", "Environment", "DbServer"};
        private HashSet<string> idSpecificValues = new HashSet<string>();
        private Dictionary<string, IEnumerable<string>> labelsAndIdentities = new Dictionary<string, IEnumerable<string>>();

        private TagDictionary(string identifier, bool isLabel, params Tuple<string, TagSource>[] sources)
        {
            if (sources.Length == 0) { sources = new []{ Tuple.Create("", TagSource.Environment)};}
            foreach (var source in sources)
            {
                switch (source.Item2)
                {
                    case TagSource.Environment:
                        LoadEnviroment();
                        break;
                    case TagSource.XmlFileName:
                        LoadXmlFileName(source.Item1, identifier);
                        break;
                    case TagSource.XmlData:
                        LoadXmlData(source.Item1, identifier);
                        break;
                }
            }
            this.ToList().ForEach(pair => this[pair.Key] = GetExpandedPropertyValue(pair.Value.ToString()));
            var expandedLogins = new Dictionary<string, DbLogin>();
            foreach(var name in DbLogins.Keys)
            {
                var login = DbLogins[name];
                expandedLogins[name] = new DbLogin{DefaultDb = GetExpandedPropertyValue(login.DefaultDb), Password = login.Password, Username = GetExpandedPropertyValue(login.Username), ConnectionString = GetExpandedPropertyValue(login.ConnectionString)};
            }
            DbLogins = expandedLogins;
            Add("DbLogins", DbLogins);
            if(!isLabel)
            {
                foreach (var label in labelsAndIdentities.Keys)
                {
                    if(ContainsKey(label))
                    {
                        throw new InvalidDataException(
                            string.Format(
                                "A label for a method group has been specified that clashes with a property name. The conflicting label is {0}",
                                label));
                    }
                    var labelEnumerable = new List<IDictionary<string, object>>();
                    foreach(var id in labelsAndIdentities[label])
                    {
                        labelEnumerable.Add(new TagDictionary(id, true, sources));
                    }
                    this[label] = labelEnumerable;
                }
            }
        }
        
        public TagDictionary(string identifier, params Tuple<string, TagSource>[] sources) : this(identifier, false, sources)
        {
            
        }

        public TagDictionary(string identifier, string xmlData) : 
            this(identifier, Tuple.Create("", TagSource.Environment), Tuple.Create(xmlData, TagSource.XmlData))
        { }

        private void LoadXmlFileName(string fileName, string identifier)
        {
            var doc = XDocument.Load(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            PropertiesFromXml(identifier, doc);
        }

        private void LoadXmlData(string xml, string identifier)
        {
            if(string.IsNullOrEmpty(xml)) return;
            var doc = XDocument.Parse(xml);
            PropertiesFromXml(identifier, doc);
        }

        private static Tuple<string, string> GetTagInPropertyValue(XDocument doc, string propName)
        {
            var xEl = doc.XPathSelectElement(string.Format("Structure/{0}", propName)) ??
                      doc.XPathSelectElements("/Structure/Properties/Property").FirstOrDefault(
                          el => el.Attribute("name").Value == propName);
            return Tuple.Create(propName, xEl == null ? "" : xEl.Value);
        }

        private void PropertiesFromXml(string identifier, XDocument doc)
        {
            if(!string.IsNullOrEmpty(identifier)) AddOrDiscard("identity", identifier);
            ValidTagsInProperties
                .Select(str => GetTagInPropertyValue(doc, str))
                .ToList()
                .ForEach(tuple => AddOrDiscard(tuple.Item1, tuple.Item2));

            var matchingGroupProperties =
                doc.XPathSelectElements("/Structure/PropertyGroups/PropertyGroup")
                    .Where(p => p.Name == "PropertyGroup").Where(p => p.Attribute("identity").Value == identifier)
                    .Select(pg => pg.XPathSelectElements("Properties/Property"))
                    .Aggregate(new List<XElement>(), (list, elements) => list.Concat(elements).ToList());
            var generalProperties =
                doc.XPathSelectElements("/Structure/Properties/Property");
            var labelledGroups =
                doc.XPathSelectElements("/Structure/PropertyGroups/PropertyGroup")
                    .Where(pg => pg.Elements().Select(e => e.Name).Contains("Label"));

            foreach (var prop in matchingGroupProperties)
            {
                var key = prop.Attribute("name").Value;
                var value = prop.Value.Trim();
                AddOrDiscard(key, value, true);
            }
            foreach (var prop in generalProperties)
            {
                var key = prop.Attribute("name").Value;
                var value = prop.Value.Trim();
                AddOrDiscard(key, value);
            }
            foreach (var labelledGroup in labelledGroups)
            {
                var labels = labelledGroup.Elements("Label").Select(e => e.Value);
                var identity = labelledGroup.Attribute("identity").Value;
                foreach (var label in labels)
                {
                    if (labelsAndIdentities.Keys.Contains(label))
                    {
                        labelsAndIdentities[label] = labelsAndIdentities[label].Concat(new List<string> {identity});
                    }
                    else
                    {
                        labelsAndIdentities[label] = new List<string> {identity};
                    }
                }
            }

            var dbLoginElements =
                doc.XPathSelectElements("/Structure/DbLogins/DbLogin");
            foreach (var dbLoginElement in dbLoginElements)
            {
                var fullname = dbLoginElement.XPathSelectElement("Name").Value;
                const string prefix = "tagClientCode-tagEnvironment-";
                var key = fullname.StartsWith(prefix) ? fullname.Substring(prefix.Length) : fullname;
                if (!DbLogins.ContainsKey(key))
                {
                    DbLogins.Add(key,
                    new DbLogin
                        {
                            Username = fullname,
                            Password = dbLoginElement.XPathSelectElement("Password").Value,
                            DefaultDb = dbLoginElement.XPathSelectElement("DefaultDb").Value,
                            ConnectionString = string.Format("Data Source={{{{ DbServer }}}}; Initial Catalog={0}; User ID={1}; Password={2};", dbLoginElement.XPathSelectElement("DefaultDb").Value, fullname, dbLoginElement.XPathSelectElement("Password").Value)
                        });
                }
            }
        }

        private string GetExpandedPropertyValue(string baseValue)
        {
            var shortTagList = ValidTagsInProperties.Where(Keys.Contains).ToDictionary(str => str, str => this[str]);

            // TODO: Remove this replacement set once move to delimetered tags complete
            var djangoStyleValue = baseValue
                .Replace("tagDbServer", "{{ DbServer }}")
                .Replace("tagClientCode", "{{ ClientCode }}")
                .Replace("tagEnvironment", "{{ Environment }}");
            return djangoStyleValue.RenderTemplate(shortTagList);
        }

        private void LoadEnviroment()
        {
            var env = Environment.GetEnvironmentVariables();
            foreach (var key in env.Keys)
            {
                var wantedKey = key.ToString();
                if(wantedKey.StartsWith("Octopus"))
                {
                    wantedKey = wantedKey
                        .Split(new[] {"Octopus"}, 2, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (wantedKey.EndsWith("Name"))
                        wantedKey = wantedKey.Substring(0, wantedKey.Length - "Name".Length);
                }
                AddOrDiscard(wantedKey, env[key].ToString());
            }
        }

        public void AddOrDiscard(string key, string value, bool idSpecific = false)
        {
            if (!Keys.Contains(key))
                Add(key, value); 
            if (Keys.Contains(key) && !idSpecificValues.Contains(key) && idSpecific)
                this[key] = value;
            if (idSpecific) idSpecificValues.Add(key);
        }

        public string GetDbPassword(string dbUserName)
        {
            return DbLogins[dbUserName].Password;
        }
    }

    public enum TagSource
    {
        Environment,
        XmlFileName,
        XmlData
    }
}