using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsPentagramEngine
{
    public static class Extensors
    {
        public static List<int> Notes(this List<InputFormat> notes)
        {
            List<int> res = new List<int>();
            foreach (var inote in notes)
            {
                res.AddRange(inote.MidiNotes);
            }
            return res.Distinct().OrderBy(x=>x).ToList();
        }
    }
}
