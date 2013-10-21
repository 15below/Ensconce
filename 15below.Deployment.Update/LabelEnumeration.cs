using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FifteenBelow.Deployment.Update
{
    public class LabelEnumeration : IDictionary<string, IDictionary<string, object>>, IEnumerable<IDictionary<string, object>>
    {
        private readonly IDictionary<string, IDictionary<string, object>> dict = new Dictionary<string, IDictionary<string, object>>();
        
        IEnumerator<IDictionary<string, object>> IEnumerable<IDictionary<string, object>>.GetEnumerator()
        {
            return dict.Select((kv) => kv.Value).GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, IDictionary<string, object>>> IEnumerable<KeyValuePair<string, IDictionary<string, object>>>.GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.Select((kv) => kv.Value).GetEnumerator();
        }

        public void Add(KeyValuePair<string, IDictionary<string, object>> item)
        {
            dict.Add(item);
        }

        public void Clear()
        {
            dict.Clear();
        }

        public bool Contains(KeyValuePair<string, IDictionary<string, object>> item)
        {
            return dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IDictionary<string, object>>[] array, int arrayIndex)
        {
            dict.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, IDictionary<string, object>> item)
        {
            return dict.Remove(item);
        }

        public int Count { get; private set; }
        public bool IsReadOnly { get; private set; }
        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        public void Add(string key, IDictionary<string, object> value)
        {
            dict.Add(key, value);
        }

        public bool Remove(string key)
        {
            return dict.Remove(key);
        }

        public bool TryGetValue(string key, out IDictionary<string, object> value)
        {
            return dict.TryGetValue(key, out value);
        }

        public IDictionary<string, object> this[string key]
        {
            get { return dict[key]; }
            set { dict[key] = value; }
        }

        public ICollection<string> Keys { get; private set; }
        public ICollection<IDictionary<string, object>> Values { get; private set; }
    }
}
