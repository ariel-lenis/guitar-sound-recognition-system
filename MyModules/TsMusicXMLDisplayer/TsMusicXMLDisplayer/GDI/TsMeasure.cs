using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;


namespace TsMusicXMLDisplayer.GDI
{
    public partial class TsMeasure:Drawing.ITsDrawable
    {
        public enum EModifier { Sharp, Bemol, Normally };
        public enum EScaleMode { Major, Minor };
        public TsPartwise Partwise { get; set; }
        public Elements.TsClef Clef { get; set; }
        public Elements.TsTime Time { get; set; }
        public Elements.TsArmor Armor { get; set; }
        public Dictionary<int, EModifier> CurrentModifiers { get; set; }
        public List<Elements.TsElement> Elements { get; set; }

        public Drawing.TsLineOnPaper Line { get; set; }


        int measureId;

        public int MeasureId { get { return this.measureId; } }
        

        public void AddElement(Elements.TsElement element)
        {
            if (element is Elements.TsClef)
            {
                if (this.Clef != null)
                    throw new Exception("Already have a clef.");
                else
                    this.Clef = element as Elements.TsClef;
            }
            if (element is Elements.TsTime)
            {
                if (this.Time != null)
                    throw new Exception("Already have a time.");
                else
                    this.Time = element as Elements.TsTime;
            }
            if (element is Elements.TsArmor)
            {
                if (this.Armor != null)
                    throw new Exception("Already have an armor.");
                else
                    this.Armor = element as Elements.TsArmor;
            }

            this.Elements.Add(element);
        }

        public TsMeasure(TsPartwise partwise,int idx)
        {
            this.measureId = idx;
            this.Partwise = partwise;
        }

        public int NotePosition(Elements.TsNoteData note)
        {
            int notevalue = (int)note.Note * note.Octave;
            int clefvalue = (int)Clef.Note * Clef.Octave;
            return notevalue - clefvalue;
        }
        
        public void Draw(TsMeasure measure, Drawing.TsDrawEngine param)
        {
            /*
            param.Graphics.DrawRectangle(param.Enviroment.LinesColor.Pen, param.Cursor.X, param.Cursor.Y, 200, 200);
            foreach (var i in Elements)
                i.Draw(param);
            param.Cursor.X += 200;
            */
            param.Draw(this,this);
            foreach (var i in this.Elements)
                i.Draw(this,param);
        }




        public List<Drawing.TsCacheGraphics> CacheGraphics
        {
            get;
            set;
        }

        public void CalculateCachePosition(Drawing.TsDrawEngine param, Dictionary<string, object> additional)
        {
            AssignParameters();
            PrepareExtras();

            additional = new Dictionary<string, object>();
            Tools.TsAlterationsCursor acursor = new Tools.TsAlterationsCursor(this, (this.measureId == 0) ? null : this.Partwise.Measures[this.measureId - 1]);
            additional.Add("AlterationsCursor", additional);

            Tools.TsSuperRectangle sr = new Tools.TsSuperRectangle();

            for (int i = 0; i < this.Elements.Count;i++ )
            {
                var ielement = this.Elements[i];                
                ielement.CalculateCachePosition(param, additional);
                foreach (var j in ielement.CacheGraphics)
                {
                    var rectangle = j.Rectangle;
                    j.Rectangle = rectangle;
                    sr.AddRectangle(rectangle);
                }
            }

            //float y5 = param.PositionOfLine(5);
            //float y1 = param.PositionOfLine(1);

            float y5 = param.PositionOfLine(8);
            float y1 = param.PositionOfLine(-2);


           
            sr.AddRectangle(new D.RectangleF() { 
                //Y = param.PositionOfLine(5),
                //Height = y1-y5
                Y = param.PositionOfLine(8),
                Height = y1-y5
            });
            

            Drawing.TsCacheGraphics cache = new Drawing.TsCacheGraphics();
            
            cache.Rectangle = new D.RectangleF()
            {
                //X=200,
                //X = param.Cursor.X,
                //Y = param.Cursor.Y,
                Width = sr.Rectangle.Width,
                Height=sr.Rectangle.Height
            };
           
            cache.Origin = new D.PointF(0, sr.OriginY);

            for (int i = 1; i <= 5;i++ )
            { 
                cache.Lines.Add(new Drawing.TsCacheLine()
                {
                    X1 = 0,
                    Y1=param.PositionOfLine(i),
                    X2 = cache.Rectangle.Width,
                    Y2 = param.PositionOfLine(i)
                });
            }
            this.CacheGraphics = new List<Drawing.TsCacheGraphics>() { cache };
        }

