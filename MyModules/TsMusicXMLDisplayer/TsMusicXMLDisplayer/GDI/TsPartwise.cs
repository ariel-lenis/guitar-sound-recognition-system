using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D=System.Drawing;

namespace TsMusicXMLDisplayer.GDI
{
    public class TsPartwise: Drawing.ITsDrawable
    {
        public string Title { get; set; }
        public string Autor { get; set; }
        public string Copyright { get; set; }
        public int BeatsPerMinute { get; set; }

        public List<TsMeasure> Measures { get; set; }

        public List<Drawing.TsLineOnPaper> Lines { get; set; }

        public List<Drawing.TsGDIPaper> Papers { get; set; }
        
        Drawing.TsDrawingEnviroment enviroment;
        Drawing.TsDrawEngine engine;

        Drawing.TsGDITheDocument thedocument;
        

        public Drawing.TsGDITheDocument TheDocument { get { return thedocument; } }

        public Drawing.TsDrawEngine TheEngine { get { return this.engine; } }

        public TsPartwise(/*Drawing.TsDrawingEnviroment enviroment*/)
        {
            //this.enviroment = enviroment;
            this.Measures = new List<TsMeasure>();
            this.Lines = new List<Drawing.TsLineOnPaper>();
            this.Papers = new List<Drawing.TsGDIPaper>();
            Drawing.TsDrawEngine engine = new Drawing.TsDrawEngine();
            //engine.
            //this.thedocument = new Drawing.TsGDITheDocument();
        }

        public Drawing.TsGDITheDocument CreateDocument(Drawing.TsDrawingEnviroment enviroment)
        {
            this.enviroment = enviroment;
            engine = new Drawing.TsDrawEngine();
            engine.Enviroment = this.enviroment;
            
            engine.FontM = new MusicalFont();
            engine.FontM.SetSize(enviroment.HeightSpace);
            engine.Symbols = new Elements.TsSymbols(engine.FontM);

            this.thedocument = new Drawing.TsGDITheDocument(engine);
            this.thedocument.Partwise = this;

            return this.thedocument;
        }

        public Elements.TsClef FindClef(TsMeasure beforeof)
        {
            for (int i = 0; this.Measures[i] != beforeof && i < this.Measures.Count; i++)
                if (this.Measures[i].Clef != null)
                    return this.Measures[i].Clef;
            return null;
        }

        public TsMeasure FindMeasureWith(bool clef, bool armor, bool time)
        {
            if(this.Measures==null || this.Measures.Count==0) return null;
            foreach (var i in this.Measures)
            {
                if (clef && i.Clef != null) return i;
                if (armor && i.Armor != null) return i;
                if (time && i.Time != null) return i;
            }
            return null;
        }


        public void Draw(TsMeasure measure, Drawing.TsDrawEngine param)
        {
            /*
            param.Cursor = new Drawing.TsCursor(this.enviroment);
            param.Graphics.Clear(D.Color.White);

            foreach (var iline in Lines)
                iline.Draw(null, param);
             * */
        }


