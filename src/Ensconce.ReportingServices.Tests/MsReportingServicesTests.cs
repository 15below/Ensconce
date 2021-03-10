using NUnit.Framework;

namespace Ensconce.ReportingServices.Tests
{
    [TestFixture]
    [Explicit("Requires real reporting services connection")]
    public class MsReportingServicesTests
    {
        private MsReportingServices sut;

        [SetUp]
        public void Setup()
        {
            var reportingServicesUrl = "http://btn-rs01.15b.local/ReportServer_NON1/ReportService2010.asmx";
            var loginDomain = "15below";
            var loginUser = "RS-Deploy-NON1";
            var loginPassword = "";

            sut = new MsReportingServices(reportingServicesUrl, loginDomain, loginUser, loginPassword);
        }

        [Test]
        public void GetAllReports()
        {
            //Arrange

            //Act
            var reports = sut.GetAllReports();

            //Assert
            Assert.IsNotEmpty(reports);
        }
    }
}
