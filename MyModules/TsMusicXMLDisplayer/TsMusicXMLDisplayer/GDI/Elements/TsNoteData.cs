using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI.Elements
{
    public class TsNoteData
    {
        public enum Enote { C = 0, D, E, F, G, A, B };
        public enum Ealter { Normal = 0, Sharp = 1, Bemol = -1 };

        public int Octave { get; set; }
        public Enote Note { get; set; }
        public Ealter Alter { get; set; }

        public int RelativePosition
        {
            get
            {
                return Octave * 7 + (int)Note;
            }
        }

        public void LoadFromNote(string note)
        {
            char[] notes = { 'C', 'D', 'E', 'F', 'G', 'A', 'B' };
            char cnote = note[0];

            for (int i = 0; i < notes.Length; i++)
                if (notes[i] == cnote)
                { 
                    this.Note = (Enote)i;
                    break;
                }

            this.Octave = int.Parse(note.Substring(1));
        }

        public void LoadFromRelativeNote(string note, TsClef tsClef)
        {
            this.LoadFromNote(note);
            this.Octave += tsClef.NoteData.Octave;
        }

        public int PentagramSubLine(TsClef clef)
        {
            int line = clef.Line;
            int subline = (clef.Line - 1) * 2;
            TsNoteData clefnote = clef.NoteData;        
            return subline + this.RelativePosition - clefnote.RelativePosition;            
        }


    }
}
