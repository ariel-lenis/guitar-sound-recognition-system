using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D = System.Drawing;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public class TsSymbols
    {
        public class TsSymbolProperties
        {
            public char Code { get; set; }
            public float CenterX { get; set; }
            public float CenterY { get; set; }

            public float LPercent { get; set; }
            public float TPercent { get; set; }
            public float BPercent { get; set; }
            public float RPercent { get; set; }

            public D.RectangleF Rectangle { get { return new D.RectangleF(LPercent, TPercent, RPercent - LPercent, BPercent - TPercent); } }

            public TsSymbolProperties()
            {
                this.CenterX = 0f;
                this.CenterY = 0.5f;
                this.LPercent = 0;
                this.TPercent = 0;
                this.RPercent = 1;
                this.BPercent = 1;
            }

            public float TopToCenter
            {
                get { return this.CenterY - this.TPercent; }
            }


        }

        MusicalFont font;

        public TsSymbols(MusicalFont font)
        {
            this.font = font;
        }

        public enum ESymbols
        {
            Num0=48,
            Num1=49,
            Num2=50,
            Num3=51,
            Num4=52,
            Num5=53,
            Num6=54,
            Num7=55,
            Num8=56,
            Num9=57,

            Bemol=(int)'b',
            Sharp=(int)'#',
            Becuadro=(int)'n',

            Redonda,
            RedondaNegra,
            Dot,
        
            Silence001,
            Silence002,
            Silence004,
            Silence008,
            Silence016,
            Silence032,
            Silence064,
            Silence128,

            ClaveSol,  
          
            CorcheaDown,
            CorcheaUp,
            LargeCorcheaDown,
            LargeCorcheaUp
        }

        public TsSymbolProperties this[ESymbols symbol]
        {
            get
            {
                switch (symbol)
                {
                    case ESymbols.Num0:
                        return new TsSymbolProperties() { Code = (char)48, TPercent = 0.41955f, BPercent = 0.57555f, LPercent = 5E-05f, RPercent = 0.12925f };
                    case ESymbols.Num1:
                        return new TsSymbolProperties() { Code = (char)49, TPercent = 0.42245f, BPercent = 0.5727f, LPercent = 5E-05f, RPercent = 0.08945f };
                    case ESymbols.Num2:
                        return new TsSymbolProperties() { Code = (char)50, TPercent = 0.41885f, BPercent = 0.5742f, LPercent = 5E-05f, RPercent = 0.1217f };
                    case ESymbols.Num3:
                        return new TsSymbolProperties() { Code = (char)51, TPercent = 0.41885f, BPercent = 0.57485f, LPercent = 5E-05f, RPercent = 0.11595f };
                    case ESymbols.Num4:
                        return new TsSymbolProperties() { Code = (char)52, TPercent = 0.42245f, BPercent = 0.572f, LPercent = 5E-05f, RPercent = 0.11735f };
                    case ESymbols.Num5:
                        return new TsSymbolProperties() { Code = (char)53, TPercent = 0.42245f, BPercent = 0.572f, LPercent = 0f, RPercent = 0.10705f };
                    case ESymbols.Num6:
                        return new TsSymbolProperties() { Code = (char)54, TPercent = 0.41955f, BPercent = 0.57555f, LPercent = 5E-05f, RPercent = 0.11955f };
                    case ESymbols.Num7:
                        return new TsSymbolProperties() { Code = (char)55, TPercent = 0.42025f, BPercent = 0.57485f, LPercent = 5E-05f, RPercent = 0.1224f };
                    case ESymbols.Num8:
                        return new TsSymbolProperties() { Code = (char)56, TPercent = 0.41885f, BPercent = 0.57485f, LPercent = 5E-05f, RPercent = 0.11665f };
                    case ESymbols.Num9:
                        return new TsSymbolProperties() { Code = (char)57, TPercent = 0.41955f, BPercent = 0.57485f, LPercent = 5E-05f, RPercent = 0.12025f };

                    case ESymbols.Bemol:
                        return new TsSymbolProperties() { Code = 'b', TPercent = 0.3218f, BPercent = 0.5615f, LPercent = 5E-05f, RPercent = 0.0712f };
                    case ESymbols.Sharp:
                        return new TsSymbolProperties() { Code = '#', TPercent = 0.363f, BPercent = 0.6336f, LPercent = 0f, RPercent = 0.08055f };
                    case ESymbols.Becuadro:
                        return new TsSymbolProperties() { Code = 'n', TPercent = 0.3719f, BPercent = 0.6232f, LPercent = 5E-05f, RPercent = 0.063f };
                    
                    case ESymbols.Redonda:
                        return new TsSymbolProperties() { Code = (char)61559, TPercent = 0.4525f, BPercent = 0.5426f, LPercent = 5E-05f, RPercent = 0.1475f };
                    case ESymbols.RedondaNegra:
                        return new TsSymbolProperties() { Code = (char)61647, CenterX=0, TPercent = 0.45315f, BPercent = 0.54195f, LPercent = 5E-05f, RPercent = 0.10705f };
                    case ESymbols.Dot:
                        return new TsSymbolProperties() { Code = '.', CenterX = 0, TPercent = 0.4804f, BPercent = 0.51395f, LPercent = 5E-05f, RPercent = 0.0336f };
                        
                    case ESymbols.Silence001:
                        return new TsSymbolProperties() { Code = (char)183, TPercent = 0.40995f, BPercent = 0.4553f, LPercent = 5E-05f, RPercent = 0.11235f };
                    case ESymbols.Silence002:
                        return new TsSymbolProperties() { Code = (char)238, TPercent = 0.4525f, BPercent = 0.49715f, LPercent = 5E-05f, RPercent = 0.112f };
                    case ESymbols.Silence004:
                        return new TsSymbolProperties() { Code = (char)206, TPercent = 0.35405f, BPercent = 0.6242f, LPercent = 5E-05f, RPercent = 0.0916f };
                    case ESymbols.Silence008:
                        return new TsSymbolProperties() { Code = (char)228, TPercent = 0.47005f, BPercent = 0.635f, LPercent = 5E-05f, RPercent = 0.09375f };
                    case ESymbols.Silence016:
                        return new TsSymbolProperties() { Code = (char)197, TPercent = 0.38265f, BPercent = 0.635f, LPercent = 5E-05f, RPercent = 0.11815f };
                    case ESymbols.Silence032:
                        return new TsSymbolProperties() { Code = (char)168, TPercent = 0.29105f, BPercent = 0.635f, LPercent = 5E-05f, RPercent = 0.14315f };
                    case ESymbols.Silence064:
                        return new TsSymbolProperties() { Code = (char)244, TPercent = 0.20155f, BPercent = 0.635f, LPercent = 5E-05f, RPercent = 0.16785f };
                    case ESymbols.Silence128:
                        return new TsSymbolProperties() { Code = (char)229, TPercent = 0.11135f, BPercent = 0.635f, LPercent = 5E-05f, RPercent = 0.19225f };
                    
                    case ESymbols.ClaveSol:
                        return new TsSymbolProperties() { Code = '&', CenterX = 0, CenterY = 0.45315f, TPercent = 0.01965f, BPercent = 0.63965f, LPercent = 5E-05f, RPercent = 0.21875f };
                                                                                                     
                    case ESymbols.LargeCorcheaUp:
                        return new TsSymbolProperties() { Code = (char)61546, CenterX = 0.09845f+0.003f, CenterY = 0.18395f, TPercent = 0.18395f, BPercent = 0.4531f, LPercent = 0.09845f, RPercent = 0.19655f };
                    case ESymbols.LargeCorcheaDown:
                        return new TsSymbolProperties() { Code = (char)61514, CenterX = 0 + 0.002f, CenterY = 0.81045f, TPercent = 0.542f, BPercent = 0.81045f, LPercent = 0f, RPercent = 0.0984f };
                    case ESymbols.CorcheaUp:
                        return new TsSymbolProperties() { Code = (char)61691, CenterX = 0.09845f + 0.003f, CenterY = 0.4313f, TPercent = 0.4313f, BPercent = 0.62645f, LPercent = 0.09845f, RPercent = 0.1951f };
                    case ESymbols.CorcheaDown:
                        return new TsSymbolProperties() { Code = (char)61680, CenterX = 0f + 0.002f, CenterY = 0.55375f, TPercent = 0.36515f, BPercent = 0.55375f, LPercent = 0f, RPercent = 0.0992f };
                }
                return null;
            }
            //Code=(char)61546,TPercent=0.18395f, BPercent=0.4531f, LPercent=0.0985f, RPercent=0.19655f
        }
        /*
        public static TsSymbolProperties Bemol = new TsSymbolProperties(){ Code= 'b'};
        public static TsSymbolProperties Sharp = new TsSymbolProperties() { Code = '#' };

        public static TsSymbolProperties Redonda = new TsSymbolProperties() { Code = (char)61559 };
        public static TsSymbolProperties RedondaNegra = new TsSymbolProperties() { Code = (char)61647 };

        public static TsSymbolProperties Silence001 = new TsSymbolProperties() { Code = (char)183 };
        public static TsSymbolProperties Silence002 = new TsSymbolProperties() { Code = (char)238 };
        public static TsSymbolProperties Silence004 = new TsSymbolProperties() { Code = (char)206 };
        public static TsSymbolProperties Silence008 = new TsSymbolProperties() { Code = (char)228 };
        public static TsSymbolProperties Silence016 = new TsSymbolProperties() { Code = (char)197 };
        public static TsSymbolProperties Silence032 = new TsSymbolProperties() { Code = (char)168 };
        public static TsSymbolProperties Silence064 = new TsSymbolProperties() { Code = (char)244 };
        public static TsSymbolProperties Silence128 = new TsSymbolProperties() { Code = (char)229 };

        public static TsSymbolProperties ClaveSol = new TsSymbolProperties() { Code = '&' };
         
        
        */
    }
}

