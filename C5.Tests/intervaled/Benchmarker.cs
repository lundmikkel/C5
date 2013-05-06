using System;
using System.Collections.Generic;
using System.Diagnostics;
using C5.intervaled;
using NUnit.Framework;
using System.Linq;

namespace C5.Tests.intervaled
{
    public static class Utils
    {
        public static void Shuffle<T>(this T[] list)
        {
            var random = new Random();
            var n = list.Length;
            while (--n > 0)
                list.Swap(random.Next(n + 1), n);
        }

        public static void Swap<T>(this T[] list, int i, int j)
        {
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    abstract class WithoutContainment
    {
        protected IIntervaled<int> Intervaled;
        private const int Repetitions = 1000000;

        protected abstract IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals);

        public IInterval<int>[] GenerateIntervals(int count)
        {
            var intervals = new IInterval<int>[count];

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(i, i + 5);

            return intervals;
        }

        [Test, TestCaseSource(typeof(WithoutContainmentOrOverlaps), "DoubleCounts")]
        public void RangeQuery(string name, int count)
        {
            Intervaled = Factory(GenerateIntervals(count));

            var length = 20;
            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20) / Repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < Repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            Console.WriteLine("Average creation time for {0} intervals: {1} ns", Intervaled.Count, (float) sw.ElapsedMilliseconds / Repetitions * 1000);
        }

        [Test, TestCaseSource(typeof(WithoutContainment), "ConstructorCounts")]
        public void Constroctor(int count)
        {
            var intervals = GenerateIntervals(count);

            var sw = new Stopwatch();

            for (var i = 0; i < Repetitions; i++)
            {
                intervals.Shuffle();
                sw.Start();
                Intervaled = Factory(intervals);
                sw.Stop();
            }

            sw.Stop();
            Console.WriteLine("Average creation time for {0} intervals: {1} ms", intervals.Length, sw.ElapsedMilliseconds / Repetitions);
        }

