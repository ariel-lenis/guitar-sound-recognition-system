using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI.Tools
{
    public class TsAlterationsCursor
    {
        struct ANote 
        {
            public Elements.TsNoteData.Enote Note;
            public int Octave;
        }

        TsMeasure measure;

        Dictionary<Elements.TsNoteData.Enote, Elements.TsNoteData.Ealter> armorAlters;
        Dictionary<ANote, Elements.TsNoteData.Ealter> normalAlters;

        public TsAlterationsCursor(TsMeasure measure,TsMeasure previous)
        {
            this.measure = measure;
            armorAlters = new Dictionary<Elements.TsNoteData.Enote, Elements.TsNoteData.Ealter>();
            normalAlters = new Dictionary<ANote, Elements.TsNoteData.Ealter>();
           
            armorAlters.Add(Elements.TsNoteData.Enote.C, Elements.TsNoteData.Ealter.Normal);
            armorAlters.Add(Elements.TsNoteData.Enote.D, Elements.TsNoteData.Ealter.Normal);
            armorAlters.Add(Elements.TsNoteData.Enote.E, Elements.TsNoteData.Ealter.Normal);
            armorAlters.Add(Elements.TsNoteData.Enote.F, Elements.TsNoteData.Ealter.Normal);
            armorAlters.Add(Elements.TsNoteData.Enote.G, Elements.TsNoteData.Ealter.Normal);
            armorAlters.Add(Elements.TsNoteData.Enote.A, Elements.TsNoteData.Ealter.Normal);
            armorAlters.Add(Elements.TsNoteData.Enote.B, Elements.TsNoteData.Ealter.Normal);



            if(measure.Armor!=null)
            { 
                var notes = measure.Armor.ParseArmor();
                var alter = measure.Armor.ArmorAlter();

                foreach (var inote in notes)
                    armorAlters[inote.Note] = alter;
            }

            if (previous == null) return;

            var element = previous.Elements.Last();

            if (!(element is Elements.TsNote)) return;

            Elements.TsNote note = element as Elements.TsNote;

            if (note.Groups == null) return;

            TsGroupElement group = note.Groups.FirstOrDefault(x => x.GroupType == TsGroupElement.EGroupType.Ligadura && x.IsStart(note));
            if (group == null) return;

            foreach (var inote in note.Chords)
            { 
                Elements.TsNoteData.Ealter alter = GetStatus(inote.Note,inote.Octave);
                if (alter != inote.Alter)
                    AddAlter(inote.Note, inote.Octave, inote.Alter);
             }

        }

        public Elements.TsNoteData.Ealter GetStatus(Elements.TsNoteData.Enote note, int octave)
        {
            ANote anote = new ANote() { Note = note, Octave = octave };
            if (normalAlters.ContainsKey(anote)) return normalAlters[anote];
            return armorAlters[note];
        }

        public void AddAlter(Elements.TsNoteData.Enote note, int octave, Elements.TsNoteData.Ealter alter)
        {
            ANote anote = new ANote() { Note = note, Octave = octave };
            bool exists = normalAlters.ContainsKey(anote);

            if (exists)
                normalAlters[anote] = alter;
            else
                normalAlters.Add(anote, alter);
        }



        public List<Elements.TsNoteData> GetNeededAlterations(List<Elements.TsNoteData> required)
        {
            if (required == null) return null;
            List<Elements.TsNoteData> result = new List<Elements.TsNoteData>();
            foreach (var inote in required)
            {
                Elements.TsNoteData.Ealter current = this.GetStatus(inote.Note, inote.Octave);
                if (current != inote.Alter)
                    result.Add(inote);                
            }
            result.Sort((x, y) => x.RelativePosition - y.RelativePosition);
            return result;
        }
    }
}
