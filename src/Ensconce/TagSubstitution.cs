using System.IO;
using System.Text;
using FifteenBelow.Deployment.Update;

namespace Ensconce
{
    internal static class TagSubstitution
    {
        internal static void DefaultUpdate()
        {
            Logging.Log("Updating config with substitution file {0}", Arguments.SubstitutionPath);

            UpdateFile.UpdateFiles(Arguments.SubstitutionPath, TextRendering.TagDictionary);
        }

        internal static void UpdateFiles()
        {
            Logging.Log("Updating template filter files");

            var templateFiles = new DirectoryInfo(Arguments.DeployFrom).EnumerateFiles(Arguments.TemplateFilters, SearchOption.AllDirectories);

            foreach (var templateFile in templateFiles)
            {
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