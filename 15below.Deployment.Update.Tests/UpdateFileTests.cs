using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using NUnit.Framework;

namespace FifteenBelow.Deployment.Update.Tests
{
    [TestFixture]
    public class UpdateFileTests
    {
        [Test]
        public void SubstituteOldValueWithNewStaticValue()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(@"TestUpdateFiles\TestSubstitution1.xml", @"TestUpdateFiles\TestConfig1.xml"));
            Assert.AreEqual("newvalue", newConfig.XPathSelectElement("/root/value").Value);
        }

        [Test]
        public void LimitSubstitutionsByFileName()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(@"TestUpdateFiles\TestSubstitution2.xml", @"TestUpdateFiles\TestConfig1.xml"));
            Assert.AreEqual("oldValue", newConfig.XPathSelectElement("/root/value").Value);
        }

        [Test]
        public void SubstituteInNameSpacedFile()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(@"TestUpdateFiles\TestSubstitution3.xml", @"TestUpdateFiles\TestConfig2.xml"));
            var nms = new XmlNamespaceManager(new NameTable());
            nms.AddNamespace("c", "http://madeup.com");
            Assert.AreEqual("newvalue", newConfig.XPathSelectElement("/c:root/c:value", nms).Value);
        }

        [Test]
        public void SubstituteOldValueWithNewTaggedValue()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution4.xml", @"TestUpdateFiles\TestConfig1.xml", new Dictionary<string, object> {{"taggedReplacementContent", "newvalue"}}
                ));
            Assert.AreEqual("newvalue", newConfig.XPathSelectElement("/root/value").Value);
        }

        [Test]
        public void SubstituteOldValueWithNewTaggedXmlValue()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution5.xml", @"TestUpdateFiles\TestConfig1.xml", new Dictionary<string, object> {{"taggedReplacementContent", "newvalue"}}
                ));
            Assert.AreEqual("newvalue", newConfig.XPathSelectElement("/root/value").Descendants().First().Name.LocalName);
        }

        [Test]
        public void SubstituteOldValueWithNewLoopedTaggedXmlValues()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution6.xml", @"TestUpdateFiles\TestConfig1.xml", new Dictionary<string, object>
                                                                {
                                                                    {"configList", new []{"newvalue1", "newvalue2","newvalue3"}}
                                                                }
                ));
            Assert.AreEqual(XElement.Parse("<value><newvalue1/><newvalue2 /><newvalue3/></value>").ToString(), newConfig.XPathSelectElement("/root/value").ToString());
        }

        [Test]
        public void SubstituteOldValueWithNewLoopedTaggedXmlValuesAsXml()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution6.xml", @"TestUpdateFiles\TestConfig1.xml", new Dictionary<string, object>
                                                                {
                                                                    {"configList", new []{"newvalue1", "newvalue2","newvalue3"}}
                                                                }
                ));
            Assert.AreEqual("", newConfig.XPathSelectElement("/root/value/newvalue1").Value);
        }

        [Test]
        public void ThrowNewMissingArgExceptionIfNoTagValue()
        {
            Assert.Throws<ArgumentException>(() => UpdateFile.Update(@"TestUpdateFiles\TestSubstitution5.xml", @"TestUpdateFiles\TestConfig1.xml"));
        }

        [Test]
        public void ChangeValueOfAttributeWithFixedSub()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution7.xml", @"TestUpdateFiles\TestConfig3.xml"
                ));
            Assert.AreEqual("after", newConfig.XPathSelectElement("/root/value").Attribute("myAttr").Value);
        }
        
        [Test]
        public void DoNotRemoveAttributesUnlessSpecified()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution7.xml", @"TestUpdateFiles\TestConfig3.xml"
                ));
            Assert.AreEqual("quack", newConfig.XPathSelectElement("/root/value").Attribute("duckAttr").Value);
        }
        
        [Test]
        public void DoNotChangeChildrenUnlessNewValueSpecified()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution7.xml", @"TestUpdateFiles\TestConfig3.xml"
                ));
            Assert.AreEqual("oldValue", newConfig.XPathSelectElement("/root/value").Value);
        }

        [Test]
        public void ChangeValueOfAttributeWithFixedSubEvenWhenOldAttributesRemoved()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution8.xml", @"TestUpdateFiles\TestConfig3.xml"
                ));
            Assert.AreEqual("after", newConfig.XPathSelectElement("/root/value").Attribute("myAttr").Value);
        }

        [Test]
        public void RemoveAttributesIfSpecified()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution8.xml", @"TestUpdateFiles\TestConfig3.xml"
                ));
            Assert.IsNull(newConfig.XPathSelectElement("/root/value").Attribute("duckAttr"));
        }

        [Test]
        public void RemoveChildrenIfEmptyReplacementSpecified()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution8.xml", @"TestUpdateFiles\TestConfig3.xml"
                ));
            Assert.AreEqual("", newConfig.XPathSelectElement("/root/value").Value);
        }

        [Test]
        public void SubstituteTaggedAttributeValue()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution9.xml", @"TestUpdateFiles\TestConfig3.xml", new Dictionary<string, object>{{"newValue", "after"}}
                ));
            Assert.AreEqual("after", newConfig.XPathSelectElement("/root/value").Attribute("myAttr").Value);
        }
        
        [Test]
        public void AppendAfterWorks()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution14.xml", @"TestUpdateFiles\TestConfig1.xml", new Dictionary<string, object>{{"newValue", "after"}}
                ));
            Assert.IsTrue(newConfig.Root.Descendants().Select(el => el.Name).Contains("NewTag"));
        }
        
        [Test]
        public void AddChildContentWorks()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution15.xml", @"TestUpdateFiles\TestConfig1.xml", new Dictionary<string, object>{{"newValue", "after"}}
                ));
            Assert.IsTrue(newConfig.XPathSelectElements("/root/value/NewTag").Count() == 1);
        }

        [Test]
        public void EmptyChildContentWorks()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution16.xml", @"TestUpdateFiles\TestConfig1.xml", new Dictionary<string, object>{{"newValue", "after"}}
                ));
            Assert.IsTrue(newConfig.XPathSelectElements("/root/value/NewTag").Count() == 0);
        }

        [Test]
        public void MultipleChildContentWorks()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution17.xml", @"TestUpdateFiles\TestConfig1.xml", new Dictionary<string, object>{{"newValue", "after"}}
                ));
            Assert.AreEqual("1", newConfig.XPathSelectElements("/root/value/one").First().Value);
            Assert.AreEqual("2", newConfig.XPathSelectElements("/root/value/two").First().Value);
        }
        
        [Test]
        public void UpdateAllTouchesAllFiles()
        {
            var configs = UpdateFile.UpdateAll(@"TestUpdateFiles\TestSubstitution3.xml",
                                               new Dictionary<string, object> {{"tagValue", "Tagged!"}});
            var nms = new XmlNamespaceManager(new NameTable());
            nms.AddNamespace("c", "http://madeup.com");
            Assert.AreEqual("newvalue",
                            XDocument.Parse(configs.Where(c => c.Item1 == @"TestUpdateFiles\TestConfig2.xml").Single().Item2).
                                XPathSelectElement("/c:root/c:value", nms).Value);
            Assert.AreEqual("Tagged!",
                            XDocument.Parse(configs.Where(c => c.Item1 == @"TestUpdateFiles\TestConfig1.xml").Single().Item2).XPathSelectElement
                                ("/root/value").Value);
        }

        [Test]
        public void UpdateAllKnowsAboutTaggedFiles()
        {
            var configs = UpdateFile.UpdateAll(@"TestUpdateFiles\TestSubstitution13.xml",
                                               new Dictionary<string, object> {{"FilePath", "TaggedPath"}});
            Assert.AreEqual("newvalue",
                            XDocument.Parse(configs.Where(c => c.Item1 == @"TestUpdateFiles\TestConfig-TaggedPath.xml").Single().Item2).
                                XPathSelectElement("/root/value").Value);
        }

        [Test]
        public void TagsOutsideSpecifiedXPathsUnchanged()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution9.xml", @"TestUpdateFiles\TestConfig3.xml", new Dictionary<string, object>{{"newValue", "after"}}
                ));
            Assert.AreEqual("{{ tagged }}", newConfig.XPathSelectElement("/root/myValue").Value);
        }

        [Test]
        public void ReplaceFileWithTemplateWorks()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution10.xml", @"TestUpdateFiles\TestConfig2.xml", new Dictionary<string, object>{{"tagged", "after"}}
                ));
            Assert.AreEqual("after", newConfig.XPathSelectElement("/root/myValue").Value);
        }

        [Test]
        public void NewFileWithTemplateWorks()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution11.xml", @"TestUpdateFiles\DoesntExist.xml", new Dictionary<string, object>{{"tagged", "after"}}
                ));
            Assert.AreEqual("after", newConfig.XPathSelectElement("/root/myValue").Value);
        }

        [Test]
        public void TemplateAndChangesWork()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution18.xml", @"TestUpdateFiles\TestConfig2.xml", new Dictionary<string, object>{{"tagged", "after"}}
                ));
            Assert.AreEqual("after", newConfig.XPathSelectElement("/root/myValue").Value);
            Assert.AreEqual("afterAttr", newConfig.XPathSelectElement("/root/myValue").Attribute("myAttr").Value);
        }

        [Test]
        public void TemplateAndChangesWorkWithTemplatedXPaths()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution19.xml", @"TestUpdateFiles\TestConfig2.xml",
                new Dictionary<string, object> {{"tagged", "after"}, {"Environment", "LOC"}}
                                                ));
            Assert.AreEqual("after", newConfig.XPathSelectElement("/root/myValue-LOC").Value);
            Assert.AreEqual("afterAttr", newConfig.XPathSelectElement("/root/myValue-LOC").Attribute("myAttr").Value);
        }

        [Test]
        public void InvalidSubstitutionXmlShouldThrow()
        {
            Assert.Throws<XmlSchemaValidationException>(() => XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution20.xml", @"TestUpdateFiles\TestConfig2.xml",
                new Dictionary<string, object> {{"tagged", "after"}, {"Environment", "LOC"}}
                                                )));
        }

        [Test]
        public void NDjangoFiltersAvailable()
        {
            var newConfig = XDocument.Parse(UpdateFile.Update(
                @"TestUpdateFiles\TestSubstitution12.xml", @"TestUpdateFiles\TestConfig1.xml", new Dictionary<string, object>{{"newvalue", "AfTer"}}
                ));
            Assert.AreEqual("after", newConfig.XPathSelectElement("/root/value").Value);
        }
    }
}
