using System;
using System.Linq;
using System.Text.RegularExpressions;
using C5.Intervals;
using C5.Performance.Wpf.Benchmarks;
using C5.Intervals.Tests;

namespace C5.Performance.Wpf.Report_Benchmarks
{
    abstract class IntervalBenchmarkable : Benchmarkable
    {
        internal IInterval<int>[] Intervals;
        internal readonly Func<int, IInterval<int>[]> IntervalConstruction;
        internal readonly Func<IInterval<int>[], IIntervalCollection<IInterval<int>, int>> IntervalCollectionConstruction;
        internal IIntervalCollection<IInterval<int>, int> IntervalCollection;
        internal readonly IInterval<int>[] EmptyIntervals = new IInterval<int>[0];
        private static string camelCaseToSpaces(string s)
        {
            return Regex.Replace(s, "(\\B[A-Z])", " $1");
        }

        internal IInterval<int>[] IntervalsNotInCollection(IIntervalCollection<IInterval<int>, int> intervals)
        {
            var intervalsNotInCollection = new ArrayList<IInterval<int>>();
            for (var index = 0; index < Intervals.Length; index++)
            {
                var newInterval = BenchmarkTestCases.RandomInterval();
                while (intervals.Contains(newInterval))
                    newInterval = BenchmarkTestCases.RandomInterval();
                intervalsNotInCollection.Add(newInterval);
            }
            return intervalsNotInCollection.ToArray();
        }

        public override string BenchMarkName()
        {
            var collectionType = IntervalCollectionConstruction.Method.Name;
            return String.Format("{0} {1} {2}", collectionType, camelCaseToSpaces(this.GetType().Name), camelCaseToSpaces(IntervalConstruction.Method.Name));
        }

        public static IIntervalCollection<IInterval<int>, int> DIT(IInterval<int>[] intervals)
        {
            return !intervals.Any() ? new DynamicIntervalTree<IInterval<int>, int>() : new DynamicIntervalTree<IInterval<int>, int>(intervals);
        }
        
        public static IIntervalCollection<IInterval<int>, int> IBS(IInterval<int>[] intervals)
        {
            return !intervals.Any() ? new IntervalBinarySearchTree<IInterval<int>, int>() : new IntervalBinarySearchTree<IInterval<int>, int>(intervals);
        }

        protected IntervalBenchmarkable(Func<int, IInterval<int>[]> intervalConstruction, Func<IInterval<int>[], IIntervalCollection<IInterval<int>, int>> intervalCollectionConstruction)
        {
            IntervalConstruction = intervalConstruction;
            IntervalCollectionConstruction = intervalCollectionConstruction;
        }
    }
}