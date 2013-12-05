using System;
using C5.intervals;
using C5.Tests.intervals;

namespace C5.IntervalHotSpots
{
    class Program
    {
        static void Main(string[] args)
        {
            const int repeats = 1000;
//            IntervalBinarySearchTreeOld<IInterval<int>, int> dummy = null;
            IntervalBinarySearchTreeAvl<IInterval<int>, int> dummy = null;
            for (var i = 0; i < repeats; i++)
            {
//                dummy = new IntervalBinarySearchTreeOld<IInterval<int>, int>(BenchmarkTestCases.DataSetB(1000));
                dummy = new IntervalBinarySearchTreeAvl<IInterval<int>, int>(BenchmarkTestCases.DataSetB(1000));
                dummy.Clear();
            }
                
            Console.Out.WriteLine(dummy.Count);
        }
    }
}
