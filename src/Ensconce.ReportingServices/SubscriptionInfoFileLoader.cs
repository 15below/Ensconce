using Ensconce.ReportingServices.SSRS2010;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ensconce.ReportingServices
{
    public static class SubscriptionInfoFileLoader
    {
        public static IEnumerable<ReportSubscription> GetSubscriptions(string reportName, string reportPath, string sourceFolder)
        {
            var reportsDirectoryInfo = new DirectoryInfo(sourceFolder);
            var subscriptionFileInfos = reportsDirectoryInfo.GetFiles(reportName + "*.subinfo");

            foreach (var subscriptionFileInfo in subscriptionFileInfos)
            {
                var subscriptionInfoText = File.ReadAllLines(subscriptionFileInfo.FullName);
                yield return GetSubscription(subscriptionInfoText, reportPath, subscriptionFileInfo.Name);
            }
        }

        public static ReportSubscription GetSubscription(string[] subscriptionInfoText, string subscriptionPath, string subscriptionName)
        {
            if (SubscriptionInfo(subscriptionInfoText, "subscriptionOn").ToLower() == "true")
            {
                var eventType = SubscriptionInfo(subscriptionInfoText, "eventType");
                var scheduleXml = SubscriptionInfo(subscriptionInfoText, "scheduleXml");
                var subscriptionType = SubscriptionInfo(subscriptionInfoText, "subscriptionType");
                var subscriptionTypeParameters = GetSubscriptionTypeParameters(subscriptionInfoText);
                var extSettings = new ExtensionSettings
                {
                    ParameterValues = subscriptionTypeParameters,
                    Extension = string.IsNullOrEmpty(subscriptionType) ? "Report Server Email" : "Report Server FileShare"
                };

                ParameterValue[] reportParameterValues = null;
                var reportParameters = SubscriptionInfo(subscriptionInfoText, "reportParameters");

                if (!string.IsNullOrEmpty(reportParameters))
                {
                    var reportParameterParts = reportParameters.Split(';');
                    reportParameterValues = new ParameterValue[reportParameterParts.Length];
                    Log("Found {0} report parameter values to set", reportParameterParts.Length);

                    for (var i = 0; i <= reportParameterParts.Length - 1; i++)
                    {
                        var reportValue = new ParameterValue();
                        var parameterKeyValues = reportParameterParts[i].Split('=');

                        Log("Setting parameter '{0}', with '{1}'", parameterKeyValues[0], parameterKeyValues[1]);

                        reportValue.Name = parameterKeyValues[0];
                        reportValue.Value = parameterKeyValues[1];
                        reportParameterValues[i] = reportValue;
                    }
                }
                else
                {
                    Log("Subscription report parameters not found in '{0}'", subscriptionName);
                }

                return new ReportSubscription
                {
                    Enabled = true,
                    Name = subscriptionName,
                    Path = subscriptionPath,
                    ExtensionSettings = extSettings,
                    Description = $"{subscriptionName} - Subscription",
                    EventType = eventType,
                    ScheduleXml = scheduleXml,
                    Parameters = reportParameterValues
                };
            }

            Log("Subscription not set to 'On' for '{0}'", subscriptionName);
            return new ReportSubscription
            {
                Enabled = false
            };
        }

        private static ParameterValue[] GetSubscriptionTypeParameters(string[] subscriptionInfoText)
        {
            ParameterValue[] extensionParams;
            switch (SubscriptionInfo(subscriptionInfoText, "subscriptionType"))
            {
                case "FILESHARE":
                case "fileshare":
                case "CSV":
                case "csv":
                    extensionParams = new ParameterValue[7];
                    extensionParams[0] = new ParameterValue
                    {
                        Name = "PATH",
                        Value = SubscriptionInfo(subscriptionInfoText, "subscriptionToFile_FilePath")
                    };
                    //Set the filename to always have a timestamp
                    extensionParams[1] = new ParameterValue
                    {
                        Name = "FILENAME",
                        Value = SubscriptionInfo(subscriptionInfoText, "subscriptionToFile_FileName") + "_@timestamp"
                    };
                    // Add a file extension always
                    extensionParams[2] = new ParameterValue { Name = "FILEEXTN", Value = "True" };
                    extensionParams[3] = new ParameterValue
                    {
                        Name = "USERNAME",
                        Value = SubscriptionInfo(subscriptionInfoText, "subscriptionToFile_UserName")
                    };
                    extensionParams[4] = new ParameterValue
                    {
                        Name = "PASSWORD",
                        Value = SubscriptionInfo(subscriptionInfoText, "subscriptionToFile_Password")
                    };
                    var fileShareSubscriptionRenderFormat = SubscriptionInfo(subscriptionInfoText, "subscriptionRenderFormat");
                    extensionParams[5] = new ParameterValue
                    {
                        Name = "RENDER_FORMAT",
                        Value = !string.IsNullOrEmpty(fileShareSubscriptionRenderFormat) ? fileShareSubscriptionRenderFormat.ToUpper() : "CSV"
                    };
                    extensionParams[6] = new ParameterValue { Name = "WRITEMODE", Value = "Overwrite" };
                    return extensionParams;

                default:
                    extensionParams = new ParameterValue[10];
                    extensionParams[0] = new ParameterValue
                    {
                        Name = "TO",
                        Value = SubscriptionInfo(subscriptionInfoText, "subscriptionSendTo")
                    };
                    extensionParams[1] = new ParameterValue
                    {
                        Name = "CC",
                        Value = SubscriptionInfo(subscriptionInfoText, "subscriptionCCto")
                    };
                    extensionParams[2] = new ParameterValue
                    {
                        Name = "BCC",
                        Value = SubscriptionInfo(subscriptionInfoText, "subscriptionBCCto")
                    };
                    extensionParams[3] = new ParameterValue { Name = "ReplyTo", Value = "system@15below.com" };
                    extensionParams[4] = new ParameterValue { Name = "IncludeReport", Value = "True" };
                    var emailSubscriptionRenderFormat = SubscriptionInfo(subscriptionInfoText, "subscriptionRenderFormat");
                    extensionParams[5] = new ParameterValue
                    {
                        Name = "RenderFormat",
                        Value = !string.IsNullOrEmpty(emailSubscriptionRenderFormat) ? emailSubscriptionRenderFormat.ToUpper() : "EXCEL"
                    };
                    extensionParams[6] = new ParameterValue
                    {
                        Name = "Subject",
                        Value = SubscriptionInfo(subscriptionInfoText, "subjectPrefix") + " - @ReportName executed at @ExecutionTime"
                    };
                    extensionParams[7] = new ParameterValue
                    {
                        Name = "Comment",
                        Value = SubscriptionInfo(subscriptionInfoText, "emailBodyText")
                    };
                    extensionParams[8] = new ParameterValue { Name = "IncludeLink", Value = "False" };
                    extensionParams[9] = new ParameterValue { Name = "Priority", Value = "NORMAL" };
                    return extensionParams;
            }
        }

        private static string SubscriptionInfo(string[] subscriptionInfoText, string key)
        {
            foreach (var subInfoLine in subscriptionInfoText)
            {
                var subInfoLineParts = subInfoLine.Split(',');
                if (subInfoLineParts[0] == key)
                {
                    return subInfoLineParts[1];
                }
            }
            return string.Empty;
        }

        private static void Log(string message, params object[] values)
        {
            Console.WriteLine(message, values);
            Console.Out.Flush();
        }
    }
}
