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
        private const string StringProvider = "ensconceString://";
        private static readonly ErrorTemplate Error = new ErrorTemplate();
        private static readonly Lazy<ITemplateManager> TemplateManager = new Lazy<ITemplateManager>(() => GetTemplateManager(false));
        private static readonly Lazy<ITemplateManager> XmlTemplateManager = new Lazy<ITemplateManager>(() => GetTemplateManager(true));

        private static ITemplateManager GetTemplateManager(bool xmlSafe)
        {
            return new TemplateManagerProvider().WithLoader(new StringLoader())
                                                .WithSetting(Constants.TEMPLATE_STRING_IF_INVALID, Error)
                                                .WithSetting(Constants.DEFAULT_AUTOESCAPE, xmlSafe)
                                                .WithSetting(Constants.EXCEPTION_IF_ERROR, true)
                                                .WithSetting(Constants.RELOAD_IF_UPDATED, false)
                                                .WithFilters(NDjango.FiltersCS.FilterManager.GetFilters().Where(f => f.name != "default"))
                                                .WithFilter("concat", new NDjangoExpansions.ConcatFilter())
                                                .WithFilter("default", new NDjangoExpansions.DefaultFilter())
                                                .WithFilter("exists", new NDjangoExpansions.ExistsFilter())
                                                .WithFilter("empty", new NDjangoExpansions.EmptyFilter())
                                                .WithFilter("startsWith", new NDjangoExpansions.StartsWithFilter())
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
            lock (Error)
            {
                string replacementValue;

                try
                {
                    Error.Invoked = false;
                    replacementValue = templateManager.RenderTemplate(StringProvider + template, values).ReadToEnd();
                }
                catch (Exception)
                {
                    replacementValue = string.Empty;
                    Error.Invoked = true;
                }

                if (Error.Invoked)
                {
                    if (string.IsNullOrWhiteSpace(replacementValue) || !replacementValue.Contains(Error.ToString()))
                    {
                        throw new ArgumentException(string.Format("Tag substitution errored on template string:\n{0}", template));
                    }

                    var attemptedRender = replacementValue.Replace(Error.ToString(), "[ERROR OCCURRED HERE]");
                    throw new ArgumentException(string.Format("Tag substitution failed on template string:\n{0}\n\nAttempted rendering was:\n{1}", template, attemptedRender));
                }

                return replacementValue;
            }
        }

        public class StringLoader : ITemplateLoader
        {
            public TextReader GetTemplate(string path)
            {
                //If the path starts with our own string provider treat it as text.
                //If not, then it's an actual file path, so use a standard stream reader.
                //Although we do not pass file paths into this function, ndjango does internally.
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

        public class ErrorTemplate
        {
            public bool Invoked;

            public override string ToString()
            {
                Invoked = true;
                return "{RandomText-668B7536-C32B-4D86-B065-70C143EB4AD9}";
            }

            public override bool Equals(object obj)
            {
                Invoked = true;
                return false;
            }

            public override int GetHashCode()
            {
                Invoked = true;
                return -1;
            }
        }
    }
}
