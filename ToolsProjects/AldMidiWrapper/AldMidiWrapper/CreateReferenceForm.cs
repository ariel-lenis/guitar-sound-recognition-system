using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AldMidiWrapper
{
    public partial class CreateReferenceForm : Form
    {
        public CreateReferenceForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var format = new WaveFormat(44100, 1);
            var ws = new WaveFileWriter("d:\\here.wav", format);

            short[] data = new short[format.SampleRate];

            for (int i = 0; i < data.Length; i++)
            {
                if (i % 2 == 0)
                    data[i] = short.MaxValue;
                else
                    data[i] = short.MinValue;
            }

            IntPtr ptr = Marshal.AllocHGlobal(data.Length*sizeof(short));
            Marshal.Copy(data, 0, ptr, data.Length);
            byte[] bytes = new byte[data.Length * sizeof(short)];
            Marshal.Copy(ptr, bytes, 0, bytes.Length);

            ws.Write(bytes, 0, bytes.Length);
            ws.Flush();
            ws.Dispose();
        }
    }
}
