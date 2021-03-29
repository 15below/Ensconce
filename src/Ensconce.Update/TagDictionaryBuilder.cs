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

            return TagDictionaries.GetOrAdd(fixedPath, s => new Lazy<TagDictionary>(() => BuildTagDictionary(fixedPath, tags)));
        }

        private static TagDictionary BuildTagDictionary()
        {
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");
            Logging.Log("Building Tag Dictionary {0}", instanceName);
            var tags = TagDictionary.FromIdentifier(instanceName);
            Logging.Log("Built Tag Dictionary {0}", instanceName);
            return tags;
        }

        private static TagDictionary BuildTagDictionary(string fixedPath, Lazy<TagDictionary> fallbackDictionary)
        {
            TagDictionary tags;

            var (fileExists, path) = GetFullPathIfExists(fixedPath);

            if (fileExists)
            {
                var instanceName = Environment.GetEnvironmentVariable("InstanceName");
                Logging.Log("Loading xml config from file {0}", path);
                var configXml = Retry.Do(() => File.ReadAllText(path), TimeSpan.FromSeconds(5));
                Logging.Log("Re-Building Tag Dictionary (Using config file '{0}')", path);
                tags = TagDictionary.FromXml(instanceName, configXml);
                Logging.Log("Re-Built Tag Dictionary (Using config file '{0}')", path);
            }
            else
            {
                Logging.Log("WARNING: No structure file found at: '{0}' using base dictionary", path);
                tags = fallbackDictionary.Value;
            }

            return tags;
        }

        private static (bool exists, string path) GetFullPathIfExists(string path)
        {
            string fullPath;
            try
            {
                Logging.Log("Getting full path for '{0}'", path);
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return (false, path);
            }

            try
            {
                Logging.Log("Checking if '{0}' exists", fullPath);
                return (File.Exists(fullPath), fullPath);
            }
            catch (Exception)
            {
                return (false, fullPath);
            }
        }
    }
}
