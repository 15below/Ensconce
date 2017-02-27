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

            var updatedContents = UpdateFile.UpdateAll(Arguments.SubstitutionPath, TextRendering.TagDictionary);

            foreach (var updatedContent in updatedContents)
            {
                using (var fs = new StreamWriter(updatedContent.Item1))
                {
                    fs.Write(updatedContent.Item2);
                }
            }
        }

        internal static void UpdateFiles()
        {
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
                using (var writeStream = new StreamWriter(templateFile.FullName, false, encoding))
                {
                    writeStream.Write(template.Render());
                }
            }
        }
    }
}