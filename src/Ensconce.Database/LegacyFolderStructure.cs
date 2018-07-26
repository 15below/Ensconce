using System.IO;
using roundhouse;

namespace Ensconce
{
    public class LegacyFolderStructure : IDatabaseFolderStructure
    {
        public void SetMigrateFolders(Migrate migrateSettings, string schemaScriptsFolder)
        {
            migrateSettings.Set(x => x.SqlFilesDirectory = schemaScriptsFolder).Set(
                x => x.RunAfterCreateDatabaseFolderName = RunAfterCreateDatabaseFolderName(schemaScriptsFolder))
                .Set(x => x.IndexesFolderName = IndexesFolderName(schemaScriptsFolder))
                .Set(
                    x =>
                    x.RunAfterOtherAnyTimeScriptsFolderName =
                    RunAfterOtherAnyTimeScriptsFolderName(schemaScriptsFolder))
                .Set(x => x.SprocsFolderName = SprocsFolderName(schemaScriptsFolder))
                .Set(x => x.FunctionsFolderName = FunctionsFolderName(schemaScriptsFolder))
                .Set(x => x.ViewsFolderName = ViewsFolderName(schemaScriptsFolder))
                .Set(x => x.UpFolderName = UpFolderName(schemaScriptsFolder));
        }

        private string getFolderNameIfExistsOrDefault(string schemaScriptsFolder, string[] folderNames,
                                                      string defaultFolderName)
        {
            foreach (var folderName in folderNames)
            {
                if (Directory.Exists(Path.Combine(schemaScriptsFolder, folderName))) return folderName;
            }
            return defaultFolderName;
        }

        private string RunAfterCreateDatabaseFolderName(string schemaScriptsFolder)
        {
            return getFolderNameIfExistsOrDefault(schemaScriptsFolder,
                                                  new[] { @"Create Scripts", "Tables" }, "runAfterCreateDatabase");
        }

        private string FunctionsFolderName(string schemaScriptsFolder)
        {
            return getFolderNameIfExistsOrDefault(schemaScriptsFolder,
                                                  new[] { @"Programmability\Functions" }, "functions");
        }

        private string SprocsFolderName(string schemaScriptsFolder)
        {
            return getFolderNameIfExistsOrDefault(schemaScriptsFolder,
                                                  new[] { @"Programmability\Stored Procedures" }, "sprocs");
        }

        private string UpFolderName(string schemaScriptsFolder)
        {
            return getFolderNameIfExistsOrDefault(schemaScriptsFolder,
                                                  new[] { "Change Scripts", @"Programmability\Change Scripts" }, "Up"); 
        }

        private string ViewsFolderName(string schemaScriptsFolder)
        {
            return "Views";
        }

        private string RunAfterOtherAnyTimeScriptsFolderName(string schemaScriptsFolder)
        {
            return getFolderNameIfExistsOrDefault(schemaScriptsFolder,
                                                  new[] { "Base Data", @"Programmability\Base Data" }, "RunAfterOtherAnyTimeScripts");
        }

        private string IndexesFolderName(string schemaScriptsFolder)
        {
            return getFolderNameIfExistsOrDefault(schemaScriptsFolder,
                                                  new[] { "Referential", @"Programmability\Referential" }, "Indexes");
        }
    }
}