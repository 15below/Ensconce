using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Ensconce.Console
{
    internal static class Backup
    {
        internal static void DoBackup()
        {
            var backupSources = Arguments.BackupSources.Select(x => x.Render()).ToList();
            var backupDestination = Arguments.BackupDestination.Render();

            if (File.Exists(backupDestination))
            {
                if (Arguments.BackupOverwrite)
                {
                    File.Delete(backupDestination);
                }
                else
                {
                    throw new Exception($"File {backupDestination} already exists, to overwite, use the --backupOverite option");
                }
            }

            if (backupSources.Count == 1)
            {
                Logging.Log($"Adding {backupSources[0]} to {backupDestination}");
                ZipFile.CreateFromDirectory(backupSources[0], backupDestination, CompressionLevel.Optimal, true);
                Logging.Log($"Added {backupSources[0]} to {backupDestination}");
            }
            else
            {
                using (var zipFile = ZipFile.Open(backupDestination, ZipArchiveMode.Create))
                {
                    foreach (var backupSource in backupSources)
                    {
                        Logging.Log($"Adding {backupSource} to {backupDestination}");

                        var backupSourceDirectory = new DirectoryInfo(backupSource);
                        var files = backupSourceDirectory.GetFiles("**", SearchOption.AllDirectories);
                        if (files.Length == 0)
                        {
                            zipFile.CreateEntry(backupSourceDirectory.Name);
                        }
                        else
                        {
                            foreach (var file in files)
                            {
                                if (backupSourceDirectory.Parent != null)
                                {
                                    zipFile.CreateEntryFromFile(file.FullName, file.FullName.Replace(backupSourceDirectory.Parent.FullName, ""), CompressionLevel.Optimal);
                                }
                                else
                                {
                                    zipFile.CreateEntryFromFile(file.FullName, file.FullName.Replace(backupSourceDirectory.FullName, ""), CompressionLevel.Optimal);
                                }
                            }
                        }

                        Logging.Log($"Added {backupSource} to {backupDestination}");
                    }
                }
            }
        }
    }
}
