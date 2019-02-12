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
            var backupSources = Arguments.BackupSources.Select(x => new DirectoryInfo(x.Render())).ToList();
            var backupDestination = new FileInfo(Arguments.BackupDestination.Render());

            if (backupDestination.Exists)
            {
                if (Arguments.BackupOverwrite)
                {
                    backupDestination.Delete();
                }
                else
                {
                    throw new Exception($"File {backupDestination} already exists, to overwite, use the --backupOverite option");
                }
            }
            else if (backupDestination.Directory != null && !backupDestination.Directory.Exists)
            {
                backupDestination.Directory.Create();
            }

            if (backupSources.Count == 1)
            {
                Logging.Log($"Adding {backupSources[0]} to {backupDestination.FullName}");
                ZipFile.CreateFromDirectory(backupSources[0].FullName, backupDestination.FullName, CompressionLevel.Optimal, true);
                Logging.Log($"Added {backupSources[0]} to {backupDestination.FullName}");
            }
            else
            {
                using (var zipFile = ZipFile.Open(backupDestination.FullName, ZipArchiveMode.Create))
                {
                    foreach (var backupSource in backupSources)
                    {
                        Logging.Log($"Adding {backupSource} to {backupDestination.FullName}");

                        var files = backupSource.GetFiles("**", SearchOption.AllDirectories);
                        if (files.Length == 0)
                        {
                            zipFile.CreateEntry(backupSource.Name);
                        }
                        else
                        {
                            foreach (var file in files)
                            {
                                if (backupSource.Parent != null)
                                {
                                    zipFile.CreateEntryFromFile(file.FullName, file.FullName.Replace(backupSource.Parent.FullName, ""), CompressionLevel.Optimal);
                                }
                                else
                                {
                                    zipFile.CreateEntryFromFile(file.FullName, file.FullName.Replace(backupSource.FullName, ""), CompressionLevel.Optimal);
                                }
                            }
                        }

                        Logging.Log($"Added {backupSource} to {backupDestination.FullName}");
                    }
                }
            }
        }
    }
}
