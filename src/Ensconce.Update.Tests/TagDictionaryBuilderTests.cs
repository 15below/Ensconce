using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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
            Assert.IsNotEmpty(dictionary.Value, "TagDictionary was empty");
            Assert.AreEqual(EnvClientCode, dictionary.Value["ClientCode"], "TagDictionary[\"ClientCode\"] was not correct");
            Assert.AreEqual(EnvEnvironment, dictionary.Value["Environment"], "TagDictionary[\"Environment\"] was not correct");
            Assert.False(dictionary.Value.ContainsKey("ClientDomain"), "TagDictionary ClientDomain was found");
        }

        [Test]
        public void BuildTagDictionary_EmptyString()
        {
            //Act
            var dictionary = TagDictionaryBuilder.Build(string.Empty);

            //Assert
            Assert.IsNotEmpty(dictionary.Value, "TagDictionary was empty");
            Assert.AreEqual(EnvClientCode, dictionary.Value["ClientCode"], "TagDictionary[\"ClientCode\"] was not correct");
            Assert.AreEqual(EnvEnvironment, dictionary.Value["Environment"], "TagDictionary[\"Environment\"] was not correct");
            Assert.False(dictionary.Value.ContainsKey("ClientDomain"), "TagDictionary ClientDomain was found");
        }

        [Test]
        public void BuildTagDictionary_ValidPathAndContent()
        {
            //Act
            var dictionary = TagDictionaryBuilder.Build(Path.Combine(testFilePath, "structure.xml"));

            //Assert
            Assert.IsNotEmpty(dictionary.Value, "TagDictionary was empty");
            Assert.AreEqual(EnvClientCode, dictionary.Value["ClientCode"], "TagDictionary[\"ClientCode\"] was not correct");
            Assert.AreEqual(EnvEnvironment, dictionary.Value["Environment"], "TagDictionary[\"Environment\"] was not correct");
            Assert.AreEqual($"{EnvClientCode}.{EnvEnvironment}.example.com", dictionary.Value["ClientDomain"], "TagDictionary[\"ClientDomain\"] was not correct");
        }

        [Test]
        public void BuildTagDictionary_ValidPathAndInvalidContent()
        {
            //Act
            var dictionary = TagDictionaryBuilder.Build(Path.Combine(testFilePath, "incorrectStructure.xml"));

            //Assert
            Assert.IsNotEmpty(dictionary.Value, "TagDictionary was empty");
            Assert.AreEqual(EnvClientCode, dictionary.Value["ClientCode"], "TagDictionary[\"ClientCode\"] was not correct");
            Assert.AreEqual(EnvEnvironment, dictionary.Value["Environment"], "TagDictionary[\"Environment\"] was not correct");
            Assert.False(dictionary.Value.ContainsKey("ClientDomain"), "TagDictionary ClientDomain was found");
        }

        [Test]
        public void BuildTagDictionary_NonExistantPath()
        {
            //Act
            var dictionary = TagDictionaryBuilder.Build(Path.Combine());

            //Assert
            Assert.IsNotEmpty(dictionary.Value, "TagDictionary was empty");
            Assert.AreEqual(EnvClientCode, dictionary.Value["ClientCode"], "TagDictionary[\"ClientCode\"] was not correct");
            Assert.AreEqual(EnvEnvironment, dictionary.Value["Environment"], "TagDictionary[\"Environment\"] was not correct");
            Assert.False(dictionary.Value.ContainsKey("ClientDomain"), "TagDictionary ClientDomain was found");
        }

        [Test]
        public void BuildTagDictionary_InvalidPath()
        {
            //Act
            var dictionary = TagDictionaryBuilder.Build(@"D:\FixedStructure\D:\FixedStructure\#{FixedStructureFile}");

            //Assert
            Assert.IsNotEmpty(dictionary.Value, "TagDictionary was empty");
            Assert.AreEqual(EnvClientCode, dictionary.Value["ClientCode"], "TagDictionary[\"ClientCode\"] was not correct");
            Assert.AreEqual(EnvEnvironment, dictionary.Value["Environment"], "TagDictionary[\"Environment\"] was not correct");
            Assert.False(dictionary.Value.ContainsKey("ClientDomain"), "TagDictionary ClientDomain was found");
        }
    }
}
