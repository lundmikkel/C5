using System;
using C5.intervals;

namespace C5.Performance
{
    public class IbsAvlAddBenchmarker : Benchmarkable
    {
        private IntervalBinarySearchTreeAVL<int> collection;
        private IInterval<int>[] intervals;

        public override void CollectionSetup()
        {
            intervals = C5.Tests.intervals.BenchmarkTestCases.DataSetC(CollectionSize);
            collection = new IntervalBinarySearchTreeAVL<int>();
        }

        public override void Setup() { }

        public override double Call(int i)
        {
            foreach (var interval in intervals)
                collection.Add(interval);

            return collection.Count;
        }

        public override string BenchMarkName()
        {
            return "IBS Add (AVL)";
        }
    }
}
