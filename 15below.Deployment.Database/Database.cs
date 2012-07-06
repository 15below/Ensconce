using System.Data.Common;
using System.IO;
using roundhouse;
using roundhouse.databases;

namespace FifteenBelow.Deployment
{
    public class Database
    {
        protected readonly string databaseName;
        protected IDatabaseFolderStructure databaseFolderStructure;

        public string ConnectionString { get; protected set; }

        public Database(string databaseName)
            : this(databaseName, null)
        {
        }

        public Database(string databaseName, IDatabaseFolderStructure databaseFolderStructure)
        {
            this.databaseName = databaseName;
            this.ConnectionString = GetLocalConnectionStringFromDatabaseName(this.databaseName);
            this.databaseFolderStructure = databaseFolderStructure;
        }

        public Database(DbConnectionStringBuilder connectionStringBuilder, IDatabaseFolderStructure databaseFolderStructure)
        {
            this.ConnectionString = connectionStringBuilder.ToString();
            this.databaseFolderStructure = databaseFolderStructure;
        }


        public virtual void Deploy()
        {
            this.Deploy(Directory.GetCurrentDirectory());
        }

        public void Deploy(string schemaScriptsFolder, string repository = "", string restoreFromPath = "")
        {
            if (!Directory.Exists(schemaScriptsFolder))
                throw new DirectoryNotFoundException(
                    string.Format(
                        "Database schema scripts folder {0}\r\ndoes not exist", schemaScriptsFolder));

            if (!string.IsNullOrWhiteSpace(restoreFromPath) && !File.Exists(restoreFromPath))
                throw new FileNotFoundException(string.Format("Restore Path {0}\r\ndoes not exist", restoreFromPath));
            
            var logger = new roundhouse.infrastructure.logging.custom.ConsoleLogger();

            var roundhouseMigrate = new Migrate();
            if (databaseFolderStructure != null) databaseFolderStructure.SetMigrateFolders(roundhouseMigrate, schemaScriptsFolder);

            roundhouseMigrate.Set(x => x.ConnectionString = this.ConnectionString)
                .Set(x => x.VersionFile = Path.Combine(schemaScriptsFolder, "_BuildInfo.txt"))
                .Set(x => x.WithTransaction = true)
                .Set(x => x.Silent = true)
                .Set(x => x.RecoveryMode=RecoveryMode.NoChange)
                .Set(x => x.RepositoryPath = repository)
                .Set(x => x.WarnOnOneTimeScriptChanges=true)
                .SetCustomLogging(logger);

            if(!string.IsNullOrWhiteSpace(restoreFromPath) )
            {
                string database = Path.GetFileNameWithoutExtension(restoreFromPath);
                roundhouseMigrate   .Set(x => x.RestoreFromPath = restoreFromPath)
                                    .Set(x => x.Restore = !string.IsNullOrWhiteSpace(restoreFromPath))
                                    .Set(x => x.RestoreCustomOptions = string.Format(", MOVE '{0}' TO '{1}{0}.mdf', MOVE '{0}_log' TO '{1}{0}_log.LDF'", database, @"c:\Temp"));
            }

            if (string.IsNullOrWhiteSpace(restoreFromPath))
            {
                roundhouseMigrate.Run();
            }
            else
            {
                roundhouseMigrate.RunRestore();    
            }
        }

        protected string GetLocalConnectionStringFromDatabaseName(string database)
        {
            return string.Format("Data Source=(local);Initial Catalog={0};Trusted_Connection=Yes", database);
        }
    }
}
