using System;
using C5.Performance.Wpf.Benchmarks;
using C5.Intervals.Tests;
using C5.Intervals;

namespace C5.Performance.Wpf.Report_Benchmarks
{
    class ConstructAddUnsorted : IntervalBenchmarkable
    {
        public ConstructAddUnsorted(Func<int, IInterval<int>[]> intervalConstruction, Func<IInterval<int>[], IIntervalCollection<IInterval<int>, int>> intervalCollectionConstruction)
            : base(intervalConstruction, intervalCollectionConstruction)
        { }

        public override void CollectionSetup()
        {
            Intervals = IntervalConstruction(CollectionSize);
            Intervals.Shuffle();
            IntervalCollection = IntervalCollectionConstruction(EmptyIntervals);
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
        }

        public override void Setup()
        {
            Intervals.Shuffle();
            IntervalCollection.Clear();
        }

        public override double Call(int i)
        {
            foreach (var interval in Intervals)
                IntervalCollection.Add(interval);
            return 1;
        }
    }
}
