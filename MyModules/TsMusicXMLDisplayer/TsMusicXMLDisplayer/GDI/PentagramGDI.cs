using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using W = System.Windows.Forms;
using D = System.Drawing;
using D2D = System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Drawing.Text;
using System.ComponentModel;

namespace TsMusicXMLDisplayer.GDI
{
    public class PentagramGDI
    {
        D.Bitmap bmp;
        D.Graphics gbmp;

        D.Pen fpen;
        D.Brush fbrush;
        D.Color fcolor;

        D.Color bcolor;

        int w, h;

        float[] linesy;
        float sh,dh;
        float paddingx = 10;
        MusicXML.scorepartwise res;

        float currentx = 0;

        MusicalFont musicalfont;

        public D.Bitmap Bitmap { get { return bmp; } }

        public PentagramGDI()
        {
            bcolor = D.Color.FromArgb(255,255,200);
            fcolor = D.Color.Gray;
            fpen = new D.Pen(fcolor);
            fbrush = new D.SolidBrush(fcolor);


            LoadScore();

            linesy = new float[5];

            musicalfont = new MusicalFont();


        }

        public void Resize(int w, int h)
        {
            if (bmp != null)
            {
                gbmp.Dispose();
                bmp.Dispose();
            }
            //w *= 2;
            //h *= 2;
            bmp = new D.Bitmap(w, h);
            gbmp = D.Graphics.FromImage(bmp);
            //gbmp.SmoothingMode = D2D.SmoothingMode.AntiAlias;
            
            this.w = w;
            this.h = h;
            currentx = 0;

            CalculateLinesY();

            musicalfont.SetSize(/*gbmp,*/ dh);

            Redraw();
            Parse(res);
        }
        void CalculateLinesY()
        {
            dh = this.h / 20.0f;
            sh = this.h / 2.0f - 2.5f * dh;
            for (int i = 0; i < 5; i++)
                linesy[i] = sh + dh * i;
        }
        public void Redraw()
        {
            if(gbmp == null) return;
            gbmp.Clear(bcolor);

            for (int i = 0; i < 5; i++)
            {
                gbmp.DrawLine(fpen, paddingx, linesy[i], this.w - 2 * paddingx, linesy[i]);
            }
            gbmp.DrawLine(fpen, paddingx, sh, paddingx, sh + 4f * dh);
            gbmp.DrawLine(fpen, this.w-2* paddingx, sh,this.w-2* paddingx, sh + 4f * dh);
        }

        public void Parse(MusicXML.scorepartwise data)
        {
            var part = data.part[0];

            for (int i = 0; i < 1+0*part.measure.Length; i++)
                ProcessMeasure(part, i);
        }

