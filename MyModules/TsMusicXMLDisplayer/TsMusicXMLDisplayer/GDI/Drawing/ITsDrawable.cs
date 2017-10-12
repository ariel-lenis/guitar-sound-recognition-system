using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D=System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public interface ITsDrawable
    {        
        void Draw(TsMeasure measure,Drawing.TsDrawEngine param);
        void CalculateCachePosition(TsDrawEngine param,Dictionary<string,object> additional);
        List<TsCacheGraphics> CacheGraphics { get; }

    }
}
