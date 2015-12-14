using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelSum
{
    class ParallelSumProgram
    {
        const int NUM_NUMBERS = 53000000;
        static readonly int[] _numbers;
        static readonly long _actualSum;

        static ParallelSumProgram()
        {
            _numbers = new int[NUM_NUMBERS];
            for (int i = 0; i < NUM_NUMBERS; i++)
            {
                _numbers[i] = i;
            }

            _actualSum = ForEachSum();
        }

        static void Main(string[] args)
        {
            SumWithPrintTotalTime(ForEachSum);

            Console.In.Read();
        }

        private static void SumWithPrintTotalTime(Func<long> sumMethod)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            long sum = sumMethod();
            sw.Stop();

            Console.Out.WriteLine(String.Format("Total time: {0} ms. Accurate: {1}", sw.ElapsedMilliseconds, _actualSum == sum));
        }

        private static long ForEachSum()
        {
            long sum = 0;
            foreach (int i in _numbers)
            {
                sum += 1;
            }

            return sum;
        }
    }
}
