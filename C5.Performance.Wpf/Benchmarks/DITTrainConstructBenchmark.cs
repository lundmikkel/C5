using C5.Intervals;
using C5.UserGuideExamples.Intervals;

namespace C5.Performance.Wpf.Benchmarks
{
    public class DITTrainConstructBenchmark : Benchmarkable
    {
        private Trains.TrainRide[] _trains;
        private DynamicIntervalTree<Trains.TrainRide, double> _intervalTrains;


        private int trainConstruct(int trainId)
        {
            _intervalTrains = new DynamicIntervalTree<Trains.TrainRide, double>(_trains);
            return 1;
        }

        public override void CollectionSetup()
        {
            // Get the number of trains from the csv file matching the collectionsize
            _trains = TrainUtilities.GetTrains(CollectionSize);

            Sorting.IntroSort(_trains, 0, CollectionSize, IntervalExtensions.CreateComparer<Trains.TrainRide, double>());

            /*
             * Setup an items array with things to look for. Not used in this benchmark.
             */
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
        }

        public override void Setup()
        {
            //            _trains.Shuffle();
        }

        public override double Call(int i)
        {
            return trainConstruct(i);
        }

        public override string BenchMarkName()
        {
            return "DIT Train Construct";
        }
    }
}