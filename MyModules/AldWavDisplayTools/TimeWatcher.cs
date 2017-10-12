using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldWavDisplayTools
{
    public class TimeWatcher
    {
        public static long HowMuchTime(string description, Action a)
        {
            Stopwatch sw=new Stopwatch();
            sw.Start();
            a();
            sw.Stop();
            Console.WriteLine(description + " " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds;
        }
    }
}
