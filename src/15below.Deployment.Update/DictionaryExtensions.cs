using System.Collections.Generic;
using System.Linq;

namespace FifteenBelow.Deployment.Update
{
    public static class DictionaryExtensions
    {
        public static void AddOrDiscard(this Dictionary<string, object> dictionary, string key, string value, bool idSpecific = false)
        {
            if (!dictionary.Keys.Contains(key)) dictionary.Add(key, value);
            if (dictionary.Keys.Contains(key) && idSpecific) dictionary[key] = value;
        }
    }
}