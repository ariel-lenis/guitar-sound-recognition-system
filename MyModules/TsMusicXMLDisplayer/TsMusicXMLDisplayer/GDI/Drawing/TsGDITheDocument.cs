using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class TsGDITheDocument:Drawing.ITsDrawable
    {
        List<TsGDIPaper> papers;
        List<TsGDIPaper> Papers { get { return partwise.Papers; } }

        TsPartwise partwise;
        Drawing.TsDrawEngine engine;

        public TsPartwise Partwise { get { return partwise; } set { this.partwise = value; this.papers = value.Papers; } }

        public TsGDITheDocument(Drawing.TsDrawEngine engine)
        { 
            //this.partwise = partwise;
            this.engine = engine;
            //this.RecreatePapers();
        }

        public void CachePapers()
        { 
            
        }

        public void Draw(TsMeasure measure, TsDrawEngine param)
        {
            foreach (var ipaper in this.papers)
                ipaper.Draw(null, param);
        }

        public void CalculateCachePosition(TsDrawEngine param, Dictionary<string, object> additional)
        {
            //this.partwise.CalculateCachePosition(param, additional);
            //this.papers = this.partwise.Papers;

            foreach (var ipaper in this.papers)
                ipaper.CacheGroups(param,additional);
        }

        public List<TsCacheGraphics> CacheGraphics
        {
            get { return null; }
        }

        public void LoadMusicXML(Parser.ParserMusicXML parser)
        {
            this.partwise = parser.Parse(/*this.engine.Enviroment, this*/);
        }
    }
}
