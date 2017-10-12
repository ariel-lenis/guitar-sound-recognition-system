using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TsFilesTools;

namespace TsPentagramEngine.MXMLC
{
    public class MusicXMLMaker
    {
        List<InputFormat> data;

        string[] compases = new string[]{ "2/4", "3/4", "4/4", "5/4", "6/4", "3/8", "6/8", "9/8", "12/8" };
        public string PartName { get; set; }
        public string PartAbreviation { get; set; }
        public string Title { get; set; }
        public string Autor { get; set; }
        public string Copyright { get; set; }

        public MusicXMLMaker(List<InputFormat> data)
        {
            this.data = data;
            this.Title = "Untitled";
            this.Autor = "No specified";
            this.Copyright = "No Copyright is specified.";
        }

        MusicXML.scorepartwise PrepareBase()
        {
            MusicXML.scorepartwise partwise = new MusicXML.scorepartwise();
            MusicXML.scorepartwisePart part = new MusicXML.scorepartwisePart();
            partwise.part = new MusicXML.scorepartwisePart[] { part };
            part.id = "P0";

            MusicXML.scorepart spart = new MusicXML.scorepart();
            spart.id = "P0";

            spart.partname = new MusicXML.partname() { Value = this.PartName };
            spart.partabbreviation = new MusicXML.partname() { Value = this.PartAbreviation };
            //spart.scoreinstrument = new MusicXML.scoreinstrument(){ id=""};

            MusicXML.partlist plist = new MusicXML.partlist();
            plist.Items = new object[] { spart };

            partwise.partlist = plist;


            partwise.movementtitle = this.Title;

            MusicXML.identification identification = new MusicXML.identification();
            identification.creator = new MusicXML.typedtext[]{ new MusicXML.typedtext(){Value=this.Autor,type="composer"}};
            identification.rights = new MusicXML.typedtext[]{ new MusicXML.typedtext(){Value=this.Copyright}};

            partwise.identification = identification;

            return partwise;
        }

        List<Measure> ParseNotes(Partwise mpartwise,List<InputFormat> data,float center,int numerator,int denominator)
        {
            List<WithDuration> elements = new List<WithDuration>();

            ConversionTools conversor = new ConversionTools(center, NoteTime.EFigures.Negra, 2);

            foreach (var idata in data)
            {
                //var lres = ConversionTools.DetermineBestEquivalence(center, NoteTime.EFigures.Negra, (float)idata.Duration.TotalSeconds);
                var lres = conversor.DetermineBestEquivalence((float)idata.Duration.TotalSeconds);

                if (lres.Count == 0)
                {
                    Console.WriteLine("0-> In Conversion...");
                    continue;
                }

                Note note = new Note();
                note.Duration = lres[0];

                foreach (var im in idata.MidiNotes)
                {
                    int octave = TsMIDITools.OctaveBase4(im);
                    string snote = TsMIDITools.NoteFor(im);
                    note.Chords.Add(NoteData.CreateFromString(snote, octave));
                }

                elements.Add(note);

                for (int i = 1; i < lres.Count; i++)
                {
                    Silence silence = new Silence();
                    silence.Duration = lres[i];
                    elements.Add(silence);
                }
            }  
            //mpartwise.Measures.Add(measu)

            var mm = conversor.CreateMeasures(elements, numerator, denominator);

            return mm;
        }

        MusicXML.direction CreateDirection(float center)
        {
            MusicXML.direction direcction = new MusicXML.direction();
            MusicXML.directiontype direcctiontype = new MusicXML.directiontype();
            MusicXML.metronome metronome = new MusicXML.metronome();

            direcctiontype.Items = new object[] { metronome };
            direcctiontype.ItemsElementName = new MusicXML.ItemsChoiceType7[] { MusicXML.ItemsChoiceType7.metronome };

            MusicXML.perminute ppm = new MusicXML.perminute();
            ppm.Value = (int)(60.0f / center) + "";
            metronome.Items = new object[] { MusicXML.notetypevalue.quarter, ppm };
            direcction.directiontype = new MusicXML.directiontype[] { direcctiontype };

            return direcction;
        }

