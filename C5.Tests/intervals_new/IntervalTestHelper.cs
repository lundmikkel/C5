using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using C5.intervals;

namespace C5.Tests.intervals_new
{
    class IntervalTestHelper
    {
        private static readonly Random Random = new Random(0);

        public static IInterval<int> RandomIntInterval()
        {
            var low = Random.Next(Int32.MinValue, Int32.MaxValue);
            var high = Random.Next(low + 2, Int32.MaxValue);

            return new IntervalBase<int>(low, high, (IntervalType) Random.Next(0, 4));
        }

        public static IInterval<int>[] ManyIntervals(int count)
        {
            Contract.Ensures(Contract.Result<IEnumerable<IInterval<int>>>().Count() == count);

            return Enumerable.Range(0, count).Select(i => RandomIntInterval()).ToArray();
        }

        public static IInterval<int>[] DuplicateIntervals(int count)
        {
            var interval = RandomIntInterval();
            return Enumerable.Range(0, count).Select(i => new IntervalBase<int>(interval)).ToArray();
        }

        public static IInterval<int>[] SingleObject(int count)
        {
            var interval = RandomIntInterval();
            return Enumerable.Range(0, count).Select(i => interval).ToArray();
        }

        public static IInterval<int> RandomIntPoint()
        {
            return new IntervalBase<int>(Random.Next(Int32.MinValue, Int32.MaxValue));
        }

        public static IInterval<int>[] NonOverlappingIntervals(int count, int length = 1, int space = 0)
        {
            Contract.Ensures(Contract.ForAll(0, count, i => Contract.ForAll(i, count, j => !Contract.Result<IInterval<int>[]>()[i].Overlaps(Contract.Result<IInterval<int>[]>()[j]))));

            var intervals = new IInterval<int>[count];

            var low = 0;
            for (var i = 0; i < count; i++)
            {
                intervals[i] = new IntervalBase<int>(low, low + length, IntervalType.LowIncluded);
                low += length + space;
            }

            return intervals;
        }
    }
}
