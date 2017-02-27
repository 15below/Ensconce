using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;

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
            var loader = new TagDictionary("ident",
                                               Tuple.Create("", TagSource.Environment),
                                               Tuple.Create(XMLFilename, TagSource.XmlFileName));
            Assert.AreEqual(QueAppServer, loader[QueAppServer]);
        }

        [Test]
        public void IdentifiedPropertiesTakePrecedence()
        {
            var loader = new TagDictionary("myId",
                                               Tuple.Create(XmlData, TagSource.XmlData));
            Assert.AreEqual(IsSysValue, loader[IsSys]);
        }

        [Test]
        public void LoadFromEnvironment()
        {
            var loader = new TagDictionary("ident", Tuple.Create("", TagSource.Environment));
            Assert.AreEqual(IsSys, loader[IsSys]);
        }

        [Test]
        public void LoadFromOctopus1XEnvironment()
        {
            var environment = "LOC";
            Environment.SetEnvironmentVariable("Environment", "");
            Environment.SetEnvironmentVariable("OctopusEnvironmentName", environment);
            var loader = new TagDictionary("ident", Tuple.Create("", TagSource.Environment));
            Assert.AreEqual(environment, loader["Environment"]);
            Environment.SetEnvironmentVariable("OctopusEnvironmentName", "");
        }

        [Test]
        public void LoadFromOctopus2XEnvironment()
        {
            var environment = "DEV";
            Environment.SetEnvironmentVariable("Environment", "");
            Environment.SetEnvironmentVariable("Octopus.Environment.Name", environment);
            var loader = new TagDictionary("ident", Tuple.Create("", TagSource.Environment));
            Assert.AreEqual(environment, loader["Environment"]);
            Environment.SetEnvironmentVariable("Octopus.Environment.Name", "");
        }

        [Test]
        public void LoadFromXmlData()
        {
            var loader = new TagDictionary("ident",
                                               Tuple.Create(
                                                   XmlData, TagSource.XmlData));
            Assert.AreEqual("AndThisWouldBeAPassword", loader["DbPassword"]);
        }

        [Test]
        public void LoadEmptyXmlData()
        {
            Assert.DoesNotThrow(() => new TagDictionary("ident", Tuple.Create("", TagSource.XmlData)));
        }

        [Test]
        public void LoadFromXmlFileName()
        {
            var loader = new TagDictionary("ident", Tuple.Create(XMLFilename, TagSource.XmlFileName));
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
            var sut = new TagDictionary("ident", Tuple.Create("", TagSource.Environment), Tuple.Create("incorrectStructure.xml", TagSource.XmlFileName));
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
            var loader = new TagDictionary("myId", Tuple.Create(XmlData, TagSource.XmlData), Tuple.Create("structure.xml", TagSource.XmlFileName));
            Assert.AreEqual(Idvalue, loader[Avalue]);
        }

        [Test]
        public void DbPasswordAccessibleViaDictionaryWithoutPrefix()
        {
            var sut = new TagDictionary("myId", Tuple.Create(XmlData, TagSource.XmlData), Tuple.Create("structure.xml", TagSource.XmlFileName));
            Assert.AreEqual("Some high entrophy random text", sut.DbLogins["AUDIT"].Password);
        }

        [TestCase("FAA.", "ClientDomain")]
        [TestCase(".LOC.", "ClientDomain")]
        public void TestTagsInPropertiesAreSubstituted(string expected, string key)
        {
            var tagDict = new TagDictionary("myId", Tuple.Create(XmlData, TagSource.XmlData), Tuple.Create("structure.xml", TagSource.XmlFileName));
            Assert.IsTrue(tagDict[key].ToString().Contains(expected));
        }

        [Test]
        public void SuccessfullyGetDbPassword()
        {
            var loader = new TagDictionary("ident", Tuple.Create(XMLFilename, TagSource.XmlFileName));
            Assert.AreEqual("NoPasswordsRoundHere", loader.GetDbPassword("config"));
        }

        [Test]
        public void DbLoginsGenerated()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.AreEqual("This isn't a password either", sut.DbLogins.Values.First(login => login.Username == "config").Password);
        }

        [Test]
        public void DbLoginNamesAreSubstituted()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.IsTrue(sut.DbLogins.Values.Select(login => login.Username).Contains(string.Format("{0}-{1}-AUDIT", Environment.GetEnvironmentVariable("ClientCode"), Environment.GetEnvironmentVariable("Environment"))));
        }

        [Test]
        public void DbLoginDefaultDbsAreSubstituted()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.IsTrue(sut.DbLogins.Values.Select(login => login.DefaultDb).Contains("FAA-LOC-AUDIT"));
        }

        [Test]
        public void DbLoginConnectionStringLoaded()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.IsTrue(sut.DbLogins.Values.Select(login => login.Username).Contains("ConnectionString"));
            var correctDbLogin = sut.DbLogins.First(login => login.Key == "ConnectionString");
            Assert.AreEqual("Actual ConnectionString", correctDbLogin.Value.ConnectionString);
        }

        [Test]
        public void DbLoginSuppliesConnectionString()
        {
            var sut = new TagDictionary("ident", XmlData);
            var login = sut.DbLogins.First();
            Assert.IsFalse(string.IsNullOrEmpty(login.Value.ConnectionString));
        }


        [Test]
        public void DbLoginWithKey()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.True(sut.DbLogins.ContainsKey("LOGIN"));
            var login = sut.DbLogins["LOGIN"];
            Assert.IsFalse(string.IsNullOrEmpty(login.ConnectionString));
            Assert.AreEqual("ZZ-ENV-LOGIN", login.Username);
            Assert.AreEqual("ZZ-ENV-LOGIN", login.DefaultDb);
        }

        [Test]
        public void DbLoginSuppliesConnectionStringWithSubbedValues()
        {
            var sut = new TagDictionary("ident", XmlData);
            var login = sut.DbLogins.First();
            Assert.IsTrue(login.Value.ConnectionString.Contains("DbServerAddress"));
        }

        [Test]
        public void LoadLabelledGroups()
        {
            var sut = new TagDictionary("ident", XmlData);
            Assert.IsTrue(sut.ContainsKey("GDS"));
        }

        [Test]
        public void LoadLabelledGroupsValuesAndNormalIdentityValuesAvailable()
        {
            var sut = new TagDictionary("ident", Tuple.Create("structure.xml", TagSource.XmlFileName));
            Assert.AreEqual("SYS", ((IEnumerable<IDictionary<string, object>>)sut["GDS"]).First()["IsSys"]);
            Assert.AreEqual("SomeUserName", sut["DbUser"]);
            Assert.AreEqual("notSYS", sut["IsSys"].ToString());
        }

        [Test]
        public void LoadLabelledGroupsBuildsEnumeratorCorrectly()
        {
            var sut = new TagDictionary("ident", Tuple.Create(XmlData, TagSource.XmlData));
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
            var sut = new TagDictionary("ident", Tuple.Create(XmlData, TagSource.XmlData));
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
            var sut = new TagDictionary("ident", Tuple.Create("", TagSource.Environment), Tuple.Create(XmlData, TagSource.XmlData));
            Assert.AreEqual(value, sut[friendlyKey]);
        }

        [Test]
        public void ForLoopWorks()
        {
            var sut = new TagDictionary("ident", Tuple.Create(XmlData, TagSource.XmlData));
            Assert.AreEqual("myIdmyId2", "{% for instance in GDS %}{{ instance.identity }}{% endfor %}".RenderTemplate(sut));
        }

        [Test]
        public void AccessByGroupType()
        {
            var sut = new TagDictionary("ident", Tuple.Create(XmlData, TagSource.XmlData));
            Assert.AreEqual("SYS", "{{ GDS.myId.IsSys }}".RenderTemplate(sut));
        }

        [Test]
        public void AccessFirstInLabel()
        {
            var sut = new TagDictionary("ident", Tuple.Create(XmlData, TagSource.XmlData));
            Assert.AreEqual("SYS", "{% with GDS|first as instance %}{{ instance.IsSys }}{% endwith %}".RenderTemplate(sut));
        }

        [Test]
        public void ExistsFilterWorksWhenTagExists()
        {
            var sut = new TagDictionary("ident", Tuple.Create(XmlData, TagSource.XmlData));
            Assert.AreEqual("LOC", "{% if Environment|exists %}{{ Environment }}{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void ExistsFilterWorksWhenTagDoesNotExist()
        {
            var sut = new TagDictionary("ident", Tuple.Create(XmlData, TagSource.XmlData));
            Assert.AreEqual(String.Empty, "{% if DoesNotExist|exists %}{{ DoesNotExist }}{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void ForLoopWithExistsWorksWhenTagExists()
        {
            var sut = new TagDictionary("ident", Tuple.Create(XmlData, TagSource.XmlData));
            Assert.AreEqual("myIdmyId2", "{% if GDS|exists %}{% for instance in GDS %}{{ instance.identity }}{% endfor %}{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void ForLoopWithExistsDoesNothingWhenTagDoesNotExist()
        {
            var sut = new TagDictionary("ident", Tuple.Create(XmlData, TagSource.XmlData));
            Assert.AreEqual(String.Empty, "{% if DoesNotExist|exists %}{% for instance in DoesNotExist %}{{ instance.identity }}{% endfor %}{% endif %}".RenderTemplate(sut));
        }

        [Test]
        public void UpdateDREnvironment_WithNotDRMachine()
        {
            Environment.SetEnvironmentVariable("Environment", "DR-LOC");
            Environment.SetEnvironmentVariable("OctopusEnvironmentName", "DR-LOC");
            Environment.SetEnvironmentVariable("IsDRMachine", "");
            var loader = new TagDictionary("ident", Tuple.Create("", TagSource.Environment));
            Assert.AreEqual("DR-LOC", loader["Environment"]);
        }

        [Test]
        public void UpdateDREnvironment_WithDRMachineSet()
        {
            Environment.SetEnvironmentVariable("Environment", "DR-LOC");
            Environment.SetEnvironmentVariable("OctopusEnvironmentName", "DR-LOC");
            Environment.SetEnvironmentVariable("IsDRMachine", "true");
            var loader = new TagDictionary("ident", Tuple.Create("", TagSource.Environment));
            Assert.AreEqual("LOC", loader["Environment"]);
        }
    }
}