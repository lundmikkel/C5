using System.Collections.Generic;
using System.Linq;
using C5.intervals;
using C5.UserGuideExamples.intervals;

namespace C5.Performance.Wpf.Benchmarks
{
    public class DITTrainSearchBenchmark : Benchmarkable
    {
        private Trains.TrainRide[] _trains;
        private IInterval<double>[] _trainsNotInCollection;
        private DynamicIntervalTree<Trains.TrainRide, double> intervalTrains;


        private int trainSearch(int trainId)
        {
            // If the id is in range of the original trains search for a train we know is there
            if (trainId < CollectionSize)
                return intervalTrains.Contains(_trains[trainId]) ? 1 : 0;
            // If the is is out of range search for a train we know is not in the collection.
            return intervalTrains.Contains(_trainsNotInCollection[(trainId - CollectionSize)]) ? 1 : 0;
        }

        private static IEnumerable<IInterval<double>> findInbetweenTrains(IEnumerable<Trains.TrainRide> trains)
        {
            var sortedTrains = trains.ToList();
            sortedTrains.Sort((t1, t2) => t1.CompareTo(t2));
            var intervalsNotInCollection = new ArrayList<IInterval<double>>();
            for (var i = 0; i < sortedTrains.Count - 1; i++)
            {
                var i1 = sortedTrains[i];
                var i2 = sortedTrains[i + 1];
                if (i1.High.CompareTo(i2.Low) < 0)
                    intervalsNotInCollection.Add(new IntervalBase<double>(i1.High, i2.Low, false, highIncluded: false));
                if (i1.High.CompareTo(i2.Low) < 0 && i == sortedTrains.Count - 2)
                    intervalsNotInCollection.Add(new IntervalBase<double>(i1.High, i2.Low, false, highIncluded: false));
            }
            return intervalsNotInCollection;
        }

        public override void CollectionSetup()
        {
            var trainRessource = Trains.parseCvs();

            // Get the number of trains from the csv file matching the collectionsize
            _trains = trainRessource.SelectMany(col => col).Take(CollectionSize).ToArray();

            // Create collection of trains that is not in the collection to have unsuccesfull searches
            _trainsNotInCollection = findInbetweenTrains(_trains).ToArray();

            intervalTrains = new DynamicIntervalTree<Trains.TrainRide, double>();
            intervalTrains.AddAll(_trains);

            /*
             * Setup an items array with things to look for.
             * Fill in random numbers from 0 to the number of trains plus the number of trains not in the collection.
             * This should make roughly half the searched succesful if we find enough space to generate as many trains not in the collection as there is trains already.
             */
            ItemsArray = SearchAndSort.FillIntArrayRandomly(CollectionSize, 0,
                CollectionSize + _trainsNotInCollection.Count());
        }

        public override void Setup()
        {
        }

        public override double Call(int i)
        {
            return trainSearch(i);
        }

        public override string BenchMarkName()
        {
            return "DIT Train Search";
        }
    }
}