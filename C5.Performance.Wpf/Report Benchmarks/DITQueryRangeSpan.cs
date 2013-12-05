using System.Linq;
using C5.intervals;
using C5.Performance.Wpf.Benchmarks;
using C5.Tests.intervals;

namespace C5.Performance.Wpf.Report_Benchmarks
{
    public class DITQueryRangeSpan : Benchmarkable
    {
        private IInterval<int>[] _intervals;
        private DynamicIntervalTree<IInterval<int>, int> _intervalCollection; 

        private int intervalSearch(int intervalId)
        {
            var success = _intervalCollection.FindOverlaps(_intervalCollection.Span);
            return success.Count();
        }

        public override void CollectionSetup()
        {
            _intervals = BenchmarkTestCases.DataSetA(CollectionSize);
            _intervalCollection = new DynamicIntervalTree<IInterval<int>, int>(_intervals);
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
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
            return "DIT Stabbing Range Span";
        }
    }
}