using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using roundhouse;

namespace FifteenBelow.Deployment
{
    public class TemporaryDatabase : Database, IDisposable
    {
        public TemporaryDatabase() : base(GenerateDbName())
        {
        }

        public TemporaryDatabase(IDatabaseFolderStructure databaseFolderStructure)
            : base(GenerateDbName(), databaseFolderStructure)
        {
        }

        private static string GenerateDbName()
        {
            return string.Format("BUILD-INT-{0}", Guid.NewGuid().ToString());
        }

        public string ReadVersion()
        {
            const string sql = "select version from [RoundhousE].[Version]";

            using (var cnn = new SqlConnection(GetLocalConnectionStringFromDatabaseName(this.databaseName)))
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

            using (var cnn = new SqlConnection(GetLocalConnectionStringFromDatabaseName(this.databaseName)))
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
            using (var cnn = new SqlConnection(GetLocalConnectionStringFromDatabaseName("master")))
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
            using (var cnn = new SqlConnection(GetLocalConnectionStringFromDatabaseName("master")))
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

        public void Dispose()
        {
            var roundhouseMigrate = new Migrate();
            roundhouseMigrate.Set(x => x.ConnectionString = GetLocalConnectionStringFromDatabaseName(this.databaseName));
            roundhouseMigrate.GetConfiguration().DoNotCreateDatabase = true;
            roundhouseMigrate.GetConfiguration().Drop = true;
            roundhouseMigrate.GetConfiguration().WithTransaction = false;
            roundhouseMigrate.Run();
        }
    }
}