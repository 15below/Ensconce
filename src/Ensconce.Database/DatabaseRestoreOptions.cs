using roundhouse;
using System.IO;

namespace Ensconce
{
    public class DatabaseRestoreOptions : IDatabaseRestoreOptions
    {
        private readonly string restorePath;

        public DatabaseRestoreOptions(string restorePath)
        {
            this.restorePath = restorePath;
        }

        public void SetRunRestoreOptions(Migrate migrateSettings)
        {
            if (!string.IsNullOrWhiteSpace(restorePath) && !File.Exists(restorePath))
                throw new FileNotFoundException(string.Format("Restore Path {0}\r\ndoes not exist", restorePath));

            if (!string.IsNullOrWhiteSpace(restorePath))
            {
                var database = Path.GetFileNameWithoutExtension(restorePath);
                migrateSettings.Set(x => x.RestoreFromPath = restorePath)
                    .Set(x => x.Restore = !string.IsNullOrWhiteSpace(restorePath))
                    .Set(x => x.RestoreCustomOptions = string.Format(@", MOVE '{0}' TO '{1}\{0}.mdf', MOVE '{0}_log' TO '{1}\{0}_log.LDF'", database, @"c:\Temp"));
            }
        }
    }
}
