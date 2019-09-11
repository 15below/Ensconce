using System;
using System.Collections;
using System.IO;

namespace Ensconce.Console
{
    internal static class FileInteraction
    {
        internal static void DeleteDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;

            // Check for and uninstall services installed in directory
            ApplicationInteraction.StopAndDeleteServicesInDirectory(directory);

            // Try and kill processes we know about in the dir
            ApplicationInteraction.StopProcessesInDirectory(directory);

            Logging.Log("Deleting from {0}", directory);
            PerformDelete(new DirectoryInfo(directory));

            Logging.Log("Ensure Directory {0} has been deleted", directory);
            Retry.Do(() =>
            {
                if (Directory.Exists(directory))
                {
                    throw new Exception("Directory still exists");
                }
            }, TimeSpan.FromMilliseconds(1000));
        }

        private static void PerformDelete(DirectoryInfo directory)
        {
            // Disable any read-only flags
            directory.Attributes = FileAttributes.Normal;

            foreach (var dir in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                dir.Attributes = FileAttributes.Normal;
            }

            foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                file.Attributes = FileAttributes.Normal;
            }

            // Delete directory tree
            Retry.Do(() => directory.Delete(true), TimeSpan.FromMilliseconds(1000));
        }

        internal static void CopyDirectory(string from, string to)
        {
            Logging.Log("Copying from {0} to {1}", from, to);

            try
            {
                if (from.EndsWith(@"\") == false) from = from + @"\";

                foreach (var file in Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories))
                {
                    var source = new FileInfo(file);
                    var destination = new FileInfo(Path.Combine(to, file.Substring(from.Length)));

                    Retry.Do(() => CheckDirectoryAndCopyFile(source, destination), TimeSpan.FromMilliseconds(500));
                }
            }
            catch (Exception ex)
            {
                var i = 1;

                foreach (DictionaryEntry data in ex.Data)
                {
                    Logging.Log("Exception Data {0}: {1} = {2}", i++, data.Key, data.Value);
                }

                throw;
            }
        }

        private static void CheckDirectoryAndCopyFile(FileInfo sourceFileInfo, FileInfo destinationFileInfo)
        {
            if (destinationFileInfo.Directory != null && !destinationFileInfo.Directory.Exists)
            {
                destinationFileInfo.Directory.Create();
            }

            sourceFileInfo.CopyTo(destinationFileInfo.FullName, true);
        }
    }
}
