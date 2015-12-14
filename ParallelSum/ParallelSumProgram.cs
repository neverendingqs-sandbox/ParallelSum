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
        const long NUM_NUMBERS = 60000000;
        static readonly long[] _numbers;
        static readonly long _actualSum;

        static ParallelSumProgram()
        {
            _numbers = new long[NUM_NUMBERS];
            for (long i = 0; i < NUM_NUMBERS; i++)
            {
                _numbers[i] = i;
            }

            _actualSum = ForEachSum();
        }

        static void Main(string[] args)
        {
            /* Sample run:
            
             Running ForEachSum
             * Add each element one at a time
             * Baseline for all the other methods
             Total time: 323 ms. Sum: 1799999970000000. Accurate: True
             
             Running LinqSum
             * Add each element one at a time using LINQ
             * Looks like LINQ adds a bit of overhead
             Total time: 462 ms. Sum: 1799999970000000. Accurate: True
             
             Running ParallelLinqSum
             * Add elements in parallel; algorithm hidden by LINQ
             * As expected, it should run faster in parallel
             Total time: 166 ms. Sum: 1799999970000000. Accurate: True
             
             Running ParallelSumWithLock
             * Contention caused by constant lock acquisition
             Total time: 4140 ms. Sum: 1799999970000000. Accurate: True
             
             Running ParallelSumByNumCores
             * Use every core to help with the sum
             * Ran faster than ParallelLinqSum, probably due to less overhead without LINQ
             Total time: 177 ms. Sum: 1799999970000000. Accurate: True
             
             Running ParallelSumByNumCoresTimesTwo
             * Creates more threads than cores to help with the sum
             * Contention caused by context switching amongst all the threads
             Total time: 324 ms. Sum: 1799999970000000. Accurate: True
             
             Running ParallelSumByNumCoresMinusOne
             * Use every core to help with the sum
             * Expected to run faster than ParallelSumByNumCores (one core untouched for the OS)
             Total time: 147 ms. Sum: 1799999970000000. Accurate: True
             
             Running ParallelSumByNumCoresMinusTwo
             * Use every core to help with the sum
             * Expected to run slightly slower than ParallelSumByNumCoresMinusOne, as less cores are used
             Total time: 146 ms. Sum: 1799999970000000. Accurate: True
            */

            /* serial */
            SumWithPrintTotalTime("ForEachSum", ForEachSum);
            SumWithPrintTotalTime("LinqSum", LinqSum);

            /* parallel */
            SumWithPrintTotalTime("ParallelLinqSum", ParallelLinqSum);
            SumWithPrintTotalTime("ParallelSumWithLock", ParallelSumWithLock);
            SumWithPrintTotalTime("ParallelSumByNumCores", ParallelSumByNumCores);
            SumWithPrintTotalTime("ParallelSumByNumCoresTimesTwo", ParallelSumByNumCoresTimesTwo);
            SumWithPrintTotalTime("ParallelSumByNumCoresMinusOne", ParallelSumByNumCoresMinusOne);
            SumWithPrintTotalTime("ParallelSumByNumCoresMinusTwo", ParallelSumByNumCoresMinusTwo);
            
            Console.Out.WriteLine("Press any key to exit.");
            Console.In.Read();
        }

        private static void SumWithPrintTotalTime(string funcName, Func<long> sumMethod)
        {
            Console.Out.WriteLine(String.Format("Running {0}", funcName));

            Stopwatch sw = new Stopwatch();
            sw.Start();
            long sum = sumMethod();
            sw.Stop();

            Console.Out.WriteLine(String.Format("Total time: {0} ms. Sum: {1}. Accurate: {2}", sw.ElapsedMilliseconds, sum, _actualSum == sum));
            Console.Out.WriteLine();
        }

        private static long ForEachSum(long[] numbers)
        {
            long sum = 0;
            foreach (long i in numbers)
            {
                sum += i;
            }

            return sum;
        }

        private static long ForEachSum()
        {
            return ForEachSum(_numbers);
        }

        private static long LinqSum()
        {
            return _numbers.Sum();
        }

        private static long ParallelLinqSum()
        {
            return _numbers.AsParallel().Sum();
        }

        private static long ParallelSumWithLock()
        {
            long sum = 0;
            object sumLock = new object();

            Parallel.For(0, _numbers.Length, i =>
            {
                lock(sumLock)
                {
                    sum += i;
                }
            });

            return sum;
        }

        private static long ParallelSumByNCores(int numCores)
        {
            long[] partialSums = new long[numCores];

            Parallel.For(0, numCores, threadNum =>
            {
                long sum = 0;
                for (int i = threadNum; i < _numbers.Length; i += numCores)
                {
                    sum += _numbers[i];
                }

                partialSums[threadNum] = sum;
            });

            return ForEachSum(partialSums);
        }

        private static long ParallelSumByNumCores()
        {
            return ParallelSumByNCores(Environment.ProcessorCount);
        }

        private static long ParallelSumByNumCoresTimesTwo()
        {
            return ParallelSumByNCores(Environment.ProcessorCount * 2);
        }

        private static long ParallelSumByNumCoresMinusOne()
        {
            return ParallelSumByNCores(Environment.ProcessorCount - 1);
        }

        private static long ParallelSumByNumCoresMinusTwo()
        {
            return ParallelSumByNCores(Environment.ProcessorCount - 2);
        }
    }
}
