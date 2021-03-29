using Ensconce.Helpers;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Ensconce.Update
{
    public static class TagDictionaryBuilder
    {
        private static readonly ConcurrentDictionary<string, Lazy<TagDictionary>> TagDictionaries = new ConcurrentDictionary<string, Lazy<TagDictionary>>();

        public static Lazy<TagDictionary> Build(string fixedPath)
        {
            var tags = TagDictionaries.GetOrAdd(string.Empty, new Lazy<TagDictionary>(BuildTagDictionary));

            if (string.IsNullOrWhiteSpace(fixedPath))
            {
                return tags;
            }

            fixedPath = fixedPath.RenderTemplate(tags);

            return TagDictionaries.GetOrAdd(fixedPath, s => new Lazy<TagDictionary>(() => BuildTagDictionary(fixedPath, tags.Value)));
        }

        private static TagDictionary BuildTagDictionary()
        {
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");
            Logging.Log("Building Tag Dictionary {0}", instanceName);
            var tags = TagDictionary.FromIdentifier(instanceName);
            Logging.Log("Built Tag Dictionary {0}", instanceName);
            return tags;
        }

        private static TagDictionary BuildTagDictionary(string fixedPath, TagDictionary fallbackDictionary)
        {
            TagDictionary tags;
            var path = Path.GetFullPath(fixedPath);

            if (FileExists(path))
            {
                var instanceName = Environment.GetEnvironmentVariable("InstanceName");
                Logging.Log("Loading xml config from file {0}", path);
                var configXml = Retry.Do(() => File.ReadAllText(path), TimeSpan.FromSeconds(5));
                Logging.Log("Re-Building Tag Dictionary (Using config file '{0}')", path);
                tags = TagDictionary.FromXml(instanceName, configXml);
                Logging.Log("Re-Built Tag Dictionary (Using config file '{0}')", path);
            }
            else if (fallbackDictionary != null)
            {
                Logging.Log("WARNING: No structure file found at: {0}", path);
                tags = fallbackDictionary;
            }
            else
            {
                Logging.Log("WARNING: No structure file found at: {0} & no fallback", path);
                tags = TagDictionary.Empty();
            }

            return tags;
        }

        private static bool FileExists(string path)
        {
            try
            {
                Logging.Log("Checking if {0} exists", path);
                return File.Exists(path);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
