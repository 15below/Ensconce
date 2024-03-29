﻿using Ensconce.Helpers;
using Ensconce.ReportingServices.SSRS2010;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;

namespace Ensconce.ReportingServices
{
    public class MsReportingServices
    {
        private readonly ReportingService2010SoapClient reportingServicesClient;

        public MsReportingServices(string reportingServicesUrl, string networkDomain, string networkLogin, string networkPassword)
        {
            Logging.Log("Creating MsReportingServices instance");

            if (!Uri.TryCreate(reportingServicesUrl, UriKind.RelativeOrAbsolute, out Uri reportingServicesUri))
            {
                throw new UriFormatException($"reporting services uri of '{reportingServicesUri}' is invalid!");
            }

            if (string.IsNullOrWhiteSpace(networkPassword))
            {
                throw new NullReferenceException("networkPassword is null or empty!");
            }

            if (string.IsNullOrWhiteSpace(networkDomain))
            {
                throw new NullReferenceException("networkDomain is null or empty!");
            }

            if (string.IsNullOrWhiteSpace(networkLogin))
            {
                throw new NullReferenceException("networkLogin is null or empty!");
            }

            var binding = new BasicHttpBinding
            {
                Name = "bindingReportingServices2010",
                Security = new BasicHttpSecurity
                {
                    Transport = new HttpTransportSecurity
                    {
                        ClientCredentialType = HttpClientCredentialType.Ntlm
                    },
                    Mode = BasicHttpSecurityMode.TransportCredentialOnly
                },
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                MaxBufferPoolSize = int.MaxValue
            };

            var endpoint = new EndpointAddress(reportingServicesUri);

            reportingServicesClient = new ReportingService2010SoapClient(binding, endpoint);
            reportingServicesClient.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            reportingServicesClient.ClientCredentials.Windows.ClientCredential = new NetworkCredential(networkLogin, networkPassword, networkDomain);
        }

        #region Public Methods

        public List<(string name, string path)> GetAllReports()
        {
            var childrenResponse = reportingServicesClient.ListChildren(new ListChildrenRequest { ItemPath = "/", Recursive = true });
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

                Logging.Log("Attempting to retrieve a list of all existing policies for itemPath: '{0}'", itemPath);

                Policy policy;
                var existingPoliciesResponse = reportingServicesClient.GetPolicies(new GetPoliciesRequest { ItemPath = itemPath });
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
                    Logging.Log("Adding new policy for User: '{0}' to existing policy list", reportingUserToAddRoleFor);

                    policy = new Policy()
                    {
                        GroupUserName = reportingUserToAddRoleFor.ToUpperInvariant(),
                        Roles = new Role[] { }
                    };

                    existingPolicies = existingPolicies.Concat(new[] { policy }).ToArray();
                }
                else
                {
                    Logging.Log("User: '{0}' already has a policy list, so using this", reportingUserToAddRoleFor);
                }

                Logging.Log("Attempting to select the role of: '{0}' for GroupUserName: '{1}'", reportingRoleToAdd,
                                  reportingUserToAddRoleFor);
                var existingRoles = policy.Roles;

                var role = existingRoles.FirstOrDefault(r =>
                    r != null &&
                    r.Name.ToUpperInvariant() == reportingRoleToAdd.ToUpperInvariant()
                );

                if (role == null)
                {
                    Logging.Log("Adding new role of: '{0}' for User: '{1}'.", reportingRoleToAdd, reportingUserToAddRoleFor);
                    role = new Role()
                    {
                        Name = reportingRoleToAdd
                    };
                    policy.Roles = existingRoles.Concat(new[] { role }).ToArray();
                }
                else
                {
                    Logging.Log("User: '{0}' already has a role of type: '{1}', nothing to add.", reportingUserToAddRoleFor, reportingRoleToAdd);
                }

