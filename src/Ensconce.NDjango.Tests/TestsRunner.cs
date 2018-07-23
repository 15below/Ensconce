using Ensconce.NDjango.Core;
using Ensconce.NDjango.Core.Filters.HtmlFilters;
using Ensconce.NDjango.Core.Filters.List;
using Ensconce.NDjango.Core.Filters.StringFilters;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using FSStringList = Microsoft.FSharp.Collections.FSharpList<string>;
using StringList = System.Collections.Generic.List<string>;

namespace Ensconce.NDjango.Tests
{
    [TestFixture]
    public partial class TestsRunner
    {
        public class Loader : Interfaces.ITemplateLoader
        {



            public Loader()
            {

                templates.Add("base", "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
"<head runat=\"server\"><title>{% block Title %}{% endblock %}</title></head>" +
"<body><div id=\"main\">{% block MainContent %}{% block Sub1 %}{% block SubSub %}{% endblock SubSub %}{% endblock Sub1 %}{% endblock %}</div></body></html>");

                templates.Add("t1", "insert1--{% block b1 %}to be replaced{% endblock %}--insert2");
                templates.Add("t22", "insert1--{% block b1 %}to be replaced22{% endblock %}{% block b2 %}to be replaced22{% endblock %}--insert2");
                templates.Add("t21", "{% extends \"t22\" %}skip - b21{% block b1 %}to be replaced21{% endblock %}skip-b21");
                templates.Add("t21-withif", "{% extends \"t22\" %}skip - b21{%block b3%}{%if 'a'%}{% block b1 %}to be replaced21{% endblock %}{%endif%}{%endblock%}skip-b21");
                templates.Add("t21ancestor", "text{% block b1 %} ancestor {% endblock %}text");
                templates.Add("t21middle", "{% extends \"t21ancestor\" %} {% block b1 %}{% if 'a' %}{% block b2 %} middle {% endblock %}{% endif %}{% endblock %}");
                templates.Add("t21ancestor-CHAIN", "{% block b1 %}ancestor{% endblock %}");
                templates.Add("t21middle-CHAIN", "{% extends \"t21ancestor-CHAIN\" %}{% block b1 %}middle {{block.super}}{% endblock %}");
                templates.Add("t21top", "{% extends \"t21middle\" %} {% block b2 %} {{block.super}} {% endblock %}");
                templates.Add("tBaseNested",
@"{% block outer %}
{% block inner1 %}
this is inner1
{% endblock inner1 %}
{% block inner2 %}
this is inner2
{% endblock inner2 %}
{% endblock outer %}");
                templates.Add("include-name", "inside included template {{ value }}");
            }
            Dictionary<string, string> templates = new Dictionary<string, string>();

            #region ITemplateLoader Members

            public TextReader GetTemplate(string name)
            {
                if (templates.ContainsKey(name))
                    return new StringReader(templates[name]);
                return new StringReader(name);
            }

            public bool IsUpdated(string source, DateTime ts)
            {
                // alternate
                //return ts.Second % 2 == 0;
                return false;
            }

            #endregion
        }

        public class TestUrlTag : Abstract.UrlTag
        {
            public override string GenerateUrl(string formatString, string[] parameters, Interfaces.IContext context)
            {
                return "/appRoot/" + String.Format(formatString.Trim('/'), parameters);
            }
        }

        Interfaces.ITemplateManager manager;
        Interfaces.ITemplateManager managerForDesigner;
        TemplateManagerProvider provider;
        public Func<IEnumerable<string>> standardTags;
        public Func<IEnumerable<string>> standardFilters;

