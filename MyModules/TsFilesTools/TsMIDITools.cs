using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsFilesTools
{
    public class TsMIDITools
    {
        public static float FrequencyToCents(float fref, float ftest)
        {
            double k = Math.Pow(2, 1.0 / 1200);
            return (float)((Math.Log(ftest) - Math.Log(fref)) / Math.Log(k));
        }
        public static float DistanceBetweenNotes(float fref,float ftest)
        {
            double k = Math.Pow(2, 1.0 / 12);
            return (float)((Math.Log(ftest) - Math.Log(fref)) / Math.Log(k));
        }
        public static int FrequencyToMidi(float f)
        {
            double fb = 8.1757989156f;
            double midi = Math.Log(f / fb) * 12 / Math.Log(2);

            int min = (int)midi;
            int max = min + 1;

            if (Math.Abs(midi - min) < Math.Abs(midi - max))
                return min;

            return max;
        }
        public static double FrequencyFor(int midi)
        {
            double bs = 8.1757989156f;
            double part = Math.Pow(2, 1.0 / 12 * midi);
            return bs * part;
        }

        public static int OctaveFor(int midi)
        {
            return midi / 12 - 5;
        }

        public static int OctaveBase4(int midi)
        {
            return midi / 12 - 1;
        }

        public static int ValueInOctave(int midi)
        {
            return midi % 12;
        }

        public static string NoteFor(int midi)
        {
            string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            return notes[midi % 12];
        }
    }
}
