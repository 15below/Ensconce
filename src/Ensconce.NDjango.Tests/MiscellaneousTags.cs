using Ensconce.NDjango.Core;
using Ensconce.NDjango.Tests.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Ensconce.NDjango.Tests
{
    public partial class TestsRunner
    {
        [Test, TestCaseSource("GetMiscellaneousTagsTests")]
        public void MiscellaneousTags(TestDescriptor test)
        {
            test.Run(manager);
        }

        public static IList<TestDescriptor> GetMiscellaneousTagsTests()
        {
            IList<TestDescriptor> lst = new List<TestDescriptor>();
            // autoescape tag
            lst.Add(new TestDescriptor("autoescape-tag", "before <{{ lt }} {% autoescape off %}inside <{{ lt }}{{ gt }}>{% endautoescape %} after {{ gt }}>", ContextObjects.p("lt", "<", "gt", ">"), ContextObjects.p("before <&lt; inside <<>> after &gt;>")));

            // ### COMMENT TAG ################################################################
            lst.Add(new TestDescriptor("comment-tag", "before {% comment %}hide this: {% invalid tag %} {% endcomment %} after", ContextObjects.empty, ContextObjects.p("before  after")));

            // DEBUG TAG
            lst.Add(new TestDescriptor("debug", "{% debug %}", ContextObjects.p("v2", null, "v3", "vee3"),
                ContextObjects.p("System.Collections.Generic.Dictionary`2[System.String,System.Object]\r\n---- NDjango Context ----\r\nSettings:\r\nautoescape = True\r\nVariables:\r\nv2 = NULL\r\nv3 = \"vee3\"\r\n")));

            // FIRSTOF TAG
            lst.Add(new TestDescriptor("firstof01", "{% firstof v1 v2 v3 \"fallback\" %}", ContextObjects.empty, ContextObjects.p("fallback")));
            lst.Add(new TestDescriptor("firstof02", "{% firstof v1 v2 v3 \"fallback\" %}", ContextObjects.p("v2", "vee2", "v3", "vee3"), ContextObjects.p("vee2")));
            lst.Add(new TestDescriptor("firstof03", "{% firstof v1 v2 v3 \"fallback\" %}", ContextObjects.p("v2", null, "v3", "vee3"), ContextObjects.p("vee3")));

            // NOW TAG
            var now = new DateTime(2019, 1, 1, 0, 0, 0);
            //lst.Add(new TestDescriptor("now 01", "{% now \"D,d F Y G:i:s O\" %}", ContextObjects.p("now", now), ContextObjects.p(now.ToString("ddd,dd MMMM yyyy %H:mm:ss zzz"))));
            //lst.Add(new TestDescriptor("now 02", "{% now \"D,d F Y G:i:\\s O\" %}", ContextObjects.p("now", now), ContextObjects.p(now.ToString("ddd,dd MMMM yyyy %H:mm:\\s zzz"))));

            lst.Add(new TestDescriptor("date filter 01", "{{ now|date:\"D,d F Y G:i:s O\" }}", ContextObjects.p("now", now), ContextObjects.p(now.ToString("ddd,dd MMMM yyyy %H:mm:ss zzz"))));
            lst.Add(new TestDescriptor("date filter 02", "{{ now|date:\"D,d F Y G:i:\\s O\" }}", ContextObjects.p("now", now), ContextObjects.p(now.ToString("ddd,dd MMMM yyyy %H:mm:\\s zzz"))));
            lst.Add(new TestDescriptor("date filter 03", "{{ now|date:\"D,d F Y G:i:\\s O\" }}", ContextObjects.p("now", null), ContextObjects.p(DateTime.MinValue.ToString("ddd,dd MMMM yyyy %H:mm:\\s zzz"))));

            // BLOCK TAG
            lst.Add(new TestDescriptor("block 01", "some html out{% block something %} some html in{% endblock %} more html out", null, ContextObjects.p("some html out some html in more html out")));

            // EXTENDS TAG
            lst.Add(new TestDescriptor("extends 01", "{% extends \"t1\" %} skip1--{% block b1 %}the replacement{% endblock %}--skip2", null, ContextObjects.p("insert1--the replacement--insert2")));
            lst.Add(new TestDescriptor("extends 01-bis", "{% extends 't1' %} skip1--{% block b1 %}the replacement{% endblock %}--skip2", null, ContextObjects.p("insert1--the replacement--insert2")));
            lst.Add(new TestDescriptor("extends 02", "{% extends \"t21\" %} skip1--{% block b1 %}the replacement1{% endblock %}{% block b2 %} the replacement2{% endblock %}--skip2", null, ContextObjects.p("insert1--the replacement1 the replacement2--insert2")));
            lst.Add(new TestDescriptor("extends 03", "{% extends \"t21\" %} skip1--{% block b1 %}the replacement1{% endblock %}--skip2", null, ContextObjects.p("insert1--the replacement1to be replaced22--insert2")));
            lst.Add(new TestDescriptor("extends 04", "{% extends \"t21\" %} skip1--{% block b1 %}the replacement1++{{ block.super }}++{% endblock %}--skip2", null,
                ContextObjects.p("insert1--the replacement1++to be replaced21++to be replaced22--insert2")));

            lst.Add(new TestDescriptor("extends 04 - breaking parents", "{% extends \"t21-withif\" %} skip1--{% block b1 %}the replacement1++{{ block.super }}++{% endblock %}--skip2", null,
                ContextObjects.p("insert1--the replacement1++to be replaced21++to be replaced22--insert2")));

            lst.Add(new TestDescriptor("extends 05", "{% extends \"t21middle\" %} {% block b2 %} child {% endblock %}", null, ContextObjects.p("text child text")));
            lst.Add(new TestDescriptor("extends 05-CHAIN", "{% extends \"t21middle-CHAIN\" %} {% block b1 %}child {{block.super}}{% endblock %}", null, ContextObjects.p("child middle ancestor")));

            // Nested block tags
            lst.Add(new TestDescriptor("nestedblocks 01",
@"{% extends ""tBaseNested"" %}
{% block outer %}{{ block.super }}new stuff
{% endblock outer %}", null, ContextObjects.p(@"
this is inner1
this is inner2
new stuff
")));
            lst.Add(new TestDescriptor("nestedblocks 02",
@"{% extends ""tBaseNested"" %}
{% block outer %}{{ block.super }}new stuff{% endblock outer %}
{% block inner2 %}new inner2{% endblock inner2 %}", null, ContextObjects.p(@"
this is inner1
new inner2
new stuff")));
            lst.Add(new TestDescriptor("nestedblocks 03 - \"don't do this!\"",
@"{% extends ""tBaseNested"" %}
{% block outer %}{{ block.super }}new stuff
{% block inner2 %}new inner2{% endblock inner2 %}
{% endblock outer %}", null, ContextObjects.p(@"
this is inner1
new inner2
new stuff
new inner2
")));

            // filter tag
            lst.Add(new TestDescriptor("filter-tag", "before <{{ lt }} {% autoescape off %}inside <{{ lt }}{{ gt }}>{% filter escape %} inside filter <{{ lt }}{{ gt }}>{% endfilter %}{% endautoescape %} after {{ gt }}>"
                , ContextObjects.p("lt", "<", "gt", ">"), ContextObjects.p("before <&lt; inside <<>> inside filter &lt;&lt;&gt;&gt; after &gt;>")));

            // include tag
            lst.Add(new TestDescriptor("include 01", "value={{ value }} {% include \"include-name\" %}", ContextObjects.p("value", "VALUE"), ContextObjects.p("value=VALUE inside included template VALUE")));

            ContextObjects.Person[] people = new ContextObjects.Person[] {
                new ContextObjects.Person("George", "Bush","Male"),
                new ContextObjects.Person("Bill","Clinton","Male"),
                new ContextObjects.Person("Margaret","Thatcher","Female"),
                new ContextObjects.Person("Condoleezza","Rice","Female"),
                new ContextObjects.Person("Pat","Smith","Unknown")
            };

            ContextObjects.Person[] peopleUnordered = new ContextObjects.Person[] {
                new ContextObjects.Person("George", "Bush","Male"),
                new ContextObjects.Person("Margaret","Thatcher","Female"),
                new ContextObjects.Person("Bill","Clinton","Male"),
                new ContextObjects.Person("Pat","Smith","Unknown"),
                new ContextObjects.Person("Condoleezza","Rice","Female")
            };

            // regroup tag
            lst.Add(new TestDescriptor("regroup-01",
@"{% regroup people by gender as gender_list %}<ul>{% for gender in gender_list %}
    <li>{{ gender.grouper }}
    <ul>{% for item in gender.list %}
        <li>{{ item.first_name }} {{ item.last_name }}</li>{% endfor %}
    </ul>
    </li>{% endfor %}
</ul>",
                ContextObjects.p("people", people),
                ContextObjects.p(
@"<ul>
    <li>Male
    <ul>
        <li>George Bush</li>
        <li>Bill Clinton</li>
    </ul>
    </li>
    <li>Female
    <ul>
        <li>Margaret Thatcher</li>
        <li>Condoleezza Rice</li>
    </ul>
    </li>
    <li>Unknown
    <ul>
        <li>Pat Smith</li>
    </ul>
    </li>
</ul>")));

            lst.Add(new TestDescriptor("regroup-02",
                @"{% regroup people by gender as gender_list %}<ul>{% for gender in gender_list %}
    <li>{{ gender.grouper }}
    <ul>{% for item in gender.list %}
        <li>{{ item.first_name }} {{ item.last_name }}</li>{% endfor %}
    </ul>
    </li>{% endfor %}
</ul>",
                ContextObjects.p("people", peopleUnordered),
                ContextObjects.p(
                    @"<ul>
    <li>Male
    <ul>
        <li>George Bush</li>
        <li>Bill Clinton</li>
    </ul>
    </li>
    <li>Female
    <ul>
        <li>Margaret Thatcher</li>
        <li>Condoleezza Rice</li>
    </ul>
    </li>
    <li>Unknown
    <ul>
        <li>Pat Smith</li>
    </ul>
    </li>
</ul>")));

            // spaceless tag
            lst.Add(new TestDescriptor("spaceless-01", "{% spaceless %}templatetag<h1>  \r\n   </h1> !\r\n <h2> </h2>{% endspaceless %}", ContextObjects.empty, ContextObjects.p("templatetag<h1></h1> !\r\n <h2></h2>")));

            // ssi tag
            lst.Add(new TestDescriptor("ssi 01", "value={{ value }} {% ssi include-name parsed %}", ContextObjects.p("value", "VALUE"), ContextObjects.p("value=VALUE inside included template VALUE")));
            lst.Add(new TestDescriptor("ssi 02", "value={{ value }} {% ssi include-name %}", ContextObjects.p("value", "VALUE"), ContextObjects.p("value=VALUE inside included template {{ value }}")));

            // tho following 3 tests are intended to test the buffer boundary - set the buffer length
            // in the ssi tag implementation to 14 for them to make sense
            lst.Add(new TestDescriptor("ssi 03", "{% ssi buffer-length %}", ContextObjects.p("value", "VALUE"), ContextObjects.p("buffer-length")));
            lst.Add(new TestDescriptor("ssi 04", "{% ssi buffer-length1 %}", ContextObjects.p("value", "VALUE"), ContextObjects.p("buffer-length1")));
            lst.Add(new TestDescriptor("ssi 05", "{% ssi buffer-length11 %}", ContextObjects.p("value", "VALUE"), ContextObjects.p("buffer-length11")));

            // template tag
            lst.Add(new TestDescriptor("templatetag-01", "{% templatetag openblock %} templatetag openblock {% templatetag closeblock %}", ContextObjects.empty, ContextObjects.p("{% templatetag openblock %}")));

            // WidthRatio tag
            lst.Add(new TestDescriptor("widthratio-01", "{% widthratio value maxValue 100 %}", ContextObjects.p("value", 175, "maxValue", "200"), ContextObjects.p("88")));

            // With tag
            lst.Add(new TestDescriptor("with-01", "{% with var.otherclass.method as newvar %}{{ newvar }}{% endwith %} {{ newvar }}", ContextObjects.p("var", new ContextObjects.SomeClass(), "newvar", "newvar"), ContextObjects.p("OtherClass.method newvar")));

            // URL tag
            lst.Add(new TestDescriptor("url-01", "{% url \"hello/{0}\" parm1 %}", ContextObjects.p("parm1", "world"), ContextObjects.p("/appRoot/hello/world")));
            lst.Add(new TestDescriptor("url-02", "{% url \"hello/{0}/{1}\" parm1, parm2%}", ContextObjects.p("parm1", "new", "parm2", "world"), ContextObjects.p("/appRoot/hello/new/world")));
            lst.Add(new TestDescriptor("url-03", "no {% url \"hello/{0}/{1}\" parm1, parm2 as foo %}url, then {{ foo }}", ContextObjects.p("parm1", "new", "parm2", "world"), ContextObjects.p("no url, then /appRoot/hello/new/world")));

            // nested simple tag implementation
            lst.Add(new TestDescriptor("simple-nested-tag-01", "{% nested p1 %}{% spaceless %}templatetag<h1>  \r\n   </h1> !\r\n <h2> </h2>{% endspaceless %}{% endnested %}", ContextObjects.p("p1", "parm1"), ContextObjects.p(typeof(Interfaces.SyntaxException))));
            lst.Add(new TestDescriptor("simple-nested-tag-02", "{% nested p1 \"p2\" %}{% spaceless %}templatetag<h1>  \r\n   </h1> !\r\n <h2> </h2>{% endspaceless %}{% endnested %}woo", ContextObjects.p("p1", "parm1"), ContextObjects.p("parm1p2starttemplatetag<h1></h1> !\r\n <h2></h2>endwoo")));

            // non-nested simple tag implementation
            lst.Add(new TestDescriptor("simple-non-nested-tag-01", "{% non-nested p1 \"p2\" %}woo", ContextObjects.p("p1", "parm1"), ContextObjects.p("parm1p2woo")));

            return lst;
        }
    }
}
