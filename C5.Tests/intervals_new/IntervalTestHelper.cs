using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using C5.Tests.intervals.DynamicIntervalTree;
using C5.intervals;

namespace C5.Tests.intervals_new
{
    class IntervalTestHelper
    {
        private static Random random = new Random(0);

        public static IInterval<int> RandomIntInterval()
        {
            var low = random.Next(Int32.MinValue, Int32.MaxValue);
            var high = random.Next(low + 1, Int32.MaxValue);

            return new IntervalBase<int>(low, high, (IntervalType) random.Next(0, 4));
        }

        public static IInterval<int> RandomIntPoint()
        {
            return new IntervalBase<int>(random.Next(Int32.MinValue, Int32.MaxValue));
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
