using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using C5.intervals;
using Microsoft.VisualBasic.FileIO;

namespace C5.UserGuideExamples.intervals
{
    public class Trains
    {
        public static void Main(string[] args)
        {
            Console.Out.WriteLine(train());
            var resources = parseCvs();
            findInbetweenIntervals(resources);
            trainFun(resources);
            Console.Read();
        }

        public static IIntervalCollection<TrainRide, double>[] parseCvs()
        {
            const string filepath = @"../../../C5.UserGuideExamples/intervals/data/train.csv";

            var resources = new IIntervalCollection<TrainRide, double>[16];

            using (var parser = new TextFieldParser(filepath) { Delimiters = new[] { "," } })
            {
                // Skip first line with header
                var parts = parser.ReadFields();
                var i = 0;
                while ((parts = parser.ReadFields()) != null)
                {
                    var resource = Int32.Parse(parts[0]) - 1;

                    var start = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    var end = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    var track = Int32.Parse(parts[3]);
                    var train = Int32.Parse(parts[4]);

                    var ride = new TrainRide(start, end, track, train);

                    if (resources[resource] == null)
                    {
                        /*
                            resources[resource] = new IntervalBinarySearchTreeAvl<TrainRide, double>();
                        /*/
                        resources[resource] = new DynamicIntervalTree<TrainRide, double>();
                        //*/
                    }
                    //if (i++ % 100 == 0)
                    //  Console.Out.WriteLine(ride);

                    resources[resource].Add(ride);
                }
            }

            return resources;
        }

        public class TrainRide : IInterval<double>
        {
            public TrainRide(double low, double high, int track, int train)
            {
                High = high;
                Low = low;
                Track = track;
                Train = train;
            }

            public double Low { get; private set; }
            public double High { get; private set; }
            public bool LowIncluded { get { return true; } }
            public bool HighIncluded { get { return true; } }

            public int Track { get; private set; }
            public int Train { get; private set; }

            public override string ToString()
            {
                return IntervalExtensions.ToString(this);
            }
        }

        private static void trainFun(IIntervalCollection<TrainRide, double>[] resources)
        {
            var numberOfTrains = resources.SelectMany(col => col.Select(tr => tr.Train)).ToArray();
            Console.Out.WriteLine("Number of trains: " + numberOfTrains.Count() + "\nDistinct number of trains: " + numberOfTrains.Distinct().Count());

            var numberOfTracks = resources.SelectMany(col => col.Select(tr => tr.Track)).Distinct().ToList();
            numberOfTracks.Sort();
            Console.Out.WriteLine("Distinct number of tracks: " + numberOfTracks.Count());

            var trainsOnSeperateTracks = new List<TrainRide>[numberOfTracks.Count];
            for (var i = 0; i < numberOfTracks.Count; i++)
                trainsOnSeperateTracks[i] =
                    resources.SelectMany(col => col.Filter(tr => tr.Track == numberOfTracks[i])).ToList();

            for (var i = 0; i < numberOfTracks.Count; i++)
            {
                var track = numberOfTracks[i];
                var trains = resources.SelectMany(col => col.Filter(tr => tr.Track == track)).ToArray();
                var trainIds = trains.Select(tr => tr.Train).Distinct();
                var distinctTrains = trainIds.Select(trainId => trains.First(tr => tr.Train == trainId));
                Console.Out.WriteLine("Track " + track + " is used by " + distinctTrains.Count() + " train(s).");

                foreach (var trainRide in trains)
                {
                    var collision =
                        trainsOnSeperateTracks[i].FindAll(
                        // Detect possible collisions
                                tr => tr.Train != trainRide.Train &&
                                      tr.Track == trainRide.Track &&
                                      tr.Overlaps(trainRide)
                                );
                    if (collision.Any())
                        Console.Out.WriteLine("{0} Possible collision detected on track {1}, with train(s) {2}.",
                            collision.Count(), trainRide.Track,
                            String.Join(",", collision.Select(tr => tr.Train).ToArray()));
                }
            }
        }

        private static void findInbetweenIntervals(IIntervalCollection<TrainRide, double>[] resources)
        {
            var sortedTrains = resources[0].ToList();
            sortedTrains.Sort((t1, t2) => t1.CompareTo(t2));
            var intervalsNotInCollection = new ArrayList<IInterval<double>>();
            for (var i = 0; i < sortedTrains.Count - 1; i++)
            {
                var i1 = sortedTrains[i];
                var i2 = sortedTrains[i + 1];
                if (i1.High.CompareTo(i2.Low) < 0)
                    intervalsNotInCollection.Add(new IntervalBase<double>(i1.High, i2.Low, lowIncluded: false, highIncluded: false));
            }
            Console.Out.WriteLine("There is {0} intervals in the collection.", sortedTrains.Count);
            Console.Out.WriteLine("Created {0} intervals that's not in the collection.", intervalsNotInCollection.Count);
            foreach (var interval in intervalsNotInCollection)
            {
                if (sortedTrains.Contains(interval))
                    Console.Out.WriteLine("You have made a terrible mistake!");
            }
        }

        private static string train()
        {
            return @"
                o    .  o  .  o .  o  .  o  .  o
           o
        .
      .        ___
     _n_n_n____i_i ________ ______________ _++++++++++++++_
  *>(____________I I______I I____________I I______________I
    /ooOOOO OOOOoo  oo oooo oo          oo ooo          ooo
------------------------------------------------------------";
        }

    }


}
