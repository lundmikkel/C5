using System.Collections.Generic;
using System.Linq;
using C5.intervals;
using C5.UserGuideExamples.intervals;

namespace C5.Performance.Wpf.Benchmarks
{
    public class IBSTrainConstructBenchmark : Benchmarkable
    {
        private Trains.TrainRide[] _trains;
        private IInterval<double>[] _trainsNotInCollection;
        private IntervalBinarySearchTreeAvl<Trains.TrainRide, double> intervalTrains;


        private int trainConstruct(int trainId)
        {
            for (var i = 0; i < trainId; i++)
                intervalTrains.Add(_trains[trainId]);
            return 1;
        }

        public override void CollectionSetup()
        {
            var trainRessource = Trains.parseCvs();

            // Get the number of trains from the csv file matching the collectionsize
            _trains = trainRessource.SelectMany(col => col).Take(CollectionSize).ToArray();

            /*
             * Setup an items array with things to look for. Not used in this benchmark.
             */
            ItemsArray = SearchAndSort.FillIntArray(CollectionSize);
        }

        public override void Setup()
        {
            intervalTrains = new IntervalBinarySearchTreeAvl<Trains.TrainRide, double>();
        }

        public override double Call(int i)
        {
            return trainConstruct(i);
        }

        public override string BenchMarkName()
        {
            return "IBS Train Construct";
        }
    }
}