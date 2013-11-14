using System;
using System.Collections.Generic;
using C5.Tests.intervals_new;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    using Interval = IntervalBase<int>;

    [TestFixture]
    abstract class IntervalCollectionTester
    {
        #region Meta

        private Random random = new Random(0);

        protected abstract Type GetCollectionType();

        /// <summary>
        /// Override to add additional parameters to constructors.
        /// </summary>
        /// <returns>An object array of the extra parameters.</returns>
        protected object[] AdditionalParameters()
        {
            return new object[0];
        }

        protected IIntervalCollection<IInterval<T>, T> Factory<T>(params IInterval<T>[] intervals) where T : IComparable<T>
        {
            var type = GetCollectionType();
            Type[] typeArgs = { typeof(IInterval<T>), typeof(T) };
            var genericType = type.MakeGenericType(typeArgs);

            var additionalParameters = AdditionalParameters();
            var parameters = new object[1 + additionalParameters.Length];
            parameters[0] = intervals;
            for (var i = 0; i < additionalParameters.Length; i++)
                parameters[i + 1] = additionalParameters[i];

            return (IIntervalCollection<IInterval<T>, T>) Activator.CreateInstance(genericType, parameters);
        }

        #endregion

        #region Test Methods

        [Test]
        public void Test()
        {
            var collection = Factory(
                new Interval(1, 2),
                new Interval(2, 3)
            );
            Assert.AreEqual(collection.Count, 2);

            var collection2 = Factory(new IntervalBase<bool>(true));
            Assert.AreEqual(collection2.Count, 1);
        }

        #region Properties

        #region Span

        [Test]
        [Category("Span")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Span_EmptyCollection_Exception()
        {
            var span = Factory(new Interval[0]).Span;
        }

        [Test]
        [Category("Span")]
        public void Span_SingleInterval_IntervalEqualsSpan()
        {
            var interval = IntervalTestHelper.RandomIntInterval();
            var span = Factory(interval).Span;
            Assert.True(span.IntervalEquals(interval));
        }

        [Test]
        [Category("Span")]
        public void Span_NonOverlappingIntervals_JoinedSpan()
        {
            IInterval<int> interval1, interval2;

            do
            {
                interval1 = IntervalTestHelper.RandomIntInterval();
                interval2 = IntervalTestHelper.RandomIntInterval();
            } while (interval1.Overlaps(interval2));

            var span = Factory(
                    interval1,
                    interval2
                ).Span;

            Assert.True(interval1.JoinedSpan(interval2).IntervalEquals(span));
        }

        [Test]
        [Category("Span")]
        public void Span_ContainedInterval_ContainerEqualsSpan()
        {
            IInterval<int> interval1, interval2;

            do
            {
                interval1 = IntervalTestHelper.RandomIntInterval();
                interval2 = IntervalTestHelper.RandomIntInterval();
            } while (!interval1.StrictlyContains(interval2));

            var span = Factory(
                    interval1,
                    interval2
                ).Span;

            Assert.True(interval1.IntervalEquals(span));
        }

        #endregion

        #region Maximum Overlap

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_EmptyCollection_Zero()
        {
            var mno = Factory(new Interval[0]).MaximumOverlap;
            Assert.AreEqual(mno, 0);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_SingleInterval_One()
        {
            var interval = IntervalTestHelper.RandomIntInterval();
            var mno = Factory(interval).MaximumOverlap;
            Assert.AreEqual(mno, 1);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_SingleReferenceObject_OneOrTwo()
        {
            var interval = IntervalTestHelper.RandomIntInterval();
            var coll = Factory(
                    interval,
                    interval
                );
            var mno = coll.MaximumOverlap;

            Assert.AreEqual(mno, coll.AllowsReferenceDuplicates ? 2 : 1);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_NonOverlappingIntervals_One()
        {
            var mno = Factory(
                IntervalTestHelper.NonOverlappingIntervals(random.Next(10, 20))
                ).MaximumOverlap;
            Assert.AreEqual(mno, 1);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_ManyOverlappingIntervals_Four()
        {
            // 0    5   10   15
            //             |
            //          (--]
            //          (--)
            //          |
            //        []
            //        |
            //      [---]
            //     |
            //    |
            //   |
            //  |
            // |
            // [--------------]

            var mno = Factory(
                    new Interval(12),
                    new Interval(9, 12, IntervalType.HighIncluded),
                    new Interval(9, 12, IntervalType.Open),
                    new Interval(9),
                    new Interval(7, 8, IntervalType.Closed),
                    new Interval(7),
                    new Interval(5, 9, IntervalType.Closed),
                    new Interval(4),
                    new Interval(3),
                    new Interval(2),
                    new Interval(1),
                    new Interval(0),
                    new Interval(0, 15, IntervalType.Closed)
                ).MaximumOverlap;

            Assert.AreEqual(mno, 4);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_AllContainedIntervals_Four()
        {
            // 0    5   10   15   20
            // 
            //          []
            //         [--]
            //        [----]
            //       [------]
            //      [--------]
            //     [----------]
            //    [------------]
            //   [--------------]
            //  [----------------]
            // [------------------]

            var coll = Factory(
                    new Interval(9, 10, IntervalType.Closed),
                    new Interval(8, 11, IntervalType.Closed),
                    new Interval(7, 12, IntervalType.Closed),
                    new Interval(6, 13, IntervalType.Closed),
                    new Interval(5, 14, IntervalType.Closed),
                    new Interval(4, 15, IntervalType.Closed),
                    new Interval(3, 16, IntervalType.Closed),
                    new Interval(2, 17, IntervalType.Closed),
                    new Interval(1, 18, IntervalType.Closed),
                    new Interval(0, 19, IntervalType.Closed)
                );

            Assert.AreEqual(coll.MaximumOverlap, coll.Count);
        }

        #endregion

        #region Allows Reference Duplicates
        #endregion

        #endregion

        #region Find Overlaps

        #region Stabbing

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_EmptyCollection_Empty()
        {
            var query = random.Next(Int32.MinValue, Int32.MaxValue);
            var coll = Factory(new Interval[0]);

            CollectionAssert.IsEmpty(coll.FindOverlaps(query));
        }

        #endregion

        #region Range
        #endregion

        #endregion

        #region Find Overlap

        #region Stabbing
        #endregion

        #region Range
        #endregion

        #endregion

        #region Count Overlaps

        #region Stabbing
        #endregion

        #region Range
        #endregion

        #endregion

        #region Extensible

        #region Is Read Only
        #endregion

        #region Add

        #region Events
        #endregion

        #endregion

        #region Add All

        #region Events
        #endregion

        #endregion

        #region Remove

        #region Events
        #endregion

        #endregion

        #region Clear

        #region Events
        #endregion

        #endregion

        #endregion

        #endregion
    }
}
