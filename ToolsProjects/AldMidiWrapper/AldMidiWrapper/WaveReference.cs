using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldMidiWrapper
{
    class AldWaveDataProvider : ISampleProvider
    {
        public WaveFormat waveFormat;
        public float[] Data;
        int position;

        public AldWaveDataProvider(float[] Data, WaveFormat waveFormat)
        {
            this.Data = Data;
            this.waveFormat = waveFormat;
            position = 0;
        }
        public void Reset()
        {
            position = 0;
        }
        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int ini = offset + position;
            if (ini > Data.Length) return 0;
            if (ini + count > Data.Length) count = Data.Length - ini;

            

            for (int i = 0; i < count; i++)
                buffer[i] = Data[ini + i];

            //Array.Copy(Data, ini, buffer, 0, count);

            position += count;

            return count;
        }
    }
}
