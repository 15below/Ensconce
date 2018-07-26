using roundhouse;

namespace Ensconce
{
    public interface IDatabaseFolderStructure
    {
        void SetMigrateFolders(Migrate migrateSettings, string schemaScriptsFolder);
    }
}