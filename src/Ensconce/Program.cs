using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using FifteenBelow.Deployment;
using FifteenBelow.Deployment.ReportingServices;
using FifteenBelow.Deployment.Update;
using Mono.Options;
using SearchOption = System.IO.SearchOption;

namespace Ensconce
{
    internal static class Program
    {
        private static readonly DateTime started = DateTime.Now;
        private static bool readFromStdIn;
        private static string databaseName;
        private static string connectionString;
        private static string fixedPath = @"D:\FixedStructure\structure.xml";
        private static string substitutionPath = "substitutions.xml";
        private static string databaseRepository = "";
        private static readonly List<string> RawToDirectories = new List<string>();
        private static readonly Dictionary<string, string> ReportingServiceVariables = new Dictionary<string, string>();
        private static readonly List<string> DeployTo = new List<string>();
        private static readonly List<string> SubstitutedFiles = new List<string>();
        private static readonly List<string> DeletedFiles = new List<string>();
        private static readonly List<string> CopiedFiles = new List<string>();
        private static string deployFrom;
        private static bool copyTo;
        private static bool replace;
        private static bool updateConfig;
        private static string templateFilters;
        private static bool warnOnOneTimeScriptChanges;
        private static bool withTransaction = true;
        private static string roundhouseOutputPath = @"E:\RH\";
        private static bool quiet;
        private static bool dropDatabase;
        private static bool dropDatabaseConfirm;
        private static bool deployReports;
        private static bool deployReportingRole;
        private static readonly Lazy<TagDictionary> LazyTags = new Lazy<TagDictionary>(BuildTagDictionary);
        private const string GitIgnoreContents = "\r\n*.zip\r\n*.bak\r\n";

