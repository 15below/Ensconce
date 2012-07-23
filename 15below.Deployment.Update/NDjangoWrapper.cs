using System;
using System.Collections.Generic;
using System.IO;
using NDjango;
using NDjango.Interfaces;

namespace FifteenBelow.Deployment.Update
{
    public static class NDjangoWrapper
    {
        private const string ErrorGuid = "{668B7536-C32B-4D86-B065-70C143EB4AD9}";
        private const string StringProvider = "string://";

        private static readonly Lazy<TemplateManagerProvider> Instance =
            new Lazy<TemplateManagerProvider>(
                () =>
                new TemplateManagerProvider()
                    .WithLoader(new StringLoader())
                    .WithSetting(Constants.TEMPLATE_STRING_IF_INVALID, ErrorGuid)
                    .WithSetting(Constants.DEFAULT_AUTOESCAPE, false)
                    .WithFilters(NDjango.FiltersCS.FilterManager.GetFilters())
                    .WithFilter("concat", new NDjangoExpansions.ConcatFilter())
                );

        static NDjangoWrapper()
        {
        }

        private static ITemplateManager GetTemplateManager()
        {
            return Instance.Value.GetNewManager();
        }

        public static string RenderTemplate(this string template, IDictionary<string, object> values)
        {
            var replacementValue = GetTemplateManager().RenderTemplate(StringProvider + template, values).ReadToEnd();
            if (replacementValue.Contains(ErrorGuid))
            {
                var attemptedRender = replacementValue.Replace(ErrorGuid, "ERROR OCCURRED HERE");
                throw new ArgumentException(
                    string.Format("Tag substitution failed on template string:\n{0}\n\nAttempted rendering was:\n{1}",
                                  template, attemptedRender));
            }
            return replacementValue;
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
