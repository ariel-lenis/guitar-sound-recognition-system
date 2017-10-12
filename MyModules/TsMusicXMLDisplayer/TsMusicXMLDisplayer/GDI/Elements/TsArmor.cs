using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public class TsArmor:TsElement
    {
        public TsMusicXMLDisplayer.GDI.TsMeasure.EScaleMode ScaleMode { get; set; }
        public int DiskDirection { get; set; }

        //string[] majorsharparmor = new string[] { "F5", "C5", "G5", "D5", "A4", "E5", "B4" };
        //string[] majorbemolarmor = new string[] { "B4", "E5", "A4", "D5", "G4", "C5", "F4" };

        string[] majorsharparmor = new string[] { "F1", "C1", "G1", "D1", "A0", "E1", "B0" };
        string[] majorbemolarmor = new string[] { "B0", "E1", "A0", "D1", "G0", "C1", "F0" };

        public TsArmor(TsMeasure measure)
            : base(measure)
        { 
        
        }

        public override void Draw(TsMeasure measure, Drawing.TsDrawEngine param)
        {
            param.Draw(measure, this);
        }
        public TsNoteData.Ealter ArmorAlter()
        {
            if (this.DiskDirection == 0) return TsNoteData.Ealter.Normal;
            else if (this.DiskDirection > 0) return TsNoteData.Ealter.Sharp;
            else return TsNoteData.Ealter.Bemol;
        }
        public List<TsNoteData> ParseArmor()
        {
            string[] armor = majorsharparmor;
            int positive = this.DiskDirection;

            if (this.DiskDirection < 0)
            {
                armor = majorbemolarmor;
                positive = -this.DiskDirection;
            }

            List<TsNoteData> res = new List<TsNoteData>();

            for (int i = 0; i < positive; i++)
            {
                TsNoteData notedata = new TsNoteData();
                notedata.LoadFromRelativeNote(armor[i], this.Measure.Clef);
                res.Add(notedata);
            }

            return res;
        }

        public override void CalculateCachePosition(Drawing.TsDrawEngine param, Dictionary<string, object> additional)
        {
            Drawing.TsCacheGraphics cache = new Drawing.TsCacheGraphics();

            TsSymbols.ESymbols symbol= TsSymbols.ESymbols.Sharp;
            //string[] armor = majorsharparmor;
            //int positive = this.DiskDirection;

            if (this.DiskDirection < 0)
            {
                symbol = TsSymbols.ESymbols.Bemol;
                //armor = majorbemolarmor;
                //positive = -this.DiskDirection;
            }

            //TsNoteData notedata = new TsNoteData();

            List<int> sublines = new List<int>();
            var armornotes = ParseArmor();

            for (int i = 0; i < armornotes.Count; i++)
            {                
                //notedata.LoadFromRelativeNote(armor[i],this.Measure.Clef);
                sublines.Add(armornotes[i].PentagramSubLine(this.Measure.Clef));
            }

            if(sublines.Count>0)
            { 
                TsSymbols.TsSymbolProperties psymbol = param.Symbols[symbol];

                int maxline = sublines.Max();
                D.RectangleF rcontainer = new D.RectangleF();
                rcontainer.Y = param.PositionOfSubLine(maxline) - psymbol.TopToCenter * param.FontM.FontHeight;

                for (int i = 0; i < armornotes.Count; i++)
                {
                    cache.Characters.Add(new Drawing.TsCacheCharacter()
                    {
                        Symbol=psymbol,
                        DeltaX=i*psymbol.Rectangle.Width*param.FontM.FontHeight*1.1f,
                        DeltaY=param.PositionOfSubLine(sublines[i])-rcontainer.Y
                    });
                }

                D.RectangleF measure = cache.CharactersRectangle(param.FontM.FontHeight);

                rcontainer.Width = measure.Width;
                rcontainer.Height = measure.Height;

                cache.Rectangle = rcontainer;
            }
            this.CacheGraphics = new List<Drawing.TsCacheGraphics>() { cache };
        }

        public override List<Drawing.TsCacheGraphics> CacheGraphics
        {
            get;
            set;
        }

        public TsArmor CreateInnerit(TsMeasure measure)
        {
            TsArmor armor = new TsArmor(measure);
            armor.DiskDirection = this.DiskDirection;
            armor.ScaleMode = this.ScaleMode;
            armor.Innerit = true;
            return armor;
        }
    }
}
