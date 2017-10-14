using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace WavAnalizerContainer
{
    public class ResizingWatchDog : IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern ushort GetAsyncKeyState(int vkey);
        public const int VK_LBUTTON = 01;

        Window w;
        Timer timer;
        bool changed;

        public event SizeChangedEventHandler SizeChanged;
        SizeChangedEventArgs lastargs;

        public ResizingWatchDog(Window w)
        {
            this.w = w;
            w.SizeChanged += w_SizeChanged;
            timer = new Timer(100);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            changed = false;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TrySend();
        }
        void TrySend()
        {
            bool isreleased = (GetAsyncKeyState(VK_LBUTTON) >> 15) == 0;
            if (isreleased && changed)
            {
                if (SizeChanged != null)
                    SizeChanged(w, lastargs);
                changed = false;
            }
        }
        void w_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            changed = true;
            lastargs = e;
        }

        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
