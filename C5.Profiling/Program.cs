using System;

namespace C5.Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new C5.Tests.intervals.IntervalBinarySearchTreeAVL.RandomRemove();
            test.SetUp();
            test.AddAndRemove();

            // var test = new C5.Tests.intervals.IntervalBinarySearchTreeAVL.BensTest();
            // test.MaximumDepth_BensCollection_Returns2();
        }
    }
}
