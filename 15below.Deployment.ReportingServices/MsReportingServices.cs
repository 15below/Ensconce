using System;
using System.IO;
using System.Linq;
using System.Net;
using FifteenBelow.Deployment.ReportingServices.SSRS2010;

namespace FifteenBelow.Deployment.ReportingServices
{

    public class MsReportingServices
    {
        private ReportingService2010 rs;
        private Uri reportingServicesUri;

        public MsReportingServices(string reportingServicesUrl, string networkDomain, string networkLogin, string networkPassword)
        {
            Log("Creating MsReportingServices instance");
            if (!Uri.TryCreate(reportingServicesUrl, UriKind.RelativeOrAbsolute, out reportingServicesUri))
                throw new UriFormatException(string.Format("reporting services uri of '{0}' is invalid!", reportingServicesUri));

            if (string.IsNullOrWhiteSpace(networkPassword))
                throw new NullReferenceException("networkPassword is null or empty!");
            if (string.IsNullOrWhiteSpace(networkDomain))
                throw new NullReferenceException("networkDomain is null or empty!");
            if (string.IsNullOrWhiteSpace(networkLogin))
                throw new NullReferenceException("networkLogin is null or empty!");

            rs = new ReportingService2010
                {
                    Url = reportingServicesUri.AbsoluteUri,
                    Credentials = new NetworkCredential(networkLogin, networkPassword, networkDomain)
                };
        }

        #region Public Methods

        /// <summary>
        /// Adds a role to Reporting Services for a user to an itemPath
        /// </summary>
        /// <param name="itemPath">the ItemPath for the role to be applied to.</param>
        /// <param name="reportingUserToAddRoleFor">the user (either domain or local user) to have the role added for.</param>
        /// <param name="reportingRoleToAdd">the Reporting Services role e.g. 'Content Manager'</param>
        public void AddRole(string itemPath, string reportingUserToAddRoleFor, string reportingRoleToAdd)
        {
            try
            {
                var supportedRoles = new[] { "Browser", "Content Manager", "My Reports", "Publisher", "Report Builder" };

                if (string.IsNullOrWhiteSpace(itemPath) || !itemPath.StartsWith("/"))
                {
                    throw new ArgumentException(string.Format("itemPath: '{0}' cannot be null or empty and must begin with a '/'", itemPath));
                }
                if (string.IsNullOrWhiteSpace(reportingUserToAddRoleFor))
                {
                    throw new ArgumentException(string.Format("reportingUserToAddRoleFor: '{0}' cannot be null or empty", reportingUserToAddRoleFor));
                }

                if (string.IsNullOrWhiteSpace(reportingRoleToAdd))
                {
                    throw new ArgumentException(string.Format("reportingRoleToAdd: '{0}' cannot be null or empty", reportingRoleToAdd));
                }
                if (!supportedRoles.Contains(reportingRoleToAdd))
                {
                    throw new ArgumentOutOfRangeException(string.Format("reportingRoleToAdd: '{0}' is not supported. Only '{1}' are supported.", reportingRoleToAdd, string.Join(", ", supportedRoles)));
                }

                Log("Attempting to retrieve a list of all existing policies for itemPath: '{0}'", itemPath);

                bool inheritParent;
                Policy policy = null;

                Policy[] existingPolicies = rs.GetPolicies(itemPath, out inheritParent);

                policy = existingPolicies.FirstOrDefault(p => p != null &&
                    p.GroupUserName.ToUpperInvariant() == reportingUserToAddRoleFor.ToUpperInvariant()
                );

                if (policy == null)
                {
                    Log("Adding new policy for User: '{0}' to existing policy list", reportingUserToAddRoleFor);

                    policy = new Policy()
                        {
                            GroupUserName = reportingUserToAddRoleFor.ToUpperInvariant(),
                            Roles = new Role[] { }
                        };

                    existingPolicies = existingPolicies.Concat(new[] { policy }).ToArray();
                }
                else
                {
                    Log("User: '{0}' already has a policy list, so using this", reportingUserToAddRoleFor);
                }

                Log("Attempting to select the role of: '{0}' for GroupUserName: '{1}'", reportingRoleToAdd,
                                  reportingUserToAddRoleFor);
                Role[] existingRoles = policy.Roles;
                Role role = null;

                role = existingRoles.FirstOrDefault(r =>
                    r != null &&
                    r.Name.ToUpperInvariant() == reportingRoleToAdd.ToUpperInvariant()
                );

                if (role == null)
                {
                    Log("Adding new role of: '{0}' for User: '{1}'.", reportingRoleToAdd, reportingUserToAddRoleFor);
                    role = new Role()
                        {
                            Name = reportingRoleToAdd
                        };
                    policy.Roles = existingRoles.Concat(new[] { role }).ToArray();
                }
                else
                {
                    Log("User: '{0}' already has a role of type: '{1}', nothing to add.", reportingUserToAddRoleFor, reportingRoleToAdd);
                }

                Log("Setting policy role of: '{0}' for user: '{1}' for itemPath: '{2}'", reportingRoleToAdd, reportingUserToAddRoleFor, itemPath);
                rs.SetPolicies(itemPath, existingPolicies);
            }
            catch (Exception ex)
            {
                Log("An error occurred whilst attempting to set roles for user in Reporting Services!");
                Log("Message: {0}", ex.Message);
                throw;
            }
            finally
            {
                if (rs != null) rs.Dispose();
            }
        }

