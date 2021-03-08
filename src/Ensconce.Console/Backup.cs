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

            using (var zipFile = ZipFile.Open(backupDestination.FullName, ZipArchiveMode.Create))
            {
                foreach (var backupSource in backupSources)
                {
                    Logging.Log($"Adding {backupSource} to {backupDestination.FullName}");
                    var fileCount = 0;
                    var emptyDirCount = 0;
                    foreach (var fileSystemInfo in backupSource.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
                    {
                        var entryName = fileSystemInfo.FullName.Replace(":\\", "_\\");

                        switch (fileSystemInfo)
                        {
                            case FileInfo file:
                                fileCount++;
                                zipFile.CreateEntryFromFile(file.FullName, entryName, CompressionLevel.Fastest);
                                break;

                            case DirectoryInfo possiblyEmptyDir when IsDirEmpty(possiblyEmptyDir):
                                emptyDirCount++;
                                zipFile.CreateEntry($"{entryName}\\", CompressionLevel.Fastest);
                                break;
                        }
                    }

                    Logging.Log($"Added {backupSource} to {backupDestination.FullName} ({fileCount} files / {emptyDirCount} empty directories)");
                }
            }
        }

        private static bool IsDirEmpty(DirectoryInfo possiblyEmptyDir)
        {
            using (var enumerator = possiblyEmptyDir.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).GetEnumerator())
            {
                return !enumerator.MoveNext();
            }
        }
    }
}
