using System;

namespace C5.Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO Crasher uden Code Contracts!
            var test = new C5.Tests.intervals.IntervalBinarySearchTreeAVL.RandomRemove();
            test.SetUp();
            test.AddAndRemove();
        }
    }
}
