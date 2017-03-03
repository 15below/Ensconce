using System;
using System.Collections.Generic;
using System.IO;
using FifteenBelow.Deployment.Update;

namespace Ensconce
{
    internal static class TextRendering
    {
        private static readonly Lazy<TagDictionary> LazyTags = new Lazy<TagDictionary>(() => Retry.Do(BuildTagDictionary, TimeSpan.FromSeconds(5)));
        public static IDictionary<string, object> TagDictionary { get { return LazyTags.Value; } }

        internal static string Render(this string s)
        {
            return s.RenderTemplate(TagDictionary);
        }

        private static TagDictionary BuildTagDictionary()
        {
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");
            Logging.Log("Building Tag Dictionary {0}", instanceName);
            var tags = new TagDictionary(instanceName);
            Logging.Log("Built Tag Dictionary {0}", instanceName);

            var renderedFixedPath = Arguments.FixedPath.RenderTemplate(tags);

            if (File.Exists(renderedFixedPath))
            {
                Logging.Log("Loading xml config from file {0}", Path.GetFullPath(renderedFixedPath));
                var configXml = Retry.Do(() => File.ReadAllText(renderedFixedPath), TimeSpan.FromSeconds(5));
                Logging.Log("Re-Building Tag Dictionary (Using Config File)");
                tags = new TagDictionary(instanceName, configXml);
                Logging.Log("Built Tag Dictionary (Using Config File)");
            }
            else
            {
                Logging.Log("No structure file found at: {0}", Path.GetFullPath(renderedFixedPath));
            }

            return tags;
        }
    }
}
