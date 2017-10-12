using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanford.Multimedia.Midi;

namespace AldFirstNetworkTrainer
{
    public class AldMidiPlayer: IDisposable
    {
        OutputDevice output = new OutputDevice(0);

        Sequence theSequence;
        Sequencer theSequencer;

        public enum EPlayerStatus { Stop, Play, Pause };
        EPlayerStatus status;

        public EPlayerStatus Status { get { return status; } }

        public delegate void DProgressChange(object sender,int percent,bool complete);
        public delegate bool DMessageForPlay(object sender, ChannelMessage msg);


        public event DProgressChange LoadProgressChange;
        public event DMessageForPlay MessageForPlay;
        public event Action<object> PlayComplete;

        InstrumentsManager instrumentsManager;
        public InstrumentsManager TheInstrumentsManager { get { return instrumentsManager; } }

        public int Position
        {
            get { return theSequencer.Position; }
            set { theSequencer.Position = value; }
        }

        public void ChangeInstrument(string instrumentName)
        {
            instrumentsManager.ChangeInstrument(theSequence, instrumentName);
        }
        public void Pause()
        {
            status = EPlayerStatus.Pause;
            theSequencer.Stop();        
        }
        public void Play()
        {
            if (status == EPlayerStatus.Pause)
                theSequencer.Continue();
            else
                theSequencer.Start();
            status = EPlayerStatus.Play;
        }
        public void Stop()
        {
            theSequencer.Position = 0;
            status = EPlayerStatus.Stop;
            theSequencer.Stop();
        }
        public int GetSequenceMaximun()
        {
            return theSequence.GetLength();
        }
        public List<List<GeneralMidiInstrument>> GetInstrumentsPerChannel()
        {
            List<List<GeneralMidiInstrument>> result = new List<List<GeneralMidiInstrument>>();
            for (int i = 0; i < theSequence.Count; i++)
                theSequence[i].GetInstrumentsPerChannel(result);
            return result;
        }
        public AldMidiPlayer()
        {
            instrumentsManager = new InstrumentsManager();
            theSequence = new Sequence() { Format = 1 };
            theSequencer = new Sequencer();
            theSequencer.Sequence = theSequence;
            theSequence.LoadCompleted += theSequence_LoadCompleted;
            theSequence.LoadProgressChanged += theSequence_LoadProgressChanged;
            theSequencer.ChannelMessagePlayed += theSequencer_ChannelMessagePlayed;
            theSequencer.Chased += theSequencer_Chased;
            theSequencer.Stopped += theSequencer_Stopped;
            theSequencer.PlayingCompleted += theSequencer_PlayingCompleted;
            this.SendMessage(new ChannelMessage(ChannelCommand.ProgramChange, 1, this.TheInstrumentsManager.GetInstrumentKey("AcousticGuitarNylon")));
        }

        void theSequencer_PlayingCompleted(object sender, EventArgs e)
        {
            if (PlayComplete != null)
                PlayComplete(this);
        }

        void theSequencer_Stopped(object sender, StoppedEventArgs e)
        {
            foreach (ChannelMessage i in e.Messages)
                SendMessage(i);
        }

        void theSequencer_Chased(object sender, ChasedEventArgs e)
        {
            foreach (ChannelMessage i in e.Messages)
                SendMessage(i);
        }

        public void SendMessage(ChannelMessage i)
        {
            if (MessageForPlay != null)
            {
                if (MessageForPlay(this, i))
                    if (!output.IsDisposed)
                        output.Send(i);
            }
            else
            {
                if(!output.IsDisposed)
                    output.Send(i);
            }
        }

        public void SendMessage(SysExMessage i)
        {
            output.Send(i);
        }

        void theSequencer_ChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
        {
            SendMessage(e.Message);
        }
        void theSequence_LoadProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (LoadProgressChange != null)
                LoadProgressChange(this, e.ProgressPercentage,false);
        }
        void theSequence_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (LoadProgressChange != null)
                LoadProgressChange(this, 100,true);
        }
        public void BeginLoadFile(string path)
        {
            theSequence.LoadAsync(path);
            theSequencer.Position = 0;
        }

        public void Dispose()
        {
            theSequencer.ChannelMessagePlayed -= theSequencer_ChannelMessagePlayed;
            theSequencer.Stopped -= theSequencer_Stopped;
            theSequencer.ChannelMessagePlayed -= theSequencer_ChannelMessagePlayed;
            theSequencer.Chased -= theSequencer_Chased;
            output.Dispose();
            this.Stop();
        }
    }
}
/*
foreach (var i in sequence1)
{
for (int j = 0; j < i.Count; j++)
{
var mevt = i.GetMidiEvent(j);
if (mevt.MidiMessage.MessageType == MessageType.Meta)
{
    MetaMessage metamsg = mevt.MidiMessage as MetaMessage;
    if (metamsg.MetaType != MetaType.TimeSignature && metamsg.MetaType != MetaType.Tempo)
    Console.WriteLine("["+metamsg.MetaType+"] "+ metamsg.GetBytes().AldToString());
}
}
}*/