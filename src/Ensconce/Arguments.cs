using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FifteenBelow.Deployment.ReportingServices;
using Mono.Options;

namespace Ensconce
{
    internal static class Arguments
    {
        internal static bool ReadFromStdIn;
        internal static string DatabaseName;
        internal static string ConnectionString;
        internal static string FixedPath;
        internal static string SubstitutionPath;
        internal static string DatabaseRepository;
        internal static readonly Dictionary<string, string> ReportingServiceVariables = new Dictionary<string, string>();
        internal static readonly List<string> DeployTo = new List<string>();
        internal static string DeployFrom;
        internal static bool CopyTo;
        internal static bool Replace;
        internal static bool UpdateConfig;
        internal static string TemplateFilters;
        internal static bool WarnOnOneTimeScriptChanges;
        internal static bool WithTransaction;
        internal static string RoundhouseOutputPath;
        internal static bool Quiet;
        internal static bool DropDatabase;
        internal static bool DeployReports;
        internal static bool DeployReportingRole;

        private static bool showHelp;
        private static bool dropDatabaseConfirm;
        private static readonly List<string> RawToDirectories = new List<string>();

        internal static void SetUpAndParseOptions(string[] args)
        {
            SetDefaultValues();
            var optionSet = GetOptionSet();
            optionSet.Parse(args);
            ValidateArguments(optionSet);
        }

        private static void SetDefaultValues()
        {
            SubstitutionPath = "substitutions.xml";
            DatabaseRepository = "";
            WithTransaction = true;

            var envWarnOnOneTimeScriptChanges = Environment.GetEnvironmentVariable("WarnOnOneTimeScriptChanges");
            if (!string.IsNullOrEmpty(envWarnOnOneTimeScriptChanges))
            {
                // Will be overridden by command-line option
                WarnOnOneTimeScriptChanges = Convert.ToBoolean(envWarnOnOneTimeScriptChanges);
            }

            var envFixedPath = Environment.GetEnvironmentVariable("FixedPath");
            FixedPath = !string.IsNullOrEmpty(envFixedPath) ? envFixedPath : @"D:\FixedStructure\structure.xml";

            var envRoundhouseOutputPath = Environment.GetEnvironmentVariable("RoundhouseOutputPath");
            RoundhouseOutputPath = !string.IsNullOrEmpty(envRoundhouseOutputPath) ? envRoundhouseOutputPath : @"E:\RH\";
        }

