using System;
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

        public void Dispose()
        {
            var roundhouseMigrate = new Migrate();
            roundhouseMigrate.Set(x => x.ConnectionString = GetConnectionString(this.databaseName));
            roundhouseMigrate.GetConfiguration().DoNotCreateDatabase = true;
            roundhouseMigrate.GetConfiguration().Drop = true;
            roundhouseMigrate.GetConfiguration().WithTransaction = false;
            roundhouseMigrate.Run();
        }
    }
}