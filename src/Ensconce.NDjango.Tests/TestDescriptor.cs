using Ensconce.NDjango.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ensconce.NDjango.Tests
{
    public struct DesignerData
    {
        public DesignerData(int position, int length, string[] values, int severity, string errorMessage)
        {
            this.Position = position;
            this.Length = length;
            this.Values = values;
            this.Severity = severity;
            this.ErrorMessage = errorMessage;
        }

        public int Position;
        public int Length;
        public string[] Values;
        public int Severity;
        public string ErrorMessage;
    }

    public class TestDescriptor
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public object[] ContextValues { get; set; }
        public object[] Result { get; set; }
        public List<DesignerData> ResultForDesigner { get; set; }
        public string[] Vars { get; set; }
        public int RecursionDepth { get; set; }

        private ResultGetter resultGetter;

        public override string ToString()
        {
            return Name;
        }

        public TestDescriptor(string name, string template, DesignerData[] designResult)
        {
            Name = name;
            Template = template;
            ResultForDesigner = new List<DesignerData>(designResult);
            RecursionDepth = 2;
        }

        public TestDescriptor(string name, string template, DesignerData[] designResult, int modelDepth)
        {
            Name = name;
            Template = template;
            ResultForDesigner = new List<DesignerData>(designResult);
            RecursionDepth = modelDepth;
        }

        public TestDescriptor(string name, string template, object[] values, object[] result, List<DesignerData> designResult, params string[] vars)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            Result = result;
            Vars = vars;
            ResultForDesigner = designResult;
        }

        public TestDescriptor(string name, string template, object[] values, object[] result, params string[] vars)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            Result = result;
            Vars = vars;
        }

        public delegate object[] ResultGetter();

        public TestDescriptor(string name, string template, object[] values, ResultGetter resultGetter, params string[] vars)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            this.resultGetter = resultGetter;
            Vars = vars;
        }

        public string RunTemplate(Interfaces.ITemplateManager manager, string templateName, IDictionary<string, object> context)
        {
            string retStr = "";
            for (int i = 0; i < 1; i++)
            {
                var template = manager.RenderTemplate(templateName, context);
                retStr = template.ReadToEnd();
            }
            return retStr;
        }

        public class SimpleNonNestedTag : SimpleTag
        {
            public SimpleNonNestedTag() : base(false, "non-nested", 2)
            {
            }

            public override string ProcessTag(Interfaces.IContext context, string contents, object[] parms)
            {
                StringBuilder res = new StringBuilder();
                foreach (object o in parms)
                    res.Append(o);

                return res
                    .Append(contents)
                    .ToString();
            }
        }

        public class SimpleNestedTag : SimpleTag
        {
            public SimpleNestedTag() : base(true, "nested", 2)
            {
            }

            public override string ProcessTag(Interfaces.IContext context, string contents, object[] parms)
            {
                StringBuilder res = new StringBuilder();
                foreach (object o in parms)
                    res.Append(o);

                return res
                    .Append("start")
                    .Append(contents)
                    .Append("end")
                    .ToString();
            }
        }

        public void Run(Interfaces.ITemplateManager manager)
        {
            if (ResultForDesigner != null)
            {
                ValidateSyntaxTree(manager);
                return;
            }

            var context = new Dictionary<string, object>();

            if (ContextValues != null)
                for (int i = 0; i <= ContextValues.Length - 2; i += 2)
                    context.Add(ContextValues[i].ToString(), ContextValues[i + 1]);

            try
            {
                if (resultGetter != null)
                    Result = resultGetter();
                Assert.AreEqual(Result[0], RunTemplate(manager, Template, context), "** Invalid rendering result");
                //if (Vars.Length != 0)
                //    Assert.AreEqual(Vars, manager.GetTemplateVariables(Template), "** Invalid variable list");
            }
            catch (Exception ex)
            {
                // Result[0] is either some object, in which case this shouldn't have happened
                // or it's the type of the exception the calling code expects.
                if (resultGetter != null)
                    Result = resultGetter();

                if (Result[0] is Type)
                {
                    Assert.AreEqual(Result[0], ex.GetType(), "Exception: " + ex.Message);
                }
                else
                {
                    throw;
                }
            }
        }

        private void ValidateSyntaxTree(Interfaces.ITemplateManager manager)
        {
            var template = manager.GetTemplate(Template, new TestTyperesolver(),
                new TypeResolver.ModelDescriptor(new TypeResolver.IDjangoType[]
                    {
                        new TypeResolver.CLRTypeDjangoType("Standard", typeof(EmptyClass))
                    }));

            //the same logic responsible for retriving nodes as in NodeProvider class (DjangoDesigner).
            var nodes = GetNodes(template.Nodes.ToList().ConvertAll
                (node => (Interfaces.INode)node)).FindAll(node =>
                    (node is Interfaces.ICompletionValuesProvider)
                    || (node.NodeType == Interfaces.NodeType.ParsingContext)
                    || (node.ErrorMessage.Message != ""));
            var actualResult = nodes.ConvertAll(
                node =>
                {
                    var valueProvider = node as Interfaces.ICompletionValuesProvider;
                    var values =
                        valueProvider == null ?
                            new List<string>()
                            : valueProvider.Values;
                    var contextValues = new List<string>(values);
                    switch (node.NodeType)
                    {
                        case Interfaces.NodeType.ParsingContext:
                            contextValues.InsertRange(0, (node.Context.TagClosures));
                            return new DesignerData(node.Position, node.Length, contextValues.ToArray(), node.ErrorMessage.Severity, node.ErrorMessage.Message);

                        case Interfaces.NodeType.Reference:
                            return new DesignerData(node.Position, node.Length, GetModelValues(node.Context.Model, RecursionDepth).ToArray(), node.ErrorMessage.Severity, node.ErrorMessage.Message);

                        default:
                            return new DesignerData(node.Position, node.Length, new List<string>(values).ToArray(), node.ErrorMessage.Severity, node.ErrorMessage.Message);
                    }
                });

            for (var i = 0; i < actualResult.Count; i++)
            {
                Assert.AreEqual(ResultForDesigner[i].Length, actualResult[i].Length, "Invalid Length");
                Assert.AreEqual(ResultForDesigner[i].Position, actualResult[i].Position, "Invalid Position");
                Assert.AreEqual(ResultForDesigner[i].ErrorMessage, actualResult[i].ErrorMessage, "Invalid ErrorMessage");
                Assert.AreEqual(ResultForDesigner[i].Severity, actualResult[i].Severity, "Invalid Severity");
                Assert.AreEqual(ResultForDesigner[i].Values.OrderBy(x => x), actualResult[i].Values.OrderBy(x => x), "Invalid Values Array " + i);
            }

            Assert.AreEqual(ResultForDesigner.Count(), actualResult.Count);
        }

        private static List<string> GetModelValues(TypeResolver.IDjangoType model, int recursionDepth)
        {
            List<string> result = new List<string>();
            int remainingSteps = recursionDepth - 1;
            foreach (var member in model.Members)
            {
                if (remainingSteps > 0)
                {
                    result.Add(member.Name);
                    result.AddRange(GetModelValues(member, remainingSteps));
                }
                else
                {
                    result.Add(member.Name);
                }
            }
            return result;
        }

        //the same logic responsible for retriving nodes as in NodeProvider class (DjangoDesigner).
        private static List<Interfaces.INode> GetNodes(IEnumerable<Interfaces.INode> nodes)
        {
            List<Interfaces.INode> result = new List<Interfaces.INode>();

            foreach (Interfaces.INode ancestor in nodes)
            {
                result.Add(ancestor);
                foreach (IEnumerable<Interfaces.INode> list in ancestor.Nodes.Values)
                {
                    result.AddRange(GetNodes(list));
                }
            }
            return result;
        }

        //the same list as in Defaults.standardTags
    }
}
