using System.Linq;
using C5.Intervals;
using C5.UserGuideExamples.Intervals;

namespace C5.Performance.Wpf.Benchmarks
{
    public class DITTrainRemoveBenchmark : Benchmarkable
    {
        private Trains.TrainRide[] _trains;
        private TrainUtilities.InBetweenTrainRide[] _trainsNotInCollection;
        private DynamicIntervalTree<Trains.TrainRide, double> _intervalTrains;


        private int trainSearch(int trainId)
        {
            // If the id is in range of the original trains search for a train we know is there
            if (trainId < CollectionSize)
                return _intervalTrains.Remove(_trains[trainId]) ? 1 : 0;

            // If the trainId is out of range search for a train we know is not in the collection and we search for a train that should not be there.
            return _intervalTrains.Remove(_trainsNotInCollection[(trainId - CollectionSize)]) ? 1 : 0;
        }



        public override void CollectionSetup()
        {
            // Get the number of trains from the csv file matching the collectionsize
            _trains = TrainUtilities.GetTrains(CollectionSize);

            // Create collection of trains that is not in the collection to have unsuccesfull searches
            _trainsNotInCollection = TrainUtilities.FindInbetweenTrains(_trains).ToArray();

            _intervalTrains = new DynamicIntervalTree<Trains.TrainRide, double>();
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
            // Add all trains after each removal benchmark
            _intervalTrains.Clear();
            _intervalTrains.AddAll(_trains);
        }

        public override double Call(int i)
        {
            return trainSearch(i);
        }

        public override string BenchMarkName()
        {
            return "DIT Train Remove";
        }
    }
}