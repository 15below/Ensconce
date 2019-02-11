using System.IO;
using System.IO.Compression;

namespace Ensconce.Console
{
    internal static class Backup
    {
        internal static void DoBackup()
        {
            var backupSource = Arguments.BackupSource.Render();
            var backupDestination = Arguments.BackupDestination.Render();

            if (File.Exists(backupDestination) && Arguments.BackupOverwrite)
            {
                File.Delete(backupDestination);
            }

            ZipFile.CreateFromDirectory(backupSource, backupDestination, CompressionLevel.Optimal, false);
        }
    }
}
