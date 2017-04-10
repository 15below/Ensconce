using System;
using System.Collections.Generic;
using System.IO;
using FifteenBelow.Deployment.Update;

namespace Ensconce
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
            var fixedPath = Arguments.FixedPath;

            //If file doesn't exist, try rendering the path as a template string
            if (!File.Exists(fixedPath))
            {
                //Create a tag dictionary from environments to render
                var tags = BuildTagDictionary(instanceName);
                fixedPath = fixedPath.RenderTemplate(tags);
                //build from that path, giving environment dictonary as a fallback
                return BuildTagDictionary(instanceName, fixedPath, tags);
            }

            //Build using fixed path, do not provide a fall back
            return BuildTagDictionary(instanceName, fixedPath, null);
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
                Logging.Log("Re-Building Tag Dictionary (Using Config File)");
                tags = new TagDictionary(instanceName, configXml);
                Logging.Log("Built Tag Dictionary (Using Config File)");
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
