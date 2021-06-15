using Cake.Core.IO;
using Ensconce.Update;
using System.IO;

namespace Ensconce.Cake
{
    public static class EnsconceFileUpdateExtensions
    {
        public static void TextSubstitute(this IFile file, FilePath fixedStructureFile)
        {
            file.Path.TextSubstitute(fixedStructureFile);
        }

        public static void TextSubstitute(this IDirectory directory, FilePath fixedStructureFile)
        {
            directory.Path.TextSubstitute(fixedStructureFile);
        }

        public static void TextSubstitute(this IDirectory directory, string filter, FilePath fixedStructureFile)
        {
            directory.Path.TextSubstitute(filter, fixedStructureFile);
        }

        public static void TextSubstitute(this FilePath file, FilePath fixedStructureFile)
        {
            var tagDictionary = fixedStructureFile == null ? TagDictionaryBuilder.BuildLazy(string.Empty) : TagDictionaryBuilder.BuildLazy(fixedStructureFile.FullPath);
            Update.ProcessFiles.UpdateFile(new FileInfo(file.FullPath), tagDictionary);
        }

        public static void TextSubstitute(this DirectoryPath directory, FilePath fixedStructureFile)
        {
            directory.TextSubstitute("*.*", fixedStructureFile);
        }

        public static void TextSubstitute(this DirectoryPath directory, string filter, FilePath fixedStructureFile)
        {
            var tagDictionary = fixedStructureFile == null ? TagDictionaryBuilder.BuildLazy(string.Empty) : TagDictionaryBuilder.BuildLazy(fixedStructureFile.FullPath);
            Update.ProcessFiles.UpdateFiles(directory.FullPath, filter, tagDictionary);
        }
    }
}
