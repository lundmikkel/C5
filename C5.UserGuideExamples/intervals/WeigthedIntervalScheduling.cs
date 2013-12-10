using System;
using System.Collections.Generic;
using System.Linq;
using C5.intervals;

namespace C5.UserGuideExamples.intervals
{
    /// <summary>
    /// The example shows how to use the extension methods from <see cref="C5.intervals"/> to solve
    /// the dynamic programming problem of finding the biggest set of non-overlapping intervals with the
    /// highest weight sum.
    /// </summary>
    public class WeigthedIntervalScheduling
    {
        public static void Main(String[] args)
        {
            // Intervals (here sorted by weight)
            var intervals = new[] {
                    new WeightedInterval(1.0, 10, 13),
                    new WeightedInterval(2.0,  1,  4),
                    new WeightedInterval(2.0,  9, 12),
                    new WeightedInterval(4.0,  5,  8),
                    new WeightedInterval(4.0,  2,  7),
                    new WeightedInterval(7.0,  3, 11)
                };

            var result = CalculateOptimalSolution(intervals);

            Console.Out.Write("The maximum weighted set ({0}) of the intervals is: {1}\n", result.Key, result.Value);
            Console.Read();
        }

        // TODO: Find a better name
        public static KeyValuePair<double, IEnumerable<WeightedInterval>> CalculateOptimalSolution(IEnumerable<WeightedInterval> intervalEnumerable)
        {
            // Make intervals to array to allow fast sorting and counting
            var intervals = intervalEnumerable as WeightedInterval[] ?? intervalEnumerable.ToArray();

            // Return if no intervals were given
            if (!intervals.Any())
                return new KeyValuePair<double, IEnumerable<WeightedInterval>>(0.0, Enumerable.Empty<WeightedInterval>());

            var count = intervals.Length;

            // Sort intervals on high then low endpoint
            var comparer = ComparerFactory<WeightedInterval>.CreateComparer((x, y) =>
                {
                    var compare = x.CompareHigh(y);
                    return compare != 0 ? compare : x.CompareLow(y);
                });
            Sorting.IntroSort(intervals, 0, count, comparer);

            // Calculate the previous non-overlapping interval for all intervals
            var p = new int[count];
            int j;
            for (j = 0; j < count; j++)
                p[j] = findP(ref intervals, j);

            var p2 = findP(intervals);

            // Iteratively calculate the solution for each subproblem
            var opt = new double[count];
            var includeInterval = new bool[count];
            for (j = 0; j < count; j++)
            {
                var included = intervals[j].Weight + (p[j] < 0 ? 0 : opt[p[j]]);
                var notIncluded = j - 1 < 0 ? 0 : opt[j - 1];

                var include = included >= notIncluded;

                opt[j] = include ? included : notIncluded;
                includeInterval[j] = include;
            }

            // Back-trace the solution
            var set = new ArrayList<WeightedInterval>();
            j = count - 1;
            while (j >= 0)
            {
                if (includeInterval[j])
                {
                    set.Add(intervals[j]);
                    j = p[j];
                }
                else
                    j--;
            }

            return new KeyValuePair<double, IEnumerable<WeightedInterval>>(opt[count - 1], set);
        }

        // TODO: Make logarithmic with search from LCList
        private static int findP(ref WeightedInterval[] intervals, int j)
        {
            var interval = intervals[j--];
            while (j >= 0 && intervals[j].Overlaps(interval))
                j--;
            return j;
        }

        private static int findP(WeightedInterval[] intervals)
        {
            for (var i = intervals.Length - 1; i >= 0; i--)
            {

            }
            return 0;
        }

        /// <summary>
        /// An interval with a floating point weight as implementation of <see cref="IInterval{T}"/>.
        /// </summary>
        public class WeightedInterval : IInterval<int>
        {
            public WeightedInterval(double weight, int low, int high)
            {
                Weight = weight;
                High = high;
                Low = low;
            }

            public int Low { get; private set; }
            public int High { get; private set; }
            public bool LowIncluded { get { return true; } }
            public bool HighIncluded { get { return true; } }
            public double Weight { get; private set; }

            public override string ToString()
            {
                return String.Format("{0} ({1})", this.ToIntervalString(), Weight);
            }
        }
    }
}