        private static OptionSet GetOptionSet()
        {
            return new OptionSet
            {
                {
                    "i|stdin",
                    "Read template string from StdIn",
                    s => ReadFromStdIn = s != null
                },
                {
                    "h|help",
                    "Show this message and exit",
                    s => showHelp = s != null
                },
                {
                    "fixedPath=",
                    @"Override path to structure.xml relative to executable (default=""D:\FixedStructure\structure.xml"")",
                    s => FixedPath = string.IsNullOrEmpty(s) ? FixedPath : s
                },
                {
                    "s|substitutionPath=",
                    "Path to substition file, relative to executable. (default=\"substitutions.xml\")",
                    s => SubstitutionPath = string.IsNullOrEmpty(s) ? SubstitutionPath : s
                },
                {
                    "d|databaseName="
                    ,"The name of the database to be deployed, assumes that the process is running on the destination server. Requires the deployFrom option. Can optionally provide the databaseRepository option.",
                    s => DatabaseName = s
                },
                {
                    "connectionString="
                    ,"The connection string for the database to be deployed, Requires the deployFrom option. Can optionally provide the databaseRepository option.",
                    s => ConnectionString = s
                },
                {
                    "databaseRepository="
                    ,"The entry to be made in the repository field in the RoundhousE version table. If not provided defaults to an empty string. NOTE! Ignored if databaseName is not provided.",
                    s => DatabaseRepository = string.IsNullOrEmpty(s) ? DatabaseRepository : s
                },
                {
                    "t|deployTo=",
                    "Path to deploy to. Required for the finalisePath and copyToPath options, multiple values can be specified.",
                    s => RawToDirectories.Add(s)
                },
                {
                    "f|deployFrom=",
                    "Path to deploy from. Required for the copyTo and databaseName options",
                    s => DeployFrom = s
                },
                {
                    "c|copyTo",
                    "Add the contents of the deployFrom directory to the deployTo directories, replacing files with the same name",
                    s => CopyTo = s != null
                },
                {
                    "r|replace",
                    "Replace the current contents of the deployTo directories",
                    s => Replace = s != null
                },
                {
                    "u|updateConfig",
                    "Update config",
                    s => UpdateConfig = s != null
                },
                {
                    "q|quiet",
                    "Turn off logging output (default=False, but always True if -i set)",
                    s => Quiet = s != null
                },
                {
                    "treatAsTemplateFilter=",
                    "File filter for files in the deploy from directory to treat as templates. These will be updated after config and before deployment.",
                    s => TemplateFilters = s
                },
                {
                    "warnOnOneTimeScriptChanges=",
                    "If one-time-scripts have had changes, only treat them as warnings, not as errors. Defaults to False.",
                    s => WarnOnOneTimeScriptChanges = Convert.ToBoolean(s)
                },
                {
                    "withTransaction=",
                    "Execute RoundhousE in transactional mode. Defaults to True.",
                    s => WithTransaction = Convert.ToBoolean(s)
                },
                {
                    "roundhouseOutputPath=",
                    @"Specify a directory for RoundhousE to store SQL files. Defaults to E:\RH\",
                    s => RoundhouseOutputPath = s
                },
                {
                    "dropDatabase",
                    "Drop database, useful if you need to test installations on a fresh database or need control of databases for performance/load tests.",
                    s => DropDatabase = s != null
                },
                {
                    "dropDatabaseConfirm",
                    "Drop database Confirmation, used to confirm that database is to be dropped (for safety)",
                    s => dropDatabaseConfirm = s != null
                },
                {
                    "dr|deployReports",
                    "Deploy Reporting service reports. See reportVariable help for example usage.",
                    s =>  DeployReports = s != null
                },
                {
                    "drr|deployReportingRole",
                    "Deploy Reporting service role for User. See reportVariable help for example usage.",
                    s => DeployReportingRole = s != null
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
        }

        private static void ValidateArguments(OptionSet p)
        {
            var filesToBeMovedOrChanged = UpdateConfig || CopyTo || Replace || !string.IsNullOrEmpty(TemplateFilters);
            var databaseOperation = !string.IsNullOrEmpty(DatabaseName) || !string.IsNullOrEmpty(ConnectionString);
            var reportOperation = DeployReports || DeployReportingRole;
            var operationRequested = filesToBeMovedOrChanged || databaseOperation || ReadFromStdIn || reportOperation;

            if (showHelp || !operationRequested)
            {
                Console.WriteLine("Configuration update console wrapper. See https://github.com/15below/Ensconce for details.");
                p.WriteOptionDescriptions(Console.Out);
                if (!showHelp) throw new OptionException("Invalid combination of options given, showing help.", "help");
            }

            if (CopyTo || Replace)
            {
                if (RawToDirectories.Count == 0)
                {
                    throw new OptionException("Error: You must specify at least one deployTo directory to use the copyTo or replace options.", "deployTo");
                }

                if (!Directory.Exists(DeployFrom))
                {
                    throw new OptionException(string.Format("Error: You must specify an existing from directory to use the copyTo or replace options. Couldn't find directory: {0}", DeployFrom), "deployFrom");
                }

                if (CopyTo && Replace)
                {
                    throw new OptionException("Error: You cannot specify both the replace and copyTo options.", "deployTo and deployFrom");
                }

                foreach (var to in RawToDirectories)
                {
                    var tempList = to.Render().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    DeployTo.AddRange(tempList);
                }
            }

            if (!string.IsNullOrEmpty(TemplateFilters) && !Directory.Exists(DeployFrom))
            {
                throw new OptionException(string.Format("Error: You cannot use filterTemplate without a valid from directory. Couldn't find directory: {0}", DeployFrom), "deployFrom");
            }

            if ((!string.IsNullOrEmpty(DatabaseName) || !string.IsNullOrEmpty(ConnectionString)) && !Directory.Exists(DeployFrom))
            {
                throw new OptionException(string.Format("Error: You cannot use databaseName without a valid from directory. Couldn't find directory: {0}", DeployFrom), "deployFrom");
            }

            if ((!string.IsNullOrEmpty(DatabaseName) || !string.IsNullOrEmpty(ConnectionString)) && !File.Exists(Path.Combine(DeployFrom, "_BuildInfo.txt")))
            {
                throw new FileNotFoundException("Error: You cannot deploy database without a valid version file. File must be named _BuildInfo.txt", "databaseName");
            }

            if (DropDatabase && !dropDatabaseConfirm)
            {
                throw new OptionException("Error: You cannot drop a database without specifying the drop database confirm argument", "dropDatabaseConfirm");
            }

            if (reportOperation && !ReportingServiceVariables.Any())
            {
                throw new OptionException("Error: You cannot deploy any reports to a reporting service instance with no variables", "reportVariable");
            }
        }
    }
}