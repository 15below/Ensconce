using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using FifteenBelow.Deployment;
using FifteenBelow.Deployment.Update;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.VisualBasic.FileIO;
using Mono.Options;
using NGit.Api;
using NGit.Dircache;
using Sharpen;
using SearchOption = System.IO.SearchOption;

namespace Ensconce
{
    internal static class Program
    {
        private static bool readFromStdIn;
        private static string configUrl = "{{ DeployService }}{{ ClientCode }}/{{ Environment }}";
        private static string databaseName;
        private static string connectionString;
        private static string fixedPath = "structure.xml";
        private static string substitutionPath = "substitutions.xml";
        private static bool finalisePath;
        private static string databaseRepository ="";
        private static readonly List<string> RawToDirectories = new List<string>();
        private static readonly List<string> DeployTo = new List<string>();
        private static string deployFrom;
        private static bool copyTo;
        private static bool replace;
        private static bool updateConfig;
        private static string templateFilters;
        private static bool quiet;
        private static readonly Lazy<TagDictionary> LazyTags = new Lazy<TagDictionary>(BuildTagDictionary);

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
            if (SetUpAndParseOptions(args)) return;

            if (readFromStdIn)
            {
                using (var input = Console.In)
                {
                    Console.Out.Write(input.ReadToEnd().Render());
                }
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
                    Encoding encoding = null;
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
                Log(string.Format(
                    "ConnectionString is {0}\ndbname is {1}\ndatabaseRepository is {2}\nDeployFrom is {3}",
                    connectionString, databaseName,
                    databaseRepository, deployFrom));

                Log(string.Format("Deploying {0}{1}", databaseName, connectionString));

                Database database=null;
                if (!string.IsNullOrEmpty(connectionString))
                {
                    database =new Database(new SqlConnectionStringBuilder(connectionString.RenderTemplate(LazyTags.Value)), new LegacyFolderStructure());
                }
                else if (!string.IsNullOrEmpty(databaseName))
                {
                    database = new Database(databaseName.RenderTemplate(LazyTags.Value),new LegacyFolderStructure());
                }

                if (database == null) throw new NullReferenceException("database");
                database.Deploy(deployFrom, databaseRepository.RenderTemplate(LazyTags.Value));
            }

            if (copyTo || replace)
            {
                DeployTo.ForEach(BackupDirectory);
            }

            if (replace)
            {
                DeployTo.ForEach(DeleteDirectory);
            }

            if (copyTo || replace)
            {
                DeployTo.ForEach(dt => CopyDirectory(deployFrom, dt));
            }

            if (finalisePath)
            {
                foreach (var deployDir in DeployTo)
                {
                    if (!Directory.Exists(deployDir))
                    {
                        throw new InvalidPathException(deployDir);
                    }

                    Finalise(deployDir);
                }
            }
        }

        private static bool SetUpAndParseOptions(string[] args)
        {
            var showHelp = false;
            var p = new OptionSet
                        {
                            {"i|stdin", "Read template string from StdIn", s => readFromStdIn = s != null},
                            {"h|help", "Show this message and exit", s => showHelp = s != null},
                            {
                                "w|webservice=",
                                @"NOTE! Ignored if env:/FixedStructure is true. Url of webservice to retrieve a structure.xml file from (can be tagged with environment variables, default=""{{ DeployService }}{{ ClientCode }}/{{ Environment }}"")",
                                s => configUrl = string.IsNullOrEmpty(s) ? configUrl : s
                                }, 
                            {
                                "fixedPath=",
                                "NOTE! Ignored if env:/FixedStructure is false. Override path to structure.xml relative to executable (default=\"structure.xml\")",
                                s => fixedPath = string.IsNullOrEmpty(s) ? fixedPath : s
                                },
                            {
                                "s|substitutionPath=",
                                "Path to substition file, relative to executable. (default=\"substitutions.xml\")",
                                s => substitutionPath = string.IsNullOrEmpty(s) ? substitutionPath : s
                                },
                            {
                                "x|finalisePath",
                                "NOTE! Will ignore all other options except webservice",
                                s => finalisePath = s != null
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
                                "Path to deploy to. Required for the finalisePath and copyToPath options, multiple values can be specified."
                                , s => RawToDirectories.Add(s)
                                },
                            {
                                "f|deployFrom=", "Path to deploy from. Required for the copyTo and databaseName options",
                                s => deployFrom = s
                                },
                            {
                                "c|copyTo",
                                "Add the contents of the deployFrom directory to the deployTo directories, replacing files with the same name"
                                , s => copyTo = s != null
                                },
                            {
                                "r|replace", "Replace the current contents of the deployTo directories",
                                s => replace = s != null
                                },
                            {
                                "u|updateConfig", "Update config", s => updateConfig = s != null
                                }, 
                            {
                                "q|quiet", "Turn off logging output (default=False, but always True if -i set)",
                                s => quiet = s != null
                                }, 
                            {
                                "treatAsTemplateFilter=", "File filter for files in the deploy from directory to treat as templates. These will be updated after config and before deployment.",
                                s => templateFilters = s
                               },
                        };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine("Try running again with the '--help' option.");
                return true;
            }

