using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sanford.Multimedia.Midi;
using System.IO;
using NAudio.Wave;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using System.Threading;
using NAudio.Wave.SampleProviders;

namespace AldMidiWrapper
{
    public partial class AldWrapper : Form
    {
        FolderBrowserDialog folderbrowser;

        List<MarkData> Marks;

        AldRecorder theRecorder;
        AldMidiPlayer theMidiPlayer;
        bool allrecording;
        int recordingidx;
        int recordingN;
        string allrecorderwavpath;

        public AldWrapper()
        {
            InitializeComponent();
        }

        void theMidiPlayer_LoadProgressChange(object sender, int percent,bool complete)
        {
            this.Invoke(new Action(delegate
            {
                progressBar1.Value = percent;
            }));
            if (complete)
            {
                this.Invoke(new Action(delegate
                {
                    ckListInstruments.Items.Clear();
                    panelPlay.Enabled = true;
                    panelLoad.Enabled = false;
                    sliderProgress.Maximum = theMidiPlayer.GetSequenceMaximun();

                    AllChannelInstruments = theMidiPlayer.GetInstrumentsPerChannel();
                    for (int i = 0; i < AllChannelInstruments.Count; i++)
                    {
                        string say = "";
                        foreach (var j in AllChannelInstruments[i])
                            say += j + " ";
                        ckListInstruments.Items.Add(say, true);
                    }
                    if(ckForceInstrument.Checked)
                        ChangeInstrument();
                    if (ckJustFirst.Checked)
                    {
                        if (ckListInstruments.Items.Count > 0)
                        {
                            for (int i = 0; i < ckListInstruments.Items.Count; i++)
                                ckListInstruments.SetItemChecked(i, false);
                            ckListInstruments.SetItemChecked(0, true);
                        }
                    }

                    timer1.Start();

                    if (allrecording)
                    this.Invoke(new Action(delegate {
                        StartRecording(allrecorderwavpath);
                    }));

                }));
            }
        }

        private void LoadMidiFile(string path)
        {
            theMidiPlayer.Stop();
            /*
            if (ckForceInstrument.Checked == true)
            {
                string instrument = cbInstrument.Text;

            }
             * */
            theMidiPlayer.BeginLoadFile(path);
        }

        List<List<GeneralMidiInstrument>> AllChannelInstruments;

        private void Form1_Load(object sender, EventArgs e)
        {
            theRecorder = new AldRecorder(44100, 1);
            theMidiPlayer = new AldMidiPlayer();
            theMidiPlayer.LoadProgressChange += theMidiPlayer_LoadProgressChange;
            theMidiPlayer.MessageForPlay += theMidiPlayer_MessageForPlay;
            theMidiPlayer.PlayComplete += theMidiPlayer_PlayComplete;

            folderbrowser = new FolderBrowserDialog();
            folderbrowser.SelectedPath = @"C:\Tesis\Tools\TestFiles\Midis2";

            this.cbInstrument.DataSource = theMidiPlayer.TheInstrumentsManager.Keys;

            cbInstrument.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbInstrument.AutoCompleteSource = AutoCompleteSource.ListItems;
        }


