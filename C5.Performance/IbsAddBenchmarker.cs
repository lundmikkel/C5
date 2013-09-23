﻿using C5.intervals;
using C5.Tests.intervals;

namespace C5.Performance
{
    public class IbsAddBenchmarker : Benchmarkable
    {
        private IInterval<int>[] _intervals;
        private IntervalBinarySearchTree<int> collection;

        public override void CollectionSetup()
        {
            _intervals = BenchmarkTestCases.DataSetC(CollectionSize);
            collection = new IntervalBinarySearchTree<int>();
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
            SearchAndSort.Shuffle(ItemsArray);
        }

        public override void Setup()
        {
        }

        public override double Call(int i)
        {
            collection.Add(_intervals[i]);
            return i;
        }

        public override string BenchMarkName()
        {
            return "IBS Add (AVL)";
        }
    }
}