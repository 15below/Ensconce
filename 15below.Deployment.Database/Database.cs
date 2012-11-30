using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using roundhouse;
using roundhouse.databases;
using roundhouse.infrastructure.logging;

namespace FifteenBelow.Deployment
{
    public class Database : IDatabase
    {
        protected IDatabaseFolderStructure DatabaseFolderStructure;
        private readonly IDatabaseRestoreOptions databaseRestoreOptions;
        private Logger logger;

        public string ConnectionString { get; protected set; }
		public bool WarnOnOneTimeScriptChanges { get; private set; }

        public Database(DbConnectionStringBuilder connectionStringBuilder) : this(connectionStringBuilder, null)
        {
        }

		public Database(DbConnectionStringBuilder connectionStringBuilder, IDatabaseFolderStructure databaseFolderStructure, bool warnOnOneTimeScriptChanges = false)
			: this(connectionStringBuilder, databaseFolderStructure, null, null, warnOnOneTimeScriptChanges)
        {
        }

        public Database(DbConnectionStringBuilder connectionStringBuilder, IDatabaseFolderStructure databaseFolderStructure, IDatabaseRestoreOptions databaseRestoreOptions, Logger logger, bool warnOnOneTimeScriptChanges = false)
        {
            this.ConnectionString = connectionStringBuilder.ToString();
            this.DatabaseFolderStructure = databaseFolderStructure;
            this.databaseRestoreOptions = databaseRestoreOptions;
            this.logger = logger ?? new roundhouse.infrastructure.logging.custom.ConsoleLogger();
			this.WarnOnOneTimeScriptChanges = warnOnOneTimeScriptChanges;
        }

        public virtual void Deploy()
        {
            this.Deploy(Directory.GetCurrentDirectory());
        }

		public void Deploy(string schemaScriptsFolder, string repository = "")
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

            roundhouseMigrate.Set(x => x.ConnectionString = this.ConnectionString)
                .Set(x => x.SqlFilesDirectory = schemaScriptsFolder)
                .Set(x => x.VersionFile = Path.Combine(schemaScriptsFolder, "_BuildInfo.txt"))
                .Set(x => x.WithTransaction = true)
                .Set(x => x.Silent = true)
                .Set(x => x.RecoveryMode = RecoveryMode.NoChange)
                .Set(x => x.RepositoryPath = repository)
				.Set(x => x.WarnOnOneTimeScriptChanges = WarnOnOneTimeScriptChanges)
                .Set(x => x.DisableTokenReplacement = true)
                .SetCustomLogging(logger);

            if(databaseRestoreOptions!=null)
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
    }
}
