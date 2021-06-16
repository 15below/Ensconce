using Ensconce.Helpers;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Ensconce.Update
{
    public static class TagDictionaryBuilder
    {
        private static readonly ConcurrentDictionary<string, Lazy<TagDictionary>> TagDictionaries = new ConcurrentDictionary<string, Lazy<TagDictionary>>();

        public static TagDictionary Build(string fixedPath)
        {
            return BuildLazy(fixedPath).Value;
        }

        public static Lazy<TagDictionary> BuildLazy(string fixedPath)
        {
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");

            if (!string.IsNullOrWhiteSpace(instanceName))
            {
                var tags = TagDictionaries.GetOrAdd(instanceName, new Lazy<TagDictionary>(() => BuildTagDictionary(instanceName)));

                var expandedPath = ExpandPath(fixedPath, tags);

                if (string.IsNullOrWhiteSpace(expandedPath))
                {
                    return tags;
                }

                return TagDictionaries.GetOrAdd($"{instanceName}_{fixedPath}", s => new Lazy<TagDictionary>(() => BuildTagDictionary(instanceName, fixedPath, tags)));
            }
            else
            {
                var tags = TagDictionaries.GetOrAdd(string.Empty, new Lazy<TagDictionary>(BuildTagDictionary));

                var expandedPath = ExpandPath(fixedPath, tags);

                if (string.IsNullOrWhiteSpace(expandedPath))
                {
                    return tags;
                }

                return TagDictionaries.GetOrAdd(fixedPath, s => new Lazy<TagDictionary>(() => BuildTagDictionary(fixedPath, tags)));
            }
        }

        private static string ExpandPath(string path, Lazy<TagDictionary> tags)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            if (path.Contains("{{") || path.Contains("{%"))
            {
                return path.RenderTemplate(tags);
            }

            return path;
        }

        private static TagDictionary BuildTagDictionary()
        {
            Logging.Log("Building tag dictionary from environment");
            var tags = TagDictionary.FromIdentifier(string.Empty);
            Logging.Log("Built tag dictionary from environment");
            return tags;
        }

        private static TagDictionary BuildTagDictionary(string instanceName)
        {
            Logging.Log("Building tag dictionary with instance '{0}' from environment", instanceName);
            var tags = TagDictionary.FromIdentifier(instanceName);
            Logging.Log("Built tag dictionary with instance '{0}' from environment", instanceName);
            return tags;
        }

        private static TagDictionary BuildTagDictionary(string fixedPath, Lazy<TagDictionary> fallbackDictionary)
        {
            ThrowIfFixedStructureFileMissing(fixedPath);

            Logging.Log("Loading xml config from file '{0}'", fixedPath);
            var configXml = Retry.Do(() => File.ReadAllText(fixedPath), TimeSpan.FromSeconds(3));
            Logging.Log("Building tag dictionary (using config file '{0}')", fixedPath);
            var tags = TagDictionary.FromXml(string.Empty, configXml);
            Logging.Log("Built tag dictionary (using config file '{0}')", fixedPath);

            return tags;
        }

        private static TagDictionary BuildTagDictionary(string instanceName, string fixedPath, Lazy<TagDictionary> fallbackDictionary)
        {
            ThrowIfFixedStructureFileMissing(fixedPath);

            Logging.Log("Loading xml config from file '{0}'", fixedPath);
            var configXml = Retry.Do(() => File.ReadAllText(fixedPath), TimeSpan.FromSeconds(3));
            Logging.Log("Building tag dictionary with instance '{0}' (using config file '{1}')", instanceName, fixedPath);
            var tags = TagDictionary.FromXml(instanceName, configXml);
            Logging.Log("Built tag dictionary with instance '{0}' (using config file '{1}')", instanceName, fixedPath);
            return tags;
        }

        private static void ThrowIfFixedStructureFileMissing(string path)
        {
            var fileExists = true;

            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    Logging.Log("Checking if '{0}' exists", path);
                    fileExists = File.Exists(path);
                }
                catch (Exception)
                {
                    fileExists = false;
                }
            }

            if (!fileExists)
            {
                throw new FileNotFoundException($"No structure file found at {path}");
            }
        }
    }
}
