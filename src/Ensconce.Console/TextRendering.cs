using System;
using System.IO;
using Ensconce.Update;

namespace Ensconce.Console
{
    internal static class TextRendering
    {
        public static readonly Lazy<TagDictionary> TagDictionary = new Lazy<TagDictionary>(() => Retry.Do(BuildTagDictionary,
                                                                                                 TimeSpan.FromSeconds(5),
                                                                                                 new[] { typeof(InvalidDataException) }));

        internal static string Render(this string s)
        {
            return s.RenderTemplate(TagDictionary);
        }

        private static TagDictionary BuildTagDictionary()
        {
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");
            var tags = new Lazy<TagDictionary>(() => BuildTagDictionary(instanceName));

            if (string.IsNullOrWhiteSpace(Arguments.FixedPath))
            {
                return tags.Value;
            }

            var fixedPath = Arguments.FixedPath.RenderTemplate(tags);

            if (File.Exists(fixedPath))
            {
                return BuildTagDictionary(instanceName, fixedPath, tags.Value);
            }

            Logging.Log("WARNING: The fixed structure path was not located at '{0}' so running without it!", fixedPath);
            return tags.Value;
        }

        private static TagDictionary BuildTagDictionary(string instanceName)
        {
            Logging.Log("Building Tag Dictionary {0}", instanceName);
            var tags = Update.TagDictionary.FromIdentifier(instanceName);
            Logging.Log("Built Tag Dictionary {0}", instanceName);
            return tags;
        }

        private static TagDictionary BuildTagDictionary(string instanceName, string fixedPath, TagDictionary fallbackDictionary)
        {
            TagDictionary tags;

            if (File.Exists(fixedPath))
            {
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