        public void CalculateCachePosition(Drawing.TsDrawEngine param, Dictionary<string, object> additional)
        {
            //Calculate the cache of all measures first...
            foreach (var i in this.Measures)
                i.CalculateCachePosition(param,additional);

            //First Paper


            float usablew = param.Enviroment.W - param.Enviroment.MarginLeft - param.Enviroment.MarginRight;
            int maxmeasuresperline = 10;
            float minmeasurewidth = usablew / maxmeasuresperline;

            List<double> sums = new List<double>();
            foreach (var imeasure in this.Measures)
                sums.Add(imeasure.MinRecommendedWidth(param));

            List<MeasuringLine> lines = new List<MeasuringLine>();
            this.Lines = new List<Drawing.TsLineOnPaper>();

            MeasuringLine cline = null;
            Drawing.TsLineOnPaper pline = null;
            double currentw=0;

            for(int i=0;i<Measures.Count;i++)
            {
                double mw = sums[i];
                if(mw<minmeasurewidth)
                    mw = minmeasurewidth;

                if (currentw == 0 || currentw + mw > usablew)
                {
                    cline = new MeasuringLine();
                    cline.Add(new MeasuringMeasure() { Measure = this.Measures[i], Mw = (float)mw });

                    pline = new Drawing.TsLineOnPaper(this);
                    pline.LineNumber = lines.Count-1;
                    pline.MeasureIdx = i;
                    pline.MeasureCount = 1;

                    this.Lines.Add(pline);

                    lines.Add(cline);
                    currentw = mw;
                }
                else
                {
                    pline.MeasureCount++;
                    cline.Add(new MeasuringMeasure() { Measure = this.Measures[i], Mw = (float)mw });
                    currentw += mw;
                }
                this.Measures[i].Line = pline;
            }

            float[] H = new float[lines.Count];

            float currentY = param.Enviroment.MarginTop;
            float currentH = 0;

            float informationH = param.Enviroment.HeightSpace * 5;



            int idx = 0;
            
            float maximunH = param.Enviroment.H - param.Enviroment.MarginTop - param.Enviroment.MarginBottom;
            float space = param.Enviroment.HeightSpace;

            

            

            for (int i = 0; i < lines.Count; i++)
            {
                H[i] = lines[i].MaxHeight();

                if (i == 0 || currentH + H[i]+space > maximunH)
                {
                    Drawing.TsGDIPaper paper = new Drawing.TsGDIPaper(this,this.Papers.Count+1);
                    paper.CalculateCachePosition(param, null);
                    this.Papers.Add(paper);
                    paper.CacheLinesIdx = i;


                    if (this.Papers.Count == 1)
                    {
                        currentY += informationH;
                        currentH += informationH;
                    }
                    else
                    {
                        currentY = param.Enviroment.MarginTop;
                        currentH = 0;
                    }
                }
                this.Lines[i].Paper = this.Papers.Last();
                this.Papers.Last().CacheLinesCount++;

                float originY = lines[i].MaxOrigin();
                float centerY = currentY + /*H[i] / 2*/ originY;
                float currentX = 0;

                currentH += H[i] + space;

                this.Lines[i].SetDimensions(param,param.Enviroment.MarginLeft, currentY,usablew, H[i]);

                currentY = centerY + H[i] - originY + param.Enviroment.HeightSpace;  /*+= H[i] + param.Enviroment.HeightSpace*/ ;

                for(int j=0;j<lines[i].Measures.Count;j++)
                {
                    float measurew = lines[i].WidthPercent(j) * usablew;

                    this.Measures[idx].AdaptPosition(param,j==0, currentX, centerY * 0 + originY, measurew);

                    currentX += measurew;
                    idx++;
                }
            }

            
        }

        class MeasuringMeasure
        {
            public TsMeasure Measure{get;set;}
            public float Mw { get; set; }
            
        }

        class MeasuringLine
        {
            public List<MeasuringMeasure> Measures { get;set; }
            public float W { get; set; }

            public MeasuringLine()
            {
                this.Measures = new List<MeasuringMeasure>();
                this.W = 0;
            }
            public float MaxOrigin()
            {
                return Measures.Max(x => x.Measure.CacheGraphics[0].Origin.Y);
            }
            public float MaxHeight()
            {
                Tools.TsSuperRectangle sr = new Tools.TsSuperRectangle();
                foreach (var imeasure in Measures)
                    sr.AddRectangle(imeasure.Measure.CacheGraphics[0].Rectangle, imeasure.Measure.CacheGraphics[0].Origin);
                return sr.Rectangle.Height;
            }
            public void Add(MeasuringMeasure ms)
            {
                this.Measures.Add(ms);
                this.W += ms.Mw;
            }

            public float WidthPercent(int idx)
            {
                return Measures[idx].Mw / this.W;
            }

        }

        public List<Drawing.TsCacheGraphics> CacheGraphics
        {
            get { throw new NotImplementedException(); }
        }
    }
}
