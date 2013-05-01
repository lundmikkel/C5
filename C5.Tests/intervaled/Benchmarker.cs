using System;
using System.Collections.Generic;
using System.Diagnostics;
using C5.intervaled;
using NUnit.Framework;

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
        private const int Repetitions = 200;

        protected abstract IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals);

        public IInterval<int>[] GenerateIntervals(int count = 1000000)
        {
            var intervals = new IInterval<int>[count];

            for (var i = 0; i < count; i++)
                intervals[i] = new IntervalBase<int>(i, i + 5);

            return intervals;
        }

        [Test, TestCaseSource(typeof(WithoutContainment), "ConstructorCounts")]
        public void Constroctor(int count)
        {
            var intervals = GenerateIntervals(count);

            var sw = new Stopwatch();

            const int tenth = Repetitions / 10;
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
