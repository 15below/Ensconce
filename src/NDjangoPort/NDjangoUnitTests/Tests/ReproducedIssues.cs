using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NDjango.UnitTests.Data;

namespace NDjango.UnitTests
{
    public partial class Tests
    {
 //       [Test, TestCaseSource("GetReproducedIssues")]
        public void ReproducedIssues(TestDescriptor test)
        {
            test.Run(manager);
        }

        private static List<string> GetAHrefs(int amount)
        {
            StringBuilder sbBisBig = new StringBuilder();
            StringBuilder sbBisBigResult = new StringBuilder();

            for (int i = 0; i < amount; i++)
            {
                sbBisBig.Append(@"
                    <a href=""{{ root }}/script/visualizer/{{ revision.DrugCategoryId|default:""0"" }}/?scriptRevisionId={{ revision.ScriptRevisionId|default:""0"" }}&amp;drugId={{ revision.DrugId|default:""0"" }} &amp;entryPointId={{ revision.EntryPointId|default:""0"" }} &amp;scriptRevisionType={{ revision.ScriptRevisionType|default:""0"" }}"">View</a>&nbsp;&nbsp; 
                    <a href=""{{ root }}/script/create/{{ revision.DrugCategoryId|default:""0"" }}/?scriptRevisionId={{ revision.ScriptRevisionId|default:""0"" }}&amp;drugId={{ revision.DrugId|default:""0"" }} &amp;entryPointId={{ revision.EntryPointId|default:""0"" }} &amp;scriptRevisionType={{ revision.ScriptRevisionType|default:""0"" }}"">Edit</a> 
");
                sbBisBigResult.Append(@"
                    <a href=""http://www.hill30.com/script/visualizer/12/?scriptRevisionId=123&amp;drugId=1234 &amp;entryPointId=12345 &amp;scriptRevisionType=123456"">View</a>&nbsp;&nbsp; 
                    <a href=""http://www.hill30.com/script/create/12/?scriptRevisionId=123&amp;drugId=1234 &amp;entryPointId=12345 &amp;scriptRevisionType=123456"">Edit</a> 
");
            }
            List<string> retList = new List<string>(2);

            retList.Add(sbBisBig.ToString());
            retList.Add(sbBisBigResult.ToString());

            return retList;
        }


        public static IList<TestDescriptor> GetReproducedIssues()
        {
            IList<TestDescriptor> lst = new List<TestDescriptor>();




            List<string> lstIssue219bisBig = GetAHrefs(5000);



            lst.Add(new TestDescriptor("issue219-bisBig",
                lstIssue219bisBig[0],
                ContextObjects.p(
                    "root", "http://www.hill30.com",
                    "revision", ContextObjects.dictStr("DrugCategoryId", 12,
                                                    "ScriptRevisionId", 123,
                                                    "DrugId", 1234,
                                                    "EntryPointId", 12345,
                                                    "ScriptRevisionType", 123456)), ContextObjects.p(lstIssue219bisBig[1])));



            List<string> lstIssue219 = GetAHrefs(40);


            lst.Add(new TestDescriptor("issue219",
                lstIssue219[0],
                ContextObjects.p(
                    "root", "http://www.hill30.com",
                    "revision", ContextObjects.dictStr("DrugCategoryId", 12,
                                                    "ScriptRevisionId", 123,
                                                    "DrugId", 1234,
                                                    "EntryPointId", 12345,
                                                    "ScriptRevisionType", 123456)),
                ContextObjects.p(lstIssue219[1])));

            List<string> lstIssue219Bis = GetAHrefs(4);

            lst.Add(new TestDescriptor("issue219-bis",
                lstIssue219Bis[0],
    ContextObjects.p(
        "root", "http://www.hill30.com",
        "revision", ContextObjects.dictStr("DrugCategoryId", 12,
                                        "ScriptRevisionId", 123,
                                        "DrugId", 1234,
                                        "EntryPointId", 12345,
                                        "ScriptRevisionType", 123456)), ContextObjects.p(lstIssue219Bis[1])));

            return lst;

        }
    }

}
