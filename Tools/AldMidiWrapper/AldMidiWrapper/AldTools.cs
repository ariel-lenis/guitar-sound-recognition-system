using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.IO;
using System.Runtime.InteropServices;

namespace AldMidiWrapper
{
    public class AldTools
    {
        public static float[] GenerateReferenceWaveAsFloat(double seconds, int samplerate)
        {
            int samples = (int)(samplerate * seconds);
            float[] data = new float[samples];

            float current = 1;

            for (int i = 0; i < data.Length; i++)
            {
                if (i % 10 == 0)
                    current = (current == 1) ? -1 : 1;
                if (i >= data.Length - 10)
                    data[i] = 0;
                else
                    data[i] = current;
            }
            
            return data;
        }

        public static TimeSpan GetWaveFileStartData(string filename)
        {
            WaveFileReader wfr = new WaveFileReader(filename);

            /*
            //float val = 0;
            while (true)
            {
                double prom = 0;
                for (int i = 0; i < 5;i++ )
                    prom+= Math.Abs(wfr.ReadNextSampleFrame()[0]);
                if (prom > 0.001 && prom < 0.9) break;
            }*/

            short last = short.MinValue;
            TimeSpan lasttime;
            while (true)
            {
                int cont = 0;
                
                while (wfr.ReadNextSampleFrame()[0] > -1) ;

                lasttime = wfr.CurrentTime;
                
                for (int i = 0; i < 35; i++)
                {
                    short aux = wfr.ReadNextSampleFrameShort()[0];
                    if ((aux == short.MaxValue && last == short.MinValue) || (last == short.MaxValue && aux == short.MinValue))
                        cont++;
                    else break;
                    last = aux;
                }
                if (cont == 35) break;
            }

            wfr.Close();
            wfr.Dispose();
            return lasttime;
        }
        public static void Adjust(List<MarkData> times, TimeSpan startdata)
        {
            var m0 = times.First().Time;
            var delta = startdata.Subtract(m0);
            foreach (var i in times)
                i.Time=i.Time.Add(delta);
            /*
            TimeSpan markstart = new TimeSpan();
            foreach (var i in times)
                if (i.Command == Sanford.Multimedia.Midi.ChannelCommand.NoteOn)
                {
                    markstart = i.Time;
                    break;
                }
            TimeSpan deltatime = startdata.Subtract(markstart);
            foreach (var i in times)
                i.Time.Add(deltatime);
             * */
        }
        public static void SaveMarks(string filename, List<MarkData> marks)
        {
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs);
            foreach (var i in marks)            
                sw.WriteLine(i.Time + " " + i.Command + " " + i.Byte1 + " " + i.Byte2);
            sw.Close();
            sw.Dispose();
            fs.Close();
            fs.Dispose();
        }
    }
}
