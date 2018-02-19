using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FifteenBelow.Deployment.Update.Tests
{
    [TestFixture]
    public class TagDictionaryTests
    {
        public TagDictionaryTests()
        {
            testLock = new Mutex();
        }

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            testLock.WaitOne();
            var testEnv = new Dictionary<string, string>
                              {
                                  {IsSys, IsSys},
                                  {QueAppServer, QueAppServer},
                                  {"ClientCode", EnvClientCode},
                                  {"Environment", EnvEnvironment}
                              };
            testEnv.ToList().ForEach(
                kv => Environment.SetEnvironmentVariable(kv.Key, kv.Value, EnvironmentVariableTarget.Process));
            Directory.SetCurrentDirectory("TagDictionaryTestFiles");
        }

        [TearDown]
        public void TearDown()
        {
            Directory.SetCurrentDirectory("..");
            testLock.ReleaseMutex();
        }

        #endregion

        private Mutex testLock;
        private const string EnvEnvironment = "LOC";
        private const string EnvClientCode = "FAA";
        private const string IsSys = "IsSys";
        private const string IsSysValue = "SYS";
        private const string XMLFilename = "structure.xml";
        private const string QueAppServer = "QueueAppServer";
        private const string Avalue = "avalue";
        private const string Idvalue = "idvalue";

        private readonly Lazy<string> xml = new Lazy<string>(() => new StreamReader(File.OpenRead(@"webservice-structure.xml")).ReadToEnd());

        private string XmlData
        {
            get { return xml.Value; }
        }

        [Test]
        public void FirstParamTakesPrecedence()
        {
            var loader = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.Environment, "" }, { TagSource.XmlFileName, XMLFilename } });
            Assert.AreEqual(QueAppServer, loader[QueAppServer]);
        }

        [Test]
        public void IdentifiedPropertiesTakePrecedence()
        {
            var loader = new TagDictionary("myId", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual(IsSysValue, loader[IsSys]);
        }

        [Test]
        public void LoadFromEnvironment()
        {
            var loader = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.Environment, "" } });
            Assert.AreEqual(IsSys, loader[IsSys]);
        }

        [Test]
        public void LoadFromOctopus1XEnvironment()
        {
            var environment = "LOC";
            Environment.SetEnvironmentVariable("Environment", "");
            Environment.SetEnvironmentVariable("OctopusEnvironmentName", environment);
            var loader = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.Environment, "" } });
            Assert.AreEqual(environment, loader["Environment"]);
            Environment.SetEnvironmentVariable("OctopusEnvironmentName", "");
        }

        [Test]
        public void LoadFromOctopus2XEnvironment()
        {
            var environment = "DEV";
            Environment.SetEnvironmentVariable("Environment", "");
            Environment.SetEnvironmentVariable("Octopus.Environment.Name", environment);
            var loader = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.Environment, "" } });
            Assert.AreEqual(environment, loader["Environment"]);
            Environment.SetEnvironmentVariable("Octopus.Environment.Name", "");
        }

        [Test]
        public void LoadFromXmlData()
        {
            var loader = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("AndThisWouldBeAPassword", loader["DbPassword"]);
        }

        [Test]
        public void LoadEmptyXmlData()
        {
            Assert.DoesNotThrow(() => new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, "" } }));
        }

        [Test]
        public void LoadFromXmlFileName()
        {
            var loader = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlFileName, XMLFilename } });
            Assert.AreEqual("SomeUserName", loader["DbUser"]);
        }

        [Test]
        public void LoadEnvironmentOnlyWithInvalidXML()
        {
            var sut = new TagDictionary("ident", "ThisIsNotXML");
            Assert.AreEqual(EnvClientCode, sut["ClientCode"]);
        }

        [Test]
        public void LoadEnvironmentOnlyWithInvalidXMLFile()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.Environment, "" }, { TagSource.XmlFileName, "incorrectStructure.xml" } });
            Assert.AreEqual(EnvClientCode, sut["ClientCode"]);
        }

        [TestCase(QueAppServer, QueAppServer)]
        [TestCase("Overridden!", "DbEncoded")]
        public void TestDefaultLoader(string expected, string key)
        {
            var loader = new TagDictionary("myId", XmlData);
            Assert.AreEqual(expected, loader[key]);
        }

        [Test]
        public void IdValueTakesPrecidenceEvenFromLaterSource()
        {
            var loader = new TagDictionary("myId", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData }, { TagSource.XmlFileName, "structure.xml" } });
            Assert.AreEqual(Idvalue, loader[Avalue]);
        }

        private static string GetDbLoginValue(TagDictionary dic, string db, string value)
        {
            var dbLogins = dic["DbLogins"] as SubTagDictionary;
            var dbSettings = dbLogins[db] as Dictionary<string, object>;
            return dbSettings[value] as string;
        }

        [Test]
        public void DbPasswordAccessibleViaDictionaryWithoutPrefix()
        {
            var sut = new TagDictionary("myId", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData }, { TagSource.XmlFileName, "structure.xml" } });
            Assert.AreEqual("Some high entrophy random text", GetDbLoginValue(sut, "AUDIT", "Password"));
        }

        [TestCase("FAA.", "ClientDomain")]
        [TestCase(".LOC.", "ClientDomain")]
        public void TestTagsInPropertiesAreSubstituted(string expected, string key)
        {
            var tagDict = new TagDictionary("myId", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData }, { TagSource.XmlFileName, "structure.xml" } });
            Assert.IsTrue(tagDict[key].ToString().Contains(expected));
        }

        [Test]
        public void SuccessfullyGetDbPassword()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlFileName, XMLFilename } });
            Assert.AreEqual("NoPasswordsRoundHere", GetDbLoginValue(sut, "config", "Password"));
        }

        [Test]
        public void DbLoginsGenerated()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.AreEqual("This isn't a password either", GetDbLoginValue(sut, "config", "Password"));
        }

        [Test]
        public void DbLoginNamesAreSubstituted()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.AreEqual(string.Format("{0}-{1}-AUDIT", Environment.GetEnvironmentVariable("ClientCode"), Environment.GetEnvironmentVariable("Environment")), GetDbLoginValue(sut, "AUDIT", "Username"));
        }

        [Test]
        public void DbLoginDefaultDbsAreSubstituted()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.AreEqual("FAA-LOC-AUDIT", GetDbLoginValue(sut, "AUDIT", "DefaultDb"));
        }

        [Test]
        public void DbLoginConnectionStringLoaded()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.AreEqual("Actual ConnectionString", GetDbLoginValue(sut, "ConnectionString", "ConnectionString"));
        }

        [Test]
        public void DbLoginWithKey()
        {
            var sut = new TagDictionary("ident", XmlData);

            Assert.AreEqual("Data Source=DbServerAddress; Initial Catalog=ZZ-ENV-LOGIN; User ID=ZZ-ENV-LOGIN; Password=Some high entrophy random text;", GetDbLoginValue(sut, "LOGIN", "ConnectionString"));
            Assert.AreEqual("ZZ-ENV-LOGIN", GetDbLoginValue(sut, "LOGIN", "Username"));
            Assert.AreEqual("ZZ-ENV-LOGIN", GetDbLoginValue(sut, "LOGIN", "DefaultDb"));
        }

        [Test]
        public void DBLoginsWork()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("ZZ-ENV-LOGIN", "{{ DbLogins.LOGIN.Username }}".RenderTemplate(sut));
        }

        [Test]
        public void LoadLabelledGroups()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.IsTrue(sut.ContainsKey("GDS"));
        }

        [Test]
        public void LoadFlatLabelledGroups()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.IsTrue(sut.ContainsKey("FlatGroup"));
        }

        [Test]
        public void LoadLabelledGroupsValuesAndNormalIdentityValuesAvailable()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlFileName, "structure.xml" } });
            Assert.AreEqual("SYS", ((IEnumerable<IDictionary<string, object>>)sut["GDS"]).First()["IsSys"]);
            Assert.AreEqual("SomeUserName", sut["DbUser"]);
            Assert.AreEqual("notSYS", sut["IsSys"].ToString());
        }

        [Test]
        public void LoadLabelledGroupsBuildsEnumeratorCorrectly()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            var isSysCollection = ((IEnumerable<IDictionary<string, object>>)sut["GDS"]).Select(gds => gds[IsSys].ToString());
            Assert.IsTrue(new HashSet<string> { "SYS", "SYS2" }.IsSupersetOf(new HashSet<string>(isSysCollection)));
        }

        [Test]
        public void PropertyAndLabelWithSameNameThrowException()
        {
            Environment.SetEnvironmentVariable("GDS", "This shouldn't be here");
            Assert.Throws<InvalidDataException>(() => new TagDictionary("ident", XmlData));
            Environment.SetEnvironmentVariable("GDS", null);
        }

        [Test]
        public void InstanceNameIsAccessibleWhileEnumerating()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            var isSysCollection = ((IEnumerable<IDictionary<string, object>>)sut["GDS"]).Select(gds => gds["identity"].ToString());
            Assert.IsTrue(new HashSet<string> { "myId", "myId2" }.IsSupersetOf(new HashSet<string>(isSysCollection)));
        }

        [TestCase("OctopusEnvironmentName", "DIF", "Environment")]
        [TestCase("OctopusPackageVersion", "5.2.0.1", "PackageVersion")]
        [TestCase("MachineName", "localhost", "MachineName")]
        public void OctopusVariablesConvertedToFriendlyTagNames(string key, string value, string friendlyKey)
        {
            Environment.SetEnvironmentVariable("Environment", null);
            Environment.SetEnvironmentVariable(key, value);
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.Environment, "" }, { TagSource.XmlData, XmlData } });
            Assert.AreEqual(value, sut[friendlyKey]);
        }

        [Test]
        public void UseAnInvalidProperty_Throws()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            var error = Assert.Throws<ArgumentException>(() => "{{ NotARealProperty }}".RenderTemplate(sut));
            Assert.AreEqual("Tag substitution failed on template string:\n{{ NotARealProperty }}\n\nAttempted rendering was:\n[ERROR OCCURRED HERE]", error.Message);
        }

        [Test]
        public void ForLoopAccessGlobalPropertyFromInstance_Throws()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            var error = Assert.Throws<ArgumentException>(() => "{% for instance in GDS %}{{ instance.DbUser }}{% endfor %}".RenderTemplate(sut));
            Assert.AreEqual("Tag substitution failed on template string:\n{% for instance in GDS %}{{ instance.DbUser }}{% endfor %}\n\nAttempted rendering was:\n[ERROR OCCURRED HERE][ERROR OCCURRED HERE]", error.Message);
        }

        [Test]
        public void ForLoopAccessGlobalPropertyFromInstanceInIf_Throws()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            var error = Assert.Throws<ArgumentException>(() => "{% for instance in GDS %}{% if instance.DbUser == \"true\" %}TextHere{% endif %}{% endfor %}".RenderTemplate(sut));
            Assert.AreEqual("Tag substitution errored on template string:\n{% for instance in GDS %}{% if instance.DbUser == \"true\" %}TextHere{% endif %}{% endfor %}\nObject must be of type String.", error.Message);
        }

        [Test]
        public void ForLoopAccessGlobalPropertyFromInstanceInIfEqual_Throws()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            var error = Assert.Throws<ArgumentException>(() => "{% for instance in GDS %}{% ifequal instance.DbUser \"true\" %}TextHere{% endifequal %}{% endfor %}".RenderTemplate(sut));
            Assert.AreEqual("Tag substitution errored on template string:\n{% for instance in GDS %}{% ifequal instance.DbUser \"true\" %}TextHere{% endifequal %}{% endfor %}", error.Message);
        }

        [Test]
        public void ForLoopAccessGlobalPropertyFromInstanceInIfNotEqual_Throws()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            var error = Assert.Throws<ArgumentException>(() => "{% for instance in GDS %}{% ifnotequal instance.DbUser \"true\" %}TextHere{% endifnotequal %}{% endfor %}".RenderTemplate(sut));
            Assert.AreEqual("Tag substitution errored on template string:\n{% for instance in GDS %}{% ifnotequal instance.DbUser \"true\" %}TextHere{% endifnotequal %}{% endfor %}", error.Message);
        }

        [Test]
        public void ForLoopWorks()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("myIdmyId2", "{% for instance in GDS %}{{ instance.identity }}{% endfor %}".RenderTemplate(sut));
        }

        [Test]
        public void AccessByGroupType()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("SYS", "{{ GDS.myId.IsSys }}".RenderTemplate(sut));
        }

        [Test]
        public void AccessFirstInLabel()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("SYS", "{% with GDS|first as instance %}{{ instance.IsSys }}{% endwith %}".RenderTemplate(sut));
        }

        [Test]
        public void ExistsFilterWorksWhenTagExists()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("LOC", "{% if Environment|exists %}{{ Environment }}{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void ExistsFilterWorksWhenTagDoesNotExist()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual(String.Empty, "{% if DoesNotExist|exists %}{{ DoesNotExist }}{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void ForLoopWithExistsWorksWhenTagExists()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("myIdmyId2", "{% if GDS|exists %}{% for instance in GDS %}{{ instance.identity }}{% endfor %}{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void ForLoopWithExistsDoesNothingWhenTagDoesNotExist()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual(String.Empty, "{% if DoesNotExist|exists %}{% for instance in DoesNotExist %}{{ instance.identity }}{% endfor %}{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void WhenCollectionEmpty_emptyistrue()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("empty", "{% if NotPresent|empty %}empty{% else %}not empty{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void WhenBlankValue_isEmpty()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("empty", "{% if BlankValue|empty %}empty{% else %}not empty{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void WhenNotBlnk_NotEmpty()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("not empty", "{% if DbEncoded|empty %}empty{% else %}not empty{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void WhenPropertyDoesntExist_UseDefault()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("default", "{{ NotAValue|default:\"default\" }}".RenderTemplate(sut));
        }

        [Test]
        public void WhenPropertyDoesntExist_ValueInt_UseDefault()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("1", "{{ NotAValue|default:1 }}".RenderTemplate(sut));
        }

        [Test]
        public void WhenPropertyDoesntExist_ValueSingleQuote_UseDefault()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("default", "{{ NotAValue|default:'default' }}".RenderTemplate(sut));
        }

        [Test]
        [TestCase("Hello", "el", true)]
        [TestCase("Goodbye", "el", false)]
        [TestCase("", "el", false)]
        public void Contains(string value, string contains, bool expected)
        {
            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">{value}</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.AreEqual(expected.ToString().ToLower(), "{% if Data|contains:'#contains#' %}true{% else %}false{% endif %}".Replace("#contains#", contains).RenderTemplate(sut));
        }

        [Test]
        public void Contains_EmptyString_Throws()
        {
            var sut = new TagDictionary("ident", @"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">Value</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.Throws<ArgumentException>(() => "{% if Data|contains:'' %}true{% else %}false{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void Contains_WhenPropertyDoesntExist_Throws()
        {
            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">Value</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.Throws<ArgumentException>(() => "{% if Undata|contains:'al' %}true{% else %}false{% endif %}".RenderTemplate(sut));
        }

        [Test]
        [TestCase("Hello", "el", true)]
        [TestCase("Goodbye", "el", false)]
        [TestCase("", "el", false)]
        public void Contains_WhenPropertyDoesntExist_Defaulted(string value, string contains, bool expected)
        {
            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties />
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.AreEqual(expected.ToString().ToLower(), "{% if Data|default:'#value#'|contains:'#endsWith#' %}true{% else %}false{% endif %}".Replace("#endsWith#", contains).Replace("#value#", value).RenderTemplate(sut));
        }

        [Test]
        [TestCase("Hello", "h", true)]
        [TestCase("Goodbye", "h", false)]
        [TestCase("", "h", false)]
        public void StartsWith(string value, string startsWith, bool expected)
        {
            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">{value}</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.AreEqual(expected.ToString().ToLower(), "{% if Data|startsWith:'#startsWith#' %}true{% else %}false{% endif %}".Replace("#startsWith#", startsWith).RenderTemplate(sut));
        }

        [Test]
        public void StartsWith_EmptyString_Throws()
        {
            var sut = new TagDictionary("ident", @"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">Value</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.Throws<ArgumentException>(() => "{% if Data|startsWith:'' %}true{% else %}false{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void StartsWith_WhenPropertyDoesntExist_Throws()
        {
            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">Value</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.Throws<ArgumentException>(() => "{% if Undata|startsWith:'V' %}true{% else %}false{% endif %}".RenderTemplate(sut));
        }

        [Test]
        [TestCase("Hello", "h", true)]
        [TestCase("Goodbye", "h", false)]
        [TestCase("", "h", false)]
        public void StartsWith_WhenPropertyDoesntExist_Defaulted(string value, string startsWith, bool expected)
        {
            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties />
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.AreEqual(expected.ToString().ToLower(), "{% if Data|default:'#value#'|startsWith:'#endsWith#' %}true{% else %}false{% endif %}".Replace("#endsWith#", startsWith).Replace("#value#", value).RenderTemplate(sut));
        }

        [Test]
        [TestCase("Hello", "o", true)]
        [TestCase("Goodbye", "o", false)]
        [TestCase("", "o", false)]
        public void EndsWith(string value, string endsWith, bool expected)
        {
            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">{value}</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.AreEqual(expected.ToString().ToLower(), "{% if Data|endsWith:'#endsWith#' %}true{% else %}false{% endif %}".Replace("#endsWith#", endsWith).RenderTemplate(sut));
        }

        [Test]
        public void EndsWith_EmptyString_Throws()
        {
            var sut = new TagDictionary("ident", @"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">Value</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.Throws<ArgumentException>(() => "{% if Data|endsWith:'' %}true{% else %}false{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void EndsWith_WhenPropertyDoesntExist_Throws()
        {
            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">Value</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.Throws<ArgumentException>(() => "{% if Undata|startsWith:'V' %}true{% else %}false{% endif %}".RenderTemplate(sut));
        }

        [Test]
        [TestCase("Hello", "o", true)]
        [TestCase("Goodbye", "o", false)]
        [TestCase("", "o", false)]
        public void EndsWith_WhenPropertyDoesntExist_Defaulted(string value, string endsWith, bool expected)
        {
            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties />
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            Assert.AreEqual(expected.ToString().ToLower(), "{% if Data|default:'#value#'|endsWith:'#endsWith#' %}true{% else %}false{% endif %}".Replace("#endsWith#", endsWith).Replace("#value#", value).RenderTemplate(sut));
        }

        [Test]
        [Explicit] // You can only run this with a specific certificate loaded into your computer
        public void Decrypt_Test()
        {
            string value = "fQF5wgZ4J9+lm2LbpqoxWeeao6A9x1XoIc3VTDSHBosJ1tM/mLX2XO8+AENaizmalFhCD1YKK4j7YmzEP72FwtfGgm2jazDGNb1WR440ZPL96P/tO/dKvy53KHtlkY76qFiP2KZPxRWjbem+5kpWn5lLczwl/7lQfBAM6ntawghntVAA7l7gwvKDuq2FcVIP3Njdu3DzSWPgP4P83pQxn04KtG7fO0VudUXjllI6Y/LAgpovYuC1SR3lTw33V4KVPXcOvB16bwplw+izKGWBn9Wjc6B0IxyW0tSz87ETzuL0HpiOeTZvEObrzSXlCjz3qbt4mf5gQ9QN4Gk7tLpOAZhrV1fn94IDL+l73KDbQDVo/HsX5sFFK1amFu7669kjlCOtiXxS9nZ35GtMVV2N+TC78xg0tAmYZtCOaJ+AgKoPj+PZTaR+Heu+nddiy7EAsezQ0FePxL1FN6+VcMydA16htdfdhwBzt/Sjql1LLoqSuaduOvXwftkOfhl3ylVRNnGjgHtIlAAEXtxfyitxVQSVVDgPRwfncB0S4LgpXrnuw+Bj7CuslXvCL6x9ZCp5PYqcXTYcwuv/xysFrTKM0k6xS4D6mIShlOwOxaMm0nntg3VBIXwJZULHabd/xMb2UkwQOAjKvgplLmduaSeGdS8axup326eSmEDRXQlHqUM=";

            var sut = new TagDictionary("ident", $@"<Structure xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                                                      <ClientCode>XX</ClientCode>
                                                      <Environment>LOC</Environment>
                                                      <Properties>
                                                        <Property name=""Data"">{value}</Property>
                                                        <Property name=""Certificate"">XX-NON-Certificate</Property>
                                                      </Properties>
                                                      <PropertyGroups />
                                                      <DbLogins />
                                                  </Structure>");

            var expected = "hello";

            Assert.AreEqual(expected.ToString().ToLower(), "{{ Data|decrypt:Certificate }}".RenderTemplate(sut));
        }

        [Test]
        public void UpdateDREnvironment_WithNotDRMachine()
        {
            Environment.SetEnvironmentVariable("Environment", "DR-LOC");
            Environment.SetEnvironmentVariable("OctopusEnvironmentName", "DR-LOC");
            Environment.SetEnvironmentVariable("IsDRMachine", "");
            var loader = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.Environment, "" } });
            Assert.AreEqual("DR-LOC", loader["Environment"]);
        }

        [Test]
        public void UpdateDREnvironment_WithDRMachineSet()
        {
            Environment.SetEnvironmentVariable("Environment", "DR-LOC");
            Environment.SetEnvironmentVariable("OctopusEnvironmentName", "DR-LOC");
            Environment.SetEnvironmentVariable("IsDRMachine", "true");
            var loader = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.Environment, "" } });
            Assert.AreEqual("LOC", loader["Environment"]);
        }

        [Test]
        public void EnsureThatXmlIncludesWork_AndArriveRenderered()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual(xml.Value.RenderTemplate(sut), "{% include \"webservice-structure.xml\" %}".RenderTemplate(sut));
        }

        [Test]
        public void DualLabeledGroupWorks()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("ABC123", "{{ Label1.0.Value }}".RenderTemplate(sut));
            Assert.AreEqual("ABC123", "{{ Label2.0.Value }}".RenderTemplate(sut));
            Assert.AreEqual("321CBA", "{{ Label1.1.Value }}".RenderTemplate(sut));
            Assert.Throws<ArgumentException>(() => "{{ Label2.1.Value }}".RenderTemplate(sut));
        }

        [Test]
        public void FlatLabelGroupsHaveValues()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("FlatGroup-0", "{{ FlatGroup.0.Value }}".RenderTemplate(sut));
            Assert.AreEqual("FlatGroup-1", "{{ FlatGroup.1.Value }}".RenderTemplate(sut));
        }

        [Test]
        public void LabelNodeWithoutProperties()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("999", "{{ Label3.0.Value }}".RenderTemplate(sut));
        }

        [Test]
        public void DualLabeledGroupWithContainerNodeWorks()
        {
            var sut = new TagDictionary("ident", new Dictionary<TagSource, string> { { TagSource.XmlData, XmlData } });
            Assert.AreEqual("888", "{{ LabelGrouped1.0.Value }}".RenderTemplate(sut));
            Assert.AreEqual("888", "{{ LabelGrouped2.0.Value }}".RenderTemplate(sut));
        }
    }
}