        /// <summary>
        /// Publishes reports to a reporting services instance, and deploys reports in a parentFolder/subFolder structure.
        /// </summary>
        /// <param name="parentFolder">specifies the parent folder for the reports</param>
        /// <param name="subFolder">specifies the sub folder for the reports</param>
        /// <param name="dataSourceName">specifies the data source name to use for the set of reports.</param>
        /// <param name="dataSourceConnectionString">specifies the data source connection string</param>
        /// <param name="dataSourceUserName">specifies the data source user name</param>
        /// <param name="dataSourcePassword">specifies the data source password</param>
        /// <param name="reportSourceFolder">specifies the source folder for the report to upload from.</param>
        public void PublishReports(string parentFolder, string subFolder, string dataSourceName, string dataSourceConnectionString, string dataSourceUserName, string dataSourcePassword, string reportSourceFolder)
        {
            try
            {
                // Check if parentFolder exists
                // If it does, do nothing, If it does not, create it
                CreateParentFolder(parentFolder);

                // Check if sub folder exists
                // If it does, delete it, If it does not, do nothing.
                DeleteSubFolderIfExists(parentFolder, subFolder);

                // Check if the reportSourceFolder exists on disk, and if there are any reports in it.
                // If it does, create sub folder, data source and reports
                // If it does not, skip to the end of program

                if (ShouldCreateSubFolder(reportSourceFolder))
                {
                    CreateSubFolder(parentFolder, subFolder);
                    CreateDataSource(parentFolder, subFolder, dataSourceName, dataSourceConnectionString,
                                     dataSourceUserName, dataSourcePassword);
                    CreateReportsAndSubscriptions(parentFolder, subFolder, reportSourceFolder, dataSourceName);
                }

            }
            catch (Exception ex)
            {
                Log("An error occurred whilst attempting to publish reports to Reporting Services!");
                Log("Message: {0}", ex.Message);
                throw;
            }
            finally
            {
                if (rs != null) rs.Dispose();
            }
        }

        #endregion

        #region Private Helper Methods

        private void Log(string message, params object[] values)
        {
            Console.WriteLine(message, values);
            Console.Out.Flush();
        }

        private void CreateParentFolder(string parentFolder)
        {
            CatalogItem[] items = rs.ListChildren("/", false);
            var parentFolderExists = items.Any(catalogItem =>
                catalogItem.Name.ToUpperInvariant() == parentFolder.ToUpperInvariant() &&
                catalogItem.TypeName.ToUpperInvariant() == "FOLDER");

            if (parentFolderExists)
            {
                Log("Folder '{0}' already exists", parentFolder);
            }
            else
            {
                Log("Creating folder '{0}'", parentFolder);
                rs.CreateFolder(parentFolder, "/", null);
                Log("Created folder '{0}'", parentFolder);
            }
        }

