using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace C5.Intervals.Tests
{
    // ReSharper disable RedundantArgumentNameForLiteralExpression
    // ReSharper disable RedundantArgumentDefaultValue

    [TestFixture]
    abstract class ContainmentFreeIntervalCollectionTester : IntervalCollectionTester
    {
        /*
         * Things to check for for each method:
         *  - Empty collection                                 (EmptyCollection)
         *  - Single interval collection                       (SingleInterval)
         *  - Many intervals collection - all same object      (SingleObject)
         *  - Many intervals collection - all same interval    (DuplicateIntervals)
         *  - Many intervals collection                        (ManyIntervals)
         */

        #region Meta

        // TODO: Fix when NUnit uses generic comparers
        static readonly IComparer comparer = new Comparer();
        static readonly IComparer backwardsComparer = new BackwardsComparer();

        private class Comparer : IComparer
        {
            private readonly IComparer<Interval> genericComparer = IntervalExtensions.CreateComparer<Interval, int>();

            public int Compare(object x, object y)
            {
                return genericComparer.Compare((Interval)x, (Interval)y);
            }
        }

        private class BackwardsComparer : IComparer
        {
            private readonly IComparer<Interval> genericComparer = IntervalExtensions.CreateReversedComparer<Interval, int>();

            public int Compare(object x, object y)
            {
                return genericComparer.Compare((Interval)x, (Interval)y);
            }
        }

        // TODO: Change interface once this is sorted out
        protected new IOverlapFreeIntervalCollection<I, T> CreateCollection<I, T>(params I[] intervals)
            where I : class, IInterval<T>
            where T : IComparable<T>
        {
            return (IOverlapFreeIntervalCollection<I, T>)base.CreateCollection<I, T>(intervals);
        }

        // TODO: Change interface once this is sorted out
        protected new IOverlapFreeIntervalCollection<I, T> CreateEmptyCollection<I, T>()
            where I : class, IInterval<T>
            where T : IComparable<T>
        {
            return (IOverlapFreeIntervalCollection<I, T>)base.CreateEmptyCollection<I, T>();
        }

        #endregion

        #region Test Methods

        #region Sorted

        [Test]
        [Category("Sorted")]
        public void Sorted_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            CollectionAssert.IsEmpty(collection.Sorted());
        }

        [Test]
        [Category("Sorted")]
        public void Sorted_SingleInterval_AreEqual()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            CollectionAssert.AreEqual(new[] { interval }, collection.Sorted());
            CollectionAssert.IsOrdered(collection.Sorted(), comparer);
        }

        [Test]
        [Category("Sorted")]
        public void Sorted_SingleObject_AreEqual()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : new[] { intervals.First() };
            CollectionAssert.AreEqual(expected, collection.Sorted());
            CollectionAssert.IsOrdered(collection.Sorted(), comparer);
        }

        [Test]
        [Category("Sorted")]
        public void Sorted_DuplicateIntervals_AreEqual()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : new[] { intervals.First() };
            CollectionAssert.AreEqual(expected, collection.Sorted());
            CollectionAssert.IsOrdered(collection.Sorted(), comparer);
        }

        [Test]
        [Category("Sorted"), AdditionalSeeds(36342054, -1127807792)]
        public void Sorted_ManyIntervals_AreEqual()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : NonOverlapping(intervals);
            Sorting.Timsort(expected, IntervalExtensions.CreateComparer<Interval, int>());
            CollectionAssert.AreEqual(expected, collection.Sorted());
            CollectionAssert.AllItemsAreUnique(collection.Sorted());
            CollectionAssert.IsOrdered(collection.Sorted(), comparer);
        }

        // TODO: Move to sorted tester
        // [Test]
        // [Category("Sorted")]
        // public void Sorted_LCListTrickyCase_Sorted()
        // {
        //     var collection = CreateCollection<Interval, int>(
        //         new Interval(0, 8),
        //         new Interval(1, 7),
        //         new Interval(2, 3),
        //         new Interval(4, 9),
        //         new Interval(5, 6)
        //     );
        //     CollectionAssert.IsOrdered(collection.Sorted(), comparer);
        // }

        #endregion

        #region Sorted Backwards

        [Test]
        [Category("Sorted Backwards")]
        public void SortedBackwards_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            CollectionAssert.IsEmpty(collection.SortedBackwards());
        }

        [Test]
        [Category("Sorted Backwards")]
        public void SortedBackwards_SingleInterval_AreEqual()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            CollectionAssert.AreEqual(new[] { interval }, collection.SortedBackwards());
            CollectionAssert.IsOrdered(collection.SortedBackwards(), backwardsComparer);
        }

        [Test]
        [Category("Sorted Backwards")]
        public void SortedBackwards_SingleObject_AreEqual()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : new[] { intervals.First() };
            CollectionAssert.AreEqual(expected, collection.SortedBackwards());
            CollectionAssert.IsOrdered(collection.SortedBackwards(), backwardsComparer);
        }

        [Test]
        [Category("Sorted Backwards")]
        public void SortedBackwards_DuplicateIntervals_AreEqual()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals.Reverse() : new[] { intervals.First() };
            CollectionAssert.AreEqual(expected, collection.SortedBackwards());
            CollectionAssert.IsOrdered(collection.SortedBackwards(), backwardsComparer);
        }

        [Test]
        [Category("Sorted Backwards"), AdditionalSeeds(36342054, -1127807792)]
        public void SortedBackwards_ManyIntervals_AreEqual()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var array = collection.AllowsOverlaps ? intervals : NonOverlapping(intervals);
            Sorting.Timsort(array, IntervalExtensions.CreateComparer<Interval, int>());
            var expected = array.Reverse();
            CollectionAssert.AreEqual(expected, collection.SortedBackwards());
            CollectionAssert.AllItemsAreUnique(collection.SortedBackwards());
            CollectionAssert.IsOrdered(collection.SortedBackwards(), backwardsComparer);
        }

        #endregion

        // TODO
        #region Enumerable

        #region Enumerate From Point

        [Test]
        [Category("Enumerate From Point")]
        public void EnumerateFromPoint_InvalidInput_ThrowsContractException()
        {
            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<IInterval<string>, string>();
                collection.EnumerateFrom(point: null, includeOverlaps: true);
            }));

            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<IInterval<string>, string>();
                collection.EnumerateFrom(point: null, includeOverlaps: false);
            }));
        }

        [Test]
        [Category("Enumerate From Point")]
        public void EnumerateFromPoint_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            CollectionAssert.IsEmpty(collection.EnumerateFrom(randomInt(), true));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(randomInt(), false));
        }

        [Test]
        [Category("Enumerate From Point")]
        public void EnumerateFromPoint_SingleInterval_Mixed([Range(0, 4)] int type)
        {
            var interval = SingleInterval(type: type);
            var collection = CreateCollection<Interval, int>(interval);
            var expected = new[] { interval };

            if (interval.LowIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.Low, true));
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.Low, false));
            }
            else
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.Low, true));
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.Low, false));
            }

            if (interval.HighIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.High, true));
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.High, false));
            }
            else
            {
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.High, true));
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.High, false));
            }
        }

        [Test]
        [Category("Enumerate From Point")]
        public void EnumerateFromPoint_SingleObject_Mixed([Range(0, 4)] int type)
        {
            var intervals = SingleObject(type);
            var interval = intervals.First();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : intervals.Take(1).ToArray();

            if (interval.LowIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.Low, true));
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.Low, false));
            }
            else
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.Low, true));
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.Low, false));
            }

            if (interval.HighIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.High, true));
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.High, false));
            }
            else
            {
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.High, true));
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.High, false));
            }
        }

        [Test]
        [Category("Enumerate From Point")]
        public void EnumerateFromPoint_DuplicateIntervals_Mixed([Range(0, 4)] int type)
        {
            var intervals = DuplicateIntervals(type);
            var interval = intervals.First();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : intervals.Take(1).ToArray();

            if (interval.LowIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.Low, true));
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.Low, false));
            }
            else
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.Low, true));
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.Low, false));
            }

            if (interval.HighIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval.High, true));
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.High, false));
            }
            else
            {
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.High, true));
                CollectionAssert.IsEmpty(collection.EnumerateFrom(interval.High, false));
            }
        }

        [Test]
        [Category("Enumerate From Point")]
        public void EnumerateFromPoint_ManyIntervals_Mixed()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            intervals = collection.AllowsOverlaps ? intervals : NonOverlapping(intervals);
            foreach (var interval in intervals)
            {
                Assert.NotNull(collection.EnumerateFrom(interval.Low, true));
                Assert.NotNull(collection.EnumerateFrom(interval.Low, false));
                Assert.NotNull(collection.EnumerateFrom(interval.High, true));
                Assert.NotNull(collection.EnumerateFrom(interval.High, false));
            }
        }

        [Test]
        [Category("Enumerate From Point")]
        public void EnumerateFromPoint_ManyIntervalsMinValue_Mixed()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            CollectionAssert.AreEqual(collection.Sorted(), collection.EnumerateFrom(Int32.MinValue));
        }

        [Test]
        [Category("Enumerate From Point")]
        [TestCase(0, true, 2)]
        [TestCase(0, false, 2)]
        [TestCase(1, true, 2)]
        [TestCase(1, false, 1)]
        [TestCase(2, true, 2)]
        [TestCase(2, false, 1)]
        [TestCase(3, true, 2)]
        [TestCase(3, false, 1)]
        [TestCase(4, true, 1)]
        [TestCase(4, false, 1)]
        [TestCase(5, true, 1)]
        [TestCase(5, false, 1)]
        [TestCase(6, true, 1)]
        [TestCase(6, false, 0)]
        [TestCase(7, true, 0)]
        [TestCase(7, false, 0)]
        [TestCase(8, true, 0)]
        [TestCase(8, false, 0)]
        public void EnumerateFromPoint_FixedExample_Mixed(int point, bool includeOverlaps, int count)
        {
            #region Timeline
            //
            //      [---------]         (---------)     
            //
            // |----|----|----|----|----|----|----|----|
            // 0    1    2    3    4    5    6    7    8
            #endregion

            var collection = CreateCollection<Interval, int>(
                new Interval(1, 3, IntervalType.Closed),
                new Interval(5, 7, IntervalType.Open)
            );
            Assert.AreEqual(count, collection.EnumerateFrom(point, includeOverlaps).Count());
        }

        #endregion

        #region Enumerate Backwards From Point

        [Test]
        [Category("Enumerate Backwards From Point")]
        public void EnumerateBackwardsFromPoint_InvalidInput_ThrowsContractException()
        {
            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<IInterval<string>, string>();
                collection.EnumerateBackwardsFrom(point: null, includeOverlaps: true);
            }));

            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<IInterval<string>, string>();
                collection.EnumerateBackwardsFrom(point: null, includeOverlaps: false);
            }));
        }

        [Test]
        [Category("Enumerate Backwards From Point")]
        public void EnumerateBackwardsFromPoint_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(randomInt(), true));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(randomInt(), false));
        }

        [Test]
        [Category("Enumerate Backwards From Point")]
        public void EnumerateBackwardsFromPoint_SingleInterval_Mixed([Range(0, 4)] int type)
        {
            var interval = SingleInterval(type: type);
            var collection = CreateCollection<Interval, int>(interval);
            var expected = new[] { interval };

            if (interval.LowIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.Low, true));
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.Low, false));
            }
            else
            {
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.Low, true));
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.Low, false));
            }

            if (interval.HighIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.High, true));
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.High, false));
            }
            else
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.High, true));
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.High, false));
            }
        }

        [Test]
        [Category("Enumerate Backwards From Point")]
        public void EnumerateBackwardsFromPoint_SingleObject_Mixed([Range(0, 4)] int type)
        {
            var intervals = SingleObject(type);
            var interval = intervals.First();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : intervals.Take(1).ToArray();

            if (interval.LowIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.Low, true));
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.Low, false));
            }
            else
            {
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.Low, true));
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.Low, false));
            }

            if (interval.HighIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.High, true));
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.High, false));
            }
            else
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.High, true));
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.High, false));
            }
        }

        [Test]
        [Category("Enumerate Backwards From Point")]
        public void EnumerateBackwardsFromPoint_DuplicateIntervals_Mixed([Range(0, 4)] int type)
        {
            var intervals = DuplicateIntervals(type);
            var interval = intervals.First();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : intervals.Take(1).ToArray();

            if (interval.LowIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.Low, true));
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.Low, false));
            }
            else
            {
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.Low, true));
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.Low, false));
            }

            if (interval.HighIncluded)
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.High, true));
                CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval.High, false));
            }
            else
            {
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.High, true));
                CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval.High, false));
            }
        }

        [Test]
        [Category("Enumerate Backwards From Point")]
        public void EnumerateBackwardsFromPoint_ManyIntervals_Mixed()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            intervals = collection.AllowsOverlaps ? intervals : NonOverlapping(intervals);
            foreach (var interval in intervals)
            {
                Assert.NotNull(collection.EnumerateBackwardsFrom(interval.Low, true));
                Assert.NotNull(collection.EnumerateBackwardsFrom(interval.Low, false));
                Assert.NotNull(collection.EnumerateBackwardsFrom(interval.High, true));
                Assert.NotNull(collection.EnumerateBackwardsFrom(interval.High, false));
            }
        }

        [Test]
        [Category("Enumerate Backwards From Point")]
        public void EnumerateBackwardsFromPoint_ManyIntervalsMaxValue_Mixed()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            CollectionAssert.AreEqual(collection.SortedBackwards(), collection.EnumerateBackwardsFrom(Int32.MaxValue));
        }

        [Test]
        [Category("Enumerate Backwards From Point")]
        [TestCase(0, true, 0)]
        [TestCase(0, false, 0)]
        [TestCase(1, true, 1)]
        [TestCase(1, false, 0)]
        [TestCase(2, true, 1)]
        [TestCase(2, false, 0)]
        [TestCase(3, true, 1)]
        [TestCase(3, false, 0)]
        [TestCase(4, true, 1)]
        [TestCase(4, false, 1)]
        [TestCase(5, true, 1)]
        [TestCase(5, false, 1)]
        [TestCase(6, true, 2)]
        [TestCase(6, false, 1)]
        [TestCase(7, true, 2)]
        [TestCase(7, false, 2)]
        [TestCase(8, true, 2)]
        [TestCase(8, false, 2)]
        public void EnumerateBackwardsFromPoint_FixedExample_Mixed(int point, bool includeOverlaps, int count)
        {
            #region Timeline
            //
            //      [---------]         (---------)     
            //
            // |----|----|----|----|----|----|----|----|
            // 0    1    2    3    4    5    6    7    8
            #endregion

            var collection = CreateCollection<Interval, int>(
                new Interval(1, 3, IntervalType.Closed),
                new Interval(5, 7, IntervalType.Open)
            );
            Assert.AreEqual(count, collection.EnumerateBackwardsFrom(point, includeOverlaps).Count());
        }

        #endregion

        #region Enumerate From Interval

        [Test]
        [Category("Enumerate From Interval")]
        public void EnumerateFromInterval_InvalidInput_ThrowsContractException()
        {
            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<Interval, int>();
                collection.EnumerateFrom(interval: null, includeInterval: true);
            }));

            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<Interval, int>();
                collection.EnumerateFrom(interval: null, includeInterval: false);
            }));
        }

        [Test]
        [Category("Enumerate From Interval")]
        public void EnumerateFromInterval_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), false));
        }

        [Test]
        [Category("Enumerate From Interval")]
        public void EnumerateFromInterval_SingleInterval_Mixed()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            CollectionAssert.AreEqual(new[] { interval }, collection.EnumerateFrom(interval, true));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(interval, false));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), false));
        }

        [Test]
        [Category("Enumerate From Interval")]
        public void EnumerateFromInterval_SingleObject_Mixed()
        {
            var intervals = SingleObject();
            var interval = intervals.First();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : intervals.Take(1);
            CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval, true));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(interval, false));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), false));
        }

        [Test]
        [Category("Enumerate From Interval")]
        public void EnumerateFromInterval_DuplicateIntervals_Mixed()
        {
            var intervals = DuplicateIntervals();
            var interval = intervals.First();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : intervals.Take(1).ToArray();
            CollectionAssert.AreEqual(expected, collection.EnumerateFrom(interval, true));
            CollectionAssert.AreEqual(expected.Skip(1), collection.EnumerateFrom(interval, false));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), false));
        }

        [Test]
        [Category("Enumerate From Interval")]
        public void EnumerateFromInterval_ManyIntervals_Mixed()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            intervals = collection.AllowsOverlaps ? intervals : NonOverlapping(intervals);
            Sorting.Timsort(intervals, IntervalExtensions.CreateComparer<Interval, int>());
            var counter = 0;
            foreach (var interval in intervals)
            {
                CollectionAssert.AreEqual(intervals.Skip(counter), collection.EnumerateFrom(interval, true));
                CollectionAssert.AreEqual(intervals.Skip(counter + 1), collection.EnumerateFrom(interval, false));
                ++counter;
            }

            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateFrom(SingleInterval(), false));
        }

        [Test]
        public void EnumerateFromInterval_ManyIntervalsEnumerateFromLastIntervalExcluded_Empty()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var interval = collection.Sorted().Last();
            CollectionAssert.IsEmpty(collection.EnumerateFrom(interval, false));
        }

        #endregion

        #region Enumerate Backwards From Interval

        [Test]
        [Category("Enumerate Backwards From Interval")]
        public void EnumerateBackwardsFromInterval_InvalidInput_ThrowsContractException()
        {
            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<Interval, int>();
                collection.EnumerateBackwardsFrom(interval: null, includeInterval: true);
            }));

            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<Interval, int>();
                collection.EnumerateBackwardsFrom(interval: null, includeInterval: false);
            }));
        }

        [Test]
        [Category("Enumerate Backwards From Interval")]
        public void EnumerateBackwardsFromInterval_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), false));
        }

        [Test]
        [Category("Enumerate Backwards From Interval")]
        public void EnumerateBackwardsFromInterval_SingleInterval_Mixed()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            CollectionAssert.AreEqual(new[] { interval }, collection.EnumerateBackwardsFrom(interval, true));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval, false));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), false));
        }

        [Test]
        [Category("Enumerate Backwards From Interval")]
        public void EnumerateBackwardsFromInterval_SingleObject_Mixed()
        {
            var intervals = SingleObject();
            var interval = intervals.First();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : intervals.Take(1);
            CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval, true));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval, false));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), false));
        }

        [Test]
        [Category("Enumerate Backwards From Interval")]
        public void EnumerateBackwardsFromInterval_DuplicateIntervals_Mixed()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals.Reverse().ToArray() : intervals.Take(1).ToArray();
            var interval = expected.First();
            CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFrom(interval, true));
            CollectionAssert.AreEqual(expected.Skip(1), collection.EnumerateBackwardsFrom(interval, false));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), false));
        }

        [Test]
        [Category("Enumerate Backwards From Interval")]
        public void EnumerateBackwardsFromInterval_ManyIntervals_Mixed()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            intervals = collection.AllowsOverlaps ? intervals : NonOverlapping(intervals);
            Sorting.Timsort(intervals, IntervalExtensions.CreateComparer<Interval, int>());
            var expected = intervals.Reverse().ToArray();
            var counter = 0;
            foreach (var interval in expected)
            {
                CollectionAssert.AreEqual(expected.Skip(counter), collection.EnumerateBackwardsFrom(interval, true));
                CollectionAssert.AreEqual(expected.Skip(counter + 1), collection.EnumerateBackwardsFrom(interval, false));
                ++counter;
            }

            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), true));
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(SingleInterval(), false));
        }

        [Test]
        public void EnumerateBackwardsFromInterval_ManyIntervalsEnumerateBackwardsFromLastIntervalExcluded_Empty()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var interval = collection.SortedBackwards().Last();
            CollectionAssert.IsEmpty(collection.EnumerateBackwardsFrom(interval, false));
        }

        #endregion

        #region Enumerate From Index

        [Test]
        [Category("Enumerate From Index")]
        public void EnumerateFromIndex_InvalidInput_ThrowsContractException()
        {
            AssertThrowsContractException((() =>
            {
                var intervals = ManyIntervals();
                var collection = CreateCollection<Interval, int>(intervals);
                collection.EnumerateFromIndex(-1);
            }));

            AssertThrowsContractException((() =>
            {
                var intervals = ManyIntervals();
                var collection = CreateCollection<Interval, int>(intervals);
                collection.EnumerateFromIndex(intervals.Length);
            }));
        }

        [Test]
        [Category("Enumerate From Index")]
        public void EnumerateFromIndex_EmptyCollection_ThrowsContractException()
        {
            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<Interval, int>();
                collection.EnumerateFromIndex(0);
            }));
        }

        [Test]
        [Category("Enumerate From Index")]
        public void EnumerateFromIndex_SingleInterval_Mixed()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            var expected = new[] { interval };

            CollectionAssert.AreEqual(expected, collection.EnumerateFromIndex(0));
        }

        [Test]
        [Category("Enumerate From Index")]
        public void EnumerateFromIndex_SingleObject_Mixed()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : intervals.Take(1).ToArray();

            for (var i = 0; i < expected.Count(); ++i)
                CollectionAssert.AreEqual(expected.Skip(i), collection.EnumerateFromIndex(i));
        }

        [Test]
        [Category("Enumerate From Index")]
        public void EnumerateFromIndex_DuplicateIntervals_Mixed()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : intervals.Take(1).ToArray();

            for (var i = 0; i < expected.Count(); ++i)
                CollectionAssert.AreEqual(expected.Skip(i), collection.EnumerateFromIndex(i));
        }

        [Test]
        [Category("Enumerate From Index")]
        public void EnumerateFromIndex_ManyIntervals_Mixed()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : NonOverlapping(intervals);
            Sorting.Timsort(expected, IntervalExtensions.CreateComparer<Interval, int>());
            for (var i = 0; i < expected.Count(); ++i)
                CollectionAssert.AreEqual(expected.Skip(i), collection.EnumerateFromIndex(i));
        }

        #endregion

        #region Enumerate Backwards From Index

        [Test]
        [Category("Enumerate Backwards From Index")]
        public void EnumerateBackwardsFromIndex_InvalidInput_ThrowsContractException()
        {
            AssertThrowsContractException((() =>
            {
                var intervals = ManyIntervals();
                var collection = CreateCollection<Interval, int>(intervals);
                collection.EnumerateBackwardsFromIndex(-1);
            }));

            AssertThrowsContractException((() =>
            {
                var intervals = ManyIntervals();
                var collection = CreateCollection<Interval, int>(intervals);
                collection.EnumerateBackwardsFromIndex(intervals.Length);
            }));
        }

        [Test]
        [Category("Enumerate Backwards From Index")]
        public void EnumerateBackwardsFromIndex_EmptyCollection_ThrowsContractException()
        {
            AssertThrowsContractException((() =>
            {
                var collection = CreateEmptyCollection<Interval, int>();
                collection.EnumerateBackwardsFromIndex(0);
            }));
        }

        [Test]
        [Category("Enumerate Backwards From Index")]
        public void EnumerateBackwardsFromIndex_SingleInterval_Mixed()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            var expected = new[] { interval };

            CollectionAssert.AreEqual(expected, collection.EnumerateBackwardsFromIndex(0));
        }

        [Test]
        [Category("Enumerate Backwards From Index")]
        public void EnumerateBackwardsFromIndex_SingleObject_Mixed()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : intervals.Take(1).ToArray();

            for (var i = 0; i < expected.Count(); ++i)
                CollectionAssert.AreEqual(expected.Take(i + 1).Reverse(), collection.EnumerateBackwardsFromIndex(i));
        }

        [Test]
        [Category("Enumerate Backwards From Index")]
        public void EnumerateBackwardsFromIndex_DuplicateIntervals_Mixed()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : intervals.Take(1).ToArray();

            for (var i = 0; i < expected.Count(); ++i)
                CollectionAssert.AreEqual(expected.Take(i + 1).Reverse(), collection.EnumerateBackwardsFromIndex(i));
        }

        [Test]
        [Category("Enumerate Backwards From Index")]
        public void EnumerateBackwardsFromIndex_ManyIntervals_Mixed()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : NonOverlapping(intervals);
            Sorting.Timsort(expected, IntervalExtensions.CreateComparer<Interval, int>());
            for (var i = 0; i < expected.Count(); ++i)
                CollectionAssert.AreEqual(expected.Take(i + 1).Reverse(), collection.EnumerateBackwardsFromIndex(i));
        }

        #endregion

        #endregion

        // TODO: Write proper tests
        #region Indexed Access

        [Test]
        [Category("Indexer")]
        public void Indexer_ManyIntervals_NotNullAndMatchesIndexOf()
        {
            var intervals = NonOverlapping(ManyIntervals(maxLength: 10000));
            if (intervals.Length % 2 == 0)
                Array.Resize(ref intervals, intervals.Length - 1);

            Sorting.Timsort(intervals, IntervalExtensions.CreateComparer<Interval, int>());

            var notInCollection = intervals.Where((item, i) => i % 2 == 0).ToArray();
            var inCollection = intervals.Where((item, i) => i % 2 == 1).ToArray();

            var collection = CreateCollection<Interval, int>(inCollection);

            for (var i = 0; i < collection.Count; ++i)
            {
                var interval = collection[i];
                Assert.NotNull(interval);
                var j = collection.IndexOf(interval);
                Assert.AreEqual(i, j);
            }

            for (var i = 0; i < notInCollection.Length; ++i)
            {
                var interval = notInCollection[i];
                var j = collection.IndexOf(interval);
                Assert.That(j < 0);
                Assert.AreEqual(i, ~j);

                collection.Add(interval);
                var k = collection.IndexOf(interval);
                Assert.AreEqual(k, ~j);
                collection.Remove(interval);
            }
        }

        [Test]
        [Category("EnumerateFromIndex")]
        public void EnumerateFromIndex_ManyIntervals_MatchingInterval()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            intervals = collection.AllowsOverlaps ? intervals : NonOverlapping(intervals);
            Sorting.Timsort(intervals, IntervalExtensions.CreateComparer<Interval, int>());
            var expected = intervals;
            for (var i = 0; i < collection.Count; ++i)
            {
                CollectionAssert.AreEqual(expected.Skip(i), collection.EnumerateFromIndex(i));
            }
        }

        #endregion

        #endregion
    }

    internal class AdditionalSeedsAttribute : Attribute
    {
        public AdditionalSeedsAttribute(params int[] seeds)
        {
            //throw new NotImplementedException();
        }
    }
}
