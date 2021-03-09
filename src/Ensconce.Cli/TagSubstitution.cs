using Ensconce.Helpers;
using Ensconce.Update;

namespace Ensconce.Cli
{
    internal static class TagSubstitution
    {
        internal static void DefaultUpdate()
        {
            Logging.Log("Updating config with substitution file {0}", Arguments.SubstitutionPath);
            var tagDictionary = TagDictionaryBuilder.Build(Arguments.FixedPath);
            ProcessSubstitution.Update(Arguments.SubstitutionPath, tagDictionary, Arguments.OutputFailureContext);
        }

        internal static void UpdateFiles()
        {
            Logging.Log("Updating template filter files");
            var tagDictionary = TagDictionaryBuilder.Build(Arguments.FixedPath);
            ProcessFiles.UpdateFiles(Arguments.DeployFrom, Arguments.TemplateFilters, tagDictionary);
        }
    }
}
