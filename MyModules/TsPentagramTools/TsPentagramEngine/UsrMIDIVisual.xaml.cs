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
using TsExtraControls;
using System.Linq;


namespace TsPentagramEngine
{
	/// <summary>
	/// Interaction logic for UsrMIDIVisual.xaml
	/// </summary>
	public partial class UsrMIDIVisual : UserControl
	{
        List<InputFormat> notes;
        List<int> usednotes;
        Dictionary<int, UsrMIDILine> midilines;

        public List<InputFormat> Notes
        {
            get
            {
                return notes;
            }
            set
            {
                notes = value;
                AdaptNotes();
            }
        }

		public UsrMIDIVisual()
		{
			this.InitializeComponent();
            midilines = new Dictionary<int, UsrMIDILine>();
            this.SizeChanged += UsrMIDIVisual_SizeChanged;
		}

        void UsrMIDIVisual_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (midilines != null && midilines.Count>0)
                OrderLines();
        }


        private void AdaptNotes()
        {
            midilines = new Dictionary<int, UsrMIDILine>();
            usednotes = notes.Notes();
            

            foreach (var i in usednotes)
            {
                UsrMIDILine midiline = new UsrMIDILine();
                this.canvasContainer.Children.Add(midiline);
                midiline.LoadData(i, notes,notes.Last().EndNote);
                midilines.Add(i, midiline);
            }

            OrderLines();
        }

        private void OrderLines()
        {
            for (int i = 0; i < usednotes.Count; i++)
            {
                var midiline = midilines[usednotes[i]];
                midiline.SetX(0);
                midiline.SetY(i * canvasContainer.CurrentHeight() / usednotes.Count);
                midiline.Width = canvasContainer.CurrentWidth();
                midiline.Height = canvasContainer.CurrentHeight() / usednotes.Count;
                midiline.Redraw();
            }
        }

        
        
	}
}