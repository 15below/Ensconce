using roundhouse;
using roundhouse.databases;
using roundhouse.infrastructure.logging;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ensconce
{
    public class Database : IDatabase
    {
        protected IDatabaseFolderStructure DatabaseFolderStructure;
        private readonly IDatabaseRestoreOptions databaseRestoreOptions;
        private readonly Logger logger;

        public string ConnectionString { get; protected set; }
        public bool WarnOnOneTimeScriptChanges { get; private set; }
        public bool WithTransaction { get; set; }
        public string OutputPath { get; set; }

        public Database(DbConnectionStringBuilder connectionStringBuilder)
            : this(connectionStringBuilder, null)
        {
        }

        public Database(DbConnectionStringBuilder connectionStringBuilder, IDatabaseFolderStructure databaseFolderStructure, bool warnOnOneTimeScriptChanges = false)
            : this(connectionStringBuilder, databaseFolderStructure, null, null, warnOnOneTimeScriptChanges)
        {
        }

        public Database(DbConnectionStringBuilder connectionStringBuilder, IDatabaseFolderStructure databaseFolderStructure, IDatabaseRestoreOptions databaseRestoreOptions, Logger logger, bool warnOnOneTimeScriptChanges = false)
        {
            ConnectionString = connectionStringBuilder.ToString();
            DatabaseFolderStructure = databaseFolderStructure;
            this.databaseRestoreOptions = databaseRestoreOptions;
            this.logger = logger ?? new roundhouse.infrastructure.logging.custom.ConsoleLogger();
            WarnOnOneTimeScriptChanges = warnOnOneTimeScriptChanges;
            WithTransaction = true;
        }

        public virtual void Deploy()
        {
            Deploy(Directory.GetCurrentDirectory());
        }

        public void Deploy(string schemaScriptsFolder, string repository = "", bool dropDatabase = false, int commandTimeSpan = 30)
        {
            if (schemaScriptsFolder == string.Empty)
                schemaScriptsFolder = Assembly.GetExecutingAssembly().Directory();

            if (!Directory.Exists(schemaScriptsFolder))
                throw new DirectoryNotFoundException(
                    string.Format(
                        "Database schema scripts folder {0}\r\ndoes not exist", schemaScriptsFolder));

            var roundhouseMigrate = new Migrate();
            if (DatabaseFolderStructure != null) DatabaseFolderStructure.SetMigrateFolders(roundhouseMigrate, schemaScriptsFolder);
            if (databaseRestoreOptions != null) databaseRestoreOptions.SetRunRestoreOptions(roundhouseMigrate);

            roundhouseMigrate.Set(x => x.ConnectionString = ConnectionString)
                .Set(x => x.SqlFilesDirectory = schemaScriptsFolder)
                .Set(x => x.VersionFile = Path.Combine(schemaScriptsFolder, "_BuildInfo.txt"))
                .Set(x => x.WithTransaction = WithTransaction)
                .Set(x => x.Silent = true)
                .Set(x => x.CommandTimeout = commandTimeSpan)
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

        public static SqlConnectionStringBuilder GetLocalConnectionStringFromDatabaseName(string database)
        {
            return new SqlConnectionStringBuilder(string.Format("Data Source=(local);Initial Catalog={0};Trusted_Connection=Yes", database));
        }

        public static SqlConnectionStringBuilder GetLocalConnectionStringFromDatabaseName(string database, string user, string password)
        {
            return new SqlConnectionStringBuilder(string.Format("Data Source=(local);Initial Catalog={0};User ID={1};Password={2}", database, user, password));
        }
    }
}
