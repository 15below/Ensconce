using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Ensconce.NDjango.Tests.Data
{
    public class TestData : Dictionary<string, object>
    {
        public TestData(string path)
        {
            if (File.Exists(path))
            {
                XmlDocument data = new XmlDocument();
                data.Load(path);
                foreach (XmlElement variable in data.DocumentElement)
                {
                    object value = null;
                    if (variable.Attributes["type"] != null && variable.Attributes["value"] != null)
                    {
                        switch (variable.Attributes["type"].Value)
                        {
                            case "integer":
                                value = int.Parse(variable.Attributes["value"].Value);
                                break;

                            case "string":
                                value = variable.Attributes["value"].Value;
                                break;

                            case "boolean":
                                value = bool.Parse(variable.Attributes["value"].Value);
                                break;
                        }
                    }

                    if (variable.Attributes["type"] == null && variable.Attributes["value"] != null)
                    {
                        value = variable.Attributes["value"].Value;
                    }

                    if (!this.ContainsKey(variable.Name))
                    {
                        this.Add(variable.Name, value);
                    }
                    else
                        if (this[variable.Name] is IList)
                    {
                        ((IList)this[variable.Name]).Add(value);
                    }
                    else
                    {
                        this[variable.Name] = new List<object>(new object[] { this[variable.Name], value });
                    }
                }
            }
        }
    }
}
