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
            var reportingServicesUrl = "http://localhost/ReportServer/ReportService2010.asmx";
            var loginDomain = ".";
            var loginUser = "";
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
