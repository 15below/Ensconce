using roundhouse;
using roundhouse.databases;
using roundhouse.infrastructure.logging;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ensconce.Database
{
    public class Database : IDatabase
    {
        private readonly IDatabaseRestoreOptions databaseRestoreOptions;
        private readonly Logger logger;

        public string ConnectionString { get; protected set; }
        public bool WarnOnOneTimeScriptChanges { get; private set; }
        public bool WithTransaction { get; set; }
        public string OutputPath { get; set; }
        public Database(DbConnectionStringBuilder connectionStringBuilder, bool warnOnOneTimeScriptChanges = false)
            : this(connectionStringBuilder, null, null, warnOnOneTimeScriptChanges)
        {
        }

        public Database(DbConnectionStringBuilder connectionStringBuilder, IDatabaseRestoreOptions databaseRestoreOptions, Logger logger, bool warnOnOneTimeScriptChanges = false)
        {
            ConnectionString = connectionStringBuilder.ToString();
            this.databaseRestoreOptions = databaseRestoreOptions;
            this.logger = logger ?? new roundhouse.infrastructure.logging.custom.ConsoleLogger();
            WarnOnOneTimeScriptChanges = warnOnOneTimeScriptChanges;
            WithTransaction = true;
        }

        public virtual void Deploy()
        {
            Deploy(Directory.GetCurrentDirectory());
        }

        public void Deploy(string schemaScriptsFolder, string repository = "", bool dropDatabase = false, TimeSpan? commandTimeout = null)
        {
            if (commandTimeout == null)
            {
                commandTimeout = TimeSpan.FromSeconds(30);
            }

            if (schemaScriptsFolder == string.Empty)
            {
                schemaScriptsFolder = Assembly.GetExecutingAssembly().Directory();
            }

            if (!Directory.Exists(schemaScriptsFolder))
            {
                throw new DirectoryNotFoundException(
                    $"Database schema scripts folder {schemaScriptsFolder}\r\ndoes not exist");
            }

            var roundhouseMigrate = new Migrate();
            SetFolderNames(roundhouseMigrate);

            if (databaseRestoreOptions != null)
            {
                databaseRestoreOptions.SetRunRestoreOptions(roundhouseMigrate);
            }

            roundhouseMigrate.Set(x => x.ConnectionString = ConnectionString)
                .Set(x => x.SqlFilesDirectory = schemaScriptsFolder)
                .Set(x => x.VersionFile = Path.Combine(schemaScriptsFolder, "_BuildInfo.txt"))
                .Set(x => x.WithTransaction = WithTransaction)
                .Set(x => x.Silent = true)
                .Set(x => x.CommandTimeout = Convert.ToInt32(commandTimeout.Value.TotalSeconds))
                .Set(x =>
                {
                    if (!string.IsNullOrEmpty(OutputPath))
                    {
                        x.OutputPath = OutputPath;
                    }
                })
                .Set(x =>
                {
                    var createDatabaseCustomScript = Path.Combine(schemaScriptsFolder, "CreateDatabase.sql");
                    if (File.Exists(createDatabaseCustomScript))
                    {
                        x.CreateDatabaseCustomScript = createDatabaseCustomScript;
                    }
                })
                .Set(x => x.RecoveryMode = RecoveryMode.NoChange)
                .Set(x => x.RepositoryPath = repository)
                .Set(x => x.WarnOnOneTimeScriptChanges = WarnOnOneTimeScriptChanges)
                .Set(x => x.DisableTokenReplacement = true)
                .Set(x => x.Drop = dropDatabase)
                .Set(x => x.DisableOutput = true)
                .Set(x => x.DefaultEncoding = Encoding.Default)
                .SetCustomLogging(logger);

            if (databaseRestoreOptions != null)
            {
                roundhouseMigrate.RunRestore();
            }
            else
            {
                roundhouseMigrate.Run();
            }
        }

        private void SetFolderNames(Migrate roundhouseMigrate)
        {
            roundhouseMigrate
                .Set(x => x.RunAfterCreateDatabaseFolderName = "RunAfterCreateDatabase")
                .Set(x => x.RunAfterOtherAnyTimeScriptsFolderName = "RunAfterOtherAnyTimeScripts")
                .Set(x => x.FunctionsFolderName = "Functions")
                .Set(x => x.SprocsFolderName = "Sprocs")
                .Set(x => x.UpFolderName = "Up")
                .Set(x => x.IndexesFolderName = "Indexes")
                .Set(x => x.PermissionsFolderName = "Permissions");
        }

        public static SqlConnectionStringBuilder GetLocalConnectionStringFromDatabaseName(string database)
        {
            return new SqlConnectionStringBuilder($"Data Source=(local);Initial Catalog={database};Trusted_Connection=Yes");
        }

        public static SqlConnectionStringBuilder GetLocalConnectionStringFromDatabaseName(string database, string user, string password)
        {
            return new SqlConnectionStringBuilder($"Data Source=(local);Initial Catalog={database};User ID={user};Password={password}");
        }
    }
}
