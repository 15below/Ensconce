using System.Collections.Generic;
using System.Linq;

namespace FifteenBelow.Deployment.Update
{
    public static class DictionaryExtensions
    {
        public static void AddOrDiscard<TK, TV>(this Dictionary<TK, TV> dictionary, TK key, TV value)
        {
            if (!dictionary.Keys.Contains(key))
                dictionary.Add(key, value);
        }
    }
}