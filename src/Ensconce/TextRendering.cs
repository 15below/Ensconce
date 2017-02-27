using System;
using System.Collections.Generic;
using System.IO;
using FifteenBelow.Deployment.Update;

namespace Ensconce
{
    internal static class TextRendering
    {
        private static readonly Lazy<TagDictionary> LazyTags = new Lazy<TagDictionary>(BuildTagDictionary);
        public static IDictionary<string, object> TagDictionary { get { return LazyTags.Value; } }

        internal static string Render(this string s)
        {
            return s.RenderTemplate(LazyTags.Value);
        }

        private static TagDictionary BuildTagDictionary()
        {
            Logging.Log("Building Tag Dictionary");
            var instanceName = Environment.GetEnvironmentVariable("InstanceName");
            var tags = new TagDictionary(instanceName);
            Logging.Log("Build Tag Dictionary (Pass 1)");

            Arguments.FixedPath = Arguments.FixedPath.RenderTemplate(tags);
            if (File.Exists(Arguments.FixedPath))
            {
                Logging.Log("Reloading tags using config file {0}", Path.GetFullPath(Arguments.FixedPath));
                var configXml = Retry.Do(() => File.ReadAllText(Arguments.FixedPath), TimeSpan.FromSeconds(5));
                Logging.Log("Re-Building Tag Dictionary (Using Config File)");
                tags = new TagDictionary(instanceName, configXml);
                Logging.Log("Build Tag Dictionary (Using Config File)");
            }
            else
            {
                Logging.Log("No structure file found at: {0}", Path.GetFullPath(Arguments.FixedPath));
            }

            return tags;
        }
    }
}