using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class TsGDIPaper:Drawing.ITsDrawable
    {
        D.Bitmap bmp;
        D.Graphics gbmp;
        TsPartwise tspartwise;

        public D.Bitmap GetBitmap{ get { return bmp; }}
        public D.Graphics GetGraphics { get { return gbmp; } }

        TsDrawEngine param;
        private TsDrawEngine tsDrawEngine;
        private MusicXML.scorepartwise scorepartwise;
        private int pagenumber;
        //private TsPartwise tsPartwise;

        private int cacheLinesIdx;
        private int cacheLinesCount;

        public TsPartwise TheTsPartwise { get { return tspartwise; } }

        public int PageNumber { get { return this.pagenumber; } }

        public int CacheLinesIdx { get { return this.cacheLinesIdx; } set { this.cacheLinesIdx = value; } }
        public int CacheLinesCount { get { return this.cacheLinesCount; } set { this.cacheLinesCount = value; } }

        public void ReloadSize()
        {

        }

        public TsGDIPaper(TsPartwise tspartwise,int pagenumber)
        {
            this.tspartwise = tspartwise;
            this.pagenumber = pagenumber;
        }


        public void Dispose()
        {
            if (this.bmp != null)
            {
                this.bmp.Dispose();
                this.gbmp.Dispose();
            }
        }


        public void Draw(TsMeasure measure, TsDrawEngine param)
        {
            param.Draw(this);
            //throw new NotImplementedException();
            for (int i = this.cacheLinesIdx; i < this.cacheLinesIdx + this.cacheLinesCount; i++)
                this.TheTsPartwise.Lines[i].Draw(null, param);
        }

        public void CalculateCachePosition(TsDrawEngine param, Dictionary<string, object> additional)
        {
            this.Dispose();
            this.bmp = new D.Bitmap(param.Enviroment.W, param.Enviroment.H);
            this.gbmp = D.Graphics.FromImage(this.bmp);
            this.gbmp.Clear(D.Color.White);

            //param.Graphics = gbmp;
            //param.FontM = new MusicalFont();
            //param.FontM.SetSize(param.Graphics, param.Enviroment.HeightSpace);
            //param.Symbols = new Elements.TsSymbols(param.FontM);

            //tspartwise.CalculateCachePosition(param, null);
            //tspartwise.Draw(null, param);
        }

        public List<TsCacheGraphics> CacheGraphics
        {
            get { return null; }
        }

        public void CacheGroups(TsDrawEngine param, Dictionary<string, object> additional)
        {
            for (int i = 0; i < this.cacheLinesCount; i++)
                this.tspartwise.Lines[this.cacheLinesIdx + i].CacheGroups(param, additional);
        }
    }
}
