using Ensconce.Update;

namespace Ensconce.Cli
{
    internal static class TextRendering
    {
        internal static string Render(this string s)
        {
            var tagDictionary = TagDictionaryBuilder.BuildLazy(Arguments.FixedPath);
            return s.RenderTemplate(tagDictionary);
        }
    }
}
