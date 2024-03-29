﻿using Ensconce.Helpers;
using System;
using System.Collections;
using System.IO;

namespace Ensconce.Cli
{
    internal static class FileInteraction
    {
        internal static void DeleteDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            Retry.Do(() =>
            {
                // Check for and uninstall services installed in directory
                ApplicationInteraction.StopAndDeleteServicesInDirectory(directory);

                // Try and kill processes we know about in the dir
                ApplicationInteraction.StopProcessesInDirectory(directory);
            }, TimeSpan.FromSeconds(2));

            Logging.Log("Deleting from {0}", directory);

            // Delete directory tree
            Retry.Do(retry =>
            {
                if (retry > 0)
                {
                    // Try and kill handles in the dir
                    ApplicationInteraction.ReleaseHandlesInDirectory(directory);
                }

                var directoryInfo = new DirectoryInfo(directory);

                // Disable any read-only flags
                directoryInfo.Attributes = FileAttributes.Normal;

                foreach (var dir in directoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories))
                {
                    dir.Attributes = FileAttributes.Normal;
                }

                foreach (var file in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    file.Attributes = FileAttributes.Normal;
                }

                Directory.Delete(directory, true);
            }, TimeSpan.FromSeconds(2));

            Logging.Log("Ensure Directory {0} has been deleted", directory);
            Retry.Do(retry =>
            {
                if (retry > 0)
                {
                    // Try and kill handles in the dir
                    ApplicationInteraction.ReleaseHandlesInDirectory(directory);
                }

                if (Directory.Exists(directory))
                {
                    throw new Exception("Directory still exists");
                }
            }, TimeSpan.FromSeconds(2));
        }

        internal static void CopyDirectory(string from, string to)
        {
            Logging.Log("Copying from {0} to {1}", from, to);

            try
            {
                if (from.EndsWith(@"\") == false)
                {
                    from += @"\";
                }

                foreach (var file in Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories))
                {
                    Retry.Do(() =>
                    {
                        var source = new FileInfo(file);
                        var destination = new FileInfo(Path.Combine(to, file.Substring(from.Length)));
                        if (destination.Directory != null && !destination.Directory.Exists)
                        {
                            destination.Directory.Create();
                        }

                        source.CopyTo(destination.FullName, true);
                    }, TimeSpan.FromMilliseconds(500));
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
    }
}
