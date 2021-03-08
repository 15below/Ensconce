using System;
using System.Collections.Generic;

namespace Ensconce.Update
{
    public static class DictionaryExtensions
    {
        public static void AddOrDiscard<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, TV value)
        {
            if (!dictionary.Keys.Contains(key))
            {
                dictionary.Add(key, value);
            }
        }

        public static Lazy<TagDictionary> ToLazyTagDictionary(this IDictionary<string, object> raw)
        {
            return new Lazy<TagDictionary>(() => TagDictionary.FromDictionary(raw));
        }
    }
}
