using System.Linq;
using C5.intervals;
using C5.Performance.Wpf.Benchmarks;
using C5.Tests.intervals;

namespace C5.Performance.Wpf.Report
{
    public class IBSQueryStabbing : Benchmarkable
    {
        private IInterval<int>[] _intervals;
        private IInterval<int>[] _intervalsNot;
        private IntervalBinarySearchTreeAvl<IInterval<int>, int> _intervalCollection; 

        private int intervalSearch(int intervalId)
        {
            var stabbing = intervalId < CollectionSize ? _intervals[intervalId].Low : _intervalsNot[intervalId - CollectionSize].Low;
            var success = _intervalCollection.FindOverlaps(stabbing);
            return success.Any() ? 1 : 0;
        }

        public override void CollectionSetup()
        {
            _intervals = BenchmarkTestCases.DataSetA(CollectionSize);
            _intervalsNot = BenchmarkTestCases.DataSetNotA(CollectionSize);
            _intervalCollection = new IntervalBinarySearchTreeAvl<IInterval<int>, int>(_intervals);
            ItemsArray = SearchAndSort.FillIntArrayRandomly(CollectionSize, 0, CollectionSize * 2);
        }

        public override void Setup()
        {
        }

        public override double Call(int i)
        {
            return intervalSearch(i);
        }

        public override string BenchMarkName()
        {
            return "IBS Stabbing Query";
        }
    }
}