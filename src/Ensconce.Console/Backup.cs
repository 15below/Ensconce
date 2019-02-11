using System.IO;
using System.IO.Compression;

namespace Ensconce.Console
{
    internal static class Backup
    {
        internal static void DoBackup()
        {
            if (File.Exists(Arguments.BackupDestination) && Arguments.BackupOverwrite)
            {
                File.Delete(Arguments.BackupDestination);
            }

            ZipFile.CreateFromDirectory(Arguments.BackupSource, Arguments.BackupDestination, CompressionLevel.Optimal, false);
        }
    }
}
