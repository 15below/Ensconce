using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using roundhouse;
using roundhouse.infrastructure.logging;

namespace FifteenBelow.Deployment
{
    public class TemporaryDatabase : IDatabase, IDisposable
    {
        private readonly Database database;
        private readonly string masterDatabaseConnectionString;
        public string DatabaseName { get; private set; }

        public TemporaryDatabase() : this(null)
        {
        }

        public TemporaryDatabase(IDatabaseRestoreOptions restoreOptions)  : this(restoreOptions, null)
        {
        }

        public TemporaryDatabase(IDatabaseRestoreOptions restoreOptions, Logger logger)
        {
            DatabaseName = GenerateDbName();
            database = new Database(Database.GetLocalConnectionStringFromDatabaseName(DatabaseName), new LegacyFolderStructure(), restoreOptions, logger);
            masterDatabaseConnectionString = Database.GetLocalConnectionStringFromDatabaseName("master").ConnectionString;
        }

        [Obsolete("Temporary database only used for local testing")]
        public TemporaryDatabase(string server, string user, string password)
        {
            DatabaseName = GenerateDbName();
            masterDatabaseConnectionString = new SqlConnectionStringBuilder()
                                                 {
                                                     DataSource = server,
                                                     InitialCatalog = DatabaseName,
                                                     UserID = user,
                                                     Password = password
                                                 }.ConnectionString;

            DbConnectionStringBuilder databaseConnectionString = new SqlConnectionStringBuilder()
                                                                     {
                                                                         DataSource = server,
                                                                         InitialCatalog = DatabaseName,
                                                                         UserID = user,
                                                                         Password = password
                                                                     };
            database = new Database(databaseConnectionString, new LegacyFolderStructure());
        }

        private static string GenerateDbName()
        {
            return string.Format("BUILD-INT-{0}", Guid.NewGuid().ToString());
        }

        public string ReadVersion()
        {
            const string sql = "select version from [RoundhousE].[Version]";

            using (var cnn = new SqlConnection(database.ConnectionString))
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

            using (var cnn = new SqlConnection(database.ConnectionString))
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
            using (var cnn = new SqlConnection(masterDatabaseConnectionString))
            {
                var sql = string.Format("SELECT name FROM master.dbo.sysdatabases WHERE ([name] = N'{0}')", DatabaseName);
                cnn.Open();
                using (var cmd = new SqlCommand(sql, cnn))
                {
                    return Convert.ToString(cmd.ExecuteScalar()) == DatabaseName;
                }
            }
        }

        public IEnumerable<Table> GetTables()
        {
            using (var cnn = new SqlConnection(database.ConnectionString))
            {
                var sql = "SELECT Table_Name from information_schema.tables WHERE table_type = 'base table'";
                cnn.Open();
                using (var cmd = new SqlCommand(sql, cnn))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        yield return new Table(Convert.ToString(reader.GetString(0)));
                    }
                }
            }
        }

        public void Dispose()
        {
            var roundhouseMigrate = new Migrate();
            roundhouseMigrate.Set(x => x.ConnectionString = database.ConnectionString);
            roundhouseMigrate.GetConfiguration().DoNotCreateDatabase = true;
            roundhouseMigrate.GetConfiguration().Drop = true;
            roundhouseMigrate.GetConfiguration().WithTransaction = false;
            roundhouseMigrate.Run();
        }

        public void Deploy(string schemaScriptsFolder = "", string repository = "", bool dropDatabase = false)
        {
            database.Deploy(schemaScriptsFolder, repository, dropDatabase);
        }
    }
}