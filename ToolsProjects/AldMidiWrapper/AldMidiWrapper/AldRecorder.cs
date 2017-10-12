using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldMidiWrapper
{
    public class AldRecorder:IDisposable
    {
        int samplerate;
        int channel;

        IWaveIn wavein;
        WaveFileWriter ws;

        bool recording;
        Stopwatch playTime;

        public bool Recording { get { return recording; } }
        public TimeSpan PlayTime { get { return playTime.Elapsed; } }

        string recordingPath;
        public string RecordingPath { get { return recordingPath; } }

        public AldRecorder(int samplerate, int channel)
        {
            this.samplerate = samplerate;
            this.channel = channel;
            playTime = new Stopwatch();
        }
        void wavein_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (ws != null)
                ws.Write(e.Buffer, 0, e.BytesRecorded);
        }
        public void StartRecording(string path)
        {
            //wavein = new NAudio.Wave.WaveInEvent(); 
            //var deviceEnum = new MMDeviceEnumerator();
            //var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
            //var device = devices.First();
            //wavein = new WasapiCapture(device);
            this.recordingPath = path;
            StopRecording();
            recording = true;
            CreateWaveIn();
            ws = new WaveFileWriter(new FileStream(path, FileMode.OpenOrCreate),wavein.WaveFormat);
            playTime.Start();
            wavein.StartRecording();    
        }
        void CreateWaveIn()
        {
            wavein = new WaveIn();
            wavein.WaveFormat = new WaveFormat(samplerate, channel);
            wavein.DataAvailable += wavein_DataAvailable;

        }
        
        public void StopRecording()
        {
            if (ws != null)
            {
                wavein.StopRecording();
                ws.Close();
                ws.Dispose();
                ws = null;
                wavein.DataAvailable -= wavein_DataAvailable;
            }
            playTime.Reset();
            recording = false;
        }


        public void Dispose()
        {
            StopRecording();
        }
    }
}
