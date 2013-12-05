using C5.intervals;
using C5.Performance.Wpf.Benchmarks;
using C5.Tests.intervals;

namespace C5.Performance.Wpf.Report
{
    public class IBSConstructionAddSorted : Benchmarkable
    {
        private IInterval<int>[] _intervals;
        private IntervalBinarySearchTree<IInterval<int>, int> _intervalCollection; 

        private int intervalSearch(int intervalId)
        {
            foreach (var interval in _intervals)
                _intervalCollection.Add(interval);
            return 1;
        }

        public override void CollectionSetup()
        {
            _intervals = BenchmarkTestCases.DataSetA(CollectionSize);
            Sorting.IntroSort(_intervals, 0, CollectionSize, IntervalExtensions.CreateComparer<IInterval<int>, int>());
            _intervalCollection = new IntervalBinarySearchTree<IInterval<int>, int>();
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
        }

        public override void Setup()
        {
            _intervalCollection.Clear();
        }

        public override double Call(int i)
        {
            return intervalSearch(i);
        }

        public override string BenchMarkName()
        {
            return "IBS Construct Add Sorted";
        }
    }
}