        [Test, TestCaseSource(typeof(WithoutContainmentOrOverlaps), "QueryLengths")]
        public void RangeFixCountQuery(string name, int length)
        {
            Intervaled = Factory(GenerateIntervals(1000000));

            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20 - length) / Repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < Repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            Console.WriteLine("Average query time for {0} intervals: {1} ns", Intervaled.Count, (float) sw.ElapsedMilliseconds / Repetitions * 1000);
        }

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
    }

    abstract class WithoutContainmentOrOverlaps
    {
        protected IIntervaled<int> Intervaled;
        private const int Repetitions = 1000000;

        protected abstract IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals);

        public IInterval<int>[] GenerateIntervals(int count)
        {
            var intervals = new IInterval<int>[count];

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(i * 2, i * 2 + 1);

            return intervals;
        }

        [Test]
        public void Constroctor([Range(100000, 1000000, 100000)] int count)
        {
            var intervals = GenerateIntervals(count);

            var sw = new Stopwatch();

            for (var i = 0; i < Repetitions; i++)
            {
                intervals.Shuffle();
                sw.Start();
                Intervaled = Factory(intervals);
                sw.Stop();
            }

            sw.Stop();
            Console.WriteLine("Average creation time for {0} intervals: {1} ms", intervals.Length, sw.ElapsedMilliseconds / Repetitions);
        }

        [Test]
        public void Stabbing([Range(1000000, 10000000, 1000000)] int count)
        {
            Intervaled = Factory(GenerateIntervals(count));

            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low) / Repetitions;
            var sw = new Stopwatch();
            IInterval<int> interval = null;
            sw.Start();
            for (var i = 0; i < Repetitions; i++)
            {
                foreach (var overlap in Intervaled.FindOverlaps((int) (step * i)))
                    interval = overlap;
            }
            sw.Stop();

            Console.WriteLine("Average creation time for {0} intervals: {1} ms (total: {2})", Intervaled.Count, (float) sw.ElapsedMilliseconds / Repetitions, sw.ElapsedMilliseconds);
        }

        [Test, TestCaseSource(typeof(WithoutContainmentOrOverlaps), "DoubleCounts")]
        public void RangeQuery(string name, int count)
        {
            Intervaled = Factory(GenerateIntervals(count));

            var length = 20;
            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20) / Repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < Repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            Console.WriteLine("Average creation time for {0} intervals: {1} ns", Intervaled.Count, (float) sw.ElapsedMilliseconds / Repetitions * 1000);
        }

        [Test, TestCaseSource(typeof(WithoutContainmentOrOverlaps), "QueryLengths")]
        public void RangeFixCountQuery(string name, int length)
        {
            Intervaled = Factory(GenerateIntervals(1000000));

            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20 - length) / Repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < Repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            Console.WriteLine("Average query time for {0} intervals: {1} ns", Intervaled.Count, (float) sw.ElapsedMilliseconds / Repetitions * 1000);
        }


        public static object[] DoubleCounts = new object[]
            {
               new object[] {"A", 8000 * (int) Math.Pow(2, 0)},
               new object[] {"B", 8000 * (int) Math.Pow(2, 1)},
               new object[] {"C", 8000 * (int) Math.Pow(2, 2)},
               new object[] {"D", 8000 * (int) Math.Pow(2, 3)},
               new object[] {"E", 8000 * (int) Math.Pow(2, 4)},
               new object[] {"F", 8000 * (int) Math.Pow(2, 5)},
               new object[] {"G", 8000 * (int) Math.Pow(2, 6)},
               new object[] {"H", 8000 * (int) Math.Pow(2, 7)},
               new object[] {"I", 8000 * (int) Math.Pow(2, 8)},
               new object[] {"J", 8000 * (int) Math.Pow(2, 9)},
               new object[] {"K", 8000 * (int) Math.Pow(2, 10)},
               new object[] {"L", 8000 * (int) Math.Pow(2, 11)}
            };

        public static object[] QueryLengths = new object[]
            {
               new object[] {"A",  100},
               new object[] {"B",  200},
               new object[] {"C",  300},
               new object[] {"D",  400},
               new object[] {"E",  500},
               new object[] {"F",  600},
               new object[] {"G",  700},
               new object[] {"H",  800},
               new object[] {"I",  900},
               new object[] {"J", 1000}
            };
    }

    abstract class ContainmentOnly
    {
        protected IIntervaled<int> Intervaled;
        private const int Repetitions = 1000000;

        protected abstract IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals);

        public IInterval<int>[] GenerateIntervals(int count = 1000000)
        {
            var intervals = new IInterval<int>[count];
            var mid = count / 2;

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(mid - i, mid + i + 1);

            return intervals;
        }

        [Test, TestCaseSource(typeof(ContainmentOnly), "ConstructorCounts")]
        public void Constroctor(int count)
        {
            var intervals = GenerateIntervals(count);

            var sw = new Stopwatch();

            for (var i = 0; i < Repetitions; i++)
            {
                intervals.Shuffle();
                sw.Start();
                Intervaled = Factory(intervals);
                sw.Stop();
            }

            sw.Stop();
            Console.WriteLine("Average creation time for {0} intervals: {1} ms", intervals.Length, sw.ElapsedMilliseconds / Repetitions);
        }

        [Test, TestCaseSource(typeof(WithoutContainmentOrOverlaps), "QueryLengths")]
        public void RangeFixCountQuery(string name, int length)
        {
            Intervaled = Factory(GenerateIntervals(1000000));

            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20 - length) / Repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < Repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            Console.WriteLine("Average query time for {0} intervals: {1} ns", Intervaled.Count, (float) sw.ElapsedMilliseconds / Repetitions * 1000);
        }

        [Test, TestCaseSource(typeof(WithoutContainmentOrOverlaps), "DoubleCounts")]
        public void RangeQuery(string name, int count)
        {
            Intervaled = Factory(GenerateIntervals(count));

            var length = 20;
            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20) / Repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < Repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            Console.WriteLine("Average creation time for {0} intervals: {1} ns", Intervaled.Count, (float) sw.ElapsedMilliseconds / Repetitions * 1000);
        }

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
    }

    abstract class WithContainment
    {
        protected IIntervaled<int> Intervaled;
        private const int Repetitions = 1000000;

        protected abstract IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals);

        public IInterval<int>[] GenerateIntervals(int count = 1000000)
        {
            var intervals = new IInterval<int>[count];
            var mid = 0;

            for (int i = 0; i < count / 5; i++)
            {
                mid = i * 5;
                for (var j = 0; j < 5; j++)
                    intervals[mid + j] = new IntervalBase<int>(mid - j, mid + j + 1);
            }

            return intervals;
        }

        [Test, TestCaseSource(typeof(WithContainment), "ConstructorCounts")]
        public void Constroctor(int count)
        {
            var intervals = GenerateIntervals(count);

            var sw = new Stopwatch();

            for (var i = 0; i < Repetitions; i++)
            {
                intervals.Shuffle();
                sw.Start();
                Intervaled = Factory(intervals);
                sw.Stop();
            }

            sw.Stop();
            Console.WriteLine("Average creation time for {0} intervals: {1} ms", intervals.Length, sw.ElapsedMilliseconds / Repetitions);
        }

        [Test, TestCaseSource(typeof(WithoutContainmentOrOverlaps), "DoubleCounts")]
        public void RangeQuery(string name, int count)
        {
            Intervaled = Factory(GenerateIntervals(count));

            var length = 20;
            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20) / Repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < Repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            Console.WriteLine("Average creation time for {0} intervals: {1} ns", Intervaled.Count, (float) sw.ElapsedMilliseconds / Repetitions * 1000);
        }

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

        [Test, TestCaseSource(typeof(WithContainment), "RangeCounts")]
        public void RangeFixCountQuery(string name, int length)
        {
            Intervaled = Factory(GenerateIntervals(1000000));

            var span = Intervaled.Span;
            var step = (float) (span.High - span.Low - 20 - length) / Repetitions;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < Repetitions; i++)
            {
                var low = (int) (step * i);
                Intervaled.FindOverlaps(new IntervalBase<int>(low, low + length)).Count();
            }
            sw.Stop();

            Console.WriteLine("Average query time for {0} intervals: {1} ns", Intervaled.Count, (float) sw.ElapsedMilliseconds / Repetitions * 1000);
        }


        public static object[] RangeCounts = new object[]
            {
               new object[] {"A",  100},
               new object[] {"B",  200},
               new object[] {"C",  300},
               new object[] {"D",  400},
               new object[] {"E",  500},
               new object[] {"F",  600},
               new object[] {"G",  700},
               new object[] {"H",  800},
               new object[] {"I",  900},
               new object[] {"J", 1000}
            };
    }

    class LCList_WithoutContainment : WithoutContainment
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new LayeredContainmentList<int>(intervals);
        }
    }

    class NCList_WithoutContainment : WithoutContainment
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList<int>(intervals);
        }
    }

    class SIT_WithoutContainment : WithoutContainment
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
    }

    class LCList_WithoutContainmentOrOverlaps : WithoutContainmentOrOverlaps
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new LayeredContainmentList<int>(intervals);
        }
    }

    class NCList_WithoutContainmentOrOverlaps : WithoutContainmentOrOverlaps
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList<int>(intervals);
        }
    }

    class SIT_WithoutContainmentOrOverlaps : WithoutContainmentOrOverlaps
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
    }

    class LCList_WithContainment : WithContainment
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new LayeredContainmentList<int>(intervals);
        }
    }

    class NCList_WithContainment : WithContainment
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList<int>(intervals);
        }
    }

    class SIT_WithContainment : WithContainment
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
    }

    class LCList_ContainmentOnly : ContainmentOnly
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new LayeredContainmentList<int>(intervals);
        }
    }

    class NCList_ContainmentOnly : ContainmentOnly
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList<int>(intervals);
        }
    }

    class SIT_ContainmentOnly : ContainmentOnly
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
    }

    class Benchmarker
    {
        [Test, Ignore]
        public void Construction()
        {
            var intervals = generateRandomIntervals(1000000, 10000000);
            var count = 10;

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                var intervaled = new NestedContainmentList<int>(intervals);
            }
            sw.Stop();

            Console.WriteLine("Creation time: " + (sw.ElapsedMilliseconds / count));
        }

        public static IEnumerable<IInterval<int>> generateRandomIntervals(int count, int width)
        {
            var size = count / 5;

            var list = new ArrayList<IInterval<int>>(count);
            var random = new Random();

            var lengths = new[] { 1, 10, 100, 1000, 10000 };

            foreach (var length in lengths)
            {
                for (int i = 0; i < size; i++)
                {
                    var low = random.Next(width);
                    list.Add(new IntervalBase<int>(low, low + length));
                }
            }

            return list;
        }
    }
}
