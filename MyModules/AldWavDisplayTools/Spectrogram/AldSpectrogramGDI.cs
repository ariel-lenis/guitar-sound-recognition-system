using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF=System.Windows.Controls;
using D = System.Drawing;
using TsFFTFramework;
using AldSpecialAlgorithms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace AldWavDisplayTools
{
    public unsafe class AldSpectrogramGDI
    {        
        WPF.Image canvas;
        AldBitmapSourceCreator source;
        D.Bitmap mybmp;
        D.Bitmap bufferbmp;
        D.Graphics gbufferbmp;
        int w, h;
        int myw, myh;

        public AldSpectrogramGDI(WPF.Image canvas)
        {
            this.canvas = canvas;
            canvas.Stretch = System.Windows.Media.Stretch.Fill;
        }
        public void Resize(int w, int h)
        {
            if (w < 1) w = 1;
            if (h < 1) h = 1;
            this.w = w;
            this.h = h;
            source = new AldBitmapSourceCreator(w, h);
            if (bufferbmp != null)
                bufferbmp.Dispose();
            bufferbmp = new D.Bitmap(w, h);
            gbufferbmp = D.Graphics.FromImage(bufferbmp);
        }

        public void Display(float startpercent, float lengthpercent,float startYpercent)
        {
            if (mybmp == null) return;

            int startY = (int)(this.myh * startYpercent);
            int start = (int)(startpercent*myw);
            int length = (int)(lengthpercent*myw);

            int max = 5000;//30000;

            //Debug.WriteLine(length);

            if (startY == this.myh) startY--;

            /*
            gbufferbmp.CompositingQuality = D.Drawing2D.CompositingQuality.Invalid;
                                //gbufferbmp.InterpolationMode = D.Drawing2D.InterpolationMode.Invalid;
            gbufferbmp.PixelOffsetMode = D.Drawing2D.PixelOffsetMode.None;
            gbufferbmp.InterpolationMode = D.Drawing2D.InterpolationMode.NearestNeighbor;            
            gbufferbmp.SmoothingMode = D.Drawing2D.SmoothingMode.None;
            */

            gbufferbmp.FillRectangle(D.Brushes.White, 0, 0, w, h);

            /*
            int sum = 0;
            int i = 0;
            while (sum < length)
            {
                int acum;
                if (sum + max > length) acum = length - sum;
                else
                    acum = max;
                gbufferbmp.DrawImage(mybmp, new D.Rectangle((int)((float)(sum) / length * w)+i*10, 0, (int)((float)acum / length * w), h), new D.Rectangle(start + sum, startY, acum, myh - startY), D.GraphicsUnit.Pixel);
                gbufferbmp.DrawRectangle(D.Pens.Red, new D.Rectangle((int)((float)(sum) / length * w), 0, (int)((float)acum / length * w), h));
                sum += acum;
                i++;
            }
            */

            gbufferbmp.DrawImage(mybmp, new D.Rectangle(0, 0, w, h), new D.Rectangle(start, startY, length, myh - startY), D.GraphicsUnit.Pixel);

            //mybmp.Save("h:\\herehere.ttf", D.Imaging.ImageFormat.Tiff);

            source.DrawImage(bufferbmp);
            canvas.Source = source.Bmp;

        }
        public bool GenerateSpectogram(float[][] sdata)
        {

            FindPeaks(sdata);

            /*
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fs = new FileStream("i:\\sdata2.dat",FileMode.OpenOrCreate);
            
            formatter.Serialize(fs, sdata);
            fs.Dispose();
            */

            int columns = sdata[0].Length;
            int rows = sdata.Length;

            if (mybmp != null) mybmp.Dispose();
            int stride;

            //mybmp = CreateBitmap(columns, rows, out stride);

            //mybmp.Save("h:\\aver.jpg", D.Imaging.ImageFormat.Jpeg);

            //var bmpdata = mybmp.LockBits(new D.Rectangle(0, 0, columns, rows), D.Imaging.ImageLockMode.WriteOnly, D.Imaging.PixelFormat.Format8bppIndexed);

            if (columns % 4 == 0)
                stride = columns;
            else
                stride = columns + 4 - columns % 4;

            IntPtr xptr = Marshal.AllocHGlobal(rows * stride);

            
            //lock (mybmp)
            {
                byte[] buff = new byte[columns];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < buff.Length; j++)
                        buff[j] = (byte)(1 * 255 - sdata[rows - i - 1][j] * 255);
                    lock (buff)
                    {
                        //Marshal.Copy(buff, 0, IntPtr.Add(bmpdata.Scan0, i * stride), columns);
                    }
                    Marshal.Copy(buff, 0, IntPtr.Add(xptr, i * stride), columns);
                }
            }
            
            //var bmpdata = mybmp.LockBits(new D.Rectangle(0, 0, columns, rows), D.Imaging.ImageLockMode.ReadWrite, D.Imaging.PixelFormat.Format8bppIndexed);
            //bmpdata.Scan0 = xptr;
            //mybmp.UnlockBits(bmpdata);



            mybmp = CreateBitmap2(columns, rows,xptr, out stride);

            
            //mybmp.Save("h:\\aver.jpg",D.Imaging.ImageFormat.Tiff);
            //Thread.Sleep(100);
            
            myw = mybmp.Width;
            myh = mybmp.Height;
            
            return true;
        }

        private void FindPeaks(float[][] sdata)
        {
            return;
            int rows = sdata.GetLength(0);
            int cols = sdata[0].Length;

            float[] part = new float[rows];

            for(int i=0;i<cols;i++)
            {
                for (int j = 0; j < rows; j++)
                    part[j] = sdata[j][i];

                var res = PeakDetection.Detect(part);

                for (int j = 0; j < rows; j++)
                    sdata[j][i]=0;

                for(int j=0;j<res.locs.Length;j++)
                {
                    sdata[res.locs[j]][i] = res.pks[j];
                }
            }

        }        
        private bool GenerateSpectogramOld(float[] data, int fftsize)
        {
            int n = data.Length;
            n = (n / fftsize) * fftsize;
            int columns = n / fftsize;
            int rows = fftsize / 2;
            
            if (mybmp != null) mybmp.Dispose();
            int stride;

            mybmp = CreateBitmap(columns, rows, out stride);

            byte[] thebmp =  new byte[stride*rows];

            data = AldCudaAlgorithms.XFFTBlocks(data, 0, n, fftsize, AldCudaAlgorithms.Direcion.Forward);
            float max = data.Max();

            for (int i = 0; i < columns; i++)
            {
                int ini = fftsize * i;
                int count = fftsize;

                for (int j = 0; j < rows; j++)
                {
                    float val = (data[ini+j])/max;
                    //float val = data[ini + j] / fftsize;
                    int pix = /*(int)(val*255/max); */MapToPixelindex(val, 70, 255);
                    if (pix > 255) pix = 255;
                    pix = 255 - pix;

                    float div = 10f;

                    int lrow = (int)(rows * Math.Log10(1 + j / div) / Math.Log10(1 + rows / div));
                    if (lrow == rows) lrow = rows - 1;

                    int pixidx = i + (rows - 1 - lrow) * stride;

                    thebmp[pixidx] = (byte)pix;
                }

            }

            var bmpdata = mybmp.LockBits(new D.Rectangle(0,0,columns,rows), D.Imaging.ImageLockMode.WriteOnly, D.Imaging.PixelFormat.Format8bppIndexed);
                Marshal.Copy(thebmp, 0, bmpdata.Scan0, stride * rows);
            mybmp.UnlockBits(bmpdata);

            myw = mybmp.Width;
            myh = mybmp.Height;

            return true;
        }
        private D.Bitmap CreateBitmap(int columns, int rows, out int stride)
        {
            var bmp = new D.Bitmap(columns, rows, D.Imaging.PixelFormat.Format8bppIndexed);
            D.Imaging.ColorPalette palette = bmp.Palette;
            for (int i = 0; i <= 255; i++)
                palette.Entries[255-i] = D.Color.FromArgb((byte)i, (byte)i, (byte)i);
            bmp.Palette = palette;

            D.ImageConverter c = new D.ImageConverter();

            D.Rectangle rect = new D.Rectangle(0, 0, columns, rows);
            /*
            using (bmp)
            {
                lock (bmp)
                {
                    var bmpdata = bmp.LockBits(rect, D.Imaging.ImageLockMode.ReadOnly, D.Imaging.PixelFormat.Format8bppIndexed);
                    stride = bmpdata.Stride;
                    bmp.UnlockBits(bmpdata);
                }
            }
             * */
            D.Imaging.BitmapData b;

            if (bmp.Width % 4 == 0)
                stride = bmp.Width;
            else
                stride = bmp.Width+ 4 - bmp.Width % 4;

            return bmp;
        }


        private D.Bitmap CreateBitmap2(int columns, int rows,IntPtr data,out int stride)
        {
            if (columns % 4 == 0)
                stride = columns;
            else
                stride = columns + 4 - columns % 4;

            var bmp = new D.Bitmap(columns, rows,stride, D.Imaging.PixelFormat.Format8bppIndexed,data);
            D.Imaging.ColorPalette palette = bmp.Palette;
            for (int i = 0; i <= 255; i++)
                palette.Entries[255 - i] = D.Color.FromArgb((byte)i, (byte)i, (byte)i);
            bmp.Palette = palette;


            //var bmp2 = new D.Bitmap(columns, rows);
            //D.Graphics.FromImage(bmp2).DrawImage(bmp, 0, 0);
            //bmp2.Save("h:\\mmmm.jpg", D.Imaging.ImageFormat.Jpeg);
            return bmp;
        }


        public int MapToPixelindex(float Mag,float RangedB,int Rangeindex)
        {
            if (Mag == 0) return 0;
            double LevelIndB;

            LevelIndB = 20 * Math.Log(Mag) / Math.Log(10);
            if (LevelIndB < -RangedB)
                return 0;
            else
                return (int)(Rangeindex * (LevelIndB + RangedB) / RangedB);            
        }
    }
}