        private void ProcessMeasure(MusicXML.scorepartwisePart part, int i)
        {
            var measure = part.measure[i];

            List<MusicXML.note> notes=new List<MusicXML.note>();
            List<MusicXML.attributes> attributes=new List<MusicXML.attributes>();
            List<MusicXML.direction> directions=new List<MusicXML.direction>();
            List<MusicXML.clef> clefs = new List<MusicXML.clef>();
            List<MusicXML.time> times = new List<MusicXML.time>();

            foreach (var item in measure.Items)
            {
                if (item is MusicXML.note)
                    notes.Add(item as MusicXML.note);
                else if (item is MusicXML.attributes)
                    attributes.Add(item as MusicXML.attributes);
                else if (item is MusicXML.direction)
                    directions.Add(item as MusicXML.direction);
                else if (item is MusicXML.time)
                    times.Add(item as MusicXML.time);
                //else
                //    throw new Exception("Not Supported part measure item type : "+item.GetType().Name);
            }

            MusicXML.clef signclef = new MusicXML.clef();
            signclef.sign = MusicXML.clefsign.G;
            signclef.line = "2";
            signclef.clefoctavechange = "0";

            MusicXML.clef signtab = new MusicXML.clef();
            signtab.sign= MusicXML.clefsign.TAB;
            signtab.line="5";


            MusicXML.key key = new MusicXML.key();
            key.Items = new object[]{"0", "major" };
            key.ItemsElementName = new MusicXML.ItemsChoiceType8[] { MusicXML.ItemsChoiceType8.fifths, MusicXML.ItemsChoiceType8.mode };

            MusicXML.time time = new MusicXML.time();
            time.Items = new object[] { "4", "4" };
            time.ItemsElementName = new MusicXML.ItemsChoiceType9[] { MusicXML.ItemsChoiceType9.beats, MusicXML.ItemsChoiceType9.beattype };

            int divisions = 0;

            foreach (var item in attributes)
            {
                if (item.clef != null)
                    foreach (var itemc in item.clef)
                        if (itemc.sign == MusicXML.clefsign.TAB) signtab = itemc;
                        else if (itemc.sign != MusicXML.clefsign.TAB) signclef = itemc;
                if (item.key != null)
                    key = item.key.Last();
                if (item.time != null)
                    time = item.time.Last();
                divisions = (int)item.divisions;
            }

            if (signclef.sign != MusicXML.clefsign.G) throw new Exception("Just G sign is not supported for now.");

            int num = 0;
            int den = 0;
            for (int j = 0; j < time.ItemsElementName.Length; j++)
            {
                if (time.ItemsElementName[j] == MusicXML.ItemsChoiceType9.beats)
                    num = int.Parse(time.Items[j].ToString());
                else if (time.ItemsElementName[j] == MusicXML.ItemsChoiceType9.beattype)
                    den = int.Parse(time.Items[j].ToString());
                else
                    throw new Exception("Invalid time property.");
            }

            Redraw();
            DrawClef(signclef, signtab);
            DrawFifhts(signclef, key);
            DrawTime(time,num,den);

            List<TsNote> tsnotes = new List<TsNote>();
            foreach (var itemnote in notes)
            {
                TsNote tsnote = new TsNote();
                tsnote.LoadFromXMLNote(itemnote);

                if (tsnote.Type == TsNote.EType.Chord)
                {
                    if (tsnotes.Count == 0) throw new Exception("Error a chord without base notes!!!");
                    tsnotes.Last().Chords.Add(tsnote);
                }
                else
                    tsnotes.Add(tsnote);                
            }

            foreach(var itemnote in tsnotes)
                DrawNote(signclef, itemnote, num, den, divisions);

            //DrawFifths(signclef)


        }



