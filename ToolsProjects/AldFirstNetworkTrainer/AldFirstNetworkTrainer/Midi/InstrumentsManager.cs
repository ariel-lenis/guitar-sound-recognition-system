using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldFirstNetworkTrainer
{
    public class InstrumentsManager
    {
        Dictionary<string, GeneralMidiInstrument> Instruments;
        public InstrumentsManager()
        {
            Instruments = new Dictionary<string, GeneralMidiInstrument>();
            var names = typeof(GeneralMidiInstrument).GetEnumNames();
            var values = typeof(GeneralMidiInstrument).GetEnumValues();

            for (int i = 0; i < names.Length; i++)
                Instruments.Add(names[i], (GeneralMidiInstrument)values.GetValue(i));
        }

        public List<string> Keys { get { return Instruments.Keys.ToList(); } }

        public bool ContainsKey(string p)
        {
            return Instruments.ContainsKey(p);
        }
        public int GetInstrumentKey(string instrumentname)
        {
            return (int)Instruments[instrumentname];
        }
        public void ChangeInstrument(Sequence sequence1,string InstrumentName)
        {
            var instrument = Instruments[InstrumentName];

            for (int i = 0; i < sequence1.Count; i++)
            {
                var track = sequence1[i];
                for (int j = 0; j < track.Count; j++)
                {
                    var evt = track.GetMidiEvent(j);
                    if (evt.MidiMessage.MessageType == MessageType.Channel)
                    {
                        var message = evt.MidiMessage as ChannelMessage;

                        if (message.Command == ChannelCommand.ProgramChange)
                        {
                            var newmsg = new ChannelMessage(ChannelCommand.ProgramChange, message.MidiChannel, (int)instrument);

                            track.RemoveAt(j);
                            track.Insert(j, newmsg);
                        }
                        //else if(message.Command== ChannelCommand.)
                    }
                }
            }
        }
    }
}