        public string Parse(Stream fs,float center,int numerator,int denominator,int octavechange)
        {
            MusicXML.scorepartwise partwise = PrepareBase();
            Partwise mpartwise = new Partwise();

            //int numerator = 4;
            //int denominator = 4;

            


            List<MusicXML.scorepartwisePartMeasure> measures = new List<MusicXML.scorepartwisePartMeasure>();

            MusicXML.time time = new MusicXML.time();
            time.Items = new object[] { numerator+"", denominator+"" };
            time.ItemsElementName = new MusicXML.ItemsChoiceType9[] { MusicXML.ItemsChoiceType9.beats, MusicXML.ItemsChoiceType9.beattype };
            
            MusicXML.clef clef = new MusicXML.clef();
            clef.line = "2";
            clef.sign = MusicXML.clefsign.G;
            clef.clefoctavechange = octavechange+"";


            List<Measure> pmeasures = ParseNotes(mpartwise, data, center,numerator,denominator);

            MusicXML.scorepartwisePartMeasure measure = null;

            double currentacum = 0;

            int timetime = 1024;

            List<object> notes = null;

            MusicXML.direction direcction = CreateDirection(center);

            for (int i = 0; i < pmeasures.Count; i++)
            {
                measure = new MusicXML.scorepartwisePartMeasure();
                measure.number = measures.Count + "";
                notes = new List<object>();

                if (i==0)
                {
                    MusicXML.attributes attributes = new MusicXML.attributes();
                    attributes.clef = new MusicXML.clef[] { clef };
                    attributes.time = new MusicXML.time[] { time };
                    attributes.divisions = timetime;
                    attributes.divisionsSpecified = true;
                    notes.Add(attributes);
                    notes.Add(direcction);
                }

                for (int inote = 0; inote < pmeasures[i].Notes.Count; inote++)
                {
                    var element = pmeasures[i].Notes[inote];
                    float value = element.Duration.Value;
                    UnrestrictedDictionary<MusicXML.ItemsChoiceType1, object> dnote;

                    if (element is Note)
                    {
                        Note enote = element as Note;
                        for (int j = 0; j < enote.Chords.Count; j++)
                        {
                            dnote = new UnrestrictedDictionary<MusicXML.ItemsChoiceType1, object>();
                            if (j > 0) dnote.Add(MusicXML.ItemsChoiceType1.chord, new MusicXML.empty());

                            MusicXML.pitch pitch = new MusicXML.pitch();
                            dnote.Add(MusicXML.ItemsChoiceType1.pitch, pitch);

                            MusicXML.note note = new MusicXML.note();

                            dnote.Add(MusicXML.ItemsChoiceType1.duration, (decimal)(value * timetime));
                            if (enote.Chords[j].Alter != NoteData.EAlters.Normal)
                            {
                                pitch.alter = (int)enote.Chords[j].Alter;
                                pitch.alterSpecified = true;
                            }

                            pitch.octave = enote.Chords[j].Octave + "";
                            pitch.step = FindStep(enote.Chords[j].Note);

                            if(enote.Duration.IsStartTie || enote.Duration.IsEndTie)
                            {                                
                                MusicXML.notations notation = new MusicXML.notations();
                                note.notations = new MusicXML.notations[]{notation};

                                List<MusicXML.tied> tieds = new List<MusicXML.tied>();
                                MusicXML.tied tied;

                                if (enote.Duration.IsEndTie)
                                {
                                    tied = new MusicXML.tied();
                                    tied.type = MusicXML.startstop.stop;
                                    tieds.Add(tied);
                                    dnote.Add(MusicXML.ItemsChoiceType1.tie, new MusicXML.tie() { type = MusicXML.startstop.stop });
                                }

                                if(enote.Duration.IsStartTie)
                                {
                                    tied = new MusicXML.tied();
                                    tied.type = MusicXML.startstop.start;
                                    tieds.Add(tied);
                                    dnote.Add(MusicXML.ItemsChoiceType1.tie, new MusicXML.tie(){type= MusicXML.startstop.start});
                                }

                                notation.Items = tieds.Select(x => (object)x).ToArray();
                            }
                            note.Items = dnote.Values.Select(x => (object)x).ToArray();
                            note.ItemsElementName = dnote.Keys.ToArray();
                            notes.Add(note);
                        }
                    }
                    else
                    {
                        dnote = new UnrestrictedDictionary<MusicXML.ItemsChoiceType1, object>();
                        MusicXML.note note = new MusicXML.note();

                        dnote.Add(MusicXML.ItemsChoiceType1.rest, new MusicXML.displaystepoctave() { displayoctave = "4", displaystep = MusicXML.step.B });
                        dnote.Add(MusicXML.ItemsChoiceType1.duration, (decimal)(value * timetime));

                        note.Items = dnote.Values.Select(x => (object)x).ToArray();
                        note.ItemsElementName = dnote.Keys.ToArray();
                        notes.Add(note);
                    }
                    currentacum += value;
                }


                Console.WriteLine("Fraction:\t" + pmeasures[i].SumDurations()+"\t\tCorrect:" + (currentacum - 4) );
                measure.Items = notes.Select(x => (object)x).ToArray();
                measures.Add(measure);
                currentacum = 0;
            }

            foreach(var imeasure in measures)
                foreach (var inote in imeasure.Items.Where(x => x is MusicXML.note))
                {
                    var note = inote as MusicXML.note;
                    for (int i = 0; i < note.ItemsElementName.Length; i++)
                    {
                        if (note.ItemsElementName[i] == MusicXML.ItemsChoiceType1.pitch)
                        {
                            MusicXML.pitch pp = note.Items[i] as MusicXML.pitch;
                            if (pp.alter != 0)
                            { 
                            
                            }
                        }
                    }
                }
                

            partwise.part[0].measure = measures.ToArray();

            return this.WriteOnStream(fs, partwise);
        }
        MusicXML.step FindStep(NoteData.ENote note)
        {
            switch (note)
            {
                case NoteData.ENote.C:
                    return MusicXML.step.C;
                case NoteData.ENote.D:
                    return MusicXML.step.D;
                case NoteData.ENote.E:
                    return MusicXML.step.E;
                case NoteData.ENote.F:
                    return MusicXML.step.F;
                case NoteData.ENote.G:
                    return MusicXML.step.G;
                case NoteData.ENote.A:
                    return MusicXML.step.A;
                default:
                    return MusicXML.step.B;
            }
        }
        string WriteOnStream(Stream fs,MusicXML.scorepartwise partwise)
        { 
            
            MemoryStream ms = new MemoryStream();
            string doctype = "<!DOCTYPE score-partwise PUBLIC \"-//Recordare//DTD MusicXML 2.0 Partwise//EN\" \"http://www.musicxml.org/dtds/partwise.dtd\">";
            StreamWriter writter = new StreamWriter(ms);
            StreamReader reader = new StreamReader(ms);
            //ms.WriteString(doctype+"\n\r");

            XmlSerializer serializer = new XmlSerializer(typeof(MusicXML.scorepartwise));
            XmlSerializerNamespaces nms = new XmlSerializerNamespaces();
            //nms.Add("", "");
            serializer.Serialize(ms, partwise/*, nms*/);

            ms.Position = 0;
            ms.Seek(0, SeekOrigin.Begin);

            string all = reader.ReadToEnd();
            Console.WriteLine(all);

            ms.Position = 0;
            ms.Seek(0, SeekOrigin.Begin);
            
            StreamWriter fswritter = new StreamWriter(fs);

            fswritter.WriteLine(reader.ReadLine());
            fswritter.WriteLine(doctype);

            while (!reader.EndOfStream)
                fswritter.WriteLine(reader.ReadLine());

            fswritter.Flush();

            ms.Seek(0, SeekOrigin.Begin);
            ms.Position = 0;
            StreamReader fsreader = new StreamReader(ms);
            string res = fsreader.ReadToEnd();
            

            //fswritter.Write(reader.ReadToEnd());

            fswritter.Dispose();
            ms.Dispose();
            reader.Dispose();
            writter.Dispose();

            return res;
        }
    }
}
