using System.Collections.Generic;
using Ensconce.ReportingServices;

namespace Ensconce
{
    internal static class Reporting
    {
        internal static void RunReportingServices()
        {
            var reportingServicesUrl = GetReportingVariable("reportingServicesUrl");
            var networkDomain = GetReportingVariable("networkDomain");
            var networkLogin = GetReportingVariable("networkLogin");
            var networkPassword = GetReportingVariable("networkPassword");
            var msreports = new MsReportingServices(reportingServicesUrl, networkDomain, networkLogin, networkPassword);

            if (Arguments.DeployReportingRole) DeployReportingServiceRole(msreports);
            if (Arguments.DeployReports) PublishReports(msreports);
        }

        private static void PublishReports(MsReportingServices msreports)
        {
            var parentFolder = GetReportingVariable("parentFolder");
            var subFolder = GetReportingVariable("subFolder");
            var dataSourceName = GetReportingVariable("dataSourceName");
            var dataSourceConnectionString = GetReportingVariable("dataSourceConnectionString");
            var dataSourceUserName = GetReportingVariable("dataSourceUserName");
            var dataSourcePassword = GetReportingVariable("dataSourcePassword");
            var reportSourceFolder = GetReportingVariable("reportSourceFolder");
            msreports.PublishReports(parentFolder, subFolder, dataSourceName, dataSourceConnectionString, dataSourceUserName, dataSourcePassword, reportSourceFolder);
        }

        private static void DeployReportingServiceRole(MsReportingServices msreports)
        {
            var itemPath = GetReportingVariable("itemPath");
            var reportingUserToAddRoleFor = GetReportingVariable("reportingUserToAddRoleFor");
            var reportingRoleToAdd = GetReportingVariable("reportingRoleToAdd");
            msreports.AddRole(itemPath, reportingUserToAddRoleFor, reportingRoleToAdd);
        }

        private static string GetReportingVariable(string key)
        {
            if (!Arguments.ReportingServiceVariables.ContainsKey(key))
                throw new KeyNotFoundException(string.Format("required key: {0} not found in supplied reporting service variables.", key));

            return Arguments.ReportingServiceVariables[key];
        }
    }
}