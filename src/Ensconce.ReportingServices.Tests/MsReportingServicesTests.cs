using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ensconce.ReportingServices.SSRS2010;
using NUnit.Framework;

namespace Ensconce.ReportingServices.Tests
{
    [TestFixture]
    public class MsReportingServicesTests
    {
        private MsReportingServices sut;
        private List<string> mockEmailSubInfoContents;
        private List<string> mockFileShareSubInfoContents;

        [SetUp]
        public void Setup()
        {
            sut = new MsReportingServices("http://mock.url", "mock", "mock", "mock");
            mockEmailSubInfoContents = new List<string>
            {
                "subscriptionOn,true",
                "eventType,TimedSubscription",
                "scheduleXml,<ScheduleDefinition xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><StartDateTime xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/03/01/ReportServer\">2012-12-11T05:45:00.000+00:00</StartDateTime><MonthlyRecurrence xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/03/01/ReportServer\"><Days>1</Days><MonthsOfYear><January>true</January><February>true</February><March>true</March><April>true</April><May>true</May><June>true</June><July>true</July><August>true</August><September>true</September><October>true</October><November>true</November><December>true</December></MonthsOfYear></MonthlyRecurrence></ScheduleDefinition>",
                "subjectPrefix,Test Report",
                "emailBodyText,Your report is attached",
                "subscriptionSendTo,sendto@test.com",
                "subscriptionBCCto,bcc@test.com",
                "reportParameters,daterangetype=month"
            };
            mockFileShareSubInfoContents = new List<string>
            {
                "subscriptionOn,true",
                "eventType,TimedSubscription",
                "scheduleXml,<ScheduleDefinition xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><StartDateTime xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/03/01/ReportServer\">2012-12-11T05:45:00.000+00:00</StartDateTime><MonthlyRecurrence xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/03/01/ReportServer\"><Days>1</Days><MonthsOfYear><January>true</January><February>true</February><March>true</March><April>true</April><May>true</May><June>true</June><July>true</July><August>true</August><September>true</September><October>true</October><November>true</November><December>true</December></MonthsOfYear></MonthlyRecurrence></ScheduleDefinition>",
                "subscriptionType,FILESHARE",
                "subscriptionRenderFormat,CSV",
                @"subscriptionToFile_FilePath,\\ftpShare\Reporting",
                @"subscriptionToFile_UserName,ftpUserName",
                "subscriptionToFile_Password,IMAPASSWORD",
                "subscriptionToFile_FileName,Test_Report",
                "reportParameters,daterangetype=month"
            };
        }

        [Test]
        public void GetSubscription_GivenSubInfoFileContentsOfEmailSubscriptionType_ShouldReturnExpectedReportSubscription()
        {
            var result = sut.GetSubscription(mockEmailSubInfoContents.ToArray(), "D:/MockPath", "TestSubscription");

            var expectedParameterString = "TO: sendto@test.com" +
                                          "CC: " +
                                          "BCC: bcc@test.com" +
                                          "ReplyTo: system@15below.com" +
                                          "IncludeReport: True" +
                                          "RenderFormat: EXCEL" +
                                          "Subject: Test Report - @ReportName executed at @ExecutionTime" +
                                          "Comment: Your report is attached" +
                                          "IncludeLink: False" +
                                          "Priority: NORMAL";
            Assert.AreEqual(expectedParameterString, ParameterValueOrFieldReferencesToString(result.ExtensionSettings.ParameterValues));
            Assert.AreEqual("daterangetype: month", ParameterValueToString(result.Parameters));
            Assert.AreEqual(true, result.Enabled);
            Assert.AreEqual("TestSubscription", result.Name);
            Assert.AreEqual("D:/MockPath", result.Path);
            Assert.AreEqual("TestSubscription - Subscription", result.Description);
            Assert.AreEqual("TimedSubscription", result.EventType);
            Assert.AreEqual("<ScheduleDefinition xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><StartDateTime xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/03/01/ReportServer\">2012-12-11T05:45:00.000+00:00</StartDateTime><MonthlyRecurrence xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/03/01/ReportServer\"><Days>1</Days><MonthsOfYear><January>true</January><February>true</February><March>true</March><April>true</April><May>true</May><June>true</June><July>true</July><August>true</August><September>true</September><October>true</October><November>true</November><December>true</December></MonthsOfYear></MonthlyRecurrence></ScheduleDefinition>", result.ScheduleXml);
        }

