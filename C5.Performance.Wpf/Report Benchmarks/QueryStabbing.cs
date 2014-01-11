using System;
using System.Linq;
using C5.Performance.Wpf.Benchmarks;
using C5.Intervals.Tests;
using C5.Intervals;

namespace C5.Performance.Wpf.Report_Benchmarks
{
    class QueryStabbing : IntervalBenchmarkable
    {

        public QueryStabbing(Func<int, IInterval<int>[]> intervalConstruction, Func<IInterval<int>[], IIntervalCollection<IInterval<int>, int>> intervalCollectionConstruction)
            : base(intervalConstruction, intervalCollectionConstruction)
        { }

        public override void CollectionSetup()
        {
            Intervals = IntervalConstruction(CollectionSize);
            IntervalCollection = IntervalCollectionConstruction(Intervals);
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
        }

        public override void Setup()
        {
            ItemsArray.Shuffle();
        }

        public override double Call(int i)
        {
            var success = IntervalCollection.FindOverlaps(Intervals[i].Low);
            return success.Count();
        }
    }
}
