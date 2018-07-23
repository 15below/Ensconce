using Ensconce.NDjango.Core;
using Ensconce.NDjango.Tests.Data;
using NUnit.Framework;
using System.Collections.Generic;

namespace Ensconce.NDjango.Tests
{
    public partial class TestsRunner
    {

        [Test, TestCaseSource("GetLoopTagsTests")]
        public void LoopTags(TestDescriptor test)
        {
            test.Run(manager);
        }

        public static IList<TestDescriptor> GetLoopTagsTests()
        {

            IList<TestDescriptor> lst = new List<TestDescriptor>();

            // CYCLE TAG 
            lst.Add(new TestDescriptor("cycle01", "{% cycle a %}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.RenderingException))));
            lst.Add(new TestDescriptor("cycle02", "{% cycle a,b,c as abc %}{% cycle abc %}", ContextObjects.empty, ContextObjects.p("ab")));
            lst.Add(new TestDescriptor("cycle03", "{% cycle a,b,c as abc %}{% cycle abc %}{% cycle abc %}", ContextObjects.empty, ContextObjects.p("abc")));
            lst.Add(new TestDescriptor("cycle04", "{% cycle a,b,c as abc %}{% cycle abc %}{% cycle abc %}{% cycle abc %}", ContextObjects.empty, ContextObjects.p("abca")));
            lst.Add(new TestDescriptor("cycle05", "{% cycle %}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));
            lst.Add(new TestDescriptor("cycle06", "{% cycle a %}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.RenderingException))));
            lst.Add(new TestDescriptor("cycle07", "{% cycle a,b,c as foo %}{% cycle bar %}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.RenderingException))));
            lst.Add(new TestDescriptor("cycle08", "{% cycle a,b,c as foo %}{% cycle foo %}{{ foo }}{{ foo }}{% cycle foo %}{{ foo }}", ContextObjects.empty, ContextObjects.p("abbbcc")));
            lst.Add(new TestDescriptor("cycle09", "{% for i in test %}{% cycle a,b %}{{ i }},{% endfor %}", ContextObjects.p("test", ContextObjects.range(5)), ContextObjects.p("a0,b1,a2,b3,a4,")));
            lst.Add(new TestDescriptor("cycle09-1", "{% for i in test %}{% cycle a,b %}{{ i.0 }};{{i.1}},{% endfor %}", ContextObjects.p("test", ContextObjects.square(5, 2)), ContextObjects.p("a0-0;0-1,b1-0;1-1,a2-0;2-1,b3-0;3-1,a4-0;4-1,")));
            lst.Add(new TestDescriptor("cycle09-2", "{% for i, j in test %}{% cycle a,b %}{{ i }};{{j}},{% endfor %}", ContextObjects.p("test", ContextObjects.square(5, 2)), ContextObjects.p("a0-0;0-1,b1-0;1-1,a2-0;2-1,b3-0;3-1,a4-0;4-1,")));
            lst.Add(new TestDescriptor("cycle09-3", "{% for i,j in test %}{% cycle a,b %}{{ i }};{{j}},{% endfor %}", ContextObjects.p("test", ContextObjects.square(5, 2)), ContextObjects.p("a0-0;0-1,b1-0;1-1,a2-0;2-1,b3-0;3-1,a4-0;4-1,")));

            //// New format,
            lst.Add(new TestDescriptor("cycle10-1", "{% cycle 'a' 'b' 'c' as abc %}", ContextObjects.empty, ContextObjects.p("a")));
            lst.Add(new TestDescriptor("cycle10", "{% cycle 'a' 'b' 'c' as abc %}{% cycle abc %}", ContextObjects.empty, ContextObjects.p("ab")));
            lst.Add(new TestDescriptor("cycle11", "{% cycle 'a' 'b' 'c' as abc %}{% cycle abc %}{% cycle abc %}", ContextObjects.empty, ContextObjects.p("abc")));
            lst.Add(new TestDescriptor("cycle12", "{% cycle 'a' 'b' 'c' as abc %}{% cycle abc %}{% cycle abc %}{% cycle abc %}", ContextObjects.empty, ContextObjects.p("abca")));
            lst.Add(new TestDescriptor("cycle13", "{% for i in test %}{% cycle 'a' 'b' %}{{ i }},{% endfor %}", ContextObjects.p("test", ContextObjects.range(5)), ContextObjects.p("a0,b1,a2,b3,a4,")));
            lst.Add(new TestDescriptor("cycle14", "{% cycle one two as foo %}{% cycle foo %}", ContextObjects.p("one", "1", "two", "2"), ContextObjects.p("12")));
            lst.Add(new TestDescriptor("cycle13", "{% for i in test %}{% cycle aye bee %}{{ i }},{% endfor %}", ContextObjects.p("test", ContextObjects.range(5), "aye", "a", "bee", "b"), ContextObjects.p("a0,b1,a2,b3,a4,")));

            // for tag
            lst.Add(new TestDescriptor("for 01", "{% for i in test %}{% cycle aye bee %}{{ i }}hide this{% empty %}show this{% endfor %}", ContextObjects.p("test", null, "aye", "a", "bee", "b"), ContextObjects.p("show this")));
            lst.Add(new TestDescriptor("for 02", "{% for i in test %}{{ forloop.counter }},{% endfor %}", ContextObjects.p("test", ContextObjects.range(5)), ContextObjects.p("1,2,3,4,5,")));
            lst.Add(new TestDescriptor("for 03", "{% for i in test %}{{ forloop.revcounter0 }},{% endfor %}", ContextObjects.p("test", ContextObjects.range(5)), ContextObjects.p("4,3,2,1,0,")));
            lst.Add(new TestDescriptor("for 04", "{% for athlete in athleteList %}{{ athlete.name }},{% endfor %}", ContextObjects.p("athleteList", new AthleteList()), ContextObjects.p("Michael Jordan,Magic Johnson,")));
            lst.Add(new TestDescriptor("for 05", "{% for FirstName, LastName in athleteList %}{{ FirstName }} {{ LastName }},{% endfor %}", ContextObjects.p("athleteList", new AthleteList()), ContextObjects.p("Michael Jordan,Magic Johnson,")));
            lst.Add(new TestDescriptor("for 06", "{% for FirstName, LastName in athleteList %}{{ FirstName }} {{ LastName }},{% endfor %}",
                ContextObjects.p("athleteList", new string[][] { new string[] { "Michael", "Jordan" }, new string[] { "Magic", "Johnson" } }),
                ContextObjects.p("Michael Jordan,Magic Johnson,")));
            lst.Add(new TestDescriptor("for 07", "{% for athlete in athleteList reversed %}{{ athlete.name }},{% endfor %}", ContextObjects.p("athleteList", new AthleteList()), ContextObjects.p("Magic Johnson,Michael Jordan,")));
            lst.Add(new TestDescriptor("for 08", "{% for FirstName, LastName in athleteList %}{% if forloop.last %}{{ FirstName }} {{ LastName }}{% endif %}{% endfor %}",
                ContextObjects.p("athleteList", new string[][] { new string[] { "Michael", "Jordan" }, new string[] { "Magic", "Johnson" } }),
                ContextObjects.p("Magic Johnson")));
            lst.Add(new TestDescriptor("for 09", "{% for i in test %}run {{ forloop.counter }}:{% for j in test%}i={{forloop.parentloop.counter}},j={{forloop.counter}};{% endfor %}{% endfor %}",
                ContextObjects.p("test", ContextObjects.range(2)),
                ContextObjects.p("run 1:i=1,j=1;i=1,j=2;run 2:i=2,j=1;i=2,j=2;")));

            // ifchanged tag
            lst.Add(new TestDescriptor("ifchanged 01", "{% for i in test %}{% ifchanged %}nothing changed{%else%}same {% endifchanged %}{{ forloop.counter }},{% endfor %}", ContextObjects.p("test", ContextObjects.range(5)), ContextObjects.p("nothing changed1,same 2,same 3,same 4,same 5,")));
            lst.Add(new TestDescriptor("ifchanged 02", "{% for i in test %}{% ifchanged a %}nothing changed{% endifchanged %}{{ forloop.counter }},{% endfor %}", ContextObjects.p("test", ContextObjects.range(5)), ContextObjects.p("nothing changed1,2,3,4,5,")));
            lst.Add(new TestDescriptor("ifchanged 03", "{% for i in test %}{% ifchanged %}counter = {{ forloop.counter }}{% else %} hide this {% endifchanged %}{{ forloop.counter }},{% endfor %}", ContextObjects.p("test", ContextObjects.range(5)), ContextObjects.p("counter = 11,counter = 22,counter = 33,counter = 44,counter = 55,")));
            lst.Add(new TestDescriptor("ifchanged 04", "{% for i in test %}{% ifchanged i %}counter = {{ forloop.counter }}{% endifchanged %}{{ forloop.counter }},{% endfor %}", ContextObjects.p("test", ContextObjects.range(5)), ContextObjects.p("counter = 11,counter = 22,counter = 33,counter = 44,counter = 55,")));
            return lst;
        }
    }
}
