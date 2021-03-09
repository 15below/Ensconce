using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Ensconce.Update.Tests.ApprovalTestFiles
{
    [TestFixture]
    [Category("Approval Test")]
    public class SubstitutionApprovalTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        [Test]
        [UseReporter(typeof(NUnitReporter))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Transform()
        {
            var config = TagDictionary.FromSources("ident", new Dictionary<TagSource, string> { { TagSource.XmlFileName, @"ApprovalTestFiles\Config.xml" } });
            var lazyConfig = new Lazy<TagDictionary>(() => config);
            var result = XDocument.Parse(ProcessSubstitution.Update(@"ApprovalTestFiles\Substitution.xml", @"ApprovalTestFiles\BaseFile.xml", lazyConfig));
            Approvals.VerifyXml(result.ToString());
        }
    }
}
