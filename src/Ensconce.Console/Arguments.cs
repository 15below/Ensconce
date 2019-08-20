using Ensconce.ReportingServices;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ensconce.Console
{
    internal static class Arguments
    {
        internal static bool ReadFromStdIn { get; private set; }
        internal static string DatabaseName { get; private set; }
        internal static string ConnectionString { get; private set; }
        internal static string FixedPath { get; private set; }
        internal static string SubstitutionPath { get; private set; }
        internal static string DatabaseRepository { get; private set; }
        internal static TimeSpan DatabaseCommandTimeout { get; private set; }
        internal static Dictionary<string, string> ReportingServiceVariables { get; private set; } = new Dictionary<string, string>();
        internal static List<string> DeployTo { get; private set; } = new List<string>();
        internal static string DeployFrom { get; private set; }
        internal static bool CopyTo { get; private set; }
        internal static bool Replace { get; private set; }
        internal static bool UpdateConfig { get; private set; }
        internal static string TemplateFilters { get; private set; }
        internal static bool WarnOnOneTimeScriptChanges { get; private set; }
        internal static bool WithTransaction { get; private set; }
        internal static string RoundhouseOutputPath { get; private set; }
        internal static bool Quiet { get; private set; }
        internal static bool DropDatabase { get; private set; }
        internal static bool DeployReports { get; private set; }
        internal static bool DeployReportingRole { get; private set; }

        internal static string DictionaryPostUrl { get; private set; }
        internal static string DictionarySavePath { get; private set; }

        internal static List<string> BackupSources { get; private set; } = new List<string>();
        internal static string BackupDestination { get; private set; }
        internal static bool BackupOverwrite { get; private set; }

        internal static bool OutputFailureContext { get; private set; }

        private static bool showHelp;
        private static bool dropDatabaseConfirm;
        private static readonly List<string> RawToDirectories = new List<string>();
        private static readonly List<string> RawBackupSources = new List<string>();

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
            OutputFailureContext = false;

            var defaultDatabaseCommandTimeout = Environment.GetEnvironmentVariable("DatabaseCommandTimeout");
            DatabaseCommandTimeout = TimeSpan.FromSeconds(!string.IsNullOrWhiteSpace(defaultDatabaseCommandTimeout) ? Convert.ToInt32(defaultDatabaseCommandTimeout) : 30);

            var envWarnOnOneTimeScriptChanges = Environment.GetEnvironmentVariable("WarnOnOneTimeScriptChanges");
            if (!string.IsNullOrEmpty(envWarnOnOneTimeScriptChanges))
            {
                // Will be overridden by command-line option
                WarnOnOneTimeScriptChanges = Convert.ToBoolean(envWarnOnOneTimeScriptChanges);
            }

            var envFixedPath = Environment.GetEnvironmentVariable("FixedPath");
            FixedPath = !string.IsNullOrEmpty(envFixedPath) ? envFixedPath : string.Empty;

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
                    s => ReadFromStdIn = Logging.ReadFromStdIn = s != null
                },
                {
                    "h|help",
                    "Show this message and exit",
                    s => showHelp = s != null
                },
                {
                    "fixedPath=",
                    "Override path to structure.xml relative to executable (default=\"Env:\\FixedPath\")",
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
                    "databaseCommandTimeout=",
                    "Database Command Timeout period in seconds. If not provided defaults to a set value or 30s if not set.",
                    s => DatabaseCommandTimeout = string.IsNullOrWhiteSpace(s) ? DatabaseCommandTimeout : TimeSpan.FromSeconds(Convert.ToInt32(s))
                },
                {
                    "t|deployTo=",
                    "Path to deploy to. Required for the copyTo & replace option, multiple values can be specified.",
                    s => RawToDirectories.Add(s)
                },
                {
                    "f|deployFrom=",
                    "Path to deploy from. Required for the copyTo & replace and databaseName options",
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
                    "ofc|outputFailureContext",
                    "Output Failure Context",
                    s => OutputFailureContext = true
                },
                {
                    "q|quiet",
                    "Turn off logging output (default=False, but always True if -i set)",
                    s => Quiet = Logging.Quiet = s != null
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
                },
                {
                    "dictionaryPostUrl=",
                    @"Specify a url to post the tag directory to (as JSON)",
                    s => DictionaryPostUrl = s
                },
                {
                    "dictionarySavePath=",
                    @"Specify a file to save the tag directory to (as JSON)",
                    s => DictionarySavePath = s
                },
                {
                    "backupSource=",
                    @"Specify the source directory of the backup, multiple values can be specified.  Required for the backup option",
                    s => RawBackupSources.Add(s)
                },
                {
                    "backupDestination=",
                    @"Specify the destination file for the backup.  Required for the backup option",
                    s => BackupDestination = s
                },
                {
                    "bo|backupOverwrite",
                    @"Specify if the backup should overwrite an existing file",
                    s => BackupOverwrite = s != null
                },
            };
        }

        private static void ValidateArguments(OptionSet p)
        {
            var filesToBeMovedOrChanged = UpdateConfig || CopyTo || Replace || !string.IsNullOrEmpty(TemplateFilters);
            var databaseOperation = !string.IsNullOrEmpty(DatabaseName) || !string.IsNullOrEmpty(ConnectionString);
            var reportOperation = DeployReports || DeployReportingRole;
            var tagExportOperation = !string.IsNullOrEmpty(DictionaryPostUrl) || !string.IsNullOrEmpty(DictionarySavePath);
            var backupOperation = RawBackupSources.Any();

            var operationRequested = ReadFromStdIn || filesToBeMovedOrChanged || databaseOperation || reportOperation || tagExportOperation || backupOperation;

            if (showHelp || !operationRequested)
            {
                System.Console.WriteLine("Configuration update console wrapper. See https://github.com/15below/Ensconce for details.");
                p.WriteOptionDescriptions(System.Console.Out);
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

            if (tagExportOperation)
            {
                if (string.IsNullOrWhiteSpace(DictionaryPostUrl) && string.IsNullOrWhiteSpace(DictionarySavePath))
                {
                    throw new OptionException("Error: You must specify a dictionaryPostUrl or dictionarySavePath", "export");
                }

                if (ReadFromStdIn || filesToBeMovedOrChanged || databaseOperation || reportOperation || backupOperation)
                {
                    throw new OptionException("Error: You cannot export the dictionary to a URL along with other commands", "export");
                }
            }

            if (backupOperation)
            {
                foreach (var rawBackupSource in RawBackupSources)
                {
                    BackupSources.AddRange(rawBackupSource.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                }

                if (string.IsNullOrWhiteSpace(BackupDestination))
                {
                    throw new OptionException("Error: You must specify a backupDestination to perform the backup operation", "backupDestination");
                }

                if (ReadFromStdIn || filesToBeMovedOrChanged || databaseOperation || reportOperation || tagExportOperation)
                {
                    throw new OptionException("Error: You cannot perform a backup along with other commands", "backup");
                }
            }
        }
    }
}
