using C5.intervals;
using C5.Performance.Wpf.Benchmarks;
using C5.Tests.intervals;

namespace C5.Performance.Wpf.Report_Benchmarks
{
    public class IBSConstructionAddUnsorted : Benchmarkable
    {
        private IInterval<int>[] _intervals;
        private DynamicIntervalTree<IInterval<int>, int> _intervalCollection; 

        private int intervalSearch(int intervalId)
        {
            foreach (var interval in _intervals)
                _intervalCollection.Add(interval);
            return 1;
        }

        public override void CollectionSetup()
        {
            _intervals = BenchmarkTestCases.DataSetA(CollectionSize);
            _intervals.Shuffle();
            _intervalCollection = new DynamicIntervalTree<IInterval<int>, int>();
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
        }

        public override void Setup()
        {
            _intervals.Shuffle();
            _intervalCollection.Clear();
        }

        public override double Call(int i)
        {
            return intervalSearch(i);
        }

        public override string BenchMarkName()
        {
            return "IBS Construct Add Unsorted";
        }
    }
}