using System;
using C5.intervals;
using C5.Tests.intervals;

namespace C5.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var intervals = BenchmarkTestCases.DataSetB((int) Math.Pow(2, 13));
            intervals.Shuffle();
            new IntervalBinarySearchTree<int>(intervals);
        }
    }
}