using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using System.Linq;

namespace C5.Intervals.Tests
{
    using IntervalOfInt = IInterval<int>;

    // TODO: Move tests to the new benchmarker

    [TestFixture, Ignore]
    public class searchBenchmarker
    {
        private IInterval<int>[][] _intervalLayers;
        private int repetitions = 1000000;

        [SetUp]
        public void SetUp()
        {
            _intervalLayers = new IInterval<int>[1][];
            _intervalLayers[0] = BenchmarkTestCases.DataSetA(1000000);
        }

        [Test]
        public void Test()
        {
            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 100; i++)
            {
                foreach (var interval in _intervalLayers[0])
                {
                    findFirst(0, 0, 1000000, interval);
                }
            }

            sw.Stop();

            Console.WriteLine("Time: {0}",
                (float) sw.ElapsedMilliseconds / repetitions * 1000
            );
        }

        private int findFirst(int layer, int lower, int upper, IInterval<int> query)
        {
            int min = lower - 1, max = upper;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = _intervalLayers[layer][middle];

                var compare = query.Low.CompareTo(interval.High);

                if (compare < 0 || compare == 0 && query.LowIncluded && interval.HighIncluded)
                    max = middle;
                else
                    min = middle;
            }

            return max;
        }

        private int findFirst2(int layer, int lower, int upper, IInterval<int> query)
        {
            int min = lower - 1, max = upper;

            var intervalLayer = _intervalLayers[layer];

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                if (query.CompareLowHigh(intervalLayer[middle]) > 0)
                    min = middle;
                else
                    max = middle;
            }

            return max;
        }
    }

    [TestFixture, Ignore]
    public class enumeratorBenchmarker
    {
        private LayeredContainmentList<IntervalOfInt, int> intervals;

        [SetUp]
        public void SetUp()
        {
            intervals = new LayeredContainmentList<IntervalOfInt, int>(BenchmarkTestCases.DataSetC(1000000));
        }

        [Test]
        public void Test()
        {
            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 100; i++)
                foreach (var interval in intervals.Sorted)
                    interval.Equals(interval);

            sw.Stop();

            Console.WriteLine("Time: {0}", (float) sw.ElapsedMilliseconds / 1000);
        }
    }

    [TestFixture, Ignore]
    public class arraylistBenchmarker
    {
        [Test]
        public void Test()
        {
            /*
            var array = new int[1000000];
            for (int j = 0; j < 1000000; j++)
            {
                array[j] = j;
            }
            /*/
            var array = new ArrayList<int>(1000000);
            for (int j = 0; j < 1000000; j++)
            {
                array.Add(j);
            }
            //*/

            var sw = new Stopwatch();
            sw.Start();
            var x = 0;
            for (int i = 0; i < 100; i++)
            {

                for (int j = 0; j < 1000000; j++)
                {
                    x = array[j];
                }
            }

            sw.Stop();

            Console.WriteLine("Time: {0}", (float) sw.ElapsedMilliseconds / 1000, x);
        }

        [Test]
        public void Regular()
        {
            var array = new int[1000000];

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 1000000; j++)
                {
                    array[j] = j;
                }
            }
            sw.Stop();

            Console.WriteLine("Time: {0}", (float) sw.ElapsedMilliseconds / 1000);
        }

        [Test]
        public void Unwinding()
        {
            var array = new int[1000004];

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 1000000; j += 8)
                {
                    array[j] = j;
                    array[j + 1] = j + 1;
                    array[j + 2] = j + 2;
                    array[j + 3] = j + 3;
                    array[j + 4] = j + 4;
                    array[j + 5] = j + 5;
                    array[j + 6] = j + 6;
                    array[j + 7] = j + 7;
                }
            }
            sw.Stop();

            Console.WriteLine("Time: {0}", (float) sw.ElapsedMilliseconds / 1000);
        }
    }

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
                return Benchmarking ? 20 : 1;
            }
        }
        public static int QueryRepetitions
        {
            get
            {
                return 1;// Benchmarking ? 1000000 : 1;
            }
        }

        public static bool Benchmarking { get { return false; } }

        public static object[] DataSets = new object[] { 
            "A",// "B", "C", 
            //"D" 
        };

        public static int[] ConstructorCounts = new[]
            {
                 10000,
                 20000,
                 30000,
                 40000,
                 /*500000,
                 600000,
                 700000,
                 800000,
                 900000,
                1000000*/
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

        public static IInterval<int>[] DataSetAOpen(int count)
        {
            var intervals = new IInterval<int>[count];

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(i * 2, i * 2 + 1, IntervalType.Open);

            return intervals;
        }

        public static IInterval<int>[] DataSetNotA(int count)
        {
            var intervals = new IInterval<int>[count];

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(i * 2 + 1, i * 2 + 2);

            return intervals;
        }

        /*public static IInterval<int>[] DataSetA(int count, int length, int gap)
        {
            var intervals = new IInterval<int>[count];

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(i * 2 + gap, i * 2 + length);

            return intervals;
        }*/

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

        private static int randomInt()
        {
            var bytes = new byte[4];
            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static IInterval<int> RandomInterval()
        {
            var low = randomInt();
            var high = randomInt();
            while (high <= low)
                high = randomInt();
            var lowIncluded = randomInt() > 0;
            var highIncluded = randomInt() > 0;
            if (low == high)
                lowIncluded = highIncluded = true;
            return new IntervalBase<int>(low, high, lowIncluded, highIncluded);
        }

        public static IInterval<int>[] RandomSet(int count)
        {
            const int seed = 0;
            var r = new Random(seed);
            var intervals = new IInterval<int>[count];
            for (var i = 0; i < count; i++)
            {
                var low = r.Next(Int32.MinValue, Int32.MaxValue);
                var high = r.Next(low, Int32.MaxValue);
                var lowIncluded = r.Next(Int32.MinValue, Int32.MaxValue) > 0;
                var highIncluded = r.Next(Int32.MinValue, Int32.MaxValue) > 0;
                if (low == high)
                    lowIncluded = highIncluded = true;
                intervals[i] = new IntervalBase<int>(low, high, lowIncluded, highIncluded);
            }
            return intervals;
        }
    }

    abstract class DataSetTester
    {
        protected IIntervalCollection<IntervalOfInt, int> IntervalCollection;
        protected abstract IIntervalCollection<IntervalOfInt, int> Factory(IEnumerable<IInterval<int>> intervals);

        public abstract string Name { get; }

        [Test, Combinatorial]
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
                IntervalCollection = Factory(intervals);
                sw.Stop();
            }

            sw.Stop();

            writeTest("Constructor", dataset, intervals.Length, sw.ElapsedMilliseconds / repetitions);
            Console.WriteLine("Average construction time for {0} intervals: {1} ms",
                intervals.Length,
                sw.ElapsedMilliseconds / repetitions
            );
        }

        [Test, Combinatorial, Ignore]
        public void SearchTime_Doubling(
            [ValueSource(typeof(BenchmarkTestCases), "DataSets")] string dataset,
            [ValueSource(typeof(BenchmarkTestCases), "DoubleCounts")] int count
        )
        {
            IntervalCollection = Factory(BenchmarkTestCases.TestDataSet(dataset, count));

            const int length = 20;

            var repetitions = BenchmarkTestCases.QueryRepetitions;
            var span = IntervalCollection.Span;
            var step = (float) (span.High - span.Low - length) / repetitions;

            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < repetitions; i++)
            {
                var low = (int) (step * i);
                IntervalCollection.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }

            sw.Stop();

            writeTest("SearchTime", dataset, IntervalCollection.Count, (float) sw.ElapsedMilliseconds / repetitions * 1000);
            Console.WriteLine("Average search time for {0} intervals: {1} µs", IntervalCollection.Count, (float) sw.ElapsedMilliseconds / repetitions * 1000);
        }

        [Test, Combinatorial, Ignore]
        public void FindOverlaps_OutputSize_QueryLength(
            [ValueSource(typeof(BenchmarkTestCases), "DataSets")] string dataset,
            [ValueSource(typeof(BenchmarkTestCases), "QueryLengths")] int length
        )
        {
            IntervalCollection = Factory(BenchmarkTestCases.TestDataSet(dataset, 100));

            var repetitions = BenchmarkTestCases.QueryRepetitions;
            var span = IntervalCollection.Span;
            var step = (float) (span.High - span.Low - 20 - length) / repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < repetitions; i++)
            {
                var low = (int) (step * i);
                IntervalCollection.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            writeTest("FindOverlaps", dataset, length, (float) sw.ElapsedMilliseconds / repetitions * 1000);

            Console.WriteLine("Average query time for {0} intervals (query length: {1}): {2} µs",
                IntervalCollection.Count,
                length,
                (float) sw.ElapsedMilliseconds / repetitions * 1000
            );
        }

        [Test, Combinatorial, Ignore]
        public void CountOverlaps_OutputSize_QueryLength(
            [ValueSource(typeof(BenchmarkTestCases), "DataSets")] string dataset,
            [ValueSource(typeof(BenchmarkTestCases), "QueryLengths")] int length
        )
        {
            IntervalCollection = Factory(BenchmarkTestCases.TestDataSet(dataset, 1000000));

            var repetitions = BenchmarkTestCases.QueryRepetitions;
            var span = IntervalCollection.Span;
            var step = (float) (span.High - span.Low - 20 - length) / repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < repetitions; i++)
            {
                var low = (int) (step * i);
                IntervalCollection.CountOverlaps(new IntervalBase<int>(low, low + length));
            }
            sw.Stop();

            writeTest("CountOverlaps", dataset, length, (float) sw.ElapsedMilliseconds / repetitions * 1000);

            Console.WriteLine("Average count time for {0} intervals (query length: {1}): {2} µs",
                IntervalCollection.Count,
                length,
                (float) sw.ElapsedMilliseconds / repetitions * 1000
            );
        }

        private void writeTest(string methodName, string dataSet, float x, float y)
        {
            if (BenchmarkTestCases.Benchmarking)
            {
                var filename = String.Format("../../intervals/data/benchmarker/{0}_{1}_{2}.dat", Name, methodName,
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

    class LayeredContainmentList_Benchmarker : DataSetTester
    {
        protected override IIntervalCollection<IntervalOfInt, int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new LayeredContainmentList<IntervalOfInt, int>(intervals);
        }
        public override string Name { get { return "Layered"; } }
    }

    class NestedContainmentList_Benchmarker : DataSetTester
    {
        protected override IIntervalCollection<IntervalOfInt, int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList<IntervalOfInt, int>(intervals);
        }
        public override string Name { get { return "Nested"; } }
    }

    class NestedContainmentList2_Benchmarker : DataSetTester
    {
        protected override IIntervalCollection<IntervalOfInt, int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList2<IntervalOfInt, int>(intervals);
        }
        public override string Name { get { return "Nested2"; } }
    }

    class StaticIntervalTree_Benchmarker : DataSetTester
    {
        protected override IIntervalCollection<IntervalOfInt, int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<IntervalOfInt, int>(intervals);
        }
        public override string Name { get { return "Static"; } }
    }

    class IntervalBinarySearchTree_Benchmarker : DataSetTester
    {
        protected override IIntervalCollection<IntervalOfInt, int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new IntervalBinarySearchTree<IntervalOfInt, int>(intervals);
        }
        public override string Name { get { return "IBSAVL"; } }
    }
}
