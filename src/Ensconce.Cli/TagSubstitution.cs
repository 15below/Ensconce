using Ensconce.Helpers;
using Ensconce.Update;

namespace Ensconce.Cli
{
    internal static class TagSubstitution
    {
        internal static void DefaultUpdate()
        {
            var tagDictionary = TagDictionaryBuilder.BuildLazy(Arguments.FixedPath);
            ProcessSubstitution.Update(Arguments.SubstitutionPath, tagDictionary, Arguments.OutputFailureContext);
        }

        internal static void UpdateFiles()
        {
            var tagDictionary = TagDictionaryBuilder.BuildLazy(Arguments.FixedPath);
            ProcessFiles.UpdateFiles(Arguments.DeployFrom, Arguments.TemplateFilters, tagDictionary);
        }
    }
}
