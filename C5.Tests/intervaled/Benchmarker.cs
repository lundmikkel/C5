using System;
using System.Collections.Generic;
using System.Diagnostics;
using C5.intervaled;
using NUnit.Framework;

namespace C5.Tests.intervaled
{
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
