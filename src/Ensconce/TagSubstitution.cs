using System.IO;
using System.Text;
using Ensconce.Update;

namespace Ensconce
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

            var templateFiles = new DirectoryInfo(Arguments.DeployFrom).EnumerateFiles(Arguments.TemplateFilters, SearchOption.AllDirectories);

            foreach (var templateFile in templateFiles)
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
}