using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanford.Multimedia.Midi;

namespace AldFirstNetworkTrainer
{
    public static class AldExtensors
    {
        public static string AldToString(this byte[] where)
        {
            return UTF8Encoding.UTF8.GetString(where, 0, where.Length);
        }

        public static void GetInstrumentsPerChannel(this Track where,List<List<GeneralMidiInstrument>> result)
        {
            for (int i = 0; i < where.Count; i++)
            {
                var msg = where.GetMidiEvent(i).MidiMessage;
                if (msg.MessageType == MessageType.Channel)
                {
                    var chmsg = msg as ChannelMessage;
                    while (result.Count <= chmsg.MidiChannel) result.Add(new List<GeneralMidiInstrument>());
                    if (chmsg.Command == ChannelCommand.ProgramChange)                    
                        result[chmsg.MidiChannel].Add((GeneralMidiInstrument)chmsg.Data1);                    
                }
            }
        }
    }
}
