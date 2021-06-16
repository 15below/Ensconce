using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

namespace Ensconce.Update.Tests
{
    [TestFixture]
    public class TagDictionaryBuilderTests
    {
        private readonly Mutex singleThreadTest = new Mutex(false, "SingleThreadTest-TagDictionaryBuilderTests");
        private const string EnvEnvironment = "LOC";
        private const string EnvClientCode = "FAA";
        private string testFilePath;

        [SetUp]
        public void SetUp()
        {
            testFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TagDictionaryTestFiles");
            singleThreadTest.WaitOne();
            var testEnv = new Dictionary<string, string>
            {
                {"ClientCode", EnvClientCode},
                {"Environment", EnvEnvironment}
            };
            testEnv.ToList().ForEach(kv => Environment.SetEnvironmentVariable(kv.Key, kv.Value, EnvironmentVariableTarget.Process));
        }

        [TearDown]
        public void TearDown()
        {
            singleThreadTest.ReleaseMutex();
        }

        [Test]
        public void BuildTagDictionary_NullPath()
        {
            //Act
            var dictionary = TagDictionaryBuilder.Build(null);

            //Assert
            Assert.IsNotEmpty(dictionary, "TagDictionary was empty");
            Assert.AreEqual(EnvClientCode, dictionary["ClientCode"], "TagDictionary[\"ClientCode\"] was not correct");
            Assert.AreEqual(EnvEnvironment, dictionary["Environment"], "TagDictionary[\"Environment\"] was not correct");
            Assert.False(dictionary.ContainsKey("ClientDomain"), "TagDictionary ClientDomain was found");
        }

        [Test]
        public void BuildTagDictionary_EmptyString()
        {
            //Act
            var dictionary = TagDictionaryBuilder.Build(string.Empty);

            //Assert
            Assert.IsNotEmpty(dictionary, "TagDictionary was empty");
            Assert.AreEqual(EnvClientCode, dictionary["ClientCode"], "TagDictionary[\"ClientCode\"] was not correct");
            Assert.AreEqual(EnvEnvironment, dictionary["Environment"], "TagDictionary[\"Environment\"] was not correct");
            Assert.False(dictionary.ContainsKey("ClientDomain"), "TagDictionary ClientDomain was found");
        }

        [Test]
        public void BuildTagDictionary_ValidPathAndContent()
        {
            //Act
            var dictionary = TagDictionaryBuilder.Build(Path.Combine(testFilePath, "structure.xml"));

            //Assert
            Assert.IsNotEmpty(dictionary, "TagDictionary was empty");
            Assert.AreEqual(EnvClientCode, dictionary["ClientCode"], "TagDictionary[\"ClientCode\"] was not correct");
            Assert.AreEqual(EnvEnvironment, dictionary["Environment"], "TagDictionary[\"Environment\"] was not correct");
            Assert.AreEqual($"{EnvClientCode}.{EnvEnvironment}.example.com", dictionary["ClientDomain"], "TagDictionary[\"ClientDomain\"] was not correct");
        }

        [Test]
        public void BuildTagDictionary_ValidPathAndInvalidContent()
        {
            //Arrange
            var path = Path.Combine(testFilePath, "invalidStructure.xml");

            //Act
            var exception = Assert.Throws<XmlException>(() => TagDictionaryBuilder.Build(path));

            //Assert
            Assert.NotNull(exception);
            Assert.AreEqual("Unable to parse XML data", exception.Message);
        }

        [Test]
        public void BuildTagDictionary_ValidPathAndIncorrectStructure()
        {
            //Arrange
            var path = Path.Combine(testFilePath, "incorrectStructure.xml");

            //Act
            var exception = Assert.Throws<XmlSchemaValidationException>(() => TagDictionaryBuilder.Build(path));

            //Assert
            Assert.NotNull(exception);
            //NOTE: The entire message isn't used as the exception has different order of elements & that is framework controlled
            Assert.That(exception.Message.StartsWith("The element 'Structure' has incomplete content. List of possible elements expected:"));
        }

        [Test]
        public void BuildTagDictionary_NonExistentPath()
        {
            //Arrange
            var path = $@"D:\FixedStructure\NonExistantFile-{Guid.NewGuid()}.xml";

            //Act
            var exception = Assert.Throws<FileNotFoundException>(() => TagDictionaryBuilder.Build(path));

            //Assert
            Assert.NotNull(exception);
            Assert.AreEqual($"No structure file found at {path}", exception.Message);
        }

        [Test]
        public void BuildTagDictionary_InvalidPath()
        {
            //Act
            var exception = Assert.Throws<FileNotFoundException>(() => TagDictionaryBuilder.Build(@"D:\FixedStructure\D:\FixedStructure\#{FixedStructureFile}"));

            //Assert
            Assert.NotNull(exception);
            Assert.AreEqual("No structure file found at D:\\FixedStructure\\D:\\FixedStructure\\#{FixedStructureFile}", exception.Message);
        }
    }
}
