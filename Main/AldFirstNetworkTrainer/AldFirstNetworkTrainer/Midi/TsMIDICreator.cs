using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldFirstNetworkTrainer.Midi
{
    public class TsMIDICreator
    {
        public static int SecondsToPosition(double seconds, int tpqn, int bpm = 120)
        {
            double ticktempo = 60.0 / (tpqn * bpm);
            return (int)(seconds / ticktempo);
        }
        public static void SaveSolution(int instrumentkey,List<FrequencySolution> solution, string path)
        {
            Sanford.Multimedia.Midi.Track track = new Track();
            InstrumentsManager manager = new InstrumentsManager();
            track.Insert(0, new ChannelMessage(ChannelCommand.ProgramChange, 1, instrumentkey));

            foreach (var i in solution)
            {
                foreach (var midinote in i.Midies)
                {
                    track.Insert(SecondsToPosition(i.StartTime.TotalSeconds, 128), new Sanford.Multimedia.Midi.ChannelMessage(ChannelCommand.NoteOn, 1, midinote, 0x7f));
                    track.Insert(SecondsToPosition(i.EndTime.TotalSeconds, 128), new Sanford.Multimedia.Midi.ChannelMessage(ChannelCommand.NoteOff, 1, midinote, 0x7f));
                }
            }

            Sanford.Multimedia.Midi.Sequence sequence = new Sequence(128);
            sequence.Add(track);
            sequence.Save(path);
        }        

    }
}
