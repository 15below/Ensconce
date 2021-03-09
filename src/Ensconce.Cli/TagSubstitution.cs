using Ensconce.Update;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ensconce.Cli
{
    internal static class TagSubstitution
    {
        internal static void DefaultUpdate()
        {
            Logging.Log("Updating config with substitution file {0}", Arguments.SubstitutionPath);

            UpdateFile.UpdateFiles(Arguments.SubstitutionPath, TextRendering.TagDictionary, Arguments.OutputFailureContext);
        }

        internal static void UpdateFiles()
        {
            Logging.Log("Updating template filter files");

            var files = new DirectoryInfo(Arguments.DeployFrom).GetFiles(Arguments.TemplateFilters, SearchOption.AllDirectories);

            var exceptions = new ConcurrentQueue<Exception>();

            Parallel.ForEach(files, file =>
            {
                try
                {
                    UpdateSingleFile(file);
                }
                catch (Exception e)
                {
                    exceptions.Enqueue(e);
                }
            });

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        private static void UpdateSingleFile(FileInfo templateFile)
        {
            Logging.Log($"Updating template file {templateFile.FullName}");

            string template;
            Encoding encoding;

            using (var readStream = templateFile.OpenText())
            {
                encoding = readStream.CurrentEncoding;
                template = readStream.ReadToEnd();
            }

            File.WriteAllText(templateFile.FullName, template.Render(), encoding);
        }
    }
}
