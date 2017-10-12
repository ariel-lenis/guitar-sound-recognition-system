using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsPentagramEngine.MXMLC
{
    public class UnrestrictedDictionary<T,Q>
    {
        List<T> keys;
        List<Q> values;

        public List<T> Keys { get { return keys; } }
        public List<Q> Values { get { return values; } }

        public UnrestrictedDictionary()
        {
            keys = new List<T>();
            values = new List<Q>();
        }

        public void Add(T key, Q value)
        {
            keys.Add(key);
            values.Add(value);
        }
    }
}
