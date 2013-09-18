using System;
using C5.intervals;

namespace C5.Performance
{
    public class IbsAddBenchmarker : Benchmarkable
    {
        private IntervalBinarySearchTree<int> collection;
        private IInterval<int>[] intervals;

        protected override void CollectionSetup()
        {
            intervals = C5.Tests.intervals.BenchmarkTestCases.DataSetC(CollectionSize);
            collection = new IntervalBinarySearchTree<int>();
        }

        protected override void Setup() { }

        protected override double Call(int i)
        {
            foreach (var interval in intervals)
                collection.Add(interval);

            return collection.Count;
        }

        protected override string BenchMarkName()
        {
            return "IBS Add (AVL)";
        }
    }
}
