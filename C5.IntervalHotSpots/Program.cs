using System;
using C5.intervals;
using C5.Tests.intervals;

namespace C5.IntervalHotSpots
{
    class Program
    {
        static void Main(string[] args)
        {
            var intervals = BenchmarkTestCases.DataSetB(1000);
            const int repeats = 1000;
            IntervalBinarySearchTreeOld<IInterval<int>, int> dummy = null;
//            IntervalBinarySearchTreeAvl<IInterval<int>, int> dummy = null;
            for (var i = 0; i < repeats; i++)
            {
                dummy = new IntervalBinarySearchTreeOld<IInterval<int>, int>(intervals);
//                dummy = new IntervalBinarySearchTreeAvl<IInterval<int>, int>(intervals);
                intervals.Shuffle();
                dummy.Clear();
            }
                
            Console.Out.WriteLine(dummy.Count);
        }
    }
}
