using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbandoParalelismo
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            int n = 100;

            
            Parallel.For(2, n, (i) =>
            {
                var result = SumRootN(i);
                Console.WriteLine("root {0} : {1} on {2} ", i, result,watch.ElapsedMilliseconds);
                
            });
            
            /*
            for (int i = 2; i < n; i++)
            {
                var result = SumRootN(i);
                Console.WriteLine("root {0} : {1} ", i, result);
            }
            */
            Console.WriteLine(watch.ElapsedMilliseconds);
            Console.ReadLine();
        }
        public static double SumRootN(int root)
        {
            double result = 0;
            for (int i = 1; i < 10000000; i++)
            {
                result += Math.Exp(Math.Log(i) / root);
            }
            return result;
        }
    }
}
