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
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");

            if (!string.IsNullOrWhiteSpace(instanceName))
            {
                var tags = TagDictionaries.GetOrAdd(instanceName, new Lazy<TagDictionary>(BuildTagDictionary));

                if (string.IsNullOrWhiteSpace(fixedPath))
                {
                    return tags;
                }

                fixedPath = fixedPath.RenderTemplate(tags);

                return TagDictionaries.GetOrAdd($"{instanceName}_{fixedPath}", s => new Lazy<TagDictionary>(() => BuildTagDictionary(fixedPath, tags)));
            }
            else
            {
                var tags = TagDictionaries.GetOrAdd(string.Empty, new Lazy<TagDictionary>(BuildTagDictionary));

                if (string.IsNullOrWhiteSpace(fixedPath))
                {
                    return tags;
                }

                fixedPath = fixedPath.RenderTemplate(tags);

                return TagDictionaries.GetOrAdd(fixedPath, s => new Lazy<TagDictionary>(() => BuildTagDictionary(fixedPath, tags)));
            }
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
            TagDictionary tags;

            var (fileExists, path) = GetFullPathIfExists(fixedPath);

            if (fileExists)
            {
                Logging.Log("Loading xml config from file '{0}'", path);
                var configXml = Retry.Do(() => File.ReadAllText(path), TimeSpan.FromSeconds(5));
                Logging.Log("Building tag dictionary (using config file '{0}')", path);
                tags = TagDictionary.FromXml(string.Empty, configXml);
                Logging.Log("Built tag dictionary (using config file '{0}')", path);
            }
            else
            {
                Logging.Log("WARNING: No structure file found at: '{0}'", path);
                tags = fallbackDictionary.Value;
            }

            return tags;
        }

        private static TagDictionary BuildTagDictionary(string instanceName, string fixedPath, Lazy<TagDictionary> fallbackDictionary)
        {
            TagDictionary tags;

            var (fileExists, path) = GetFullPathIfExists(fixedPath);

            if (fileExists)
            {
                Logging.Log("Loading xml config from file '{0}'", path);
                var configXml = Retry.Do(() => File.ReadAllText(path), TimeSpan.FromSeconds(5));
                Logging.Log("Building tag dictionary with instance '{0}' (using config file '{1}')", instanceName, path);
                tags = TagDictionary.FromXml(instanceName, configXml);
                Logging.Log("Built tag dictionary with instance '{0}' (using config file '{1}')", instanceName, path);
            }
            else
            {
                Logging.Log("WARNING: No structure file found at: '{0}'", path);
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
