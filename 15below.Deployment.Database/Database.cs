using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using roundhouse;
using roundhouse.databases;

namespace FifteenBelow.Deployment
{
    public class Database
    {
        protected readonly string databaseName;
        protected readonly string connectionString;
        protected IDatabaseFolderStructure databaseFolderStructure;

        public Database(string databaseName)
            : this(databaseName, null)
        {
        }

        public Database(string databaseName, IDatabaseFolderStructure databaseFolderStructure)
        {
            this.databaseName = databaseName;
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

            roundhouseMigrate.Set(x => x.ConnectionString = GetConnectionString(this.databaseName))
                .Set(x => x.VersionFile = Path.GetFullPath(Assembly.GetExecutingAssembly().Location))
                .Set(x => x.WithTransaction = true)
                .Set(x => x.Silent = true)
                .Set(x => x.RecoveryMode=RecoveryMode.NoChange)
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

        protected string GetConnectionString(string database)
        {
            return string.Format("Data Source=(local);Initial Catalog={0};Trusted_Connection=Yes", database);
        }

        public string ReadVersion()
        {
            const string sql = "select version from [RoundhousE].[Version]";

            using (var cnn = new SqlConnection(GetConnectionString(this.databaseName)))
            {
                cnn.Open();
                using (var cmd = new SqlCommand(sql, cnn))
                {
                    return Convert.ToString(cmd.ExecuteScalar());
                }
            }
        }

        public string ReadRepository()
        {
            const string sql = "select top 1 repository_path from [RoundhousE].[Version] order by Id desc";

            using (var cnn = new SqlConnection(GetConnectionString(this.databaseName)))
            {
                cnn.Open();
                using (var cmd = new SqlCommand(sql, cnn))
                {
                    return Convert.ToString(cmd.ExecuteScalar());
                }
            }
        }

        public bool Exists()
        {
            using (var cnn = new SqlConnection(GetConnectionString("master")))
            {
                string sql = string.Format("SELECT name FROM master.dbo.sysdatabases WHERE ([name] = N'{0}')", this.databaseName);
                cnn.Open();
                using (var cmd = new SqlCommand(sql, cnn))
                {
                    return Convert.ToString(cmd.ExecuteScalar()) == this.databaseName;
                }
            }
        }

        public IEnumerable<Table> GetTables()
        {
            using (var cnn = new SqlConnection(GetConnectionString("master")))
            {
                string sql = "SELECT Table_Name from information_schema.tables WHERE table_type = 'base table'";
                cnn.Open();
                using (var cmd = new SqlCommand(sql, cnn))
                {
                    //return Convert.ToString(cmd.ExecuteScalar()) == this.databaseName;
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        yield return new Table(Convert.ToString(reader.GetString(0)));
                    }
                }
            }
        }
    }
}
