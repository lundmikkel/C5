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
            var resources = parseCvs();

            Console.Read();
        }

        private static IIntervalCollection<TrainRide, double>[] parseCvs()
        {
            const string filepath = @"../../../C5.UserGuideExamples/intervals/data/train.csv";

            var resources = new IIntervalCollection<TrainRide, double>[17];

            using (var parser = new TextFieldParser(filepath) { Delimiters = new[] { "," } })
            {
                // Skip first line with header
                var parts = parser.ReadFields();
                var i = 0;
                while ((parts = parser.ReadFields()) != null)
                {
                    var resource = Int32.Parse(parts[0]);

                    var start = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    var end = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    var track = Int32.Parse(parts[3]);
                    var train = Int32.Parse(parts[4]);

                    var ride = new TrainRide(start, end, track, train);


                    if (resources[resource] == null)
                        /*
                            resources[resource] = new IntervalBinarySearchTreeAvl<TrainRide, double>();
                        /*/
                        resources[resource] = new DynamicIntervalTree<TrainRide, double>();
                    //*/

                    if (i++ % 100 == 0)
                        Console.Out.WriteLine(ride);

                    resources[resource].Add(ride);
                }
            }

            return resources;
        }

        class TrainRide : IInterval<double>
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
    }
}