        private void DrawNote(MusicXML.clef signclef, TsNote tsnote,int beat,int beattype,int divisions)
        {


            //TsNote tsnote = new TsNote();
            //tsnote.LoadFromXMLNote(note);

            var duration = tsnote.CalculateDuration(beat, beattype, divisions);
            var parameters = DurationParameters.GetParameters(duration.Duration);

            currentx += dh*2.5f;

            float yline;
            float ynote = NotePosition(signclef, tsnote);

            //              /           \
            //Just 1        61514       61546
            //Part 1        61680       61691
            //End  2        61515       61547


            if (tsnote.Type == TsNote.EType.Note)
            {
                TsNote minnote = tsnote.MinimunChord();
                TsNote maxnote = tsnote.MaximunChord();

                int mindelta = NoteDeltaPosition(signclef, minnote);
                int maxdelta = NoteDeltaPosition(signclef, maxnote);
                int delta = NoteDeltaPosition(signclef, tsnote);

                //A two direction loop based on the position of the note
                for (int i = delta; !(i >= 0 && i <=8) ; i += (delta<0)?1:-1)
                {
                    //Just Even positions have a line
                    if (i % 2 == 0)
                    {
                        yline = PositionOfNumber(i);
                        gbmp.DrawLine(D.Pens.Gray, currentx + dh / 4, yline, currentx + dh + dh, yline);
                    }
                }

                for (int i = mindelta; i<=maxdelta; i++)
                {
                    if (i % 2 == 0 && !(i >= 0 && i <=8))
                    {
                        yline = PositionOfNumber(i);
                        gbmp.DrawLine(D.Pens.Gray, currentx + dh / 4, yline, currentx + dh + dh, yline);
                    }
                }


                int direction = 0;
                int centralindex = 4;

                if (centralindex - mindelta > maxdelta - centralindex)
                {
                    direction = 1;
                    delta = maxdelta;
                }
                else
                {
                    direction = -1;
                    delta = mindelta;
                }
                //int direction = (delta<4)?1:-1;


                bool extend = Math.Abs(delta - centralindex) > 10 && tsnote.Chords.Count == 0;
                int targetindex = 0;
                int lastposition = 0;
                int lastdirection = direction;

                //foreach (var ichord in tsnote.Chords)                
                int j=0;
                bool chain = false;
                for (int i = tsnote.Chords.Count-1; i >=0;i-- )
                {
                    var ichord = tsnote.Chords[i];
                    var chordduration = ichord.CalculateDuration(beat, beattype, divisions);
                    var chordparameters = DurationParameters.GetParameters(chordduration.Duration);
                    float chordynote = NotePosition(signclef, ichord);

                    //if(i>0)
                    /*
                    if (ichord == maxnote)
                        gbmp.DrawString(chordparameters.NoteBase + "", musicalfont.CurrentFont, D.Brushes.Violet, currentx-(j%2)*dh, chordynote - musicalfont.FontCenter + musicalfont.LineSpace / 2);
                    else if (ichord == minnote)
                        gbmp.DrawString(chordparameters.NoteBase + "", musicalfont.CurrentFont, D.Brushes.Green, currentx - (j % 2) * dh, chordynote - musicalfont.FontCenter + musicalfont.LineSpace / 2);
                    else
                    */

                    int ddirection = direction;
                    
                    chain = false;

                    if (i < tsnote.Chords.Count - 1)
                    {
                        if (lastposition == ichord.AbsolutePosition + 1)
                            chain = true;
                    }

                    if (chain)
                    {
                        ddirection = -lastdirection;
                    }
                        


                    gbmp.DrawString(chordparameters.NoteBase + "", musicalfont.CurrentFont, D.Brushes.Black, currentx+dh/4f -(ddirection+1) * dh/2*1.15f, chordynote - musicalfont.FontCenter + musicalfont.LineSpace / 2);

                    lastdirection = ddirection;
                    lastposition = ichord.AbsolutePosition;

                    j++;
                }

                //gbmp.DrawString(parameters.NoteBase + "", musicalfont.CurrentFont, D.Brushes.Black, currentx, ynote - musicalfont.FontCenter + musicalfont.LineSpace / 2);



                if (extend)
                    targetindex = centralindex;
                else
                {
                    if(parameters.Corcheas<3)
                        targetindex = delta + direction * 7;
                    else if(parameters.Corcheas==3)
                        targetindex = delta + direction * 8;
                    else
                        targetindex = delta + direction * 10;
                }
                /*
                for (int i = delta-1; i != targetindex-direction-1; i += direction)
                {
                    yline = PositionOfNumber(i);
                    char line = direction > 0 ? (char)61639 : (char)61640;
                    gbmp.DrawString(line + "", musicalfont.CurrentFont, D.Brushes.Black, currentx, yline - musicalfont.FontCenter + 0 * musicalfont.LineSpace / 2);                                    
                
                }
                */
                currentx += dh;
                float x = currentx /*+ 3 * dh / 4*/;
                //if (direction > 0)  x = x + dh*1.1f;

                gbmp.DrawLine(new D.Pen(D.Color.Black, 1.5f), x, PositionOfNumber(delta), x, PositionOfNumber(targetindex));

                gbmp.DrawLine(new D.Pen(D.Color.Black, 1.5f), x, PositionOfNumber(mindelta), x, PositionOfNumber(maxdelta));


                char ax;

                currentx += -(direction + 1) * dh/1.9f-dh/1.4f;

                if (parameters.Corcheas > 1)
                {
                    for (int i = parameters.Corcheas - 2; i >= 0; i--)
                    {
                        if (i < parameters.Corcheas - 2)
                        {
                            ax = direction > 0 ? (char)61691 : (char)61680;

                            if (direction > 0)
                                yline = PositionOfNumber(targetindex - 3);
                            else
                                yline = PositionOfNumber(targetindex);

                            gbmp.DrawString(ax + "", musicalfont.CurrentFont, D.Brushes.Black, currentx , -0.15f * dh + direction * 0.40f * dh + direction * i * dh * 0.8f + yline - musicalfont.FontCenter - (direction) * musicalfont.LineSpace / 2);
                        }
                        else
                        {                            
                            ax = direction > 0 ? (char)61547 : (char)61515;

                            if (direction > 0)
                                yline = PositionOfNumber(targetindex - 15);
                            else
                                yline = PositionOfNumber(targetindex);

                            gbmp.DrawString(ax + "", musicalfont.CurrentFont, D.Brushes.Black, currentx, -0.15f * dh + direction * 0.40f * dh + direction * i * dh * 0.8f + yline - musicalfont.FontBottom * 0.77f - (direction) * musicalfont.LineSpace / 2);
                        }
                    }
                }
                else
                {
                    ax = direction > 0 ? (char)61546 : (char)61514;
                    if (direction > 0)
                        yline = PositionOfNumber(targetindex - 15);
                    else
                        yline = PositionOfNumber(targetindex);
                    gbmp.DrawString(ax + "", musicalfont.CurrentFont, D.Brushes.Black, currentx, -0.15f * dh + direction * 0.40f * dh + direction * 0 * dh * 0.8f + yline - musicalfont.FontBottom * 0.77f - (direction) * musicalfont.LineSpace / 2);                                                           
                }





            }
            else
            {
                ynote = linesy[2];
                if (duration.Duration == TsNote.EDurations.T008)
                    ynote = linesy[1] + dh / 2;
                else if (duration.Duration == TsNote.EDurations.T032 || duration.Duration == TsNote.EDurations.T016)
                    ynote = linesy[2] + dh / 2;
                else if (duration.Duration == TsNote.EDurations.T064)
                    ynote = linesy[3] + dh / 2;
                else if (duration.Duration == TsNote.EDurations.T128)
                    ynote = linesy[3] + dh / 2;

                gbmp.DrawString(parameters.SilenceCode + "", musicalfont.CurrentFont, D.Brushes.Black, currentx, ynote - musicalfont.FontCenter + musicalfont.LineSpace / 2);
            }
            

            if (duration.Puntillos > 0) currentx += dh;
            for (int i = 0; i < duration.Puntillos; i++)
            {
                currentx += 0.5f * dh;
                foreach (var ichord in tsnote.Chords)
                {
                    int dx=0;
                    yline=0;
                    if (tsnote.Type == TsNote.EType.Note)
                    {
                        dx = NoteDeltaPosition(signclef, ichord);
                        if (dx % 2 == 0) dx++;
                        yline = PositionOfNumber(dx);
                    }
                    else
                    {
                        dx = 4;   
                        if (dx % 2 == 0) dx++;
                        yline = PositionOfNumber(dx);                        
                    }

                    gbmp.DrawString((char)61486 + "", musicalfont.CurrentFont, D.Brushes.Black, currentx + dh / 20f, yline - musicalfont.FontCenter + musicalfont.LineSpace / 2);
                }
            }

        }

