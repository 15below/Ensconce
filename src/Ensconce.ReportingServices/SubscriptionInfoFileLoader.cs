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
            if (GetSubInfoValue(subscriptionInfoText, "subscriptionOn", false, "false").ToLower() == "true")
            {
                var eventType = GetSubInfoValue(subscriptionInfoText, "eventType", true);
                var scheduleXml = GetSubInfoValue(subscriptionInfoText, "scheduleXml", true);
                var subscriptionType = GetSubInfoValue(subscriptionInfoText, "subscriptionType", false);
                var subscriptionTypeParameters = GetSubscriptionTypeParameters(subscriptionInfoText);
                var extSettings = new ExtensionSettings
                {
                    ParameterValues = subscriptionTypeParameters,
                    Extension = string.IsNullOrEmpty(subscriptionType) ? "Report Server Email" : "Report Server FileShare"
                };

                ParameterValue[] reportParameterValues = null;
                var reportParameters = GetSubInfoValue(subscriptionInfoText, "reportParameters", false);

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

        private static ParameterValueOrFieldReference[] GetSubscriptionTypeParameters(string[] subscriptionInfoText)
        {
            ParameterValueOrFieldReference[] extensionParams;
            var subscriptionType = GetSubInfoValue(subscriptionInfoText, "subscriptionType", false, "EMAIL").ToUpper();
            switch (subscriptionType)
            {
                case "FILESHARE":
                case "CSV":
                    extensionParams = new ParameterValueOrFieldReference[7];
                    extensionParams[0] = new ParameterValue
                    {
                        Name = "PATH",
                        Value = GetSubInfoValue(subscriptionInfoText, "subscriptionToFile_FilePath", true)
                    };
                    //Set the filename to always have a timestamp
                    extensionParams[1] = new ParameterValue
                    {
                        Name = "FILENAME",
                        Value = GetSubInfoValue(subscriptionInfoText, "subscriptionToFile_FileName", true) + "_@timestamp"
                    };
                    // Add a file extension always
                    extensionParams[2] = new ParameterValue { Name = "FILEEXTN", Value = "True" };
                    extensionParams[3] = new ParameterValue
                    {
                        Name = "USERNAME",
                        Value = GetSubInfoValue(subscriptionInfoText, "subscriptionToFile_UserName", true)
                    };
                    extensionParams[4] = new ParameterValue
                    {
                        Name = "PASSWORD",
                        Value = GetSubInfoValue(subscriptionInfoText, "subscriptionToFile_Password", true)
                    };
                    extensionParams[5] = new ParameterValue
                    {
                        Name = "RENDER_FORMAT",
                        Value = GetSubInfoValue(subscriptionInfoText, "subscriptionRenderFormat", false, "CSV")
                    };
                    extensionParams[6] = new ParameterValue { Name = "WRITEMODE", Value = "Overwrite" };
                    break;

                case "EMAIL":
                    extensionParams = new ParameterValueOrFieldReference[10];
                    extensionParams[0] = new ParameterValue
                    {
                        Name = "TO",
                        Value = GetSubInfoValue(subscriptionInfoText, "subscriptionSendTo", true)
                    };
                    extensionParams[1] = new ParameterValue
                    {
                        Name = "CC",
                        Value = GetSubInfoValue(subscriptionInfoText, "subscriptionCCto", false)
                    };
                    extensionParams[2] = new ParameterValue
                    {
                        Name = "BCC",
                        Value = GetSubInfoValue(subscriptionInfoText, "subscriptionBCCto", false)
                    };
                    extensionParams[3] = new ParameterValue { Name = "ReplyTo", Value = "system@15below.com" };
                    extensionParams[4] = new ParameterValue { Name = "IncludeReport", Value = "True" };
                    extensionParams[5] = new ParameterValue
                    {
                        Name = "RenderFormat",
                        Value = GetSubInfoValue(subscriptionInfoText, "subscriptionRenderFormat", false, "EXCEL").ToUpper()
                    };
                    extensionParams[6] = new ParameterValue
                    {
                        Name = "Subject",
                        Value = GetSubInfoValue(subscriptionInfoText, "subjectPrefix", true) + " - @ReportName executed at @ExecutionTime"
                    };
                    extensionParams[7] = new ParameterValue
                    {
                        Name = "Comment",
                        Value = GetSubInfoValue(subscriptionInfoText, "emailBodyText", true)
                    };
                    extensionParams[8] = new ParameterValue { Name = "IncludeLink", Value = "False" };
                    extensionParams[9] = new ParameterValue { Name = "Priority", Value = "NORMAL" };
                    break;

                default:
                    throw new Exception($"Unknown report type '{subscriptionType}', supported values: 'EMAIL','CSV','FILESHARE'");
            }

            return extensionParams;
        }

        private static string GetSubInfoValue(string[] subscriptionInfoText, string key, bool required, string defaultValue = "")
        {
            var value = string.Empty;
            foreach (var subInfoLine in subscriptionInfoText)
            {
                var subInfoLineParts = subInfoLine.Split(',');
                if (subInfoLineParts[0] == key)
                {
                    value = subInfoLineParts[1];
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                {
                    throw new Exception($"Value for '{key}' in subinfo file is empty when it is required");
                }

                return defaultValue;
            }

            return value;
        }

        private static void Log(string message, params object[] values)
        {
            Console.WriteLine(message, values);
            Console.Out.Flush();
        }
    }
}