        [SetUp]
        public void Setup()
        {
            var filters = new[]
            {
                //Core Filters
                new Filter("add", new AddFilter()),
                new Filter("get_digit", new GetDigit()),
                new Filter("divisibleby", new DivisibleByFilter()),
                new Filter("addslashes", new AddSlashesFilter()),
                new Filter("capfirst", new CapFirstFilter()),
                new Filter("escapejs", new EscapeJSFilter()),
                new Filter("fix_ampersands", new FixAmpersandsFilter()),
                new Filter("floatformat", new FloatFormatFilter()),
                new Filter("linenumbers", new LineNumbersFilter()),
                new Filter("lower", new LowerFilter()),
                new Filter("upper", new UpperFilter()),
                new Filter("make_list", new MakeListFilter()),
                new Filter("wordcount", new WordCountFilter()),
                new Filter("ljust", new LJustFilter()),
                new Filter("rjust", new RJustFilter()),
                new Filter("center", new CenterFilter()),
                new Filter("cut", new CutFilter()),
                new Filter("title", new TitleFilter()),
                new Filter("removetags", new RemoveTagsFilter()),
                new Filter("first", new FirstFilter()),
                new Filter("last", new LastFilter()),
                new Filter("length", new LengthFilter()),
                new Filter("length_is", new LengthIsFilter()),
                new Filter("random", new RandomFilter()),
                new Filter("slice", new SliceFilter()),
                new Filter("default", new DefaultFilter())
            };


            provider = new TemplateManagerProvider()
                .WithLoader(new Loader())
                .WithTag("non-nested", new TestDescriptor.SimpleNonNestedTag())
                .WithTag("nested", new TestDescriptor.SimpleNestedTag())
                .WithTag("url", new TestUrlTag())
                .WithFilters(filters);

            manager = provider.GetNewManager();
            managerForDesigner = provider.WithSetting(Constants.EXCEPTION_IF_ERROR, false).GetNewManager();
        }

        public struct StringTest
        {
            public StringTest(string name, string provided, string[] expected)
            {
                this.expected = expected;
                this.provided = provided;
                this.name = name;
            }
            string name;
            public string[] expected;
            public string provided;
            public override string ToString()
            {
                return name;
            }
        }

        /// <summary>
        /// <see cref=""/>
        /// </summary>
        /// <param name="test"></param>
        //[Test, TestCaseSource("smart_split_tests")]
        public void TestSmartSplit(StringTest test)
        {
            Regex r = new Regex(@"(""(?:[^""\\]*(?:\\.[^""\\]*)*)""|'(?:[^'\\]*(?:\\.[^'\\]*)*)'|[^\s]+)", RegexOptions.Compiled);
            MatchCollection m = r.Matches(@"'\'funky\' style'");

            Func<string[], FSStringList> of_array = (array) => ListModule.OfArray<string>(array);

            Func<FSStringList, string[]> to_string_array = (string_list) =>
            {
                var tl = string_list;
                var res = new StringList();
                while (ListModule.Length<string>(tl) > 0)
                {
                    res.Add(ListModule.Head<string>(tl));
                    tl = ListModule.Tail<string>(tl);
                }

                return res.ToArray();
            };

            //         Assert.AreEqual(to_string_array(of_array(test.expected)), to_string_array(OutputHandling.smart_split(test.provided)));
        }

        public IEnumerable<StringTest> smart_split_tests()
        {
            System.Collections.Generic.List<StringTest> result = new System.Collections.Generic.List<StringTest>();
            result.Add(new StringTest("smart split-01",
                @"This is ""a person\'s"" test.",
                new string[] { "This", "is", @"""a person\'s""", "test." }
            ));

            result.Add(new StringTest("smart split-02",
                @"Another 'person\'s' test.",
                new string[] { "Another", @"'person's'", "test." }
            ));

            result.Add(new StringTest("smart split-03",
                "A \"\\\"funky\\\" style\" test.",
                new string[] { "A", "\"\"funky\" style\"", "test." }
            ));

            result.Add(new StringTest("smart split-04",
                @"A '\'funky\' style' test.",
                new string[] { "A", @"''funky' style'", "test." }
            ));

            return result;
        }

        //[Test, TestCaseSource("split_token_tests")]
        public void TestSplitContent(StringTest test)
        {
            Func<string[], FSStringList> of_array = (array) => ListModule.OfArray<string>(array);

            Func<FSStringList, string[]> to_string_array = (string_list) =>
            {
                var tl = string_list;
                var res = new StringList();
                while (ListModule.Length<string>(tl) > 0)
                {
                    res.Add(ListModule.Head<string>(tl));
                    tl = ListModule.Tail<string>(tl);
                }

                return res.ToArray();
            };

            //Assert.AreEqual(to_string_array(of_array(test.expected)), to_string_array(OutputHandling.split_token_contents(test.provided)));
        }