            if (showHelp || !(updateConfig || copyTo || finalisePath || readFromStdIn || !string.IsNullOrEmpty(templateFilters) || !string.IsNullOrEmpty(databaseName) || !string.IsNullOrEmpty(connectionString)))
            {
                ShowHelp(p);
                return true;
            }

            if (RawToDirectories.Count > 0)
            {
                ProcessRawDirectories(RawToDirectories, DeployTo);
            }

            if (copyTo || replace)
            {
                if (DeployTo.Count == 0)
                {
                    Console.WriteLine(
                        "Error: You must specify at least one deployTo directory to use the copyTo or replace options.");
                    return true;
                }
                if (!Directory.Exists(deployFrom))
                {
                    Console.WriteLine(
                        "Error: You must specify a existing from directory to use the copyTo or replace options.");
                    return true;
                }
                if (copyTo && replace)
                {
                    Console.WriteLine("Error: You cannot specify both the replace and copyTo options.");
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(templateFilters) && !Directory.Exists(deployFrom))
            {
                Console.WriteLine("Error: You cannot use filterTemplate without a valid from directory.");
                return true;
            }

            if ((!string.IsNullOrEmpty(databaseName)||!string.IsNullOrEmpty(connectionString)) && !Directory.Exists(deployFrom))
            {
                Console.WriteLine("Error: You cannot use databaseName without a valid from directory.");
                return true;
            }

            if ((!string.IsNullOrEmpty(databaseName) || !string.IsNullOrEmpty(connectionString)) && !File.Exists(Path.Combine(deployFrom, "_BuildInfo.txt")))
            {
                Console.WriteLine("Error: You cannot deploy database without a valid version file. File must be named _BuildInfo.txt");
                return true;
            }

            return false;
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
            Console.WriteLine(string.Format(message, values));
        }

