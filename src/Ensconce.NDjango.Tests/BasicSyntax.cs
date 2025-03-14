using Ensconce.NDjango.Core;
using Ensconce.NDjango.Tests.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Ensconce.NDjango.Tests
{
    public partial class TestsRunner
    {
        [Test, TestCaseSource(nameof(GetBasicTests))]
        public void BasicSyntax(TestDescriptor test)
        {
            test.Run(manager);
        }

        private static IList<TestDescriptor> GetBasicTests()
        {
            IList<TestDescriptor> lst = new List<TestDescriptor>();
            //*
            // Plain text should go through the template parser untouched
            lst.Add(new TestDescriptor("basic-syntax01", "something cool", null, ContextObjects.p("something cool")));
            // Variables should be replaced with their value in the current context
            lst.Add(new TestDescriptor("basic-syntax02", "{{ headline }}", ContextObjects.p("headline", "Success"), ContextObjects.p("Success"), "headline"));
            Guid guid = Guid.NewGuid();
            lst.Add(new TestDescriptor("basic-syntax02", "{{ headline }}", ContextObjects.p("headline", guid), ContextObjects.p(guid.ToString()), "headline"));

            // More than one replacement variable is allowed in a template
            lst.Add(new TestDescriptor("basic-syntax03", "{{ first }} --- {{ second }}", ContextObjects.p("first", 1, "second", 2),
                ContextObjects.p("1 --- 2"), "first", "second"));
            int? p2 = null;
            lst.Add(new TestDescriptor("basic-syntax03-1", "{{ first }} --- {{ second }}", ContextObjects.p("first", null, "second", p2), ContextObjects.p(" --- ")));

            // Fail silently when a variable is not found in the current context
            lst.Add(new TestDescriptor("basic-syntax04", "as{{ missing }}df", ContextObjects.empty, ContextObjects.p("asdf", "asINVALIDdf")));

            // A variable may not contain more than one word
            lst.Add(new TestDescriptor("basic-syntax06", "{{ multi word variable }}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));

            // A variable may has to have a reference variable first
            lst.Add(new TestDescriptor("basic-syntax06-1", "{{ |filter }}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));

            // Raise TemplateSyntaxError for ContextObjects.empty variable tags
            lst.Add(new TestDescriptor("basic-syntax07", "{{ }}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));
            lst.Add(new TestDescriptor("basic-syntax08", "{{        }}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));

            // Attribute syntax allows a template to call an object's attribute
            lst.Add(new TestDescriptor("basic-syntax09", "{{ var.method }}", ContextObjects.p("var", new ContextObjects.SomeClass()), ContextObjects.p("SomeClass.method")));

            // Multiple levels of attribute access are allowed
            lst.Add(new TestDescriptor("basic-syntax10", "{{ var.otherclass.method }}", ContextObjects.p("var", new ContextObjects.SomeClass()),
                ContextObjects.p("OtherClass.method"), "var.otherclass.method"));

            // Multiple levels of attribute access are allowed
            lst.Add(new TestDescriptor("basic-syntax10-l", "{{ var.classList.0.method }} {% for oc in var.classList %}{{forloop.counter}} = {{oc.method}};{%endfor%}", ContextObjects.p("var", new ContextObjects.SomeClass()),
                ContextObjects.p("Instance 1 1 = Instance 1;2 = Instance 2;"), "var.otherclass.method"));

            // Fail silently when a variable's attribute isn't found
            lst.Add(new TestDescriptor("basic-syntax11", "{{ var.blech }}", ContextObjects.p("var", new ContextObjects.SomeClass()), ContextObjects.p("", "INVALID")));

            // Raise TemplateSyntaxError when trying to access a variable beginning with an underscore
            //lst.Add(new TestDescriptor("basic-syntax12", "{{ var.__dict__ }}", ContextObjects.p("var", new ContextObjects.SomeClass()), ContextObjects.p(typeof(SyntaxException))));

            // Raise TemplateSyntaxError when trying to access a variable containing an illegal character
            lst.Add(new TestDescriptor("basic-syntax13", "{{ va>r }}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));
            lst.Add(new TestDescriptor("basic-syntax14", "{{ (var.r) }}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));
            lst.Add(new TestDescriptor("basic-syntax15", "{{ sp%am }}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));
            lst.Add(new TestDescriptor("basic-syntax16", "{{ eggs! }}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));
            lst.Add(new TestDescriptor("basic-syntax17", "{{ moo? }}", ContextObjects.empty, ContextObjects.p(typeof(Interfaces.SyntaxException))));

            var d = new Dictionary<string, string>();
            d.Add("bar", "baz");

            // Attribute syntax allows a template to call a dictionary key's value
            lst.Add(new TestDescriptor("basic-syntax18", "{{ foo.bar }}", ContextObjects.p("foo", d), ContextObjects.p("baz")));

            // Fail silently when a variable's dictionary key isn't found
            lst.Add(new TestDescriptor("basic-syntax19", "{{ foo.spam }}", ContextObjects.p("foo", d), ContextObjects.p("", "INVALID")));

            // Fail silently when accessing a non-simple method
            lst.Add(new TestDescriptor("basic-syntax20", "{{ var.method2 }}", ContextObjects.p("var", new ContextObjects.SomeClass()), ContextObjects.p("", "INVALID")));

            // Don't get confused when parsing something that is almost, but not
            // quite, a template tag.
            lst.Add(new TestDescriptor("basic-syntax21", "a {{ moo %} b", ContextObjects.empty, ContextObjects.p("a {{ moo %} b")));
            lst.Add(new TestDescriptor("basic-syntax22", "{{ moo //}", ContextObjects.empty, ContextObjects.p("{{ moo //}")));

            // Will try to treat "moo //} {{ cow" as the variable. Not ideal, but
            // costly to work around, so this triggers an error.
            lst.Add(new TestDescriptor("basic-syntax23", "{{ moo //} {{ cow }}", ContextObjects.p("cow", "cow"), ContextObjects.p(typeof(Interfaces.SyntaxException))));

            // Embedded newlines make it not-a-tag.
            lst.Add(new TestDescriptor("basic-syntax24", "{{ moo\n }}", ContextObjects.empty, ContextObjects.p("{{ moo\n }}")));

            // Literal strings are permitted inside variables, mostly for i18n
            // purposes.
            lst.Add(new TestDescriptor("basic-syntax25", "{{ \"fred\" }}", ContextObjects.empty, ContextObjects.p("fred")));
            lst.Add(new TestDescriptor("basic-syntax25-sq1", "{{ '\"fred\"' }}", ContextObjects.empty, ContextObjects.p("\"fred\"")));
            // No unescape for double-quote inside a single quoted string
            lst.Add(new TestDescriptor("basic-syntax25-sq2", "{{ 'fr\"ed' }}", ContextObjects.empty, ContextObjects.p("fr\"ed")));
            lst.Add(new TestDescriptor("basic-syntax26", "{% autoescape off %}{{ \"\\\"fred\\\"\" }}{% endautoescape %}", ContextObjects.empty, ContextObjects.p("\"fred\"")));
            lst.Add(new TestDescriptor("basic-syntax27", "{{ _(\"\\\"fred\\\"\") }}", ContextObjects.empty, ContextObjects.p("\"fred\"")));

            // List-index syntax allows a template to access a certain item of a subscriptable object.
            lst.Add(new TestDescriptor("list-index01", "{{ var.1 }}", ContextObjects.p("var", ContextObjects.list("first item", "second item")), ContextObjects.p("second item")));

            // Fail silently when the list index is out of range.
            lst.Add(new TestDescriptor("list-index02", "{{ var.5 }}", ContextObjects.p("var", ContextObjects.list("first item", "second item")), ContextObjects.p("", "INVALID")));

            // Fail silently when the variable is not a subscriptable object.
            lst.Add(new TestDescriptor("list-index03", "{{ var.1 }}", ContextObjects.p("var", new ContextObjects.SomeClass()), ContextObjects.p("", "INVALID")));

            // Fail silently when variable is a dict without the specified key.
            lst.Add(new TestDescriptor("list-index04", "{{ var.1 }}", ContextObjects.p("var", ContextObjects.dict()), ContextObjects.p("", "INVALID")));

            // Dictionary lookup wins out when dict's key is a string.
            lst.Add(new TestDescriptor("list-index05", "{{ var.1 }}", ContextObjects.p("var", ContextObjects.dict("1", "hello")), ContextObjects.p("hello")));

            // But list-index lookup wins out when dict's key is an int, which
            // behind the scenes is really a dictionary lookup (for a dict)
            // after converting the key to an int.
            lst.Add(new TestDescriptor("list-index06", "{{ var.1 }}", ContextObjects.p("var", ContextObjects.dict(1, "hello")), ContextObjects.p("hello")));

            // Dictionary lookup wins out when there is a string and int version of the key.
            lst.Add(new TestDescriptor("list-index07", "{{ var.1 }}", ContextObjects.p("var", ContextObjects.dict("1", "hello", 1, "world")), ContextObjects.p("hello")));

            return lst;
        }
    }
}
