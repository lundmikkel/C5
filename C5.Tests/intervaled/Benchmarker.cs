using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using C5.intervaled;
using NUnit.Framework;
using System.Linq;

namespace C5.Tests.intervaled
{
    public static class Utils
    {
        static readonly Random Random = new Random(0);

        public static void Shuffle<T>(this T[] list)
        {
            var n = list.Length;
            while (--n > 0)
                list.Swap(Random.Next(n + 1), n);
        }

        public static void Swap<T>(this T[] list, int i, int j)
        {
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    public static class BenchmarkTestCases
    {
        public static int ConstructorRepetitions
        {
            get
            {
                return Benchmarking ? 200 : 1;
            }
        }
        public static int QueryRepetitions
        {
            get
            {
                return Benchmarking ? 1000000 : 1;
            }
        }

        public static bool Benchmarking { get { return false; } }

        public static object[] DataSets = new object[] { 
            "A", "B", "C", 
            //"D" 
        };

        public static int[] ConstructorCounts = new[]
            {
                 100000,
                 200000,
                 300000,
                 400000,
                 500000,
                 600000,
                 700000,
                 800000,
                 900000,
                1000000
            };

        public static int[] DoubleCounts = new[]
            {
                8000 * (int) Math.Pow(2, 0),
                8000 * (int) Math.Pow(2, 1),
                8000 * (int) Math.Pow(2, 2),
                8000 * (int) Math.Pow(2, 3),
                8000 * (int) Math.Pow(2, 4),
                8000 * (int) Math.Pow(2, 5),
                8000 * (int) Math.Pow(2, 6),
                8000 * (int) Math.Pow(2, 7),
                8000 * (int) Math.Pow(2, 8),
                8000 * (int) Math.Pow(2, 9),
                8000 * (int) Math.Pow(2, 10),
                8000 * (int) Math.Pow(2, 11)
            };

        public static int[] QueryLengths = new[]
            {
                 100,
                 200,
                 300,
                 400,
                 500,
                 600,
                 700,
                 800,
                 900,
                1000
            };

        public static IEnumerable<IInterval<int>> TestDataSet(string dataset, int count)
        {
            switch (dataset)
            {
                case "A":
                    return DataSetA(count);

                case "B":
                    return DataSetB(count);

                case "C":
                    return DataSetC(count);

                case "D":
                    return DataSetD(count);

                default:
                    return Enumerable.Empty<IInterval<int>>();
            }
        }

        public static IInterval<int>[] DataSetA(int count)
        {
            var intervals = new IInterval<int>[count];

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(i * 2, i * 2 + 1);

            return intervals;
        }

        public static IInterval<int>[] DataSetB(int count)
        {
            var intervals = new IInterval<int>[count];

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(i, i + 5);

            return intervals;
        }

        public static IInterval<int>[] DataSetC(int count)
        {
            var intervals = new IInterval<int>[count];

            for (int i = 0; i < count / 5; i++)
            {
                var mid = i * 5;
                for (var j = 0; j < 5; j++)
                    intervals[mid + j] = new IntervalBase<int>(mid - j, mid + j + 1);
            }

            return intervals;
        }

        public static IInterval<int>[] DataSetD(int count)
        {
            var intervals = new IInterval<int>[count];
            var mid = count / 2;

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(mid - i, mid + i + 1);

            return intervals;
        }
    }


    abstract class DataSetTester
    {
        protected IIntervaled<int> Intervaled;
        protected abstract IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals);

        public abstract string Name { get; }

        [Test, Combinatorial, Ignore]
        public void Constructor_ChangingCollectionSize(
            [ValueSource(typeof(BenchmarkTestCases), "DataSets")] string dataset,
            [ValueSource(typeof(BenchmarkTestCases), "ConstructorCounts")] int count
        )
        {
            var intervals = BenchmarkTestCases.TestDataSet(dataset, count).ToArray();

            var sw = new Stopwatch();
            var repetitions = BenchmarkTestCases.ConstructorRepetitions;

            for (var i = 0; i < repetitions; i++)
            {
                intervals.Shuffle();
                sw.Start();
                Intervaled = Factory(intervals);
                sw.Stop();
            }

            sw.Stop();

            writeTest("Constructor", dataset, intervals.Length, sw.ElapsedMilliseconds / repetitions);
            Console.WriteLine("Average construction time for {0} intervals: {1} ms",
                intervals.Length,
                sw.ElapsedMilliseconds / repetitions
            );
        }

        [Test, Combinatorial]
        public void SearchTime_Doubling(
            [ValueSource(typeof(BenchmarkTestCases), "DataSets")] string dataset,
            [ValueSource(typeof(BenchmarkTestCases), "DoubleCounts")] int count
        )
        {
            Intervaled = Factory(BenchmarkTestCases.TestDataSet(dataset, count));

            const int length = 20;

            var repetitions = BenchmarkTestCases.QueryRepetitions;
            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - length) / repetitions;

            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }

