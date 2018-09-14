using System;
using System.Collections.Generic;
using System.IO;
using Ensconce.Update;

namespace Ensconce.Console
{
    internal static class TextRendering
    {
        private static readonly Lazy<TagDictionary> LazyTags = new Lazy<TagDictionary>(() => Retry.Do(BuildTagDictionary, TimeSpan.FromSeconds(5)));
        public static IDictionary<string, object> TagDictionary => LazyTags.Value;

        internal static string Render(this string s)
        {
            return s.RenderTemplate(TagDictionary);
        }

        private static TagDictionary BuildTagDictionary()
        {
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");
            var tags = BuildTagDictionary(instanceName);

            if (string.IsNullOrWhiteSpace(Arguments.FixedPath))
            {
                return tags;
            }

            var fixedPath = Arguments.FixedPath.RenderTemplate(tags);

            if (!File.Exists(fixedPath))
            {
                throw new FileNotFoundException($"Unable to locate structure file at {fixedPath}", fixedPath);
            }

            return BuildTagDictionary(instanceName, fixedPath, tags);
        }

        private static TagDictionary BuildTagDictionary(string instanceName)
        {
            Logging.Log("Building Tag Dictionary {0}", instanceName);
            var tags = new TagDictionary(instanceName);
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
                tags = new TagDictionary(instanceName, configXml);
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
