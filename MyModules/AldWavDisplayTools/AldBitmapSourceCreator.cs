using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using D = System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Reflection;

namespace AldWavDisplayTools
{
    public class AldBitmapSourceCreator
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr dest, IntPtr source, int Length);
        WriteableBitmap writeableBitmap;
        int w, h;

        public WriteableBitmap Bmp { get { return writeableBitmap; } }

        public AldBitmapSourceCreator(int w,int h)
        {
            this.w = w;
            this.h = h;

            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            var dpiX = (int)dpiXProperty.GetValue(null, null);
            var dpiY = (int)dpiYProperty.GetValue(null, null);
            writeableBitmap = new WriteableBitmap(w, h, dpiX, dpiY, System.Windows.Media.PixelFormats.Bgra32, null);
        }

        public void Dispose()
        { 
            //this.writeableBitmap.
        }

        public void DrawImage(D.Bitmap bitmap)
        {
            D.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            try
            {
                int n = data.Height * data.Stride;
                int n2 = writeableBitmap.PixelHeight * writeableBitmap.BackBufferStride;

                writeableBitmap.Lock();
                
                CopyMemory(writeableBitmap.BackBuffer, data.Scan0,(writeableBitmap.BackBufferStride * bitmap.Height));

                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.Width, bitmap.Height));
                writeableBitmap.Unlock();
            }
            finally
            {
                bitmap.UnlockBits(data);
                //bitmap.Dispose();
            }
        }

    }
}
