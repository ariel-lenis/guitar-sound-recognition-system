using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AldSpecialAlgorithms;
using System.Runtime.InteropServices;

namespace TsFilesTools
{
    public unsafe class WavDescriptor
    {
        public string Riff { get; set; }
        public int ChunckSize { get; set; }
        public string Wave { get; set; }

        public string Subchunk1ID { get; set; }
        public int Subchunk1Size { get; set; }
        public Int16 AudioFormat { get; set; }
        public Int16 NumChannels { get; set; }
        public int SampleRate { get; set; }
        public int ByteRate { get; set; }
        public Int16 BlockAlign { get; set; }
        public Int16 BitsPerSample { get; set; }

        public Int16 ExtraParamSize { get; set; }
        public byte[] ExtraParam { get; set; }

        public string Subchunk2ID { get; set; }
        public int Subchunk2Size { get; set; }

        //public byte[] Data { get; set; }

        public byte[] ExtraData { get; set; }

        public TimeSpan Duration { get { return TimeSpan.FromSeconds( (double)Samples[0].Length /SampleRate); } }

        public List<short[]> Samples;


        public WavDescriptor()
        {
            this.Riff = "RIFF";
            this.Wave = "WAVE";
            this.Subchunk1ID = "fmt ";
            this.Subchunk2ID = "data";
            this.Subchunk1Size = 18;
            this.AudioFormat = 1;
            this.NumChannels = 1;
            this.SampleRate = 44100;
            this.BitsPerSample = 16;
            this.BlockAlign = (short)(this.NumChannels * BitsPerSample / 8);
            this.ByteRate = SampleRate * NumChannels * BitsPerSample / 8;
            this.Samples = new List<short[]>();
            this.ExtraParamSize = 0;
            
            this.ExtraData = new byte[0];

            this.ExtraParam = new byte[0];
           
            //this.ChunckSize = 4 + (8 + Subchunk1Size) + (8 + Subchunk2Size);
        }

        public void SaveData(string path)
        {
            FileStream file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            SaveData(file);
            file.Close();
            file.Dispose();
        
        }

        public void SaveData(Stream wavstream)
        {
            this.Subchunk2Size = Samples[0].Length * NumChannels * BitsPerSample / 8;
            this.ChunckSize = 4 + (8 + Subchunk1Size) + (8 + Subchunk2Size);

            wavstream.WriteString(Riff);
            
            wavstream.WriteInt(ChunckSize);
            wavstream.WriteString(Wave);
            wavstream.WriteString(Subchunk1ID);
            wavstream.WriteInt(Subchunk1Size);
            wavstream.WriteInt16(AudioFormat);
            wavstream.WriteInt16(NumChannels);
            wavstream.WriteInt(SampleRate);
            wavstream.WriteInt(ByteRate);
            wavstream.WriteInt16(BlockAlign);
            wavstream.WriteInt16(BitsPerSample);
            wavstream.WriteInt16(ExtraParamSize);
            wavstream.WriteBytes(ExtraParam);
            wavstream.WriteString(Subchunk2ID);
            wavstream.WriteInt(Subchunk2Size);

            for (int i = 0; i < Samples[0].Length; i++)
            {
                for (int j = 0; j < Samples.Count; j++)
                {
                    wavstream.WriteInt16(Samples[j][i]);   
                }
            }
             
            wavstream.Flush();
        }

        public static List<short[]> ProcessSamples(WavDescriptor descriptor, byte[] Data)
        {
            int bytespersample = (descriptor.BitsPerSample + 7) / 8;
            int n = Data.Length / descriptor.BlockAlign;
            List<short[]> res = new List<short[]>();

            for (int i = 0; i < descriptor.NumChannels; i++)
                res.Add(new short[n]);

            for (int i = 0; i < n; i++)
            {
                int idelta = i * descriptor.BlockAlign;
                for (int j = 0; j < descriptor.NumChannels; j++)
                    res[j][i] = BitConverter.ToInt16(Data, idelta + j * bytespersample);
            }

            return res;
        }

        public static WavDescriptor LoadDescriptor(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            var descriptor= LoadDescriptor(file);
            file.Close();
            file.Dispose();
            return descriptor;
        }

        public static WavDescriptor LoadDescriptor(Stream wavstream)
        {
            WavDescriptor descriptor = new WavDescriptor();
            descriptor.Riff = wavstream.ReadString(4);
            descriptor.ChunckSize = wavstream.ReadInt();
            descriptor.Wave = wavstream.ReadString(4);
            descriptor.Subchunk1ID = wavstream.ReadString(4);
            descriptor.Subchunk1Size = wavstream.ReadInt();
            descriptor.AudioFormat = wavstream.ReadInt16();
            descriptor.NumChannels = wavstream.ReadInt16();
            descriptor.SampleRate = wavstream.ReadInt();
            descriptor.ByteRate = wavstream.ReadInt();
            descriptor.BlockAlign = wavstream.ReadInt16();
            descriptor.BitsPerSample = wavstream.ReadInt16();
            descriptor.ExtraParamSize = wavstream.ReadInt16();

            descriptor.ExtraParam = wavstream.ReadBytes(descriptor.ExtraParamSize);

            descriptor.Subchunk2ID = wavstream.ReadString(4);
            descriptor.Subchunk2Size = wavstream.ReadInt();

            var Data = wavstream.ReadBytes(descriptor.Subchunk2Size);            

            descriptor.ExtraData = wavstream.ReadBytes((int)(wavstream.Length - wavstream.Position));

            descriptor.Samples = ProcessSamples(descriptor, Data);

            return descriptor;
        }
    }
}
