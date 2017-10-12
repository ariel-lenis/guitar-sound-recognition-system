using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TsPentagramEngine
{
    public class MidiToInputFormat
    {
        public static List<InputFormat> Parse(Sequence sequence)
        {
            var data = new List<InputFormat>();

            Track max = null;
            foreach (var i in sequence)
                if (max == null || i.Count > max.Count)
                    max = i;

            Track track = max;

            //MessageBox.Show(sequence.Division + "");

            int bpm = 120;
            double seconds = 0;

            //track.GetMidiEvent()
            

            int added = 0;
            int ended = 0;

            for (int i = 0; i < track.Count; i++)
            {
                var evt = track.GetMidiEvent(i);
                seconds = evt.AbsoluteTicks * 60.0 / bpm / sequence.Division;

                if (evt.MidiMessage is ChannelMessage)
                {
                    ChannelMessage msg = evt.MidiMessage as ChannelMessage;
                    if (msg.Command == ChannelCommand.NoteOn)
                    {
                        data.Add(new InputFormat(msg.Data1, TimeSpan.FromSeconds(seconds), TimeSpan.Zero));
                        added++;
                    }
                    if (msg.Command == ChannelCommand.NoteOff)
                    {
                        data.Last(x => x.MidiNotes.Contains(msg.Data1) && x.EndNote == TimeSpan.Zero).EndNote = TimeSpan.FromSeconds(seconds);
                        ended++;
                    }
                }
                else if (evt.MidiMessage is MetaMessage)
                {
                    MetaMessage metamsg = evt.MidiMessage as MetaMessage;
                    if (metamsg.MetaType == MetaType.Tempo)
                    {
                        TempoChangeBuilder change = new TempoChangeBuilder(metamsg);
                        Console.WriteLine("Tempo:" + change.Tempo);
                    }
                    else if (metamsg.MetaType == MetaType.TimeSignature)
                    {
                        TimeSignatureBuilder time = new TimeSignatureBuilder(metamsg);
                        Console.WriteLine("Division:" + time.ClocksPerMetronomeClick);
                    }
                    Console.WriteLine("Meta: " + metamsg.MetaType + " " + metamsg.GetBytes().Length);
                }
            }

            if (data.Last().EndNote == TimeSpan.Zero)
                data.Last().EndNote = TimeSpan.FromSeconds(seconds);

            return GroupData(data);
        }
        public static List<InputFormat> GroupData(List<InputFormat> data)
        {
            List<InputFormat> res = new List<InputFormat>();

            InputFormat last = null;

            for(int i=0;i<data.Count;i++)
            {
                if (last == null || last.StartNote!=data[i].StartNote)
                {
                    res.Add(data[i]);
                    last = data[i];
                }
                else
                    last.MidiNotes.AddRange(data[i].MidiNotes);
            }

            return res;

        }
    }
}
