using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDjango;
using NDjango.Interfaces;

namespace FifteenBelow.Deployment.Update
{
    public static class NDjangoWrapper
    {
        private const string ErrorGuid = "{668B7536-C32B-4D86-B065-70C143EB4AD9}";
        private const string StringProvider = "string://";

        private static readonly Lazy<TemplateManagerProvider> Instance = new Lazy<TemplateManagerProvider>(() => new TemplateManagerProvider().WithLoader(new StringLoader())
                                                                                                                                              .WithSetting(Constants.TEMPLATE_STRING_IF_INVALID, ErrorGuid)
                                                                                                                                              .WithSetting(Constants.DEFAULT_AUTOESCAPE, false)
                                                                                                                                              .WithFilters(NDjango.FiltersCS.FilterManager.GetFilters().Where(f => f.name != "default"))
                                                                                                                                              .WithFilter("concat", new NDjangoExpansions.ConcatFilter())
                                                                                                                                              .WithFilter("default", new NDjangoExpansions.DefaultFilter(ErrorGuid))
                                                                                                                                              .WithFilter("exists", new NDjangoExpansions.ExistsFilter(ErrorGuid))
                                                                                                                                              .WithFilter("empty", new NDjangoExpansions.EmptyFilter(ErrorGuid)));

        private static readonly Lazy<TemplateManagerProvider> XmlSafeInstance = new Lazy<TemplateManagerProvider>(() => new TemplateManagerProvider().WithLoader(new StringLoader())
                                                                                                                                                     .WithSetting(Constants.TEMPLATE_STRING_IF_INVALID, ErrorGuid)
                                                                                                                                                     .WithSetting(Constants.DEFAULT_AUTOESCAPE, true)
                                                                                                                                                     .WithFilters(NDjango.FiltersCS.FilterManager.GetFilters().Where(f => f.name != "default"))
                                                                                                                                                     .WithFilter("concat", new NDjangoExpansions.ConcatFilter())
                                                                                                                                                     .WithFilter("default", new NDjangoExpansions.DefaultFilter(ErrorGuid))
                                                                                                                                                     .WithFilter("exists", new NDjangoExpansions.ExistsFilter(ErrorGuid))
                                                                                                                                                     .WithFilter("empty", new NDjangoExpansions.EmptyFilter(ErrorGuid)));

        private static ITemplateManager GetTemplateManager()
        {
            return Instance.Value.GetNewManager();
        }

        private static ITemplateManager GetXmlSafeTemplateManager()
        {
            return XmlSafeInstance.Value.GetNewManager();
        }

        public static string RenderTemplate(this string template, IDictionary<string, object> values)
        {
            return Render(template, values, GetTemplateManager());
        }

        public static string RenderXmlTemplate(this string template, IDictionary<string, object> values)
        {
            return Render(template, values, GetXmlSafeTemplateManager());
        }

        private static string Render(string template, IDictionary<string, object> values, ITemplateManager templateManager)
        {
            var replacementValue = templateManager.RenderTemplate(StringProvider + template, values).ReadToEnd();
            CheckForTagError(template, replacementValue);
            return replacementValue;
        }

        private static void CheckForTagError(string template, string replacementValue)
        {
            if (replacementValue.Contains(ErrorGuid))
            {
                var attemptedRender = replacementValue.Replace(ErrorGuid, "[ERROR OCCURRED HERE]");
                throw new ArgumentException(
                    string.Format("Tag substitution failed on template string:\n{0}\n\nAttempted rendering was:\n{1}",
                                  template, attemptedRender));
            }
        }

        public class StringLoader : ITemplateLoader
        {
            public TextReader GetTemplate(string path)
            {
                if (path.StartsWith(StringProvider))
                {
                    var mem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(path.Substring(StringProvider.Length)));
                    return new StreamReader(mem);
                }
                return new StreamReader(path);
            }

            public bool IsUpdated(string path, DateTime timestamp)
            {
                return true;
            }
        }

    }
}
