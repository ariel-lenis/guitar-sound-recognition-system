using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI
{
    public class TsGroupElement
    {
        public enum EGroupType { Union, Ligadura };
        public EGroupType GroupType { get; set; }
        public List<Elements.TsElement> Elements { get; set; }


        public bool IsStart(Elements.TsElement element)
        {
            if (this.Elements.Count == 0) return false;
            return this.Elements[0] == element;
        }

        public bool IsEnd(Elements.TsElement element)
        {
            if (this.Elements.Count == 0) return false;
            return this.Elements[this.Elements.Count-1] == element;
        }


    }
}
