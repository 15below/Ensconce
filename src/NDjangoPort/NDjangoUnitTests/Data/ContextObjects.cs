using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace NDjango.UnitTests.Data
{
    class ContextObjects
    {
        public class OtherClass 
        {
            string value = "OtherClass.method";
            public OtherClass() { }
            public OtherClass(string value)
            {
                this.value = value;
            }


            public string method() 
            { 
                return value; 
            } 
        }

        public class SomeClass
        {
            public OtherClass otherclass = new OtherClass();

            public string method() { return "SomeClass.method"; }
            public object method2(object o) { return o; }
            public string method3() { throw new Exception(); }
            public string method4() { throw new ApplicationException(); }

            public List<OtherClass> classList = new List<OtherClass>(new OtherClass[] { new OtherClass("Instance 1"), new OtherClass("Instance 2") });
        }

        public static object[] p(params object[] prs) { return prs; }

        public static int[] range(int max)
        {
            var res = new int[max];
            for (int i = 0; i < max; i++) res[i] = i;

            return res;
        }

        public static int[,] square2(int maxI,int maxJ)
        {
            var res = new int[maxI,maxJ];
            for (int j = 0; j < maxJ; j++)
                for (int i = 0; i < maxI; i++) 
                    res[i,j] = i;

            return res;
        }

        public static string InsertCultureSep(string strFloat)
        {
            return String.Format(strFloat, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        public static List<List<string>> square(int maxI, int maxJ)
        {
            var res = new List<List<string>>(maxI);
            for (int i = 0; i < maxI; i++)
            {
                res.Add(new List<string>(maxJ));
                for (int j = 0; j < maxJ; j++)
                    res[i].Add(i + "-" + j);
            }

            return res;
        }

        public static Dictionary<object, string> dict(params object[] prs)
        {
            Dictionary<object, string> dict = new Dictionary<object, string>();
            for (int i = 0; i <= prs.Length - 2; i += 2)
                dict.Add(prs[i], prs[i + 1].ToString());

            return dict;
        }

        public static Dictionary<string, string> dictStr(params object[] prs)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            for (int i = 0; i <= prs.Length - 2; i += 2)
                dict.Add(prs[i].ToString(), prs[i + 1].ToString());

            return dict;
        }

        public static Dictionary<string, IComparable> dictComp(params object[] prs)
        {
            Dictionary<string, IComparable> dict = new Dictionary<string, IComparable>();
            for (int i = 0; i <= prs.Length - 2; i += 2)
                dict.Add(prs[i].ToString(), (IComparable)(prs[i + 1]));

            return dict;
        }


        public static List<string> list(params string[] prs)
        {
            return new List<string>(prs);
        }

        public static object[] empty { get { return new object[0]; } }

        public class Person
        {
            public Person(string first_name, string last_name, string gender)
            {
                this.first_name = first_name;
                this.last_name = last_name;
                this.gender = gender;
            }
            public string first_name;
            public string last_name;
            public string gender;
        }

    }
}
