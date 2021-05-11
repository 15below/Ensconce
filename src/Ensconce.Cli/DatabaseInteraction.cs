using Ensconce.Database;
using Ensconce.Helpers;
using System.Data.SqlClient;

namespace Ensconce.Cli
{
    internal static class DatabaseInteraction
    {
        internal static void DoDeployment()
        {
            SqlConnectionStringBuilder connStr = null;

            if (!string.IsNullOrEmpty(Arguments.ConnectionString))
            {
                connStr = new SqlConnectionStringBuilder(Arguments.ConnectionString.Render());
            }
            else if (!string.IsNullOrEmpty(Arguments.DatabaseName))
            {
                connStr = Database.Database.GetLocalConnectionStringFromDatabaseName(Arguments.DatabaseName.Render());
            }

            Logging.Log("Deploying scripts from {0} using connection string {1}", Arguments.DeployFrom, RedactPassword(connStr));

            var database = new Database.Database(connStr, Arguments.WarnOnOneTimeScriptChanges)
            {
                WithTransaction = Arguments.WithTransaction,
                OutputPath = Arguments.RoundhouseOutputPath
            };

            database.Deploy(Arguments.DeployFrom, Arguments.DatabaseRepository.Render(), Arguments.DropDatabase, Arguments.DatabaseCommandTimeout);
        }

        private static SqlConnectionStringBuilder RedactPassword(SqlConnectionStringBuilder connStr)
        {
            return string.IsNullOrEmpty(connStr.Password) ?
                   connStr :
                   new SqlConnectionStringBuilder(connStr.ConnectionString)
                   {
                       Password = new string('*', connStr.Password.Length)
                   };
        }
    }
}
