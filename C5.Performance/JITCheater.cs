using System;
using System.Linq;
using C5.intervals;
using C5.Tests.intervals;

namespace C5.Performance
{
    internal class JITCheater : Benchmarkable
    {
        private IInterval<int>[] _intervals;

        protected override string BenchMarkName()
        {
            return "OverlapSearch";
        }

        protected override void CollectionSetup()
        {
            _intervals = BenchmarkTestCases.DataSetA(size);
        }

        protected override void Setup()
        {
            _intervals.Shuffle();
        }

        protected override double Call(int i)
        {
            return _intervals.Count(interval => interval.Overlaps(new IntervalBase<int>(i)));
        }

        protected override String Info()
        {
            return String.Format("{0,8:D}", size);
        }
    }
}