using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsPentagramEngine
{
    public class InputFormat
    {
        public List<int> MidiNotes;
        public TimeSpan StartNote;
        public TimeSpan EndNote;

        public TimeSpan DeltaTime
        {
            get { return EndNote.Subtract(StartNote); }
        }

        public override string ToString()
        {
            return this.Duration.ToString();
        }


        public TimeSpan Duration 
        {
            get 
            {
                return this.EndNote.Subtract(this.StartNote);
            }
        }

        public InputFormat(int midinote, TimeSpan startnote, TimeSpan endnote)
        {
            this.MidiNotes = new List<int>();
            this.MidiNotes.Add(midinote);
            this.StartNote = startnote;
            this.EndNote = endnote;
        }

        public InputFormat(List<int> midinotes, TimeSpan startnote, TimeSpan endnote)
        {
            this.MidiNotes = midinotes;
            this.StartNote = startnote;
            this.EndNote = endnote;
        }
    }
}