            sw.Stop();

            writeTest("SearchTime", dataset, Intervaled.Count, (float) sw.ElapsedMilliseconds / repetitions * 1000);
            Console.WriteLine("Average search time for {0} intervals: {1} µs", Intervaled.Count, (float) sw.ElapsedMilliseconds / repetitions * 1000);
        }

        [Test, Combinatorial]
        public void FindOverlaps_OutputSize_QueryLength(
            [ValueSource(typeof(BenchmarkTestCases), "DataSets")] string dataset,
            [ValueSource(typeof(BenchmarkTestCases), "QueryLengths")] int length
        )
        {
            Intervaled = Factory(BenchmarkTestCases.TestDataSet(dataset, 1000000));

            var repetitions = BenchmarkTestCases.QueryRepetitions;
            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20 - length) / repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            writeTest("FindOverlaps", dataset, length, (float) sw.ElapsedMilliseconds / repetitions * 1000);

            Console.WriteLine("Average query time for {0} intervals (query length: {1}): {2} µs",
                Intervaled.Count,
                length,
                (float) sw.ElapsedMilliseconds / repetitions * 1000
            );
        }

        [Test, Combinatorial]
        public void CountOverlaps_OutputSize_QueryLength(
            [ValueSource(typeof(BenchmarkTestCases), "DataSets")] string dataset,
            [ValueSource(typeof(BenchmarkTestCases), "QueryLengths")] int length
        )
        {
            Intervaled = Factory(BenchmarkTestCases.TestDataSet(dataset, 1000000));

            var repetitions = BenchmarkTestCases.QueryRepetitions;
            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20 - length) / repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.CountOverlaps(new IntervalBase<int>(low, low + length));
            }
            sw.Stop();

            writeTest("CountOverlaps", dataset, length, (float) sw.ElapsedMilliseconds / repetitions * 1000);

            Console.WriteLine("Average count time for {0} intervals (query length: {1}): {2} µs",
                Intervaled.Count,
                length,
                (float) sw.ElapsedMilliseconds / repetitions * 1000
            );
        }

        private void writeTest(string methodName, string dataSet, float x, float y)
        {
            if (BenchmarkTestCases.Benchmarking)
            {
                var filename = String.Format("../../intervaled/data/benchmarker/{0}_{1}_{2}.dat", Name, methodName,
                                             dataSet);

                using (var f = File.AppendText(filename))
                {
                    f.WriteLine(String.Format(
                        //new System.Globalization.CultureInfo("en-US"),
                        "{0:n1}\t{1:n3}",
                        x, y
                                    ));
                }
            }
        }
    }

    class LayeredContainmentList_ContainmentsOnly : DataSetTester
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new LayeredContainmentList<int>(intervals);
        }
        public override string Name { get { return "Layered"; } }
    }

    class LayeredContainmentList2_ContainmentsOnly : DataSetTester
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new LayeredContainmentList2<int>(intervals);
        }
        public override string Name { get { return "Layered2"; } }
    }

    class LayeredContainmentList3_ContainmentsOnly : DataSetTester
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new LayeredContainmentList3<int>(intervals);
        }
        public override string Name { get { return "Layered3"; } }
    }

    class NestedContainmentList_ContainmentsOnly : DataSetTester
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList<int>(intervals);
        }
        public override string Name { get { return "Nested"; } }
    }

    class NestedContainmentList2_ContainmentsOnly : DataSetTester
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList2<int>(intervals);
        }
        public override string Name { get { return "Nested2"; } }
    }

    class StaticIntervalTree_ContainmentsOnly : DataSetTester
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
        public override string Name { get { return "Static"; } }
    }
}
