using System;
using System.Linq;
using C5.Performance.Wpf.Benchmarks;

namespace C5.Performance.Wpf.Report_Benchmarks
{
    class QueryStabbing : IntervalBenchmarkable
    {
        private IInterval<int>[] _intervalsNotInCollection; 

        public QueryStabbing(Func<int, IInterval<int>[]> intervalConstruction, Func<IInterval<int>[], IIntervalCollection<IInterval<int>, int>> intervalCollectionConstruction)
            : base(intervalConstruction, intervalCollectionConstruction)
        {}

        public override void CollectionSetup()
        {
            Intervals = IntervalConstruction(CollectionSize);
            IntervalCollection = IntervalCollectionConstruction(Intervals);
            _intervalsNotInCollection = IntervalsNotInCollection(IntervalCollection);
            ItemsArray = SearchAndSort.FillIntArrayRandomly(CollectionSize, 0, CollectionSize * 2);
        }

        public override void Setup()
        {}

        public override double Call(int i)
        {
            var stabbing = i < CollectionSize ? Intervals[i].Low : _intervalsNotInCollection[i - CollectionSize].Low;
            var success = IntervalCollection.FindOverlaps(stabbing);
            return success.Count();
        }
    }
}
