using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //6934
            //6992

            //7303
            Random rnd = new Random();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            double a = (float)rnd.NextDouble();
            double b = (float)rnd.NextDouble();
            double c = (float)rnd.NextDouble();
            for (int i = 0; i < 10000000; i++)
            {
                a = a * a;
                b = a + a * a;
                c = a / b-a;
            }
            Console.WriteLine("E:" + sw.ElapsedMilliseconds);
            Console.ReadKey(true);
        }
    }
}
