using C5.Intervals;
using C5.Performance.Wpf.Benchmarks;
using C5.Intervals.Tests;

namespace C5.Performance.Wpf.Report_Benchmarks
{
    public class IBSIntervalSetLazy : Benchmarkable
    {
        private IInterval<int>[] _intervals;
        private IntervalBinarySearchTree<IInterval<int>, int> _intervalCollection; 

        private int intervalSearch(int intervalId)
        {
            // TODO Do Something that shows the laze interval set creation here
            return 1;
        }

        public override void CollectionSetup()
        {
            _intervals = BenchmarkTestCases.DataSetB(CollectionSize);
            _intervalCollection.AddAll(_intervals);
            _intervalCollection = new IntervalBinarySearchTree<IInterval<int>, int>(_intervals);

            /*
             * Setup an items array with things to look for.
             * Fill in random numbers from 0 to the number of trains plus the number of trains not in the collection.
             * This should make roughly half the searched succesful if we find enough space to generate as many trains not in the collection as there is trains already.
             */
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
            return "IBS Intervalset Lazy";
        }
    }
}