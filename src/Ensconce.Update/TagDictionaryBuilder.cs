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
            if (fixedPath == null)
            {
                fixedPath = string.Empty;
            }

            return TagDictionaries.GetOrAdd(fixedPath, s => new Lazy<TagDictionary>(() => BuildTagDictionary(fixedPath)));
        }

        private static TagDictionary BuildTagDictionary(string fixedPath)
        {
            var tags = TagDictionaries.GetOrAdd(string.Empty, new Lazy<TagDictionary>(BuildTagDictionary));

            if (string.IsNullOrWhiteSpace(fixedPath))
            {
                return tags.Value;
            }

            fixedPath = fixedPath.RenderTemplate(tags);

            if (File.Exists(fixedPath))
            {
                return BuildTagDictionary(fixedPath, tags.Value);
            }

            Logging.Log("WARNING: The fixed structure path was not located at '{0}' so running without it!", fixedPath);
            return tags.Value;
        }

        private static TagDictionary BuildTagDictionary()
        {
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");
            Logging.Log("Building Tag Dictionary {0}", instanceName);
            var tags = Update.TagDictionary.FromIdentifier(instanceName);
            Logging.Log("Built Tag Dictionary {0}", instanceName);
            return tags;
        }

        private static TagDictionary BuildTagDictionary(string fixedPath, TagDictionary fallbackDictionary)
        {
            TagDictionary tags;

            if (File.Exists(fixedPath))
            {
                var instanceName = Environment.GetEnvironmentVariable("InstanceName");
                Logging.Log("Loading xml config from file {0}", Path.GetFullPath(fixedPath));
                var configXml = Retry.Do(() => File.ReadAllText(fixedPath), TimeSpan.FromSeconds(5));
                Logging.Log((fallbackDictionary != null ? "Re-" : "") + "Building Tag Dictionary (Using config file)");
                tags = Update.TagDictionary.FromXml(instanceName, configXml);
                Logging.Log((fallbackDictionary != null ? "Re-" : "") + "Built Tag Dictionary (Using config file)");
            }
            else
            {
                Logging.Log("No structure file found at: {0}", Path.GetFullPath(fixedPath));
                tags = fallbackDictionary ?? BuildTagDictionary();
            }

            return tags;
        }
    }
}