        private void DeleteSubFolderIfExists(string parentFolder, string subFolder)
        {
            CatalogItem[] items = rs.ListChildren("/" + parentFolder, false);
            var subFolderExists = items.Any(catalogItem =>
                catalogItem.Name.ToUpperInvariant() == subFolder.ToUpperInvariant() &&
                catalogItem.TypeName.ToUpperInvariant() == "FOLDER");

            if (subFolderExists)
            {
                Log(@"Deleting sub folder '{0}\{1}'.", parentFolder, subFolder);
                rs.DeleteItem(string.Format("/{0}/{1}", parentFolder, subFolder));
                Log(@"Deleted sub folder '{0}\{1}'.", parentFolder, subFolder);
            }
            else
            {
                Log(@"Sub folder '{0}\{1}' does not exist.", parentFolder, subFolder);
            }
        }

        private bool ShouldCreateSubFolder(string reportSourceFolder)
        {
            var createSubFolder = false;
            if (Directory.Exists(reportSourceFolder))
            {
                DirectoryInfo reportsDirectoryInfo = new DirectoryInfo(reportSourceFolder);
                FileInfo[] reportFiles = reportsDirectoryInfo.GetFiles("*.rdl");

                if (reportFiles.Length > 0)
                {
                    createSubFolder = true;
                }
            }
            return createSubFolder;
        }

        private void CreateSubFolder(string parentFolder, string subFolder)
        {
            Log(@"Creating sub folder '{0}\{1}'.", parentFolder, subFolder);
            rs.CreateFolder(subFolder, "/" + parentFolder, null);
            Log(@"Created sub folder '{0}\{1}'.", parentFolder, subFolder);
        }

        private void CreateDataSource(string parentFolder, string subFolder, string dataSourceName, string dataSourceConnectionString, string dataSourceUserName, string dataSourcePassWord)
        {
            var definition = new DataSourceDefinition
            {
                CredentialRetrieval = CredentialRetrievalEnum.Store,
                ConnectString = dataSourceConnectionString,
                Enabled = true,
                EnabledSpecified = true,
                Extension = "SQL",
                ImpersonateUser = false,
                ImpersonateUserSpecified = true,
                Prompt = null,
                WindowsCredentials = false,
                UserName = dataSourceUserName,
                Password = dataSourcePassWord
            };

            Log("Creating data source {0} with value: {1}", dataSourceName, dataSourceConnectionString);
            rs.CreateDataSource(dataSourceName, string.Format("/{0}/{1}", parentFolder, subFolder), true, definition, null);
            Log("Created data source {0}", dataSourceName);
        }

        private void CreateReportsAndSubscriptions(string parentFolder, string subFolder, string reportSourceFolder, string dataSourceName)
        {
            var reportsDirectoryInfo = new DirectoryInfo(reportSourceFolder);
            var reportFiles = reportsDirectoryInfo.GetFiles("*.rdl");
            var targetFolder = string.Format("/{0}/{1}", parentFolder, subFolder);
            foreach (FileInfo fileInfo in reportFiles)
            {
                string reportName = fileInfo.Name.Replace(".rdl", "");
                CreateReport(reportName, reportSourceFolder, targetFolder);
                SetReportDataSource(reportName, dataSourceName, targetFolder);
                CreateSubscriptions(reportName, targetFolder + "/" + reportName, reportSourceFolder);
            }
        }

        private void CreateReport(string reportName, string sourceFolder, string targetFolder)
        {
            Byte[] reportDefinition = GetReportDefinition(reportName, sourceFolder);
            CatalogItem catalogItem = null;
            Warning[] warnings = null;
            Log("Creating report '{0}'", reportName);
            catalogItem = rs.CreateCatalogItem("Report", reportName, targetFolder, true, reportDefinition, null, out warnings);

            if (catalogItem == null)
            {
                Log("Report '{0}' not created ", reportName);
            }
            else
            {
                Log("Created report '{0}'", reportName);
            }
            PrintWarnings(warnings);
        }

        private Byte[] GetReportDefinition(string reportName, string sourceFolder)
        {
            Byte[] definition = null;
            FileStream stream = File.OpenRead(sourceFolder + "\\" + reportName + ".rdl");
            definition = new Byte[stream.Length];
            stream.Read(definition, 0, Convert.ToInt32(stream.Length));
            stream.Close();
            return definition;
        }

        private void PrintWarnings(Warning[] warnings)
        {
            if ((warnings != null))
            {
                Log("There were {0} warnings", warnings.Length);
                int count = 0;
                foreach (Warning warning in warnings)
                {
                    count = count + 1;
                    Log("{0}) {1}", count, warning.Message);
                }
            }
            else
            {
                Log("There were no warnings.");
            }
        }

