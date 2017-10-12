using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsFilesTools
{
    public class TimeMark
    {
        public TimeSpan MarkTime;
        public string ExtraData;
        //Midi Note
        private int Data1;
        public int Data2;
        private float predictedFrequency;

        

        int samplerate;

        public int SampleRate { get { return samplerate; } }

        public TimeMark RelatedMark;

        public int MarkPosition { get { return (int)(MarkTime.TotalSeconds * samplerate); } }

        public TimeMark(TimeSpan marktime, string extradata, int data2,int samplerate,float PredictedFrequency)
        {
            this.samplerate = samplerate;
            this.MarkTime = marktime; /*marktime.TotalSeconds * samplerate;*/
            this.ExtraData = extradata;
            //this.Data1 = data1;

            this.Data2 = data2;
            this.predictedFrequency = PredictedFrequency;
            
            this.Data1 = (PredictedFrequency>0)?TsMIDITools.FrequencyToMidi(PredictedFrequency):0;
        }

        public TimeMark(int data1, int data2, int samplerate,TimeSpan marktime, string extradata)
        {
            this.samplerate = samplerate;
            this.MarkTime = marktime; /*marktime.TotalSeconds * samplerate;*/
            this.ExtraData = extradata;
            this.Data2 = data2;
            this.predictedFrequency = (float)TsMIDITools.FrequencyFor(data1);
            this.Data1 = data1;
        }


        public string SNote { get { return this.Note + "" + this.Octave; } }

        public TimeMark ToNoteOFF()
        {
            return new TimeMark(MarkTime, "NoteOff", Data2, samplerate, this.predictedFrequency);        
        }

        public string Note
        {
            get { return TsMIDITools.NoteFor(Data1); }
        }
        public int Octave
        {
            get { return TsMIDITools.OctaveFor(Data1); }
        }
        public float Frequency
        {
            set
            {
                this.predictedFrequency = value;
                this.Data1 = (value > 0) ? TsMIDITools.FrequencyToMidi(value) : 0;
            }
            get
            {
                return this.predictedFrequency;            
            }
        }

        public override string ToString()
        {
            return MarkTime + " " + ExtraData + " " + Data1 + " " + Data2 + " " + this.predictedFrequency;
        }

        public string Type { get { return ExtraData; } }

        public bool IsNoteOn { get { return ExtraData == "NoteOn"; } }

        public bool IsNoteOff { get { return ExtraData == "NoteOff"; } }

        public int AproximateMIDI { get { return Data1; } }
    }
}