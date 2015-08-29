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
    abstract class SortedIntervalCollectionTester : IntervalCollectionTester
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
        private static readonly IComparer comparer = new Comparer();

        private class Comparer : IComparer
        {
            private readonly IComparer<Interval> genericComparer = IntervalExtensions.CreateComparer<Interval, int>();

            public int Compare(object x, object y)
            {
                return genericComparer.Compare((Interval)x, (Interval)y);
            }
        }

        protected new ISortedIntervalCollection<I, T> CreateCollection<I, T>(params I[] intervals)
            where I : class, IInterval<T>
            where T : IComparable<T>
        {
            return (ISortedIntervalCollection<I, T>) base.CreateCollection<I, T>(intervals);
        }

        protected new ISortedIntervalCollection<I, T> CreateEmptyCollection<I, T>()
            where I : class, IInterval<T>
            where T : IComparable<T>
        {
            return (ISortedIntervalCollection<I, T>) base.CreateEmptyCollection<I, T>();
        }

        #endregion

        #region Test Methods

        #region Sorted

        [Test]
        [Category("Sorted")]
        public void Sorted_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            CollectionAssert.IsEmpty(collection.Sorted);
        }

        [Test]
        [Category("Sorted")]
        public void Sorted_SingleInterval_AreEqual()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            CollectionAssert.AreEqual(new[] { interval }, collection.Sorted);
            CollectionAssert.IsOrdered(collection.Sorted, comparer);
        }

        [Test]
        [Category("Sorted")]
        public void Sorted_SingleObject_AreEqual()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : new[] { intervals.First() };
            CollectionAssert.AreEqual(expected, collection.Sorted);
            CollectionAssert.IsOrdered(collection.Sorted, comparer);
        }

        [Test]
        [Category("Sorted")]
        public void Sorted_DuplicateIntervals_AreEqual()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : new[] { intervals.First() };
            CollectionAssert.AreEqual(expected, collection.Sorted);
            CollectionAssert.IsOrdered(collection.Sorted, comparer);
        }

        [Test]
        [Category("Sorted"), AdditionalSeeds(36342054, -1127807792)]
        public void Sorted_ManyIntervals_AreEqual()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = InsertedIntervals(collection, intervals).ToArray();
            Sorting.Timsort(expected, IntervalExtensions.CreateComparer<Interval, int>());

            CollectionAssert.AreEqual(expected, collection.Sorted);
            CollectionAssert.AllItemsAreUnique(collection.Sorted);
            CollectionAssert.IsOrdered(collection.Sorted, comparer);
        }

        [Test]
        [Category("Sorted")]
        public void Sorted_LCListTrickyCase_Sorted()
        {
            var collection = CreateCollection<Interval, int>(
                new Interval(0, 8),
                new Interval(1, 7),
                new Interval(2, 3),
                new Interval(4, 9),
                new Interval(5, 6)
            );
            CollectionAssert.IsOrdered(collection.Sorted, comparer);
        }

        #endregion
        
        #endregion
    }
}
