using System;
using C5.Intervals;
using C5.Intervals.Tests;

namespace C5.IntervalHotSpots
{
    class Program
    {
        static void Main(string[] args)
        {
            var intervals = BenchmarkTestCases.DataSetB(1000);
            const int repeats = 1000;
            IntervalBinarySearchTreeOld<IInterval<int>, int> dummy = null;
            //            IntervalBinarySearchTree<IInterval<int>, int> dummy = null;
            for (var i = 0; i < repeats; i++)
            {
                dummy = new IntervalBinarySearchTreeOld<IInterval<int>, int>(intervals);
                //                dummy = new IntervalBinarySearchTree<IInterval<int>, int>(intervals);
                intervals.Shuffle();
                dummy.Clear();
            }

            Console.Out.WriteLine(dummy.Count);
        }
    }
}