        private static void CopyDirectory(string from, string to)
        {
            Log("Copying from {0} to {1}", from, to);
            new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory(from, to, true);
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
                Log("Uninstalling service {0}", serviceDetails.DisplayName);
                Process.Start("sc", string.Format("delete \"{0}\"", serviceDetails.ServiceName)).WaitForExit();
            }
        }

        public static IEnumerable<ServiceDetails> GetServicesInstalledInDirectory(string directory)
        {
            var wqlObjectQuery = new WqlObjectQuery("SELECT * FROM Win32_Service");
            var managementObjectSearcher = new ManagementObjectSearcher(wqlObjectQuery);
            var managementObjectCollection = managementObjectSearcher.Get();

            return managementObjectCollection
                .OfType<ManagementObject>()
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
                    if(!string.IsNullOrEmpty(item.Path) && Path.GetFullPath(item.Path).Contains(new DirectoryInfo(directory).FullName))
                    {
                        item.Process.Kill();
                    }
                }
            }

        }

        private static void DeleteDirectory(string dir)
        {
            // Check for and uninstall services installed in directory
            StopAndDeleteServicesInDirectory(dir);

            // Try and kill processes we know about in the dir
            StopProcessesInDirectory(dir);

            if (!Directory.Exists(dir)) return;
            Log("Deleting from {0}", dir);
            var directory = new DirectoryInfo(dir);
            TurnOffReadOnly(directory);
            new Microsoft.VisualBasic.Devices.Computer().FileSystem.DeleteDirectory(dir, DeleteDirectoryOption.DeleteAllContents);
        }

        private static void TurnOffReadOnly(DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles())
            {
                file.IsReadOnly = false;
            }
            foreach (var dir in directory.GetDirectories())
            {
                TurnOffReadOnly(dir);
            }
        }

        private static void Finalise(string directory)
        {
            Log("Finalising {0}", directory);
            var filePath = new FilePath(directory);
            Git repo;
            try
            {
                repo = Git.Open(filePath);
            }
            catch (Exception)
            {
                repo = Git.Init().SetBare(false).SetDirectory(filePath).Call();
            }
            repo.Add().AddFilepattern(".").Call();
            string message;
            try
            {
                message = string.Format("Package {{ PackageNameAndVersion }} has finalised directory {0}".Render(), directory);
            }
            catch (Exception)
            {
                message = string.Format("Unknown package has finalised directory {0}", directory);
            }
            repo.Commit().SetMessage(message).SetAuthor(
                "Ensconce",
                "Deployment@15below.com").Call();
            repo.GetRepository().Close();
        }

        private static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine("Configuration update console wrapper. See some github address for details.");
            optionSet.WriteOptionDescriptions(Console.Out);
        }

        private static void DefaultUpdate()
        {
            Log("Updating config with substitution file {0}", substitutionPath);
            var tags = LazyTags.Value;
            var updatedContents = UpdateFile.UpdateAll(substitutionPath, tags);
            foreach (var updatedContent in updatedContents)
            {
                using (var fs = new StreamWriter(updatedContent.Item1))
                {
                    fs.Write(updatedContent.Item2);
                }
            }
        }

        private static void BackupDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return;
            Log("Backing up {0}", dirPath);

            var dir = new DirectoryInfo(dirPath);
            var dirLength = dir.FullName.TrimEnd('\\', '/').Length + 1;
            var parent = Directory.GetParent(dirPath);
            var backupName = dir.Name + ".old.zip";
            using (var zipStream = new ZipOutputStream(File.Create(Path.Combine(parent.FullName, backupName))))
            {
                CompressDir(zipStream, dir, dirLength);
                zipStream.IsStreamOwner = true;
            }
        }

        private static void CompressDir(ZipOutputStream zipStream, DirectoryInfo dir, int dirLength)
        {
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var zipEntry = new ZipEntry(ZipEntry.CleanName(file.FullName.Substring(dirLength)));
                zipEntry.DateTime = file.LastWriteTime;
                zipEntry.Size = file.Length;
                zipStream.PutNextEntry(zipEntry);

                var buffer = new byte[4096];
                using (var streamReader = File.OpenRead(file.FullName))
                {
                    while (true)
                    {
                        var bytes = streamReader.Read(buffer, 0, buffer.Length);
                        if (bytes > 0)
                        {
                            zipStream.Write(buffer, 0, bytes);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                zipStream.CloseEntry();
            }
            foreach (var directory in dir.GetDirectories())
            {
                CompressDir(zipStream, directory, dirLength);
            }
        }

        private static string Render(this string s)
        {
            return s.RenderTemplate(LazyTags.Value);
        }

        private static TagDictionary BuildTagDictionary()
        {
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");
            var fixedStructure = Convert.ToBoolean(Environment.GetEnvironmentVariable("FixedStructure"));
            var tags = new TagDictionary(instanceName);
            var configXml = "";
            if (fixedStructure)
            {
                if (File.Exists(fixedPath))
                    configXml = File.ReadAllText(fixedPath);
            }
            else
            {
                var uri = new Uri(configUrl.RenderTemplate(tags));
                var request = System.Net.WebRequest.Create(uri);
                request.Method = "GET";
                var response = request.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    configXml = new StreamReader(stream).ReadToEnd();
                }
            }
            tags = new TagDictionary(instanceName, configXml);
            return tags;
        }
    }
}