        [Test]
        public void GetSubscription_GivenSubInfoFileContentsOfFileShareSubscriptionType_ShouldReturnExpectedReportSubscription()
        {
            var result = sut.GetSubscription(mockFileShareSubInfoContents.ToArray(), "D:/MockPath", "TestSubscription");

            var expectedParameterString = @"PATH: \\ftpShare\Reporting" +
                                          @"FILENAME: Test_Report_@timestamp" +
                                          @"FILEEXTN: True" +
                                          @"USERNAME: ftpUserName" +
                                          @"PASSWORD: IMAPASSWORD" +
                                          @"RENDER_FORMAT: CSV" +
                                          @"WRITEMODE: Overwrite";
            Assert.AreEqual(expectedParameterString, ParameterValueOrFieldReferencesToString(result.ExtensionSettings.ParameterValues));
            Assert.AreEqual("daterangetype: month", ParameterValueToString(result.Parameters));
            Assert.AreEqual(true, result.Enabled);
            Assert.AreEqual("TestSubscription", result.Name);
            Assert.AreEqual("D:/MockPath", result.Path);
            Assert.AreEqual("TestSubscription - Subscription", result.Description);
            Assert.AreEqual("TimedSubscription", result.EventType);
            Assert.AreEqual("<ScheduleDefinition xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><StartDateTime xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/03/01/ReportServer\">2012-12-11T05:45:00.000+00:00</StartDateTime><MonthlyRecurrence xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/03/01/ReportServer\"><Days>1</Days><MonthsOfYear><January>true</January><February>true</February><March>true</March><April>true</April><May>true</May><June>true</June><July>true</July><August>true</August><September>true</September><October>true</October><November>true</November><December>true</December></MonthsOfYear></MonthlyRecurrence></ScheduleDefinition>", result.ScheduleXml);
        }

        [Test]
        [TestCase("Email", "CSV", ExpectedResult = "CSV", TestName = "EMAIL-CSV")]
        [TestCase("Email", "EXCEL", ExpectedResult = "EXCEL", TestName = "EMAIL-EXCEL")]
        [TestCase("Email", "WORD", ExpectedResult = "WORD", TestName = "EMAIL-WORD")]
        [TestCase("Email", "XML", ExpectedResult = "XML", TestName = "EMAIL-XML")]
        [TestCase("Email", "PDF", ExpectedResult = "PDF", TestName = "EMAIL-PDF")]
        [TestCase("Email", "MHTML", ExpectedResult = "MHTML", TestName = "EMAIL-MHTML")]
        [TestCase("Email", "INVALID", ExpectedResult = "INVALID", TestName = "EMAIL-INVALID")]
        [TestCase("Email", "", ExpectedResult = "EXCEL", TestName = "EMAIL-Default")]
        [TestCase("FileShare", "CSV", ExpectedResult = "CSV", TestName = "FileShare-CSV")]
        [TestCase("FileShare", "EXCEL", ExpectedResult = "EXCEL", TestName = "FileShare-EXCEL")]
        [TestCase("FileShare", "WORD", ExpectedResult = "WORD", TestName = "FileShare-WORD")]
        [TestCase("FileShare", "XML", ExpectedResult = "XML", TestName = "FileShare-XML")]
        [TestCase("FileShare", "PDF", ExpectedResult = "PDF", TestName = "FileShare-PDF")]
        [TestCase("FileShare", "MHTML", ExpectedResult = "MHTML", TestName = "FileShare-MHTML")]
        [TestCase("FileShare", "INVALID", ExpectedResult = "INVALID", TestName = "FileShare-INVALID")]
        [TestCase("FileShare", "", ExpectedResult = "CSV", TestName = "FileShare-Default")]
        public string GetSubscription_GivenSubInfoFileContentsAndSubscriptionTypeAndRenderFormat_ShouldReturnExpectedReportSubscriptionWithRenderFormat(string subscriptionType, string renderFormat)
        {
            ReportSubscription result;
            if (subscriptionType == "Email")
            {
                mockEmailSubInfoContents.Remove("subscriptionRenderFormat,CSV");
                if (!string.IsNullOrEmpty(renderFormat))
                {
                    mockEmailSubInfoContents.Add($"subscriptionRenderFormat,{renderFormat}");
                }

                result = sut.GetSubscription(mockEmailSubInfoContents.ToArray(), "D:/MockPath", "TestSubscription");
            }
            else
            {
                mockFileShareSubInfoContents.Remove("subscriptionRenderFormat,CSV");
                if (!string.IsNullOrEmpty(renderFormat))
                {
                    mockFileShareSubInfoContents.Add($"subscriptionRenderFormat,{renderFormat}");
                }

                result = sut.GetSubscription(mockFileShareSubInfoContents.ToArray(), "D:/MockPath", "TestSubscription");
            }

            return result.ExtensionSettings.ParameterValues
                .Select(p1 => (ParameterValue)p1)
                .FirstOrDefault(p2 => subscriptionType == "FileShare" && p2.Name == "RENDER_FORMAT" ||
                                      subscriptionType == "Email" && p2.Name == "RenderFormat")
                ?.Value;
        }

        private static string ParameterValueOrFieldReferencesToString(IReadOnlyList<ParameterValueOrFieldReference> parameters)
        {
            var parameterValues = new ParameterValue[parameters.Count];
            for (var i = 0; i < parameters.Count; i++)
            {
                parameterValues[i] = (ParameterValue)parameters[i];
            }

            return ParameterValueToString(parameterValues);
        }

        private static string ParameterValueToString(IEnumerable<ParameterValue> parameterValues)
        {
            var results = new StringBuilder();
            foreach (var parameterValue in parameterValues)
            {
                results.Append($"{parameterValue.Name ?? string.Empty}: {parameterValue.Value ?? string.Empty}");
            }

            return results.ToString();
        }
    }
}
