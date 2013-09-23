using System.Linq;
using C5.intervals;
using C5.Tests.intervals;

namespace C5.Performance.Wpf.Benchmarks
{
    internal class JITCheater : Benchmarkable
    {
        private IInterval<int>[] _intervals;

        public override string BenchMarkName()
        {
            return "OverlapSearch";
        }

        public override void CollectionSetup()
        {
            _intervals = BenchmarkTestCases.DataSetA(CollectionSize);
        }

        public override void Setup()
        {
            _intervals.Shuffle();
        }

        public override double Call(int i)
        {
            return _intervals.Count(interval => interval.Overlaps(new IntervalBase<int>(i)));
        }
    }
}