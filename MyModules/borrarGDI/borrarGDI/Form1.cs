using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace borrarGDI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int GetStride(int w, int bytesperpixel)
        {
            int p = w * bytesperpixel;
            if (p % 4 == 0) return w;
            return p + 4 - p % 4;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            BinaryFormatter formater = new BinaryFormatter();
            FileStream fs = new FileStream("i:\\sdata2.dat",FileMode.Open);
            float[][] sdata = (float[][])formater.Deserialize(fs);
            fs.Dispose();

            int w = sdata[0].Length;
            int h = sdata.Length;
            Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            ColorPalette palette = bmp.Palette;

            for (int i = 0; i <= 255; i++)
                palette.Entries[255 - i] = Color.FromArgb((byte)i, (byte)i, (byte)i);
            bmp.Palette = palette;

            int stride = GetStride(w,1);

            byte[] buffer = new byte[stride];


            var lockd = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)            
                    buffer[j] = (byte)(sdata[i][j] * 255);
                Marshal.Copy(buffer, 0, IntPtr.Add(lockd.Scan0, i * stride), stride);
            }
            bmp.UnlockBits(lockd);
            //MessageBox.Show(stride + "");

            Graphics g = pictureBox1.CreateGraphics();
           
            g.CompositingQuality = CompositingQuality.Invalid;
            //gbufferbmp.InterpolationMode = D.Drawing2D.InterpolationMode.Invalid;
            g.PixelOffsetMode = PixelOffsetMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = SmoothingMode.None;



            g.DrawImage(bmp, new Rectangle(0,0,pictureBox1.Width,pictureBox1.Height),new Rectangle(0,0,w,h), GraphicsUnit.Pixel);

        }
    }
}