        private void DrawTime(MusicXML.time time,int num,int den)
        {

            currentx += 20;
            gbmp.DrawString(num.ToString(), musicalfont.CurrentFont, D.Brushes.Black, currentx, linesy[1] - musicalfont.FontCenter + musicalfont.LineSpace / 2);
            gbmp.DrawString(den.ToString(), musicalfont.CurrentFont, D.Brushes.Black, currentx, linesy[3] - musicalfont.FontCenter + musicalfont.LineSpace / 2);
        }

        private void DrawFifhts(MusicXML.clef signclef, MusicXML.key key)
        {
            if (signclef == null) return;

            string[] majorsharparmor = new string[] {"F5","C5","G5","D5","A4","E5","B4"};
            string[] majorbemolarmor = new string[] { "B4", "E5", "A4", "D5", "G4", "C5", "F4" };

            string[] armor=majorsharparmor;

            string fifhts="";
            string mode="";
            string sign = "#";

            for (int i = 0; i < key.ItemsElementName.Length; i++)
                if (key.ItemsElementName[i] == MusicXML.ItemsChoiceType8.fifths)
                    fifhts = key.Items[i].ToString();
                else if (key.ItemsElementName[i] == MusicXML.ItemsChoiceType8.mode)
                    mode = key.Items[i].ToString();

            int number = int.Parse(fifhts);
            int anumber=number;
            if (number < 0)
            {
                sign = "b";
                armor = majorbemolarmor;
                anumber=-number;
            }
            //float[] positions= ArmorPositions(signclef, armor);
            for (int i = 0; i < anumber; i++)
            {
                float y = NotePosition(signclef, armor[i]);

                //float y = positions[i];

                //gbmp.FillEllipse(D.Brushes.Red, 200+i*dh, y-dh/2, dh, dh);
                currentx +=dh;
                gbmp.DrawString(sign, musicalfont.CurrentFont, D.Brushes.Black, currentx, y - musicalfont.FontCenter+musicalfont.LineSpace/2);

                //gbmp.DrawString("#", musicalfont.CurrentFont, D.Brushes.Black, new D.RectangleF(200 + i * dh, y - dh, dh, dh), D.StringFormat.GenericTypographic);

                //gbmp.FillRectangle(D.Brushes.Green, 0,0,1000,1000);
                //gbmp.Clear(D.Color.White);
            
            }            
        }
        /*
        private float[] ArmorPositions(MusicXML.clef clef, string[] armor)
        {
            int iclef = ClefToNumber(clef);
            int clefline = int.Parse(clef.line);
            int lineposition = (clefline - 1) * 2;

            // if (iclef >= inote) inote += 7;

            float[] res = new float[armor.Length];
            int[] notes = new int[armor.Length];

            for (int i = 0; i < res.Length; i++)
            {
                int inote = TsNote.NoteToNumber(armor[i]);

                if (i == 0 && iclef > inote) inote += 7;
                if (i > 0)
                {
                    bool condition = Math.Abs(notes[1] - inote - 7) < Math.Abs(notes[1] - inote);
                    int diference = (int)(notes[1] - inote);
                    if (i%2==1)
                        inote += 7;
                    if (i % 2 == 0)
                        inote += 7;
                }
                notes[i] = inote;
                int delta = lineposition + inote - iclef;
                res[i] = PositionOfNumber(delta); 
            }
            return res;
        }
        */


