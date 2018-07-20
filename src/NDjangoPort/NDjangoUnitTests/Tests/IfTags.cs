using NDjango.Interfaces;
using NDjango.UnitTests.Data;
using NUnit.Framework;
using System.Collections.Generic;

namespace NDjango.UnitTests
{
    public partial class Tests
    {

        [Test, TestCaseSource("GetIfTagTests")]
        public void IfTag(TestDescriptor test)
        {
            test.Run(manager);
        }

        public static IList<TestDescriptor> GetIfTagTests()
        {

            IList<TestDescriptor> lst = new List<TestDescriptor>();

            // ### SMART IF TAG ################################################################
            lst.Add(new TestDescriptor("if-tag < 01", "{% if foo < bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", 1, "bar", 2), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag < 02", "{% if foo < bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", 1, "bar", 1), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag < 03", "{% if foo < bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", 2, "bar", 1), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag < 04", "{% if foo > bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", 1, "bar", 2), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag < 05", "{% if foo == bar %}yes{%else %}no{% endif %}", ContextObjects.p("foo", 1, "bar", 2), ContextObjects.p("no")));

            // AND and OR raised a TemplateSyntaxError in django 1.1 but we can use OR and AND in one expression since django 1.2
            lst.Add(new TestDescriptor("if-tag-orand", "{% if foo or bar and baz %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-andor", "{% if foo and bar or baz %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "baz", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-andand", "{% if foo and bar and baz %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true, "baz", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag->and", "{% if foo > bar and baz %}yes{% else %}no{% endif %}", ContextObjects.p("foo", 3, "bar", 2, "baz", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag->and<", "{% if foo > bar and baz < bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", 3, "bar", 2, "baz", 1), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not-or-and01", "{% if foo or not bar and baz  %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false, "baz", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not-or-and02", "{% if foo or not bar and baz  %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false, "baz", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not-or-and03", "{% if foo or not bar and baz  %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true, "baz", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not-or-and04", "{% if foo or not bar and baz  %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false, "baz", false), ContextObjects.p("no")));

            // ### IF TAG ################################################################
            lst.Add(new TestDescriptor("if-tag01", "{% if foo %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag02", "{% if foo %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag02-1", "{% if foo %}yes{% else %}no{% endif %}", ContextObjects.p("foo", null), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag03", "{% if foo %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("no")));

            // AND
            lst.Add(new TestDescriptor("if-tag-and01", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-and02", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and03", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and04", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and05", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and06", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and07", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and08", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("bar", true), ContextObjects.p("no")));

            // OR
            lst.Add(new TestDescriptor("if-tag-or01", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-or02", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-or03", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-or04", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-or05", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-or06", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-or07", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-or08", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("bar", true), ContextObjects.p("yes")));

            // TODO, multiple ORs

            // NOT
            lst.Add(new TestDescriptor("if-tag-not01", "{% if not foo %}no{% else %}yes{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not02", "{% if not %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not03", "{% if not %}yes{% else %}no{% endif %}", ContextObjects.p("not", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not04", "{% if not not %}no{% else %}yes{% endif %}", ContextObjects.p("not", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not05", "{% if not not %}no{% else %}yes{% endif %}", ContextObjects.p(), ContextObjects.p("no")));

            lst.Add(new TestDescriptor("if-tag-not06", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not07", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not08", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not09", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not10", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("no")));

            lst.Add(new TestDescriptor("if-tag-not11", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not12", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not13", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not14", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not15", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("no")));

            lst.Add(new TestDescriptor("if-tag-not16", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not17", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not18", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not19", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not20", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("yes")));

            lst.Add(new TestDescriptor("if-tag-not21", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not22", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not23", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not24", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not25", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("yes")));

            lst.Add(new TestDescriptor("if-tag-not26", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not27", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not28", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not29", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not30", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("yes")));

            lst.Add(new TestDescriptor("if-tag-not31", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not32", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not33", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not34", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not35", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("yes")));



            lst.Add(new TestDescriptor("if-tag-error01", "{% if foo and %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p(typeof(SyntaxException))));
            lst.Add(new TestDescriptor("if-tag-error02", "{% if foo or %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p(typeof(SyntaxException))));
            lst.Add(new TestDescriptor("if-tag-error03", "{% if not foo and %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p(typeof(SyntaxException))));
            lst.Add(new TestDescriptor("if-tag-error04", "{% if not foo or %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p(typeof(SyntaxException))));
            lst.Add(new TestDescriptor("if-tag-error05", "{% if not foo %}yes{% else %}no", ContextObjects.p("foo", true), ContextObjects.p(typeof(SyntaxException))));

            // IFEqual TAG
            lst.Add(new TestDescriptor("ifequal-tag-01", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("ifequal-tag-02", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("ifequal-tag-03", "{% ifnotequal foo bar %}yes{% else %}no{% endifnotequal %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("ifequal-tag-04", "{% ifnotequal foo bar %}yes{% else %}no{% endifnotequal %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("ifequal-tag-05", "{% ifequal foo \"true\" %}yes{% else %}no{% endifequal %}", ContextObjects.p("foo", "true"), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("ifequal-tag-06", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}", ContextObjects.p("foo", "true", "bar", false), ContextObjects.p("no")));

            return lst;
        }
    }
}
