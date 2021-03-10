using Ensconce.ReportingServices.SSRS2010;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ensconce.ReportingServices
{
    public class MsReportingServices
    {
        private readonly ReportingServicesCaller reportingServicesCaller;

        public MsReportingServices(string reportingServicesUrl, string networkDomain, string networkLogin, string networkPassword)
        {
            Log("Creating MsReportingServices instance");
            reportingServicesCaller = new ReportingServicesCaller(reportingServicesUrl, networkDomain, networkLogin, networkPassword);
        }

        #region Public Methods

        public List<(string name, string path)> GetAllReports()
        {
            var childrenResponse = reportingServicesCaller.CallReport(rs => rs.ListChildren(new ListChildrenRequest { ItemPath = "/", Recursive = true }));
            return childrenResponse.CatalogItems.Select(x => (x.Name, x.Path)).ToList();
        }

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
                    throw new ArgumentException($"itemPath: '{itemPath}' cannot be null or empty and must begin with a '/'");
                }
                if (string.IsNullOrWhiteSpace(reportingUserToAddRoleFor))
                {
                    throw new ArgumentException($"reportingUserToAddRoleFor: '{reportingUserToAddRoleFor}' cannot be null or empty");
                }

                if (string.IsNullOrWhiteSpace(reportingRoleToAdd))
                {
                    throw new ArgumentException($"reportingRoleToAdd: '{reportingRoleToAdd}' cannot be null or empty");
                }
                if (!supportedRoles.Contains(reportingRoleToAdd))
                {
                    throw new ArgumentOutOfRangeException($"reportingRoleToAdd: '{reportingRoleToAdd}' is not supported. Only '{string.Join(", ", supportedRoles)}' are supported.");
                }

                Log("Attempting to retrieve a list of all existing policies for itemPath: '{0}'", itemPath);

                Policy policy;
                var existingPoliciesResponse = reportingServicesCaller.CallReport(rs => rs.GetPolicies(new GetPoliciesRequest { ItemPath = itemPath }));
                var existingPolicies = existingPoliciesResponse.Policies;

                if (reportingUserToAddRoleFor.Contains("\\"))
                {
                    policy = existingPolicies.FirstOrDefault(p => p != null &&
                        p.GroupUserName.ToUpperInvariant() == reportingUserToAddRoleFor.ToUpperInvariant()
                    );
                }
                else
                {
                    policy = existingPolicies.FirstOrDefault(p => p != null &&
                        p.GroupUserName.ToUpperInvariant().Split('\\')[1] == reportingUserToAddRoleFor.ToUpperInvariant()
                    );
                }

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
                var existingRoles = policy.Roles;

                var role = existingRoles.FirstOrDefault(r =>
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
                reportingServicesCaller.CallReport(rs => rs.SetPolicies(new SetPoliciesRequest { ItemPath = itemPath, Policies = existingPolicies }));
            }
            catch (Exception ex)
            {
                Log("An error occurred whilst attempting to set roles for user in Reporting Services!");
                Log("Message: {0}", ex.Message);
                throw;
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
        }

        #endregion Public Methods

        #region Private Helper Methods

        private void Log(string message, params object[] values)
        {
            Console.WriteLine(message, values);
            Console.Out.Flush();
        }

        private void CreateParentFolder(string parentFolder)
        {
            var listChildrenResponse = reportingServicesCaller.CallReport(rs => rs.ListChildren(new ListChildrenRequest { ItemPath = "/", Recursive = false }));
            var items = listChildrenResponse.CatalogItems;
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
                reportingServicesCaller.CallReport(rs => rs.CreateFolder(new CreateFolderRequest { Parent = parentFolder, Folder = "/" }));
                Log("Created folder '{0}'", parentFolder);
            }
        }

        private void DeleteSubFolderIfExists(string parentFolder, string subFolder)
        {
            var listChildrenResponse = reportingServicesCaller.CallReport(rs => rs.ListChildren(new ListChildrenRequest { ItemPath = "/" + parentFolder, Recursive = false }));
            var items = listChildrenResponse.CatalogItems;
            var subFolderExists = items.Any(catalogItem =>
                catalogItem.Name.ToUpperInvariant() == subFolder.ToUpperInvariant() &&
                catalogItem.TypeName.ToUpperInvariant() == "FOLDER");

            if (subFolderExists)
            {
                Log(@"Deleting sub folder '{0}\{1}'.", parentFolder, subFolder);
                reportingServicesCaller.CallReport(rs => rs.DeleteItem(new DeleteItemRequest { ItemPath = $"/{parentFolder}/{subFolder}" }));
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
                var reportsDirectoryInfo = new DirectoryInfo(reportSourceFolder);
                var reportFiles = reportsDirectoryInfo.GetFiles("*.rdl");

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
            reportingServicesCaller.CallReport(rs => rs.CreateFolder(new CreateFolderRequest { Folder = subFolder, Parent = parentFolder }));
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
            reportingServicesCaller.CallReport(rs => rs.CreateDataSource(new CreateDataSourceRequest { DataSource = dataSourceName, Parent = $"/{parentFolder}/{subFolder}", Overwrite = true, Definition = definition }));
            Log("Created data source {0}", dataSourceName);
        }

        private void CreateReportsAndSubscriptions(string parentFolder, string subFolder, string reportSourceFolder, string dataSourceName)
        {
            var reportsDirectoryInfo = new DirectoryInfo(reportSourceFolder);
            var reportFiles = reportsDirectoryInfo.GetFiles("*.rdl");
            var targetFolder = $"/{parentFolder}/{subFolder}";
            foreach (var fileInfo in reportFiles)
            {
                var reportName = fileInfo.Name.Replace(".rdl", "");
                CreateReport(reportName, reportSourceFolder, targetFolder);
                SetReportDataSource(reportName, dataSourceName, targetFolder);
                CreateSubscriptions(reportName, targetFolder + "/" + reportName, reportSourceFolder);
            }
        }

        private void CreateReport(string reportName, string sourceFolder, string targetFolder)
        {
            var reportDefinition = GetReportDefinition(reportName, sourceFolder);
            Log("Creating report '{0}'", reportName);
            var createCreateCatalogItemResponse = reportingServicesCaller.CallReport(rs => rs.CreateCatalogItem(new CreateCatalogItemRequest { ItemType = "Report", Name = reportName, Parent = targetFolder, Overwrite = true, Definition = reportDefinition }));
            var warnings = createCreateCatalogItemResponse.Warnings;
            var catalogItem = createCreateCatalogItemResponse.ItemInfo;

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
            Byte[] definition;
            var stream = File.OpenRead(sourceFolder + "\\" + reportName + ".rdl");
            definition = new Byte[stream.Length];
            stream.Read(definition, 0, Convert.ToInt32(stream.Length));
            stream.Close();
            return definition;
        }

        private void PrintWarnings(Warning[] warnings)
        {
            if (warnings != null)
            {
                Log("There were {0} warnings", warnings.Length);
                var count = 0;
                foreach (var warning in warnings)
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
            var getItemDataSourcesResponse = reportingServicesCaller.CallReport(rs => rs.GetItemDataSources(new GetItemDataSourcesRequest { ItemPath = targetFolder + "/" + reportName }));
            var dataSource = getItemDataSourcesResponse.DataSources;

            Log("Report '{0}' has {1} data sources", reportName, dataSource.Length);

            var dataSources = new DataSource[dataSource.Length];
            int i;

            for (i = 0; i <= dataSource.Length - 1; i++)
            {
                var ds = new DataSource { Item = reference, Name = dataSource[i].Name };
                dataSources[i] = ds;
                Log("Report '{0}' setting data source '{1}' to '{2}'", reportName, ds.Name, dataSourceName);
            }

            reportingServicesCaller.CallReport(rs => rs.SetItemDataSources(new SetItemDataSourcesRequest { ItemPath = targetFolder + "/" + reportName, DataSources = dataSources }));
        }

        private void CreateSubscriptions(string reportName, string reportPath, string sourceFolder)
        {
            var subscriptions = SubscriptionInfoFileLoader.GetSubscriptions(reportName, reportPath, sourceFolder).Where(x => x.Enabled).ToList();

            if (subscriptions.Count == 0)
            {
                Log("No enabled subscriptions found for report '{0}'", reportName);
            }
            else
            {
                foreach (var subscription in subscriptions)
                {
                    Log("Creating subscription '{0}'", subscription.Name);
                    reportingServicesCaller.CallReport(rs => rs.CreateSubscription(new CreateSubscriptionRequest { ItemPath = subscription.Path, ExtensionSettings = subscription.ExtensionSettings, Description = subscription.Description, EventType = subscription.EventType, MatchData = subscription.ScheduleXml, Parameters = subscription.Parameters }));
                    Log("Created subscription '{0}'", subscription.Name);
                }
            }
        }

        #endregion Private Helper Methods
    }
}
