using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FontInspector
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            PrivateFontCollection font = new PrivateFontCollection();
            font.AddFontFile(@"C:\Users\nxt\Desktop\fonts\Musical.ttf");
            //font.AddFontFile(@"C:\Users\nxt\Desktop\PHOTSE__.TTF");
            //font.AddFontFile(@"C:\Users\nxt\Desktop\denemo.ttf");
            
            var musicalfont = new Font(font.Families[0],50,GraphicsUnit.Pixel);


            listBox2.Font = musicalfont;

            textBox1.Font = musicalfont;
            
            for (int i = 0; i < 255; i++)
            {
                listBox2.Items.Add((char)i + "");
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Text = (int)(listBox2.SelectedItem.ToString())[0] + "";
            textBox1.Text += (char)61570; //listBox2.SelectedItem;
            //MessageBox.Show(listBox2.SelectedIndex+"");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Text = (int)textBox1.SelectedText[0]+"";
        }


    }
}