        public IEnumerable<StringTest> split_token_tests()
        {
            System.Collections.Generic.List<StringTest> result = new System.Collections.Generic.List<StringTest>();
            result.Add(new StringTest("split token-01",
                @"This is _(""a person\'s"") test.",
                new string[] { "This", "is", @"_(""a person\'s"")", "test." }
            ));

            result.Add(new StringTest("split token-02",
                @"Another 'person\'s' test.",
                new string[] { "Another", @"'person's'", "test." }
            ));

            result.Add(new StringTest("split token-03",
                "A \"\\\"funky\\\" style\" test.",
                new string[] { "A", "\"\"funky\" style\"", "test." }
            ));
            /*
                        result.Add(new StringTest("split token-04",
                            "This is _(\"a person's\" test).",
                            new string[] { "This", "is", "\"a person's\" test)." }
                        ));
            // */
            return result;
        }

        //    Assert.AreEqual(to_string_array(of_array(new string[] { "This", "is", "\"a person's\" test)." })),
        //        to_string_array(OutputHandling.split_token_contents("This is _(\"a person's\" test).")));

        //    Assert.AreEqual(to_string_array(of_array(new string[] { "Another", "_('person\'s')", "test." })),
        //        to_string_array(OutputHandling.split_token_contents("Another '_(person\'s)' test.")));

        //    Assert.AreEqual(to_string_array(of_array(new string[] { "A", "_(\"\\\"funky\\\" style\" test.)" })),
        //        to_string_array(OutputHandling.split_token_contents("A _(\"\\\"funky\\\" style\" test.)")));
        //}

        private Dictionary<string, object> CreateContext(string path)
        {
            var result = new Dictionary<string, object>();
            if (File.Exists(path))
            {
                XmlDocument data = new XmlDocument();
                data.Load(path);
                foreach (XmlElement variable in data.DocumentElement)
                {
                    object value = null;
                    if (variable.Attributes["type"] != null && variable.Attributes["value"] != null)
                        switch (variable.Attributes["type"].Value)
                        {
                            case "integer":
                                value = int.Parse(variable.Attributes["value"].Value);
                                break;
                            case "string":
                                value = variable.Attributes["value"].Value;
                                break;
                            case "boolean":
                                value = bool.Parse(variable.Attributes["value"].Value);
                                break;
                        }

                    if (variable.Attributes["type"] == null && variable.Attributes["value"] != null)
                        value = variable.Attributes["value"].Value;

                    if (!result.ContainsKey(variable.Name))
                        result.Add(variable.Name, value);
                    else
                        if (result[variable.Name] is IList)
                        ((IList)result[variable.Name]).Add(value);
                    else
                        result[variable.Name] = new System.Collections.Generic.List<object>(new object[] { result[variable.Name], value });

                }
            }
            return result;
        }

        //        [Test, TestCaseSource("TemplateTestEnum1")]
        //public void Test1(string path)
        //{
        //    string retVal = TestDescriptor.runTemplate(manager, File.ReadAllText(path + ".django"), CreateContext(path + ".xml"));
        //    string retBase = File.ReadAllText(path + ".htm");
        //    Assert.AreEqual(retBase, retVal, String.Format("RESULT!!!!!!!!!!!!!!!!:\r\n{0}", retVal));
        //}


        //public IEnumerable<string> TemplateTestEnum1
        //{
        //    get
        //    {
        //        var result = new System.Collections.Generic.List<string>();
        //        result.Add("../Tests/Templates/Test1____/Scripts/create");
        //        return result;
        //    }
        //}




        ////        [Test, TestCaseSource("TestEnumerator")]
        //        public void Test(string path)
        //        {
        //            string retVal = TestDescriptor.runTemplate(manager, File.ReadAllText(path + ".django"), CreateContext(path + ".xml"));
        //            string retBase = File.ReadAllText(path + ".htm");
        //            Assert.AreEqual(retBase, retVal,String.Format("RESULT!!!!!!!!!!!!!!!!:\r\n{0}",retVal));
        //        }

        //        public IEnumerable<string> TestEnumerator
        //        {
        //            get
        //            {
        //                var result = new System.Collections.Generic.List<string>();
        //                foreach (string file in Directory.GetFiles("../../Tests", "*.django", SearchOption.AllDirectories))
        //                    result.Add(file.Substring(0, file.LastIndexOf(".")));
        //                return result;
        //            }
        //        }
    }
}