                Logging.Log("Setting policy role of: '{0}' for user: '{1}' for itemPath: '{2}'", reportingRoleToAdd, reportingUserToAddRoleFor, itemPath);
                reportingServicesClient.SetPolicies(new SetPoliciesRequest { ItemPath = itemPath, Policies = existingPolicies });
            }
            catch (Exception ex)
            {
                Logging.LogError("An error occurred whilst attempting to set roles for user in Reporting Services! Message: {0}", ex.Message);
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
                Logging.LogError("An error occurred whilst attempting to publish reports to Reporting Services! Message: {0}", ex.Message);
                throw;
            }
        }

        #endregion Public Methods

        #region Private Helper Methods

        private void CreateParentFolder(string parentFolder)
        {
            var listChildrenResponse = reportingServicesClient.ListChildren(new ListChildrenRequest { ItemPath = "/", Recursive = false });
            var items = listChildrenResponse.CatalogItems;
            var parentFolderExists = items.Any(catalogItem =>
                catalogItem.Name.ToUpperInvariant() == parentFolder.ToUpperInvariant() &&
                catalogItem.TypeName.ToUpperInvariant() == "FOLDER");

            if (parentFolderExists)
            {
                Logging.Log("Folder '/{0}' already exists", parentFolder);
            }
            else
            {
                Logging.Log("Creating folder '/{0}'", parentFolder);
                reportingServicesClient.CreateFolder(new CreateFolderRequest { Folder = parentFolder, Parent = "/" });
                Logging.Log("Created folder '/{0}'", parentFolder);
            }
        }

        private void DeleteSubFolderIfExists(string parentFolder, string subFolder)
        {
            var listChildrenResponse = reportingServicesClient.ListChildren(new ListChildrenRequest { ItemPath = $"/{parentFolder}", Recursive = false });
            var items = listChildrenResponse.CatalogItems;
            var subFolderExists = items.Any(catalogItem =>
                catalogItem.Name.ToUpperInvariant() == subFolder.ToUpperInvariant() &&
                catalogItem.TypeName.ToUpperInvariant() == "FOLDER");

            if (subFolderExists)
            {
                Logging.Log(@"Deleting sub folder '/{0}/{1}'.", parentFolder, subFolder);
                reportingServicesClient.DeleteItem(new DeleteItemRequest { ItemPath = $"/{parentFolder}/{subFolder}" });
                Logging.Log(@"Deleted sub folder '/{0}/{1}'.", parentFolder, subFolder);
            }
            else
            {
                Logging.Log(@"Sub folder '/{0}/{1}' does not exist.", parentFolder, subFolder);
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
            Logging.Log(@"Creating sub folder '/{0}/{1}'.", parentFolder, subFolder);
            reportingServicesClient.CreateFolder(new CreateFolderRequest { Folder = subFolder, Parent = $"/{parentFolder}" });
            Logging.Log(@"Created sub folder '/{0}/{1}'.", parentFolder, subFolder);
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

            Logging.Log("Creating data source {0} with value: {1} in /{2}/{3}", dataSourceName, dataSourceConnectionString, parentFolder, subFolder);
            reportingServicesClient.CreateDataSource(new CreateDataSourceRequest { DataSource = dataSourceName, Parent = $"/{parentFolder}/{subFolder}", Overwrite = true, Definition = definition });
            Logging.Log("Created data source {0}", dataSourceName);
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
                CreateSubscriptions(reportName, $"{targetFolder}/{reportName}", reportSourceFolder);
            }
        }

        private void CreateReport(string reportName, string sourceFolder, string targetFolder)
        {
            var reportDefinition = GetReportDefinition(reportName, sourceFolder);
            Logging.Log("Creating report '{0}' in '{1}'", reportName, targetFolder);
            var createCreateCatalogItemResponse = reportingServicesClient.CreateCatalogItem(new CreateCatalogItemRequest { ItemType = "Report", Name = reportName, Parent = targetFolder, Overwrite = true, Definition = reportDefinition });
            var warnings = createCreateCatalogItemResponse.Warnings;
            var catalogItem = createCreateCatalogItemResponse.ItemInfo;

            if (catalogItem == null)
            {
                Logging.Log("Report '{0}' not created ", reportName);
            }
            else
            {
                Logging.Log("Created report '{0}'", reportName);
            }
            PrintWarnings(warnings);
        }

        private Byte[] GetReportDefinition(string reportName, string sourceFolder)
        {
            Byte[] definition;
            var stream = File.OpenRead($"{sourceFolder}\\{reportName}.rdl");
            definition = new Byte[stream.Length];
            stream.Read(definition, 0, Convert.ToInt32(stream.Length));
            stream.Close();
            return definition;
        }

        private void PrintWarnings(Warning[] warnings)
        {
            if (warnings != null)
            {
                Logging.Log("There were {0} warnings", warnings.Length);
                var count = 0;
                foreach (var warning in warnings)
                {
                    count += 1;
                    Logging.LogWarn("{0}) {1}", count, warning.Message);
                }
            }
            else
            {
                Logging.Log("There were no warnings.");
            }
        }

        private void SetReportDataSource(string reportName, string dataSourceName, string targetFolder)
        {
            Logging.Log("Setting DataSource For Report: " + reportName);

            var reference = new DataSourceReference { Reference = $"{targetFolder}/{dataSourceName}" };
            var getItemDataSourcesResponse = reportingServicesClient.GetItemDataSources(new GetItemDataSourcesRequest { ItemPath = $"{targetFolder}/{reportName}" });
            var dataSource = getItemDataSourcesResponse.DataSources;

            Logging.Log("Report '{0}' has {1} data sources", reportName, dataSource.Length);

            var dataSources = new DataSource[dataSource.Length];
            int i;

            for (i = 0; i <= dataSource.Length - 1; i++)
            {
                var ds = new DataSource { Item = reference, Name = dataSource[i].Name };
                dataSources[i] = ds;
                Logging.Log("Report '{0}' setting data source '{1}' to '{2}'", reportName, ds.Name, dataSourceName);
            }

            reportingServicesClient.SetItemDataSources(new SetItemDataSourcesRequest { ItemPath = $"{targetFolder}/{reportName}", DataSources = dataSources });
        }

        private void CreateSubscriptions(string reportName, string reportPath, string sourceFolder)
        {
            var subscriptions = SubscriptionInfoFileLoader.GetSubscriptions(reportName, reportPath, sourceFolder).Where(x => x.Enabled).ToList();

            if (subscriptions.Count == 0)
            {
                Logging.Log("No enabled subscriptions found for report '{0}'", reportName);
            }
            else
            {
                foreach (var subscription in subscriptions)
                {
                    Logging.Log("Creating subscription '{0}'", subscription.Name);
                    reportingServicesClient.CreateSubscription(new CreateSubscriptionRequest { ItemPath = subscription.Path, ExtensionSettings = subscription.ExtensionSettings, Description = subscription.Description, EventType = subscription.EventType, MatchData = subscription.ScheduleXml, Parameters = subscription.Parameters });
                    Logging.Log("Created subscription '{0}'", subscription.Name);
                }
            }
        }

        #endregion Private Helper Methods
    }
}
