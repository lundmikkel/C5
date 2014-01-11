using System;
using System.Linq;
using C5.Performance.Wpf.Benchmarks;
using C5.Intervals;

namespace C5.Performance.Wpf.Report_Benchmarks
{
    class ConstructAddAllInConstructor : IntervalBenchmarkable
    {
        public ConstructAddAllInConstructor(Func<int, IInterval<int>[]> intervalConstruction, Func<IInterval<int>[], IIntervalCollection<IInterval<int>, int>> intervalCollectionConstruction)
            : base(intervalConstruction, intervalCollectionConstruction)
        { }

        public override void CollectionSetup()
        {
            Intervals = IntervalConstruction(CollectionSize);
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
        }

        public override void Setup()
        { }

        public override double Call(int i)
        {
            IntervalCollection = IntervalCollectionConstruction(Intervals);
            return 1;
        }
    }
}
