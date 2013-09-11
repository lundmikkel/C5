using System;
using System.Linq;
using C5.intervals;
using C5.Tests.intervals;

namespace C5.Performance {
    class JITCheater : Benchmarkable
    {
        private const String BenchmarkName = "OverlapSearch";
        private IInterval<int>[] _intervals; 
        private int _size;

        public override void StartBenchmark(int minCollectionSize = 100, int maxCollectionSize = 50000)
        {
            SystemInfo();
            for (_size = minCollectionSize; _size <= maxCollectionSize; _size *= 2)
            {
                _intervals = C5.Tests.intervals.BenchmarkTestCases.DataSetA(_size);
                Benchmark(BenchmarkName, Info(), Call, Setup);
            }
        }

        protected override void Setup()
        {
            _intervals.Shuffle();
        }

        protected override double Call(int i) {
            return _intervals.Count(interval => interval.Overlaps(new IntervalBase<int>(i)));
        }

        protected override String Info()
        {
            return String.Format("{0,8:D}", _size);
        }
    }
}
