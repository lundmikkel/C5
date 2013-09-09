using System;
using C5.intervals;
using C5.Tests.intervals;

namespace C5.Performance {
    class IBSProfiling : ProfilingBase
    {
        public new int NumberOfRuns = 2000;

        public override void SingleProfilingRun()
        {
            Console.Out.WriteLine(this.NumberOfRuns);
            var ibs = new IntervalBinarySearchTree<int>(BenchmarkTestCases.DataSetB(30));
        }
    }
}
