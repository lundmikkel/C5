using System;
using C5.Intervals;
using C5.Intervals.Tests;

namespace C5.Performance.Wpf.Benchmarks
{
    public class IbsAvlRandomRemoveBenchmarker : Benchmarkable
    {
        private IntervalBinarySearchTree<IInterval<int>, int> _collection;
        private IList<IInterval<int>> _intervals;
        private readonly Random _random = new Random(0);

        public override void CollectionSetup()
        {
            _collection = new IntervalBinarySearchTree<IInterval<int>, int>();
            _intervals = new ArrayList<IInterval<int>>(CollectionSize);
            _intervals.AddAll(BenchmarkTestCases.DataSetB(CollectionSize));
            _intervals.Shuffle(_random);
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
            SearchAndSort.Shuffle(ItemsArray);
        }

        public override void Setup() { }

        public override double Call(int i)
        {
            foreach (var interval in _intervals)
                _collection.Add(interval);

            foreach (var interval in _intervals)
                _collection.Remove(interval);

            return _collection.Count;
        }

        public override string BenchMarkName()
        {
            return "IBS RandomRemove (AVL)";
        }
    }
}
