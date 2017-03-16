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
        private const string ErrorText = "[ERROR OCCURRED HERE]";

        private static readonly Lazy<ITemplateManager> TemplateManager = new Lazy<ITemplateManager>(() => GetTemplateManager(false));
        private static readonly Lazy<ITemplateManager> XmlTemplateManager = new Lazy<ITemplateManager>(() => GetTemplateManager(true));

        private static ITemplateManager GetTemplateManager(bool xmlSafe)
        {
            return new TemplateManagerProvider().WithLoader(new StringLoader())
                                                .WithSetting(Constants.TEMPLATE_STRING_IF_INVALID, ErrorGuid)
                                                .WithSetting(Constants.DEFAULT_AUTOESCAPE, xmlSafe)
                                                .WithFilters(NDjango.FiltersCS.FilterManager.GetFilters().Where(f => f.name != "default"))
                                                .WithFilter("concat", new NDjangoExpansions.ConcatFilter())
                                                .WithFilter("default", new NDjangoExpansions.DefaultFilter(ErrorGuid))
                                                .WithFilter("exists", new NDjangoExpansions.ExistsFilter(ErrorGuid))
                                                .WithFilter("empty", new NDjangoExpansions.EmptyFilter(ErrorGuid))
                                                .GetNewManager();
        }

        public static string RenderTemplate(this string template, IDictionary<string, object> values)
        {
            return Render(template, values, TemplateManager.Value);
        }

        public static string RenderXmlTemplate(this string template, IDictionary<string, object> values)
        {
            return Render(template, values, XmlTemplateManager.Value);
        }

        private static string Render(string template, IDictionary<string, object> values, ITemplateManager templateManager)
        {
            var replacementValue = templateManager.RenderTemplate(template, values).ReadToEnd();

            if (replacementValue.Contains(ErrorGuid))
            {
                var attemptedRender = replacementValue.Replace(ErrorGuid, ErrorText);
                throw new ArgumentException(string.Format("Tag substitution failed on template string:\n{0}\n\nAttempted rendering was:\n{1}", template, attemptedRender));
            }

            return replacementValue;
        }

        public class StringLoader : ITemplateLoader
        {
            public TextReader GetTemplate(string path)
            {
                var mem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(path));
                return new StreamReader(mem);
            }

            public bool IsUpdated(string path, DateTime timestamp)
            {
                return true;
            }
        }
    }
}
