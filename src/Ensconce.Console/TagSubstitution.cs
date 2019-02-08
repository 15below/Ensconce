using Ensconce.Update;
using System.IO;
using System.Linq;
using System.Text;

namespace Ensconce.Console
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

            new DirectoryInfo(Arguments.DeployFrom)
                .EnumerateFiles(Arguments.TemplateFilters, SearchOption.AllDirectories)
                .AsParallel()
                .WithDegreeOfParallelism(4)
                .Select(x =>
                {
                    UpdateSingleFile(x);
                    return x;
                })
                .ToList();
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
