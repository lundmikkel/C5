﻿using C5.intervals;
using C5.UserGuideExamples.intervals;

namespace C5.Performance.Wpf.Benchmarks
{
    public class IBSTrainConstructBenchmark : Benchmarkable
    {
        private Trains.TrainRide[] _trains;
        private IntervalBinarySearchTreeAvl<Trains.TrainRide, double> _intervalTrains;


        private int trainConstruct(int trainId)
        {
            _intervalTrains = new IntervalBinarySearchTreeAvl<Trains.TrainRide, double>(_trains);
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
            return "IBS Train Construct";
        }
    }
}