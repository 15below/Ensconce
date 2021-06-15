using Ensconce.Update;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace Ensconce.Cli
{
    internal static class DictionaryExport
    {
        internal static void ExportTagDictionary()
        {
            var tagDictionary = TagDictionaryBuilder.BuildLazy(Arguments.FixedPath);
            var tagDictionaryJson = JsonConvert.SerializeObject(tagDictionary, Formatting.Indented);

            if (!string.IsNullOrWhiteSpace(Arguments.DictionarySavePath))
            {
                File.WriteAllText(Arguments.DictionarySavePath, tagDictionaryJson);
            }

            if (!string.IsNullOrWhiteSpace(Arguments.DictionaryPostUrl))
            {
                using (var cli = new WebClient { Headers = { [HttpRequestHeader.ContentType] = "application/json" } })
                {
                    cli.UploadString(Arguments.DictionaryPostUrl, tagDictionaryJson);
                }
            }
        }
    }
}
