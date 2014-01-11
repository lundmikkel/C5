using System;
using C5.Intervals;
using C5.Performance.Wpf.Benchmarks;

namespace C5.Performance.Wpf.Report_Benchmarks
{
    class ConstructAddSorted : IntervalBenchmarkable
    {
        public ConstructAddSorted(Func<int, IInterval<int>[]> intervalConstruction, Func<IInterval<int>[], IIntervalCollection<IInterval<int>, int>> intervalCollectionConstruction)
            : base(intervalConstruction, intervalCollectionConstruction)
        {}

        public override void CollectionSetup()
        {
            Intervals = IntervalConstruction(CollectionSize);
            Sorting.IntroSort(Intervals, 0, CollectionSize, IntervalExtensions.CreateComparer<IInterval<int>, int>());
            IntervalCollection = IntervalCollectionConstruction(EmptyIntervals);
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
        }

        public override void Setup()
        {
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
