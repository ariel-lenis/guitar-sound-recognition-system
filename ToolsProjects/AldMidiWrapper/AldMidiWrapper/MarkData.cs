using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldMidiWrapper
{
    public class MarkData
    {
        public TimeSpan Time;
        public Sanford.Multimedia.Midi.ChannelCommand Command;
        public int Byte1;
        public int Byte2;

        public MarkData(TimeSpan time, Sanford.Multimedia.Midi.ChannelMessage msg)
        {
            Time = time;
            Command = msg.Command;
            Byte1 = msg.Data1;
            Byte2 = msg.Data2;
        }
        public override string ToString()
        {
            return Time.ToString() + " " + Command;
        }
    }
}
