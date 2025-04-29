using Ensconce.NDjango.Core;
using Ensconce.NDjango.Custom;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ensconce.Update
{
    public static class NDjangoWrapper
    {
        private const string StringProvider = "ensconceString://";
        private static readonly ErrorTemplate Error = new ErrorTemplate();
        private static readonly Lazy<Interfaces.ITemplateManager> TemplateManager = new Lazy<Interfaces.ITemplateManager>(() => GetTemplateManager(false));
        private static readonly Lazy<Interfaces.ITemplateManager> XmlTemplateManager = new Lazy<Interfaces.ITemplateManager>(() => GetTemplateManager(true));
        private static readonly object Locker = new object();

        private static Interfaces.ITemplateManager GetTemplateManager(bool xmlSafe)
        {
            lock (Locker)
            {
                return new TemplateManagerProvider().WithLoader(new StringLoader())
                                                    .WithSetting(Constants.TEMPLATE_STRING_IF_INVALID, Error)
                                                    .WithSetting(Constants.DEFAULT_AUTOESCAPE, xmlSafe)
                                                    .WithSetting(Constants.EXCEPTION_IF_ERROR, true)
                                                    .WithSetting(Constants.RELOAD_IF_UPDATED, false)
                                                    .WithFilters(NDjango.Custom.Filters.Loader.CustomFilters)
                                                    .WithFilters(Ensconce.NDjango.Core.Filters.Loader.StringFilters)
                                                    .WithFilters(Ensconce.NDjango.Core.Filters.Loader.HtmlFilters)
                                                    .WithFilters(Ensconce.NDjango.Core.Filters.Loader.ListFilters)
                                                    .GetNewManager();
            }

        }

        public static string RenderTemplateEncrypted(this string template, Lazy<TagDictionary> values, string certificate)
        {
            if (string.IsNullOrWhiteSpace(certificate))
            {
                return RenderTemplate(template, values);
            }

            lock (Locker)
            {
                var raw = Render(template, values, TemplateManager.Value);

                if (string.IsNullOrWhiteSpace(raw))
                {
                    return raw;
                }

                var renderedCertificate = certificate.Contains("{{") || certificate.Contains("{%") ? Render(certificate, values, TemplateManager.Value) : Render($"{{{{ {certificate}|default:'{certificate}' }}}}", values, TemplateManager.Value);
                var tagDictionary = TagDictionary.FromDictionary(new Dictionary<string, object>
                {
                    { "data", raw },
                    { "cert", renderedCertificate }
                });
                return Render("{{ data|encrypt:cert }}", new Lazy<TagDictionary>(() => tagDictionary), TemplateManager.Value);
            }
        }

        public static string RenderTemplate(this string template, Lazy<TagDictionary> values)
        {
            lock (Locker)
            {
                return Render(template, values, TemplateManager.Value);
            }
        }

        public static string RenderXmlTemplate(this string template, Lazy<TagDictionary> values)
        {
            lock (Locker)
            {
                return Render(template, values, XmlTemplateManager.Value);
            }
        }

        private static string Render(string template, Lazy<TagDictionary> values, Interfaces.ITemplateManager templateManager)
        {
            if (!template.Contains("{{") && !template.Contains("{%"))
            {
                //Use an empty tag dictionary because we don't have tags
                //For some reason, this "fixes" something in files...but i have no idea what!
                values = new Lazy<TagDictionary>(TagDictionary.Empty);
            }

            Exception exception = null;
            string replacementValue;

            try
            {
                Error.Invoked = false;
                replacementValue = templateManager.RenderTemplate(StringProvider + template, values.Value).ReadToEnd();
            }
            catch (Exception ex)
            {
                exception = ex;
                replacementValue = string.Empty;
                Error.Invoked = true;
            }

            if (Error.Invoked)
            {
                if (string.IsNullOrWhiteSpace(replacementValue) || !replacementValue.Contains(Error.ToString()))
                {
                    throw new NDjangoWrapperException($"Tag substitution errored on template string:\n{template}", exception);
                }

                var attemptedRender = replacementValue.Replace(Error.ToString(), "[ERROR OCCURRED HERE]");
                throw new NDjangoWrapperException($"Tag substitution failed on template string:\n{template}\n\nAttempted rendering was:\n{attemptedRender}", exception);
            }

            return replacementValue;
        }

        public class StringLoader : Interfaces.ITemplateLoader
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

        public class NDjangoWrapperException : Exception
        {
            public NDjangoWrapperException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
