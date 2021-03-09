using Ensconce.Helpers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ensconce.Update
{
    public static class ProcessFiles
    {
        public static void UpdateFiles(string rootFolder, string filter, Lazy<TagDictionary> tagValues)
        {
            Logging.Log("Updating template filter files");

            var files = new DirectoryInfo(rootFolder).GetFiles(filter, SearchOption.AllDirectories);

            var exceptions = new ConcurrentQueue<Exception>();

            Parallel.ForEach(files, file =>
            {
                try
                {
                    UpdateSingleFile(file, tagValues);
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

        private static void UpdateSingleFile(FileInfo templateFile, Lazy<TagDictionary> tagValues)
        {
            Logging.Log($"Updating template file {templateFile.FullName}");

            string template;
            Encoding encoding;

            using (var readStream = templateFile.OpenText())
            {
                encoding = readStream.CurrentEncoding;
                template = readStream.ReadToEnd();
            }

            File.WriteAllText(templateFile.FullName, template.RenderTemplate(tagValues), encoding);
        }
    }
}
