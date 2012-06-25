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

        private static readonly Lazy<TemplateManagerProvider> Instance =
            new Lazy<TemplateManagerProvider>(
                () =>
                new TemplateManagerProvider().WithLoader(new StringLoader()).WithSetting(
                    Constants.TEMPLATE_STRING_IF_INVALID, ErrorGuid).WithFilters(NDjango.FiltersCS.FilterManager.GetFilters())
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
            var replacementValue = GetTemplateManager().RenderTemplate(template, values).ReadToEnd();
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
