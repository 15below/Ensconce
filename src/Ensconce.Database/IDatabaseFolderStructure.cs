using roundhouse;

namespace Ensconce.Database
{
    public interface IDatabaseFolderStructure
    {
        void SetMigrateFolders(Migrate migrateSettings, string schemaScriptsFolder);
    }
}