        private void SetReportDataSource(string reportName, string dataSourceName, string targetFolder)
        {
            Log("Setting DataSource For Report: " + reportName);

            var reference = new DataSourceReference { Reference = targetFolder + "/" + dataSourceName };
            var dataSource = rs.GetItemDataSources(targetFolder + "/" + reportName);

            Log("Report '{0}' has {1} data sources", reportName, dataSource.Length);

            var dataSources = new DataSource[dataSource.Length];
            var i = 0;

            for (i = 0; i <= dataSource.Length - 1; i++)
            {
                var ds = new DataSource { Item = reference, Name = dataSource[i].Name };
                dataSources[i] = ds;
                Log("Report '{0}' setting data source '{1}' to '{2}'", reportName, ds.Name, dataSourceName);
            }

            rs.SetItemDataSources(targetFolder + "/" + reportName, dataSources);
        }

        private void CreateSubscriptions(string reportName, string reportPath, string sourceFolder)
        {
            var reportsDirectoryInfo = new DirectoryInfo(sourceFolder);
            var subscriptionFileInfos = reportsDirectoryInfo.GetFiles(reportName + "*.subinfo");

            if (subscriptionFileInfos.Length == 0)
            {
                Log("No subscription files found for report '{0}'", reportName);
            }
            else
            {
                foreach (var subscriptionFileInfo in subscriptionFileInfos)
                {
                    CreateSubscription(subscriptionFileInfo, reportPath);
                }
            }
        }

        private void CreateSubscription(FileInfo subscriptionFile, string reportPath)
        {
            string[] subscriptionInfoText = File.ReadAllLines(subscriptionFile.FullName);

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
                string reportParameters = SubscriptionInfo(subscriptionInfoText, "reportParameters");

                if (!string.IsNullOrEmpty(reportParameters))
                {
                    string[] reportParameterParts = reportParameters.Split(';');
                    reportParameterValues = new ParameterValue[reportParameterParts.Length];
                    Log("Found {0} report parameter values to set", reportParameterParts.Length);

                    for (int i = 0; i <= reportParameterParts.Length - 1; i++)
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
                    Log("Subscription report parameters not found in '{0}'", subscriptionFile.Name);
                }

                Log("Creating subscription '{0}'", subscriptionFile.Name);
                rs.CreateSubscription(reportPath, extSettings, subscriptionFile.Name + " - Subscription", eventType, scheduleXml, reportParameterValues);
                Log("Created subscription '{0}'", subscriptionFile.Name);
            }
            else
            {
                Log("Subscription not set to 'On' for '{0}'", subscriptionFile.Name);
            }
        }

        private ParameterValue[] GetSubscriptionTypeParameters(string[] subscriptionInfoText)
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
                    var subscriptionRenderFormat = SubscriptionInfo(subscriptionInfoText, "subscriptionRenderFormat");
                    extensionParams[5] = new ParameterValue
                        {
                            Name = "RENDER_FORMAT",
                            Value = !string.IsNullOrEmpty(subscriptionRenderFormat) ? subscriptionRenderFormat : "CSV"
                        };
                    extensionParams[6] = new ParameterValue { Name = "WRITEMODE", Value = "Overwrite" };
                    return extensionParams;
                default:
                    extensionParams = new ParameterValue[9];
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
                    extensionParams[5] = new ParameterValue { Name = "RenderFormat", Value = "EXCEL" };
                    extensionParams[6] = new ParameterValue
                        {
                            Name = "Subject",
                            Value =
                                SubscriptionInfo(subscriptionInfoText, "subjectPrefix") +
                                " - @ReportName executed at @ExecutionTime"
                        };
                    extensionParams[7] = new ParameterValue { Name = "IncludeLink", Value = "False" };
                    extensionParams[8] = new ParameterValue { Name = "Priority", Value = "NORMAL" };
                    return extensionParams;
            }
        }

        private string SubscriptionInfo(string[] subscriptionInfoText, string key)
        {
            foreach (string subInfoLine in subscriptionInfoText)
            {
                string[] subInfoLineParts = subInfoLine.Split(',');
                if (subInfoLineParts[0] == key)
                {
                    return subInfoLineParts[1];
                }
            }
            return string.Empty;
        }

        #endregion
    }
}
