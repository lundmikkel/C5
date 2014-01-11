using C5.Intervals;

namespace C5.Performance.Wpf.Benchmarks
{
    public class IbsAvlOldIntervalSetsBenchmarker : Benchmarkable
    {
        private IntervalBinarySearchTreeOld<IInterval<int>, int> _collection;
        private IInterval<int>[] _intervals;

        public override void CollectionSetup()
        {
            _intervals = Intervals.Tests.BenchmarkTestCases.DataSetAOpen(CollectionSize);
            _collection = new IntervalBinarySearchTreeOld<IInterval<int>, int>();
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
            SearchAndSort.Shuffle(ItemsArray);
        }

        public override void Setup()
        {
            _collection.Clear();
        }

        public override double Call(int i)
        {
            foreach (var interval in _intervals)
                _collection.Add(interval);
            return _collection.Count;
        }

        public override string BenchMarkName()
        {
            return "IBS Interval Set Old";
        }
    }
}
