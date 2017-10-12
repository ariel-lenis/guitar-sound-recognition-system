using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TsFilesTools;
using System.Linq;
using TsExtraControls;

namespace TsPentagramEngine
{
	/// <summary>
	/// Interaction logic for UsrMIDILine.xaml
	/// </summary>
	public partial class UsrMIDILine : UserControl
	{
        List<InputFormat> myNotes;
        int note;
        TimeSpan duration;

        Dictionary<InputFormat, Rectangle> sections;

        Color[] colors = new Color[] {Colors.Red,Colors.Blue,Colors.Green,Colors.Yellow,Colors.Violet,Colors.Orange,Colors.Cyan};

		public UsrMIDILine()
		{
			this.InitializeComponent();
            sections = new Dictionary<InputFormat, Rectangle>();
            this.SizeChanged += UsrMIDILine_SizeChanged;

            colors = new Color[12];

            colors[0] = Colors.Red;
            colors[1] = Misc(Colors.Red,Colors.Blue);
            colors[2] = Colors.Blue;
            colors[3] = Misc(Colors.Blue, Colors.Green);
            colors[4] = Colors.Green;
            colors[5] = Colors.Yellow;
            colors[6] = Misc(Colors.Yellow, Colors.Violet);
            colors[7] = Colors.Violet;
            colors[8] = Misc(Colors.Violet, Colors.Orange);
            colors[9] = Colors.Orange;
            colors[10] = Misc(Colors.Orange, Colors.Cyan);
            colors[11] = Colors.Cyan;

		}

        Color Misc(Color a, Color b)
        {
            return Color.FromRgb((byte)((a.R + b.R) / 2), (byte)((a.G + b.G) / 2), (byte)((a.B + b.B) / 2));
        }

        void UsrMIDILine_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (myNotes != null)
                Redraw();
        }

        public void LoadData(int note, List<InputFormat> notes,TimeSpan duration)
        {
            this.txtName.Text = TsMIDITools.NoteFor(note) + (TsMIDITools.OctaveFor(note)+4);

            this.duration = duration;
            this.note = note;
            this.myNotes = notes.Where(x=>x.MidiNotes.Contains(note)).ToList();

            sections.Clear();

            int valueinoctave = TsMIDITools.ValueInOctave(note);       


            for (int i = 0; i < myNotes.Count; i++)
            {
                Rectangle rect = new Rectangle();
                //rect.Fill = new SolidColorBrush(colors[valueinoctave]);
                rect.Fill = new SolidColorBrush(Colors.Green);
                sections.Add(myNotes[i], rect);
                canvasPaint.Children.Add(rect);
            }

            Redraw();
        }

        internal void Redraw()
        {
            for (int i = 0; i < myNotes.Count; i++)
            {
                Rectangle rect = sections[myNotes[i]];
                rect.SetX(myNotes[i].StartNote.TotalSeconds / duration.TotalSeconds * canvasPaint.CurrentWidth());
                rect.SetY(0);
                var ax = myNotes[i];
                rect.Width = myNotes[i].DeltaTime.TotalSeconds / duration.TotalSeconds * canvasPaint.CurrentWidth();
                rect.Height = canvasPaint.CurrentHeight();
            }
        }
    }
}
