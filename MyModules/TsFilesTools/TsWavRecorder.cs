using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TsFilesTools
{
    public unsafe class TsWavRecorder
    {
        WaveAPI.WAVEFORMATEX format;
        IntPtr hwavein;
        WaveAPI.WAVEHDR* headers;

        List<short[]> record = new List<short[]>();
        int maxpackets;

        WaveAPI.DwaveInProc callback;

        bool recording;

        const int BUFFERSN = 64;

        bool []buffersflag = new bool[BUFFERSN];

        public bool Recording { get { return recording; } }

        bool stopped;
        
        public short[] GetRecord()
        {
            if (!recording && !stopped) return new short[0];

            short[] res;            
            lock (record)
            {
                int n = record.Sum(x=>x.Length);
                res = new short[n];
                int cont = 0;

                foreach (var i in record)
                    for (int j = 0; j < i.Length; j++)
                        res[cont++] = i[j];                
            }
            return res;
        }

        public short[] GetSubRecord(int lastn)
        {
            if (!recording && !stopped) return new short[0];

            short[] res;
            lock (record)
            {
                List<short[]> rec = new List<short[]>();

                for (int i = record.Count - 1; i >= record.Count - lastn && i>=0; i--)
                    rec.Add(record[i]);

                int n = rec.Sum(x => x.Length);
                res = new short[n];
                int cont = 0;

                foreach (var i in rec)
                    for (int j = 0; j < i.Length; j++)
                        res[cont++] = i[j];
            }
            return res;
        }
        
        void waveInProc(IntPtr hwi, uint uMsg, uint dwInstance, WaveAPI.WAVEHDR* dwParam1, IntPtr dwParam2)
        {
            //if (!recording) return;

	        //printf("msg:%d\n",uMsg);
	        switch(uMsg)
	        {
		        case WaveAPI.WIM_CLOSE:

			        break;

                case WaveAPI.WIM_DATA:
		        {
                    WaveAPI.MMSYSERR mRes;
			        WaveAPI.WAVEHDR* pHdr = dwParam1;

                    if (WaveAPI.WaveHdrFlags.WHDR_DONE == (WaveAPI.WaveHdrFlags.WHDR_DONE & pHdr->dwFlags))
			        {
                        EnQueue(pHdr);
				        //Debug.WriteLine("Recorded:{0}",pHdr->dwBytesRecorded);

                        if (recording)
                        {
                            mRes = WaveAPI.waveInAddBuffer(hwi, pHdr, Marshal.SizeOf(typeof(WaveAPI.WAVEHDR)));
                            if (mRes != WaveAPI.MMSYSERR.NOERROR)
                                Debug.WriteLine("Error adding");
                            buffersflag[(int)pHdr->dwUser] = false;
                        }
                        else
                        {
                            Marshal.FreeHGlobal(pHdr->lpData);
                            WaveAPI.waveInUnprepareHeader(hwavein, pHdr, Marshal.SizeOf(typeof(WaveAPI.WAVEHDR)));
                            buffersflag[(int)pHdr->dwUser] = true;
                        }
			        }
		        }
		        break;

                case WaveAPI.WIM_OPEN:
			        break;

		        default:
			        break;
	        }
        }

        private unsafe void EnQueue(WaveAPI.WAVEHDR* pHdr)
        {
            short[] aux = new short[pHdr->dwBytesRecorded / sizeof(short)];
            Marshal.Copy(pHdr->lpData, aux, 0, aux.Length);

            lock (record)
            {
                record.Add(aux);
                if (maxpackets > 0 && record.Count > maxpackets)
                    record.RemoveAt(0);
            }
        }

        public TsWavRecorder(int maxpackets = 0)
        {
            this.maxpackets = maxpackets;

            format = new WaveAPI.WAVEFORMATEX();
            format.cbSize = (ushort)Marshal.SizeOf(typeof(WaveAPI.WAVEFORMATEX));
            format.nChannels = 1;
            format.wBitsPerSample = 16;
            format.nBlockAlign = (ushort)(format.nChannels * format.wBitsPerSample / 8);
            format.nSamplesPerSec = 44100;
            format.wFormatTag = WaveAPI.WAVE_FORMAT_PCM;
            format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;            
        }
        public bool Start()
        {
            stopped = false;

            record = new List<short[]>();
            recording = false;
            hwavein = IntPtr.Zero;

            WaveAPI.MMSYSERR res = WaveAPI.waveInOpen(ref hwavein, WaveAPI.WAVE_MAPPER, ref format,callback = new WaveAPI.DwaveInProc(waveInProc), 0, WaveAPI.CALLBACK_FUNCTION);
            if (res != WaveAPI.MMSYSERR.NOERROR)
            {
                Debug.WriteLine("Error opening");
                return false;
            }

            headers = (WaveAPI.WAVEHDR*)Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WaveAPI.WAVEHDR)) * BUFFERSN);

            for (int i = 0; i < BUFFERSN; i++)
            {
                buffersflag[i] = false;
                headers[i].lpData = Marshal.AllocHGlobal((int)format.nAvgBytesPerSec/100);
                headers[i].dwBufferLength = (int)(format.nAvgBytesPerSec)/100;
                headers[i].dwUser = (IntPtr)i;
                headers[i].dwFlags = 0;

                res = WaveAPI.waveInPrepareHeader(hwavein, &headers[i], Marshal.SizeOf(typeof(WaveAPI.WAVEHDR)));
                if (res != WaveAPI.MMSYSERR.NOERROR)
                {
                    Debug.WriteLine("Error header :"+res);
                    return false;
                }
                res = WaveAPI.waveInAddBuffer(hwavein, &headers[i], Marshal.SizeOf(typeof(WaveAPI.WAVEHDR)));
                if (res != WaveAPI.MMSYSERR.NOERROR)
                {
                    Debug.WriteLine("Error adding");
                    return false;
                }
            }
            Debug.Write("Starting wave");
            Debug.WriteLine(WaveAPI.waveInStart(hwavein));
            recording = true;
            return true;
        }
        public bool Stop()
        {
            recording = false;

            int cflag = 0;
            
            do
            {
                cflag = 0;                
                for (int i = 0; i < buffersflag.Length; i++)
                    if (!buffersflag[i])
                        cflag++;
                Thread.Sleep(50);
            } while (cflag > 0);


            //Thread.Sleep(2000);
            var res  = WaveAPI.waveInStop(hwavein);

            Debug.WriteLine(res);
            stopped = true;
            return res== WaveAPI.MMSYSERR.NOERROR;

        }
        public void Dispose()
        {
            if ((IntPtr)headers != IntPtr.Zero)
            {
                /*
                for (int i = 0; i < BUFFERSN; i++)
                {
                    Marshal.FreeHGlobal(headers[i].lpData);
                    WaveAPI.waveInUnprepareHeader(hwavein, &headers[i], Marshal.SizeOf(typeof(WaveAPI.WAVEHDR)));
                }
                 */
                Marshal.FreeHGlobal((IntPtr)headers);
            }

            if (hwavein != IntPtr.Zero)
            {
                Debug.Write("Closing : ");
                Debug.WriteLine(WaveAPI.waveInClose(hwavein));
            }

        }
    }
}