        private float NotePosition(MusicXML.clef clef, TsNote thenote)
        {
            int delta = NoteDeltaPosition(clef, thenote);
            return PositionOfNumber(delta);
        }

        private int NoteDeltaPosition(MusicXML.clef clef, TsNote thenote)
        {
            TsNote clefnote = new TsNote();
            clefnote.LoadFromXMLClef(clef);

            int clefline = int.Parse(clef.line);
            int lineposition = (clefline - 1) * 2;

            int delta = lineposition + thenote.RelativePosition - clefnote.RelativePosition;
            return delta;
        }
        private float NotePosition(MusicXML.clef clef, string note)
        {
            //----------    8
            //              7
            //----------    6
            //              5
            //----------    4
            //              3
            //----------    2
            //              1
            //----------    0

            //TsNote clefnote = new TsNote();
            //clefnote.LoadFromXMLClef(clef);

            TsNote thenote = new TsNote();
            thenote.LoadFromNote(note);

            /*
            int clefline = int.Parse(clef.line);
            int lineposition = (clefline - 1) * 2;

            int delta = lineposition+thenote.RelativePosition - clefnote.RelativePosition;

            return PositionOfNumber(delta);
                * */

            return NotePosition(clef, thenote);
        }


        private float PositionOfNumber(int delta)
        {
            float start = linesy[4];
            return start - 0.5f * dh * delta;
        }

        private void DrawClef(MusicXML.clef signclef, MusicXML.clef signtab)
        {
            int line = int.Parse(signclef.line);
            currentx += 10;
            gbmp.DrawString("&", musicalfont.CurrentFont, D.Brushes.Black, currentx, linesy[4] - musicalfont.FontCenter + musicalfont.LineSpace / 2*0);
            currentx += 40;
        }



        public D.Bitmap CustomLoadBitmap(string name)
        {
            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = path + "\\GDI\\img\\" + name;
            if(File.Exists(path))
                return (D.Bitmap)D.Image.FromFile(path);


            return new D.Bitmap(1, 1);

        }

        public void LoadScore()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MusicXML.scorepartwise));
            FileStream fs = new FileStream(@"C:\Users\Nachobertinho\Desktop\testtesttest.xml", FileMode.Open);
            res = (MusicXML.scorepartwise)serializer.Deserialize(fs);
            fs.Dispose();
        }
    }
}
