using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D=System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public abstract class TsElement : Drawing.ITsDrawable
    {
        //List<TsElement> AllElements
        public TsMeasure Measure { get; set; }
        public Drawing.TsCacheGraphics CachePosition { get; set; }
        public List<TsGroupElement> Groups { get; set; }

        public bool Innerit { get; set; }

        public TsElement(TsMeasure measure)
        {
            this.Measure = measure;
        }
        public abstract void Draw(TsMeasure measure, Drawing.TsDrawEngine param);
        public abstract void CalculateCachePosition(Drawing.TsDrawEngine param, Dictionary<string, object> additional);
        public abstract List<Drawing.TsCacheGraphics> CacheGraphics { get; set; }

        public virtual List<TsNoteData> RequiredNoteData { get { return null; } }
    }
}