        bool theMidiPlayer_MessageForPlay(object sender, ChannelMessage msg)
        {
            if (ckListInstruments.Items.Count == 0) return true;

            if (ckListInstruments.GetItemChecked(msg.MidiChannel))
            {
                if (theRecorder.Recording)
                {
                    MarkData newmark = new MarkData(theRecorder.PlayTime,msg);
                    Marks.Add(newmark);
                }
                return true;
            }
            return false;
        }
        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (theMidiPlayer.Status == AldMidiPlayer.EPlayerStatus.Stop)
                Play();
            else if (theMidiPlayer.Status == AldMidiPlayer.EPlayerStatus.Play)
                this.theMidiPlayer.Pause();
            else if (theMidiPlayer.Status == AldMidiPlayer.EPlayerStatus.Pause)
                this.theMidiPlayer.Play();
        }
        void Play()
        {
            Console.WriteLine("StartPlaying:" + theRecorder.PlayTime);
            this.theMidiPlayer.Stop();
            this.theMidiPlayer.Play();
            timer1.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            theMidiPlayer.Dispose();
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopAll();
        }
        void StopAll()
        {
            timer1.Stop();
            this.theMidiPlayer.Stop();
            /*
            this.Invoke(new Action(delegate
            {
                this.sliderProgress.Value = 0;
            }));*/
        }
        private void cbInstrument_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter && theMidiPlayer.TheInstrumentsManager.ContainsKey(cbInstrument.Text))
                ChangeInstrument();
   
        }
        private void ChangeInstrument()
        {
            bool playing = theMidiPlayer.Status == AldMidiPlayer.EPlayerStatus.Play;
            theMidiPlayer.Pause();
            theMidiPlayer.ChangeInstrument(cbInstrument.Text);
            if (playing)
                this.theMidiPlayer.Play();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {            
            if (!sliderProgress.Capture)
                if (this.theMidiPlayer.Position <= this.sliderProgress.Maximum)
                    SafeChangePosition(this.theMidiPlayer.Position);               
        }
        public void SafeChangePosition(int value)
        {
            this.sliderProgress.ValueChanged -= sliderProgress_ValueChanged;
            this.sliderProgress.Value = value;
            this.Text = value + "/" + sliderProgress.Maximum;
            this.sliderProgress.ValueChanged += sliderProgress_ValueChanged;
        }
        private void sliderProgress_ValueChanged(object sender, EventArgs e)
        {
           this.theMidiPlayer.Position = this.sliderProgress.Value;
        }
        void StartRecording(string path)
        {
            Marks = new List<MarkData>();
            StopAll();
            panelConfig.Enabled = false;
            theRecorder.StartRecording(path);
            if (!allrecording)
            {
                btnStartRecord.Text = "Stop Recording";
                btnRecordAll.Enabled = false;
            }
            else
            {
                btnRecordAll.Text = "Stop All Recording";
                btnStartRecord.Enabled = false;            
            }
            Task.Run(new Action(delegate
            {
                Thread.Sleep(500);
                this.Invoke(new Action(delegate { Play(); }));
            }));        
        }
        public void StopRecording()
        {
            theRecorder.StopRecording();
            StopAll();  

            string datafile = this.theRecorder.RecordingPath.Replace(".wav", ".aldmidi");            
            AldTools.SaveMarks(datafile, Marks);



            if (!allrecording)
            {
                this.Invoke(new Action(delegate
                {
                    btnStartRecord.Enabled = true;
                    btnRecordAll.Text = "Start All Recording";
                    btnStartRecord.Text = "Start Recording";
                    btnRecordAll.Enabled = true;
                    panelConfig.Enabled = true;
                    MessageBox.Show("End one xD");
                }));
            }
            else
            {
                recordingidx++;
                if (recordingidx == recordingN)
                {
                    this.Invoke(new Action(delegate
                    {
                        allrecording = false;
                        btnStartRecord.Enabled = true;
                        btnRecordAll.Text = "Start All Recording";
                        panelConfig.Enabled = true;
                        MessageBox.Show("End All xD");
                    }));
                }
                else
                    ContinueAllRecording();
            }
        }

        void theMidiPlayer_PlayComplete(object obj)
        {
            if (theRecorder.Recording)
            {
                Task.Run(new Action(delegate
                {
                    Thread.Sleep(3000);
                    StopRecording();
                }));                
            }
        }
        private void btnStartRecord_Click(object sender, EventArgs e)
        {
            if (!theRecorder.Recording)
            {
                if (saveFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                StartRecording(saveFileDialog1.FileName);
            }
            else
                StopRecording();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            if (folderbrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var res = System.IO.Directory.EnumerateFiles(folderbrowser.SelectedPath, "*.mid").ToList();
                if (res.Count == 0)
                {
                    MessageBox.Show("No hay archivos midi.");
                    return;
                }
                listMidiFiles.DataSource = res;
            }
        }
        private void listMidiFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listMidiFiles.SelectedIndex >= 0)
                LoadMidiFile(listMidiFiles.SelectedItem.ToString());
        }

        private void ckListInstruments_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ckListInstruments.Items.Count; i++)
                ckListInstruments.SetItemChecked(i, true);
        }

        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ckListInstruments.Items.Count; i++)
                ckListInstruments.SetItemChecked(i, false);
        }

        private void cbInstrument_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ckForceInstrument_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.theMidiPlayer.TheInstrumentsManager.ContainsKey(cbInstrument.Text))
                cbInstrument.Text = cbInstrument.Items[0].ToString();
            cbInstrument.Enabled = !ckForceInstrument.Checked;
            if (ckForceInstrument.Checked)
                ChangeInstrument();            
        }

        private void ckJustFirst_CheckedChanged(object sender, EventArgs e)
        {

            this.btnCheckAll.Enabled = !ckJustFirst.Checked;
            this.btnUncheckAll.Enabled = !ckJustFirst.Checked;
            this.ckListInstruments.Enabled = !ckJustFirst.Checked;

            if (ckListInstruments.Items.Count > 0)
            {
                for (int i = 0; i < ckListInstruments.Items.Count; i++)
                    ckListInstruments.SetItemChecked(i, false);
                ckListInstruments.SetItemChecked(0, true);
            }
        }

        private void btnRecordAll_Click(object sender, EventArgs e)
        {
            if (!allrecording)
            {
                if (this.folderbrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                allrecording = true;
                recordingidx = 0;
                recordingN = listMidiFiles.Items.Count;
                progressFiles.Maximum = recordingN;
                ContinueAllRecording();
            }
            else
            {
                allrecording = false;
                StopRecording();
            }
        }

        private void ContinueAllRecording()
        {
            string midipath = listMidiFiles.Items[recordingidx].ToString();
            string path = this.folderbrowser.SelectedPath;
            if (!path.EndsWith("\\")) path += "\\";
            path += midipath.Substring(midipath.LastIndexOf("\\") + 1).Replace(".midi", ".wav").Replace(".mid", ".wav");
            allrecorderwavpath = path;
            this.Invoke(new Action(delegate
            {
                progressFiles.Value = recordingidx;
            }));
            LoadMidiFile(midipath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //theMidiPlayer.SendMessage(new ChannelMessage(ChannelCommand.ProgramChange,1,theMidiPlayer.TheInstrumentsManager.GetInstrumentKey(cbInstrument.Text)));

            theMidiPlayer.SendMessage(new ChannelMessage(ChannelCommand.ProgramChange, 1, theMidiPlayer.TheInstrumentsManager.GetInstrumentKey("AcousticGuitarNylon")));
            
            Task.Run(delegate {
                theMidiPlayer.SendMessage(new ChannelMessage(ChannelCommand.NoteOn, 1, 70, 0x7f));
                Thread.Sleep(3000);
                theMidiPlayer.SendMessage(new ChannelMessage(ChannelCommand.NoteOff, 1, 70, 0x7f));
            
            });

            
        }
    }
}
