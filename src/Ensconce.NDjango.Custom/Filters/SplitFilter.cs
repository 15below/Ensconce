using Ensconce.NDjango.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ensconce.NDjango.Custom.Filters
{
    [Interfaces.Name("split")]
    public class SplitFilter : Interfaces.IFilter
    {
        public object Perform(object value)
        {
            throw new NotImplementedException();
        }

        public object PerformWithParam(object value, object parameter)
        {
            if (!(parameter is string s) || String.IsNullOrEmpty(s))
            {
                throw new Exception($"split parameter must be a non-empty string");
            }
            else if (value is ErrorTemplate)
            {
                throw new Exception($"The value to be split is an ndjango error");
            }
            else
            {
                var stringValue = (value ?? "").ToString();
                var delimiter = (parameter ?? "").ToString();
                
                // Split the string on the delimiter and return as IEnumerable
                var splitResults = stringValue.Split(new string[] { delimiter }, StringSplitOptions.None);
                return splitResults.AsEnumerable();
            }
        }

        public object DefaultValue
        {
            get { return null; }
        }
    }
}
