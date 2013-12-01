using System;
using System.Globalization;
using C5.intervals;
using Microsoft.VisualBasic.FileIO;

namespace C5.UserGuideExamples.intervals
{
    public class Trains
    {
        public static void Main(string[] args)
        {
            Console.Out.WriteLine(train());
            TrainUtilities.FindInbetweenTrains(ParseDataSetB());
            TrainUtilities.PrintTrainStatistics(ParseDataSetB());
            Console.Read();
        }

        public static ArrayList<TrainRide>[] ParseDataSetA()
        {
            const string filepath = @"../../../C5.UserGuideExamples/intervals/data/train.csv";

            var resources = new ArrayList<TrainRide>[16];

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
                        resources[resource] = new ArrayList<TrainRide>();
                        //*/
                    }
                    resources[resource].Add(ride);
                }
            }

            return resources;
        }

        public static ArrayList<TrainRide> ParseDataSetB()
        {
            const string filepath = @"../../../C5.UserGuideExamples/intervals/data/trainsWithoutRessources.csv";

            var resources = new ArrayList<TrainRide>();

            using (var parser = new TextFieldParser(filepath) { Delimiters = new[] { "," } })
            {
                // Skip first line with header
                var parts = parser.ReadFields();
                while ((parts = parser.ReadFields()) != null)
                {
                    var start = double.Parse(parts[0], CultureInfo.InvariantCulture);
                    var end = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    var track = Int32.Parse(parts[2]);
                    var train = Int32.Parse(parts[3]);

                    var ride = new TrainRide(start, end, track, train);
                    resources.Add(ride);
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

            public override int GetHashCode()
            {
                var hash = 17;
                hash = hash * 23 + High.GetHashCode();
                hash = hash * 23 + Low.GetHashCode();
                hash = hash * 23 + Track.GetHashCode();
                hash = hash * 23 + Train.GetHashCode();
                return hash;
            }

            public override string ToString()
            {
                return IntervalExtensions.ToString(this);
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