        private static int Main(string[] args)
        {
            try
            {
                MainLogic(args);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Something went wrong. :(");
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                return -1;
            }
        }

        private static void MainLogic(string[] args)
        {
            // Set up some environment variables required on 2k8 servers: as a bonus, ensure NGit doesn't find any actual users settings...
            Environment.SetEnvironmentVariable("HOMEDRIVE", Path.GetPathRoot(Directory.GetCurrentDirectory()));
            Environment.SetEnvironmentVariable("HOMEPATH", Directory.GetCurrentDirectory());

            SetUpAndParseOptions(args);

            Log("Arguments parsed");

            if (readFromStdIn)
            {
                using (var input = Console.In)
                {
                    Console.Out.Write(input.ReadToEnd().Render());
                }

                // No other operations can be performed when reading from stdin
                return;
            }

            if (deployReports || deployReportingRole)
            {
                RunReportingServices();
                // No other operations can be performed when deploying reports
                return;
            }

            if (updateConfig)
            {
                DefaultUpdate();
            }

            if (!string.IsNullOrEmpty(templateFilters))
            {
                var templateFiles = new DirectoryInfo(deployFrom).EnumerateFiles(templateFilters, SearchOption.AllDirectories);
                foreach (var templateFile in templateFiles)
                {
                    string template;
                    Encoding encoding;
                    using (var readStream = templateFile.OpenText())
                    {
                        encoding = readStream.CurrentEncoding;
                        template = readStream.ReadToEnd();
                    }
                    using (var writeStream = new StreamWriter(templateFile.FullName, false, encoding))
                    {
                        writeStream.Write(template.Render());
                    }
                }
            }

            if (!string.IsNullOrEmpty(connectionString) || !string.IsNullOrEmpty(databaseName))
            {
                SqlConnectionStringBuilder connStr = null;

                if (!string.IsNullOrEmpty(connectionString))
                {
                    connStr = new SqlConnectionStringBuilder(connectionString.RenderTemplate(LazyTags.Value));
                }
                else if (!string.IsNullOrEmpty(databaseName))
                {
                    connStr = Database.GetLocalConnectionStringFromDatabaseName(databaseName.RenderTemplate(LazyTags.Value));
                }
                Log("Deploying scripts from {0} using connection string {1}", deployFrom, connStr.ConnectionString);
                var database = new Database(connStr, new LegacyFolderStructure(), warnOnOneTimeScriptChanges);
                database.WithTransaction = withTransaction;
                database.OutputPath = roundhouseOutputPath;
                database.Deploy(deployFrom, databaseRepository.RenderTemplate(LazyTags.Value), dropDatabase);
            }

            if (replace)
            {
                DeployTo.ForEach(DeleteDirectory);
                Thread.Sleep(500); //Allow for the delete to complete.
            }

            if (copyTo || replace)
            {
                DeployTo.ForEach(dt => CopyDirectory(deployFrom, dt));
            }

            Log("Ensconce operation complete");
        }
        
        private static void SetUpAndParseOptions(string[] args)
        {
            var showHelp = false;
            var p = new OptionSet
                        {
                            {
                                "i|stdin",
                                "Read template string from StdIn",
                                s => readFromStdIn = s != null
                            },
                            {
                                "h|help",
                                "Show this message and exit",
                                s => showHelp = s != null
                            },
                            {
                                "fixedPath=",
                                @"Override path to structure.xml relative to executable (default=""D:\FixedStructure\structure.xml"")",
                                s => fixedPath = string.IsNullOrEmpty(s) ? fixedPath : s
                            },
                            {
                                "s|substitutionPath=",
                                "Path to substition file, relative to executable. (default=\"substitutions.xml\")",
                                s => substitutionPath = string.IsNullOrEmpty(s) ? substitutionPath : s
                            },
                            {
                                "d|databaseName="
                                ,"The name of the database to be deployed, assumes that the process is running on the destination server. Requires the deployFrom option. Can optionally provide the databaseRepository option.",
                                s => databaseName = s
                            },
                            {
                                "connectionString="
                                ,"The connection string for the database to be deployed, Requires the deployFrom option. Can optionally provide the databaseRepository option.",
                                s => connectionString = s
                            },
                            {
                                "databaseRepository="
                                ,"The entry to be made in the repository field in the RoundhousE version table. If not provided defaults to an empty string. NOTE! Ignored if databaseName is not provided.",
                                s => databaseRepository = string.IsNullOrEmpty(s) ? databaseRepository : s
                            },
                            {
                                "t|deployTo=",
                                "Path to deploy to. Required for the finalisePath and copyToPath options, multiple values can be specified.",
                                s => RawToDirectories.Add(s)
                            },
                            {
                                "f|deployFrom=",
                                "Path to deploy from. Required for the copyTo and databaseName options",
                                s => deployFrom = s
                            },
                            {
                                "c|copyTo",
                                "Add the contents of the deployFrom directory to the deployTo directories, replacing files with the same name",
                                s => copyTo = s != null
                            },
                            {
                                "r|replace",
                                "Replace the current contents of the deployTo directories",
                                s => replace = s != null
                            },
                            {
                                "u|updateConfig",
                                "Update config",
                                s => updateConfig = s != null
                            },
                            {
                                "q|quiet",
                                "Turn off logging output (default=False, but always True if -i set)",
                                s => quiet = s != null
                            },
                            {
                                "treatAsTemplateFilter=",
                                "File filter for files in the deploy from directory to treat as templates. These will be updated after config and before deployment.",
                                s => templateFilters = s
                            },
                            {
                                "warnOnOneTimeScriptChanges=",
                                "If one-time-scripts have had changes, only treat them as warnings, not as errors. Defaults to False.",
                                s => warnOnOneTimeScriptChanges = Convert.ToBoolean(s)
                            },
                            {
                                "withTransaction=",
                                "Execute RoundhousE in transactional mode. Defaults to True.",
                                s => withTransaction = Convert.ToBoolean(s)
                            },
                            {
                                "roundhouseOutputPath=",
                                @"Specify a directory for RoundhousE to store SQL files. Defaults to E:\RH\",
                                s => roundhouseOutputPath = s
                            },
                            {
                                "dropDatabase",
                                "Drop database, useful if you need to test installations on a fresh database or need control of databases for performance/load tests.",
                                s => dropDatabase = s != null
                            },
                            {
                                "dropDatabaseConfirm",
                                "Drop database Confirmation, used to confirm that database is to be dropped (for safety)",
                                s => dropDatabaseConfirm = s != null
                            },
                            {
                                "dr|deployReports",
                                "Deploy Reporting service reports. See reportVariable help for example usage.",
                                s =>  deployReports = s != null
                            },
                            {
                                "drr|deployReportingRole",
                                "Deploy Reporting service role for User. See reportVariable help for example usage.",
                                s => deployReportingRole = s != null
                            },
                            {
                                "rsv|reportVariable=",
                                DeployHelp.ExampleUsage,
                                s => {
                                    var reportingServiceVariables = s.Split(separator: new [] { '=' }, count: 2);
                                    ReportingServiceVariables.Add(reportingServiceVariables[0], reportingServiceVariables[1]);
                                }
                            }
                        };

            var envWarnOnOneTimeScriptChanges = Environment.GetEnvironmentVariable("WarnOnOneTimeScriptChanges");
            if (!string.IsNullOrEmpty(envWarnOnOneTimeScriptChanges))
            {
                // Will be overridden by command-line option
                warnOnOneTimeScriptChanges = Convert.ToBoolean(envWarnOnOneTimeScriptChanges);
            }

            var envFixedPath = Environment.GetEnvironmentVariable("FixedPath");
            if (!string.IsNullOrEmpty(envFixedPath))
            {
                // Will be overridden by command-line option
                fixedPath = envFixedPath;
            }

            var envRoundhouseOutputPath = Environment.GetEnvironmentVariable("RoundhouseOutputPath");
            if (!string.IsNullOrEmpty(envRoundhouseOutputPath))
            {
                // Will be overridden by command-line option
                roundhouseOutputPath = envRoundhouseOutputPath;
            }

            p.Parse(args);

            var filesToBeMovedOrChanged = updateConfig || copyTo || replace || !string.IsNullOrEmpty(templateFilters);
            var databaseOperation = !string.IsNullOrEmpty(databaseName) || !string.IsNullOrEmpty(connectionString);
            var reportOperation = deployReports || deployReportingRole;
            var operationRequested = filesToBeMovedOrChanged || databaseOperation || readFromStdIn || reportOperation;

            if (showHelp || !operationRequested)
            {
                ShowHelp(p);
                if (!showHelp) throw new OptionException("Invalid combination of options given, showing help.", "help");
            }

            if (RawToDirectories.Count > 0)
            {
                ProcessRawDirectories(RawToDirectories, DeployTo);
            }

            if (copyTo || replace)
            {
                if (DeployTo.Count == 0)
                {
                    throw new OptionException("Error: You must specify at least one deployTo directory to use the copyTo or replace options.", "deployTo");
                }
                if (!Directory.Exists(deployFrom))
                {
                    throw new OptionException(
                        String.Format("Error: You must specify an existing from directory to use the copyTo or replace options. Couldn't find directory: {0}", deployFrom), "deployFrom");
                }
                if (copyTo && replace)
                {
                    throw new OptionException("Error: You cannot specify both the replace and copyTo options.", "deployTo and deployFrom");
                }
            }

            if (!string.IsNullOrEmpty(templateFilters) && !Directory.Exists(deployFrom))
            {
                throw new OptionException(String.Format("Error: You cannot use filterTemplate without a valid from directory. Couldn't find directory: {0}", deployFrom), "deployFrom");
            }

            if ((!string.IsNullOrEmpty(databaseName) || !string.IsNullOrEmpty(connectionString)) && !Directory.Exists(deployFrom))
            {
                throw new OptionException(String.Format("Error: You cannot use databaseName without a valid from directory. Couldn't find directory: {0}", deployFrom), "deployFrom");
            }

            if ((!string.IsNullOrEmpty(databaseName) || !string.IsNullOrEmpty(connectionString)) && !File.Exists(Path.Combine(deployFrom, "_BuildInfo.txt")))
            {
                throw new FileNotFoundException("Error: You cannot deploy database without a valid version file. File must be named _BuildInfo.txt", "databaseName");
            }

            if (dropDatabase && !dropDatabaseConfirm)
            {
                throw new OptionException("Error: You cannot drop a database without specifying the drop database confirm argument", "dropDatabaseConfirm");
            }

            if (reportOperation)
            {
                if (!ReportingServiceVariables.Any())
                    throw new OptionException("Error: You cannot deploy any reports to a reporting service instance with no variables", "reportVariable");

            }
        }

        private static void ProcessRawDirectories(IEnumerable<string> rawNames, List<string> processedNameList)
        {
            foreach (var to in rawNames)
            {
                var tempList = to.Render().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                processedNameList.AddRange(tempList);
            }
        }

        private static void Log(string message, params object[] values)
        {
            if (quiet || readFromStdIn) return;
            Console.Write("+{0:mm\\:ss\\.ff} - ", DateTime.Now - started);
            Console.WriteLine(message, values);
        }

        private static void LogError(string message, params object[] values)
        {
            if (quiet || readFromStdIn) return;
            Console.Error.Write("+{0:mm\\:ss\\.ff} - ", DateTime.Now - started);
            Console.Error.WriteLine(message, values);
        }

        private static void CopyDirectory(string from, string to)
        {
            Log("Copying from {0} to {1}", from, to);

            try
            {
                if (from.EndsWith(@"\") == false) from = from + @"\";

                foreach (var file in Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories))
                {
                    var source = new FileInfo(file);
                    var destination = new FileInfo(Path.Combine(to, file.Substring(from.Length)));

                    Retry.Do(() => CheckDirectoryAndCopyFile(source, destination), TimeSpan.FromMilliseconds(500));

                    // Record copied files for later finalising
                    CopiedFiles.Add(destination.FullName);
                }
            }
            catch (Exception ex)
            {
                var i = 1;

                foreach (DictionaryEntry data in ex.Data)
                {
                    Log("Exception Data {0}: {1} = {2}", i++, data.Key, data.Value);
                }

                throw;
            }
        }

        private static void CheckDirectoryAndCopyFile(FileInfo sourceFileInfo, FileInfo destinationFileInfo)
        {
            if (destinationFileInfo.Directory != null && !destinationFileInfo.Directory.Exists)
            {
                destinationFileInfo.Directory.Create();
            }

            sourceFileInfo.CopyTo(destinationFileInfo.FullName, true);
        }

        public class ServiceDetails
        {
            public string DisplayName { get; set; }
            public string ServiceName { get; set; }
            public string PathName { get; set; }
        }

        public static void StopAndDeleteServicesInDirectory(string directory)
        {
            foreach (var serviceDetails in GetServicesInstalledInDirectory(directory))
            {
                var service = ServiceController.GetServices().First(svc => svc.DisplayName == serviceDetails.DisplayName);
                if (service.Status != ServiceControllerStatus.Stopped)
                {
                    Log("Stopping service {0}", serviceDetails.DisplayName);
                    service.Stop();
                }

                VerifyServiceStopped(serviceDetails.DisplayName, 30);

                Log("Uninstalling service {0}", serviceDetails.DisplayName);
                Process.Start("sc", string.Format("delete \"{0}\"", serviceDetails.ServiceName)).WaitForExit();

                VerifyServiceUninstall(serviceDetails.DisplayName, 30);
            }
        }

        private static void VerifyServiceStopped(string serviceName, int maxWait)
        {
            var waitAttempt = 0;
            while (waitAttempt < maxWait)
            {
                if (ServiceController.GetServices().First(svc => svc.DisplayName == serviceName).Status == ServiceControllerStatus.Stopped)
                {
                    break;
                }

                Log("Still waiting for service {0} to stop", serviceName);
                Thread.Sleep(1000);
                waitAttempt++;
            }

            if (waitAttempt >= maxWait)
            {
                LogError("Service {0} didn't stop in {1} seconds!", serviceName, maxWait);
            }
        }

        private static void VerifyServiceUninstall(string serviceName, int maxWait)
        {
            var waitAttempt = 0;
            while (waitAttempt < maxWait)
            {
                if (!ServiceController.GetServices().Any(svc => svc.DisplayName == serviceName))
                {
                    break;
                }

                Log("Still waiting for service {0} to be removed", serviceName);
                Thread.Sleep(1000);
                waitAttempt++;
            }

            if (waitAttempt >= maxWait)
            {
                LogError("Service {0} still installed after {1} seconds!", serviceName, maxWait);
            }
        }

        public static IEnumerable<ServiceDetails> GetServicesInstalledInDirectory(string directory)
        {
            if (!directory.EndsWith(@"\")) directory = directory + @"\";

            var wqlObjectQuery = new WqlObjectQuery("SELECT * FROM Win32_Service");
            var managementObjectSearcher = new ManagementObjectSearcher(wqlObjectQuery);
            var managementObjectCollection = managementObjectSearcher.Get();

            return managementObjectCollection
                .OfType<ManagementObject>()
                .Where(svc => svc.GetPropertyValue("PathName") != null)
                .Where(svc => svc.GetPropertyValue("PathName").ToString().Contains(directory))
                .Select(svc => new ServiceDetails
                {
                    PathName = svc.GetPropertyValue("PathName").ToString(),
                    DisplayName = svc.GetPropertyValue("DisplayName").ToString(),
                    ServiceName = svc.GetPropertyValue("Name").ToString()
                });
        }

        private static void StopProcessesInDirectory(string directory)
        {
            if (!directory.EndsWith(@"\")) directory = directory + @"\";

            Log("Stopping processes in directory: {0}", directory);
            var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";

            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"]
                            };
                foreach (var item in query)
                {
                    if (!string.IsNullOrEmpty(item.Path) && Path.GetFullPath(item.Path).Contains(new DirectoryInfo(directory).FullName))
                    {
                        item.Process.Kill();
                        item.Process.WaitForExit();
                    }
                }
            }
        }

        private static void DeleteDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;

            // Check for and uninstall services installed in directory
            StopAndDeleteServicesInDirectory(directory);

            // Try and kill processes we know about in the dir
            StopProcessesInDirectory(directory);

            foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                // Record deleted files for later finalising
                DeletedFiles.Add(file);
            }

            Log("Deleting from {0}", directory);
            PerformDelete(new DirectoryInfo(directory));
        }

        private static void RemoveSubRepositories(string directory)
        {
            var rootGitDirectory = new DirectoryInfo(Path.Combine(directory, ".git"));

            foreach (var gitFolder in Directory.EnumerateDirectories(directory, ".git", SearchOption.AllDirectories))
            {
                var dirInfo = new DirectoryInfo(gitFolder);

                if (dirInfo.FullName.Equals(rootGitDirectory.FullName, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    Log("Removing sub repository: {0}", gitFolder);
                    PerformDelete(dirInfo);
                }
            }
        }

        private static void PerformDelete(DirectoryInfo directory)
        {
            // Disable any read-only flags
            directory.Attributes = FileAttributes.Normal;

            foreach (var dir in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                dir.Attributes = FileAttributes.Normal;
            }

            foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                file.Attributes = FileAttributes.Normal;
            }

            // Delete directory tree
            Retry.Do(() => directory.Delete(true), TimeSpan.FromMilliseconds(1000));
        }

        private static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine("Configuration update console wrapper. See https://github.com/15below/Ensconce for details.");
            optionSet.WriteOptionDescriptions(Console.Out);
        }

        private static void DefaultUpdate()
        {
            Log("Updating config with substitution file {0}", substitutionPath);

            var updatedContents = UpdateFile.UpdateAll(substitutionPath, LazyTags.Value);

            foreach (var updatedContent in updatedContents)
            {
                using (var fs = new StreamWriter(updatedContent.Item1))
                {
                    fs.Write(updatedContent.Item2);

                    // Record substituted files for later finalising
                    SubstitutedFiles.Add(updatedContent.Item1);
                }
            }
        }

        private static string Render(this string s)
        {
            return s.RenderTemplate(LazyTags.Value);
        }

        private static TagDictionary BuildTagDictionary()
        {
            Log("Building Tag Dictionary");
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");
            var tags = new TagDictionary(instanceName);
            
            fixedPath = fixedPath.RenderTemplate(tags);
            if (File.Exists(fixedPath))
            {
                Log("Reloading tags using config file {0}", Path.GetFullPath(fixedPath));
                var configXml = Retry.Do(() => File.ReadAllText(fixedPath), TimeSpan.FromSeconds(5));
                Log("Re-Building Tag Dictionary (Using Config File)");
                tags = new TagDictionary(instanceName, configXml);
            }
            else
            {
                Log("No structure file found at: {0}", Path.GetFullPath(fixedPath));
            }
            
            return tags;
        }

        private static void RunReportingServices()
        {
            var reportingServicesUrl = GetReportingVariable("reportingServicesUrl");
            var networkDomain = GetReportingVariable("networkDomain");
            var networkLogin = GetReportingVariable("networkLogin");
            var networkPassword = GetReportingVariable("networkPassword");
            var msreports = new MsReportingServices(reportingServicesUrl, networkDomain, networkLogin, networkPassword);

            if (deployReportingRole)
                DeployReportingServiceRole(msreports);
            if (deployReports)
                PublishReports(msreports);

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
            if (!ReportingServiceVariables.ContainsKey(key))
                throw new KeyNotFoundException(string.Format("required key: {0} not found in supplied reporting service variables.", key));

            return ReportingServiceVariables[key];
        }
    }
}