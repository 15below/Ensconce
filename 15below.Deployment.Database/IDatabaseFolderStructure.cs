using roundhouse;

namespace FifteenBelow.Deployment
{
    public interface IDatabaseFolderStructure
    {
        void SetMigrateFolders(Migrate migrateSettings, string schemaScriptsFolder);
    }
}