using Cake.Core.IO;
using Ensconce.Update;
using System.IO;

namespace Ensconce.Cake
{
    public static class EnsconceFileUpdateExtensions
    {
        public static void TextSubstitute(this IFile file, FilePath fixedStructureFile)
        {
            var tagDictionary = fixedStructureFile == null ? TagDictionaryBuilder.Build(string.Empty) : TagDictionaryBuilder.Build(fixedStructureFile.FullPath);
            Update.ProcessFiles.UpdateFile(new FileInfo(file.Path.FullPath), tagDictionary);
        }

        public static void TextSubstitute(this IDirectory directory, FilePath fixedStructureFile)
        {
            directory.TextSubstitute("*.*", fixedStructureFile);
        }

        public static void TextSubstitute(this IDirectory directory, string filter, FilePath fixedStructureFile)
        {
            var tagDictionary = fixedStructureFile == null ? TagDictionaryBuilder.Build(string.Empty) : TagDictionaryBuilder.Build(fixedStructureFile.FullPath);
            Update.ProcessFiles.UpdateFiles(directory.Path.FullPath, filter, tagDictionary);
        }
    }
}
