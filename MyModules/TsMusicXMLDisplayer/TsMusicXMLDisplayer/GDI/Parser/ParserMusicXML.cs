using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI.Parser
{
    public class ParserMusicXML
    {
        MusicXML.scorepartwise scorepartwise;
        string title;
        string creator;
        string copyright;

        public ParserMusicXML(MusicXML.scorepartwise scorepartwise)
        {
            this.scorepartwise = scorepartwise;
            this.title = "Untitled";
            this.creator = "No Creator";
            this.copyright = "No Copyright";

            if (this.scorepartwise.movementtitle != null)
                this.title = this.scorepartwise.movementtitle;

            if(this.scorepartwise.identification!=null)
            {
                if (this.scorepartwise.identification.creator != null && this.scorepartwise.identification.creator.Length > 0)
                    this.creator = this.scorepartwise.identification.creator[0].Value??this.creator;
                if (this.scorepartwise.identification.rights != null && this.scorepartwise.identification.rights.Length > 0)
                    this.copyright = this.scorepartwise.identification.rights[0].Value ?? this.copyright;
            }

        }

        public TsPartwise Parse()
        {
            TsPartwise partwise = new TsPartwise();
            List<TsGroupElement> groups = new List<TsGroupElement>();

            partwise.Title = this.title;
            partwise.Autor = this.creator;
            partwise.Copyright = this.copyright;

            var part = this.scorepartwise.part[0];
            var measures = part.measure;
            for (int i = 0; i < measures.Length; i++)
                partwise.Measures.Add(ParseMeasure(groups,partwise,measures, i));
            return partwise;
        }

        private TsMeasure ParseMeasure(List<TsGroupElement> groups,TsPartwise tspartwise, MusicXML.scorepartwisePartMeasure[] measures, int i)
        {
            var measure = measures[i];
            TsMeasure tsmeasure = new TsMeasure(tspartwise,i);

            List<MusicXML.note> notes = new List<MusicXML.note>();
            List<MusicXML.attributes> attributes = new List<MusicXML.attributes>();
            List<MusicXML.direction> directions = new List<MusicXML.direction>();
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

            /*
            MusicXML.clef signclef = new MusicXML.clef();
            signclef.sign = MusicXML.clefsign.G;
            signclef.line = "2";
            signclef.clefoctavechange = "0";

            MusicXML.clef signtab = new MusicXML.clef();
            signtab.sign = MusicXML.clefsign.TAB;
            signtab.line = "5";


            MusicXML.key key = new MusicXML.key();
            key.Items = new object[] { "0", "major" };
            key.ItemsElementName = new MusicXML.ItemsChoiceType8[] { MusicXML.ItemsChoiceType8.fifths, MusicXML.ItemsChoiceType8.mode };

            MusicXML.time time = new MusicXML.time();
            time.Items = new object[] { "4", "4" };
            time.ItemsElementName = new MusicXML.ItemsChoiceType9[] { MusicXML.ItemsChoiceType9.beats, MusicXML.ItemsChoiceType9.beattype };
            */

            MusicXML.clef signclef=null;
            MusicXML.clef signtab = null;
            MusicXML.key key = null;
            MusicXML.time time = null;


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

            //if (signclef.sign != MusicXML.clefsign.G) throw new Exception("Just G sign is not supported for now.");

            int num = 0;
            int den = 0;
            for (int j = 0; time != null && j < time.ItemsElementName.Length; j++)
            {
                if (time.ItemsElementName[j] == MusicXML.ItemsChoiceType9.beats)
                    num = int.Parse(time.Items[j].ToString());
                else if (time.ItemsElementName[j] == MusicXML.ItemsChoiceType9.beattype)
                    den = int.Parse(time.Items[j].ToString());
                else
                    throw new Exception("Invalid time property.");
            }


            List<TsNote> tsnotes = new List<TsNote>();

            tsmeasure.Elements = new List<Elements.TsElement>();

            if(signclef!=null)
            { 
                tsmeasure.AddElement(new Elements.TsClef(tsmeasure)
                {
                    Clef = ParseNote(signclef.sign),
                    Line = int.Parse(signclef.line),
                    Octave = 4+int.Parse(signclef.clefoctavechange??"0")                    
                });
            }

            ParseArmor(tsmeasure, key);
            
            if(time!=null)
            { 
                tsmeasure.AddElement(new Elements.TsTime(tsmeasure)
                {
                        Beats = num,
                        BeatType=den,
                        Divisions=divisions
                });
            }
            
            ParseNotes(tsmeasure, notes,groups);
            
            return tsmeasure;
        }

        private void ParseArmor(TsMeasure tsmeasure, MusicXML.key key)
        {
            if (key == null) return;

            string fifhts = "";
            string mode = "";

            for (int i = 0; i < key.ItemsElementName.Length; i++)
                if (key.ItemsElementName[i] == MusicXML.ItemsChoiceType8.fifths)
                    fifhts = key.Items[i].ToString();
                else if (key.ItemsElementName[i] == MusicXML.ItemsChoiceType8.mode)
                    mode = key.Items[i].ToString();

            int number = int.Parse(fifhts);
            int anumber = number;

            tsmeasure.AddElement(new Elements.TsArmor(tsmeasure)
            {
                DiskDirection = number,
                ScaleMode = (mode=="minor")?TsMeasure.EScaleMode.Minor:TsMeasure.EScaleMode.Major
            });
        }

        private void ParseNotes(TsMeasure measure, List<MusicXML.note> notes,List<TsGroupElement> groups)
        {
            Elements.TsNoteData notedata;
            int duration;
            bool silence;
            bool chord;
            Elements.TsNote last=null;

            bool isTieStart;
            bool isTieEnd;
            
            foreach (var itemnote in notes)
            {
                ParseNote(itemnote, out notedata, out duration, out silence, out chord,out isTieStart,out isTieEnd);
                if (silence)     
                {
                    measure.Elements.Add(new Elements.TsSilence(measure){ Duration = duration });
                    last = null;
                }
                else
                {
                    if (chord)
                    {
                        if (last == null) throw new Exception("Chord for null???");
                        last.Chords.Add(notedata);
                    }
                    else 
                    {
                        measure.Elements.Add(last=new Elements.TsNote(measure)
                        {
                             Duration = duration,
                             Chords = new Elements.TsChords { notedata}
                        });

                        if (isTieEnd)
                        {
                            int idx=-1;
                            for (int i = groups.Count - 1; i >= 0; i--)
                                if (groups[i].GroupType == TsGroupElement.EGroupType.Ligadura)
                                {
                                    idx = i;
                                    break;
                                }
                            if (idx > 0)
                            {
                                groups[idx].Elements.Add(last);
                                if (last.Groups == null)
                                    last.Groups = new List<TsGroupElement>();
                                last.Groups.Add(groups[idx]);
                                groups.RemoveAt(idx);
                            }
                        }
                        if (isTieStart)
                        {
                            TsGroupElement group = new TsGroupElement() { Elements = new List<Elements.TsElement>() { last }, GroupType = TsGroupElement.EGroupType.Ligadura };
                            groups.Add(group);
                            if (last.Groups == null)
                                last.Groups = new List<TsGroupElement>();
                            last.Groups.Add(group);
                        }

                    }
                }
            }
        }
        public void ParseNote(MusicXML.note note, out Elements.TsNoteData notedata, out int duration, out bool silence, out bool chord, out bool isTieStart, out bool isTieEnd)
        {
            MusicXML.pitch pitch = null;
            MusicXML.tie tie;
          
            duration = 0;
            silence = false;
            chord = false;
            notedata = null;

            isTieStart = false;
            isTieEnd = false;

            for (int i = 0; i < note.ItemsElementName.Length; i++)
            {
                switch (note.ItemsElementName[i])
                {
                    case MusicXML.ItemsChoiceType1.pitch:
                        pitch = note.Items[i] as MusicXML.pitch;
                        break;
                    case MusicXML.ItemsChoiceType1.duration:
                        duration = int.Parse(note.Items[i].ToString());
                        break;
                    case MusicXML.ItemsChoiceType1.chord:
                        chord = true;
                        break;
                    case MusicXML.ItemsChoiceType1.rest:
                        silence = true;
                        break;
                    case MusicXML.ItemsChoiceType1.tie:
                        tie = note.Items[i] as MusicXML.tie;
                        if (tie.type == MusicXML.startstop.start)
                            isTieStart = true;
                        else
                            isTieEnd = true;
                        break;
                }
            }

            
            
            if (!silence && pitch != null)
            {
                notedata = new Elements.TsNoteData();
                notedata.Octave=int.Parse(pitch.octave);
                notedata.Note =  (Elements.TsNoteData.Enote)NoteToNumber(pitch.step);
                if (Math.Abs(pitch.alter) > 1) throw new Exception("Bad alter!!!");
                notedata.Alter =  (Elements.TsNoteData.Ealter)pitch.alter;
            }
      
        }
        public static int NoteToNumber(MusicXML.step note)
        {
            MusicXML.step[] steps = new MusicXML.step[] { MusicXML.step.C, MusicXML.step.D, MusicXML.step.E, MusicXML.step.F, MusicXML.step.G, MusicXML.step.A, MusicXML.step.B };
            for (int i = 0; i < steps.Length; i++)
                if (steps[i] == note)
                    return i;
            throw new Exception("Bad note!!!");
        }
        private Elements.TsClef.EClef ParseNote(MusicXML.clefsign clefsign)
        {
            switch (clefsign)
            { 
                case MusicXML.clefsign.C:
                    return Elements.TsClef.EClef.C;
                case MusicXML.clefsign.F:
                    return Elements.TsClef.EClef.F;
                case MusicXML.clefsign.G:
                    return Elements.TsClef.EClef.G;
            }
            throw new Exception("Clef note is not recognized.");
        }



    }
}
