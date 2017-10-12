using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TsFilesTools
{
    public unsafe class WaveAPI
    {
        private const int MMSYSERR_BASE = 0;
        public const uint WAVE_MAPPER = unchecked((uint)(-1));
        public enum MMSYSERR : int
        {
            NOERROR = 0,
            ERROR = (MMSYSERR_BASE + 1),
            BADDEVICEID = (MMSYSERR_BASE + 2),
            NOTENABLED = (MMSYSERR_BASE + 3),
            ALLOCATED = (MMSYSERR_BASE + 4),
            INVALHANDLE = (MMSYSERR_BASE + 5),
            NODRIVER = (MMSYSERR_BASE + 6),
            NOMEM = (MMSYSERR_BASE + 7),
            NOTSUPPORTED = (MMSYSERR_BASE + 8),
            BADERRNUM = (MMSYSERR_BASE + 9),
            INVALFLAG = (MMSYSERR_BASE + 10),
            INVALPARAM = (MMSYSERR_BASE + 11),
            HANDLEBUSY = (MMSYSERR_BASE + 12),
            INVALIDALIAS = (MMSYSERR_BASE + 13),
            BADDB = (MMSYSERR_BASE + 14),
            KEYNOTFOUND = (MMSYSERR_BASE + 15),
            READERROR = (MMSYSERR_BASE + 16),
            WRITEERROR = (MMSYSERR_BASE + 17),
            DELETEERROR = (MMSYSERR_BASE + 18),
            VALNOTFOUND = (MMSYSERR_BASE + 19),
            NODRIVERCB = (MMSYSERR_BASE + 20),
            LASTERROR = (MMSYSERR_BASE + 20),
            BADFORMAT = (MMSYSERR_BASE + 32)
        }

        [DllImport("winmm.dll")]
        public static extern MMSYSERR waveOutOpen(
            ref IntPtr phwo,
            uint uDeviceID,
            ref WAVEFORMATEX pwfx,
            IntPtr dwCallback,
            uint dwInstance,
            uint fdwOpen
            );

        [UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)]
        public unsafe delegate void DwaveInProc(IntPtr hwi, uint uMsg, uint dwInstance, WaveAPI.WAVEHDR* dwParam1, IntPtr dwParam2);

        [DllImport("winmm.dll")]
        public static extern MMSYSERR waveInOpen(ref IntPtr phwi, uint uDeviceID, ref WAVEFORMATEX pwfx, DwaveInProc dwCallback, uint dwCallbackInstance, uint fdwOpen);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMSYSERR waveInPrepareHeader(IntPtr hwi, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMSYSERR waveInUnprepareHeader(IntPtr hwi, WAVEHDR* pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMSYSERR waveInAddBuffer(IntPtr hwi,WAVEHDR* pwh,int cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMSYSERR waveInClose(IntPtr hwi);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMSYSERR waveInStart(IntPtr hwi);
        
        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMSYSERR waveInStop(IntPtr hwi);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern MMSYSERR waveInReset(IntPtr hwi);


        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        [Flags]
        public enum WaveHdrFlags : uint
        {
            WHDR_DONE = 1,
            WHDR_PREPARED = 2,
            WHDR_BEGINLOOP = 4,
            WHDR_ENDLOOP = 8,
            WHDR_INQUEUE = 16
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public unsafe struct WAVEHDR
        {
            public IntPtr lpData; // pointer to locked data buffer
            public int dwBufferLength; // length of data buffer
            public uint dwBytesRecorded; // used for input only
            public IntPtr dwUser; // for client's use
            public WaveHdrFlags dwFlags; // assorted flags (see defines)
            public uint dwLoops; // loop control counter
            public IntPtr lpNext; // PWaveHdr, reserved for driver
            public IntPtr reserved; // reserved for driver
        }

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutPrepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, int uSize);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutWrite(IntPtr hwo, ref WAVEHDR pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutUnprepareHeader(IntPtr hwo, ref WAVEHDR pwh, int cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int waveOutClose(IntPtr hwo);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateEventA(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        public const uint CALLBACK_FUNCTION = 0x00030000;

        public const ushort WAVE_FORMAT_PCM = 1;

        public const uint WOM_DONE = 0x3BD;
        public const uint WOM_OPEN = 0x3BB;
        public const uint WOM_CLOSE = 0x3BC;

        public const uint WIM_OPEN = 0x3BE ;
        public const uint WIM_CLOSE = 0x3BF;
        public const uint WIM_DATA = 0x3C0;

        public const uint CALLBACK_EVENT = 0x50000;

    }
}
