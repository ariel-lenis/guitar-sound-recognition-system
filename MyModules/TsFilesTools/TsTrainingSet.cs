using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AldSpecialAlgorithms;
using System.ComponentModel;
using System.Windows.Forms;

namespace TsFilesTools
{
    public class TsTrainingSet
    {
        string waveFile;
        string subsFile;

        List<TimeMark> originalMarkers;

        public List<TimeMark> OriginalMarkers { get { return this.originalMarkers; } }

        List<TimeMark> markers;
        WavDescriptor wavDescriptor;

        public string WaveFile { get { return waveFile; } }
        public string SubsFile { get { return subsFile; } }

        float[] wave;

        [Browsable(false)]
        public float[] Wave { get { return wave; } }

        short[] rawwave;
        public short[] RawWave { get { return rawwave; } }

        int samplerate;

        public int SampleRate { get { return samplerate; } }

        int bitspersample;
        public int BitsPerSample { get { return bitspersample; } }

        TimeSpan duration;

        public TimeSpan Duration { get { return duration; } }

        public TimeSpan adjust;


        public TimeSpan Adjust 
        {
            get {return adjust;}
            set
            {
                TimeSpan delta = value.Subtract(adjust);
                foreach (var i in markers)
                    i.MarkTime=i.MarkTime.Add(delta);
                this.adjust = value;
            }
        }
        public string WaveName { get { return new FileInfo(waveFile).Name; } }
        public bool HaveSubs { get { return subsFile != null; } }

        public TsTrainingSet(string waveFile)
        {
            this.waveFile = waveFile;
            this.subsFile=waveFile.Replace(".wav",".aldmidi");
            if(!File.Exists(this.subsFile))
                this.subsFile = null;
        }


        public float MarksError(float maxseconds)
        {
            if (!this.HaveSubs) return 0;

            float[] toriginal = this.originalMarkers.Where(x=>x.IsNoteOn).Select(x => (float)x.MarkTime.TotalSeconds).Distinct().ToArray();
            float[] tmarkers = this.markers.Where(x=>x.IsNoteOn).Select(x => (float)x.MarkTime.TotalSeconds).Distinct().ToArray();

            return ErrorTools.CompareMarkers(toriginal, tmarkers,maxseconds);
        }


        public void LoadFilesInformation()
        {
            wavDescriptor = WavDescriptor.LoadDescriptor(waveFile);
            samplerate = wavDescriptor.SampleRate;
            bitspersample = wavDescriptor.BitsPerSample;
            duration = wavDescriptor.Duration;
            rawwave = wavDescriptor.Samples[0];

            int factor = (int)(short.MaxValue) + 1;

            wave = new float[rawwave.Length];

            for (int i = 0; i < rawwave.Length;i++ )
                wave[i] = (float)rawwave[i]/factor;

            wave.ChebNormalization(0.99f);


            LoadMarkers(wavDescriptor.SampleRate);
        }

        public void UnloadFilesInformation()
        {
            if(markers!=null)
                markers.Clear();
            
            wavDescriptor = null;
            markers = null;
            GC.Collect();
        }

        public List<TimeMark> Markers { get { return markers; } set { this.markers = value; } }
        public WavDescriptor WaveDescriptor { get { return wavDescriptor; } }

        private void LoadMarkers(int samplerate)
        {
            markers = new List<TimeMark>();

            if (subsFile == null) return;            
            TimeSpan? last = null;
            string lasti = "";

            List<TimeMark> notes = new List<TimeMark>();

            foreach (var i in File.ReadAllLines(subsFile))
            {
                string[] sub = i.Split(' ');

                TimeSpan time = TimeSpan.Parse(sub[0]).Add(TimeSpan.FromSeconds(0.0));
                string command = sub[1];
                byte b1, b2;
                b1 = byte.Parse(sub[2]);
                b2 = byte.Parse(sub[3]);

                float predictedf = 0;

                if (sub.Length > 4)
                    predictedf = float.Parse(sub[4]);
                else
                    predictedf = (float)TsMIDITools.FrequencyFor(b1);
                    

                var lastmark = new TimeMark(time, command, /*b1,*/ b2, samplerate,predictedf);
                markers.Add(lastmark);

                if(lastmark.IsNoteOn)
                    notes.Add(lastmark);
                if (lastmark.IsNoteOff)
                {
                    int idrmv = notes.FindLastIndex(x => x.AproximateMIDI == lastmark.AproximateMIDI);
                    notes[idrmv].RelatedMark = lastmark;
                    notes.RemoveAt(idrmv);                            
                }

                lasti = i;
                last = time;

            }

            if (notes.Count > 0)
            {
                MessageBox.Show("The File Havent integrity.");
                //Console.Beep(100, 1000);
                //throw new Exception("¬¬");
            }

            //this.originalMarkers = new List<TimeMark>();
            //this.originalMarkers.AddRange(this.markers);
            this.originalMarkers = this.markers;
        }

        public void AutoAdjust()
        {
            TsToolsMarkers.Adjust(this);
        }

        public void SaveAdjust()
        {
            if (Markers.Count == 0) return;
            adjust = new TimeSpan();
            string res = "";
            foreach (var i in Markers)
                res += i + "\n";
            
            File.WriteAllText(subsFile, res);
        }

        public void Update()
        {
            LoadMarkers(this.samplerate);
        }
    }
}