        private void PrepareExtras()
        {
            Tools.TsAlterationsCursor acursor = new Tools.TsAlterationsCursor(this,(this.measureId==0)?null:this.Partwise.Measures[this.measureId-1]);
            this.Elements.RemoveAll(x => x is Elements.TsAlter);

            for (int i = 0; i < this.Elements.Count; i++)
            {
                Elements.TsElement ielement = this.Elements[i];
                var required = ielement.RequiredNoteData;
                var needed = acursor.GetNeededAlterations(required);
                if (needed != null && needed.Count > 0)
                {
                    for(int j=0;j<needed.Count;j++)
                    {
                        Elements.TsAlter alter = new Elements.TsAlter(this);
                        alter.Note = needed[j];
                        this.Elements.Insert(i + j , alter);
                        acursor.AddAlter(alter.Note.Note, alter.Note.Octave, alter.Note.Alter);
                    }
                    i++;
                }
            }

        }

        private void AssignParameters()
        {
            TsMeasure measure;
            if (this.Armor == null)
            {
                measure = this.Partwise.FindMeasureWith(false, true, false);
                //if (measure == null) throw new Exception("Error cant found a valir armor");
                if (measure != null)
                    this.Armor = measure.Armor.CreateInnerit(this);
            }
            if(this.Time==null)
            {
                measure = this.Partwise.FindMeasureWith(false, false, true);
                if (measure == null) throw new Exception("Error cant found a valir time.");
                this.Time = measure.Time.CreateInnerit(this);
            }
            if (this.Clef == null)
            {
                measure = this.Partwise.FindMeasureWith(true, false, false);
                if (measure == null) throw new Exception("Error cant found a valir clef.");
                this.Clef = measure.Clef.CreateInnerit(this);
            }
        }

        public double MinRecommendedWidth(Drawing.TsDrawEngine param)
        {
            if (this.Elements.Count == 0)
                return 0;

            var vclef = this.Clef.CreateInnerit(this);
            vclef.CalculateCachePosition(param, null);


            return Elements.Sum(x => x.CacheGraphics[0].Rectangle.Width + param.Enviroment.HeightSpace / 2) + vclef.CacheGraphics[0].Rectangle.Width;
        }

        public void AdaptPosition(Drawing.TsDrawEngine param,bool forceclef, float currentX, float centerY, float measurew)
        {
            var rectangle = this.CacheGraphics[0].Rectangle;
            rectangle.Width = measurew;
            rectangle.X = currentX;
            rectangle.Y = (centerY - this.CacheGraphics[0].Origin.Y);
            this.CacheGraphics[0].Rectangle = rectangle;

            if (forceclef)
            {
                //this.Clef = this.Partwise.FindClef(this).CreateInnerit(this);
                this.Clef.CalculateCachePosition(param, null);
                if(!this.Elements.Contains(this.Clef))
                    this.Elements.Insert(0, this.Clef);
            }


            this.AdaptElementsPositions(param);

            for (int i = 1; i <= 5; i++)
            {
                this.CacheGraphics[0].Lines.Add(new Drawing.TsCacheLine()
                {
                    X1 = 0,
                    Y1 = param.PositionOfLine(i),
                    X2 = measurew,
                    Y2 = param.PositionOfLine(i)
                });
            }

            this.CacheGraphics[0].Lines.Add(new Drawing.TsCacheLine()
            {
                X1 = measurew,
                Y1 = param.PositionOfLine(5),
                X2 = measurew,
                Y2 = param.PositionOfLine(1)
            });
        }

        private void AdaptElementsPositions(Drawing.TsDrawEngine param)
        {
            int n = this.Elements.Count;
            float sum = this.Elements.Sum(x => x.CacheGraphics[0].Rectangle.Width);
            float totalw = this.CacheGraphics[0].Rectangle.Width;

            float margin = param.Enviroment.HeightSpace / 2;

            float spacew = (totalw - sum-margin) / n;

            float cursorX = margin;

            for(int i=0;i<this.Elements.Count;i++)
            {
                Elements.TsElement element = this.Elements[i];
                var rect = element.CacheGraphics[0].Rectangle;
                rect.X = cursorX;
                element.CacheGraphics[0].Rectangle = rect;
                cursorX += rect.Width + spacew;
            }
        }
    }
}
