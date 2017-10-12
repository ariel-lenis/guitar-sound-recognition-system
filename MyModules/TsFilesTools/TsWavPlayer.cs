using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TsFilesTools
{
    public class TsWavPlayer
    {
        public static bool flag;

        public static void WriteBytes(int samplerate, int bitspersample, IntPtr data, int length, bool disposedata)
        {
            flag = false;

            Task.Run(new Action(delegate {                            
                AsyncWriteBytes(samplerate,bitspersample,data,length,disposedata);               
            }));
            //while (!flag)
            //    Thread.Sleep(10);
            


        }

        private static void AsyncWriteBytes(int samplerate, int bitspersample, IntPtr data, int length,bool disposedata)
        {
            WaveAPI.WAVEFORMATEX WaveFormat = new WaveAPI.WAVEFORMATEX();
            WaveAPI.WAVEHDR WaveHeader = new WaveAPI.WAVEHDR();
            IntPtr hWaveOut=IntPtr.Zero;

            WaveFormat.wFormatTag = 1;     // Uncompressed sound format
            WaveFormat.nChannels = 1;                    // 1=Mono 2=Stereo
            WaveFormat.wBitsPerSample = (ushort)bitspersample;               // Bits per sample per channel
            WaveFormat.nSamplesPerSec = (ushort)samplerate;           // Sample Per Second
            WaveFormat.nBlockAlign = (ushort)(WaveFormat.nChannels * WaveFormat.wBitsPerSample / 8);
            WaveFormat.nAvgBytesPerSec = WaveFormat.nSamplesPerSec * WaveFormat.nBlockAlign;
            WaveFormat.cbSize = 0;

            IntPtr fevent = WaveAPI.CreateEventA(IntPtr.Zero, false, false, null);


            if (WaveAPI.waveOutOpen(ref hWaveOut, 0, ref WaveFormat, fevent, 0, WaveAPI.CALLBACK_EVENT) != 0)
                throw new Exception("Error Opening sound card.");


            WaveHeader.lpData = data;
            WaveHeader.dwBufferLength = length;
            WaveHeader.dwFlags = 0;
            WaveHeader.dwLoops = 0;

            if (WaveAPI.waveOutPrepareHeader(hWaveOut, ref WaveHeader, Marshal.SizeOf(typeof(WaveAPI.WAVEHDR))) != 0)
                throw new Exception("Error preparing Header!");

            if (WaveAPI.waveOutWrite(hWaveOut, ref WaveHeader, Marshal.SizeOf(typeof(WaveAPI.WAVEHDR))) != 0)
                throw new Exception("Error writing to sound card!");

            flag = true;

            WaveAPI.WaitForSingleObject(fevent, int.MinValue);



            while (WaveAPI.waveOutUnprepareHeader(hWaveOut, ref WaveHeader, Marshal.SizeOf(typeof(WaveAPI.WAVEHDR))) != 0) Thread.Sleep(100);

            WaveAPI.CloseHandle(fevent);

            while (WaveAPI.waveOutClose(hWaveOut) != 0) Thread.Sleep(100);

            if (disposedata)
                Marshal.FreeHGlobal(data);
        }        
    }
}
