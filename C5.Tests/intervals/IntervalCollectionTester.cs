using System;
using C5.Tests.intervals_new;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    using Interval = IntervalBase<int>;
    using ITH = IntervalTestHelper;

    [TestFixture]
    abstract class IntervalCollectionTester
    {
        #region Meta

        private readonly Random _random = new Random(0);

        private int randomInt()
        {
            return _random.Next(Int32.MinValue, Int32.MaxValue);
        }

        protected abstract Type GetCollectionType();

        /// <summary>
        /// Override to add additional parameters to constructors.
        /// </summary>
        /// <returns>An object array of the extra parameters.</returns>
        protected virtual object[] AdditionalParameters()
        {
            return new object[0];
        }

        protected IIntervalCollection<IInterval<T>, T> CreateCollection<T>(params IInterval<T>[] intervals) where T : IComparable<T>
        {
            var additionalParameters = AdditionalParameters();
            var parameters = new object[1 + additionalParameters.Length];
            parameters[0] = intervals;
            for (var i = 0; i < additionalParameters.Length; i++)
                parameters[i + 1] = additionalParameters[i];

            Type[] typeArgs = { typeof(IInterval<T>), typeof(T) };
            var genericType = GetCollectionType().MakeGenericType(typeArgs);
            return (IIntervalCollection<IInterval<T>, T>) Activator.CreateInstance(genericType, parameters);
        }

        protected IIntervalCollection<IInterval<T>, T> CreateEmptyCollection<T>() where T : IComparable<T>
        {
            return CreateCollection(new IInterval<T>[0]);
        }

        #endregion

        #region Test Methods

        #region Properties

        #region Span

        [Test]
        [Category("Span")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Span_EmptyCollection_Exception()
        {
            var span = CreateEmptyCollection<int>().Span;
        }

        [Test]
        [Category("Span")]
        public void Span_SingleInterval_IntervalEqualsSpan()
        {
            var interval = ITH.RandomIntInterval();
            var span = CreateCollection(interval).Span;
            Assert.True(span.IntervalEquals(interval));
        }

        [Test]
        [Category("Span")]
        public void Span_NonOverlappingIntervals_JoinedSpan()
        {
            IInterval<int> interval1, interval2;

            do
            {
                interval1 = ITH.RandomIntInterval();
                interval2 = ITH.RandomIntInterval();
            } while (interval1.Overlaps(interval2));

            var span = CreateCollection(
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
                interval1 = ITH.RandomIntInterval();
                interval2 = ITH.RandomIntInterval();
            } while (!interval1.StrictlyContains(interval2));

            var span = CreateCollection(
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
            var mno = CreateEmptyCollection<int>().MaximumOverlap;
            Assert.AreEqual(mno, 0);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_SingleInterval_One()
        {
            var interval = ITH.RandomIntInterval();
            var mno = CreateCollection(interval).MaximumOverlap;
            Assert.AreEqual(mno, 1);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_SingleReferenceObject_OneOrTwo()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateCollection(
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
            var mno = CreateCollection(
                ITH.NonOverlappingIntervals(_random.Next(10, 20))
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

            var mno = CreateCollection(
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

            var coll = CreateCollection(
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

        protected abstract bool AllowsReferenceDuplicates();

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_EmptyCollection_DefinedResult()
        {
            var coll = CreateEmptyCollection<int>();

            Assert.AreEqual(coll.AllowsReferenceDuplicates, AllowsReferenceDuplicates());
        }

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_SingleInterval_DefinedResult()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateCollection(interval);

            Assert.AreEqual(coll.AllowsReferenceDuplicates, AllowsReferenceDuplicates());
        }

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_ManyIntervals_DefinedResult()
        {
            var interval = ITH.RandomIntIntervals(_random.Next(10, 20));
            var coll = CreateCollection(interval);

            Assert.AreEqual(coll.AllowsReferenceDuplicates, AllowsReferenceDuplicates());
        }

        #endregion

        #endregion

        #region Find Overlaps

        #region Stabbing

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_EmptyCollection_Empty()
        {
            var query = randomInt();
            var coll = CreateEmptyCollection<int>();

            CollectionAssert.IsEmpty(coll.FindOverlaps(query));
        }

        #endregion

        #region Range

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_EmptyCollection_Empty()
        {
            var query = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            CollectionAssert.IsEmpty(coll.FindOverlaps(query));
        }

        #endregion

        #endregion

        #region Find Overlap

        #region Stabbing

        [Test]
        [Category("Find Overlap Stabbing")]
        public void FindOverlapStabbing_EmptyCollection_False()
        {
            var query = randomInt();
            IInterval<int> interval = null;
            var coll = CreateEmptyCollection<int>();

            Assert.False(coll.FindOverlap(query, ref interval));
            Assert.IsNull(interval);
        }

        #endregion

        #region Range

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_EmptyCollection_False()
        {
            var query = ITH.RandomIntInterval();
            IInterval<int> interval = null;
            var coll = CreateEmptyCollection<int>();

            Assert.False(coll.FindOverlap(query, ref interval));
            Assert.IsNull(interval);
        }

        #endregion

        #endregion

        #region Count Overlaps

        #region Stabbing

        [Test]
        [Category("Count Overlaps Stabbing")]
        public void CountOverlapsStabbing_EmptyCollection_Zero()
        {
            var query = randomInt();
            var coll = CreateEmptyCollection<int>();

            Assert.AreEqual(coll.CountOverlaps(query), 0);
        }

        #endregion

        #region Range

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_EmptyCollection_Zero()
        {
            var query = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            Assert.AreEqual(coll.CountOverlaps(query), 0);
        }

        #endregion

        #endregion

        #region Extensible

        #region Is Read Only

        #endregion

        #region Add

        [Test]
        [Category("Add")]
        public void Add_IsReadOnly_Exception()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            if (coll.IsReadOnly)
            {
                try
                {
                    coll.Add(interval);
                    Assert.Fail();
                }
                catch (ReadOnlyCollectionException)
                {
                    Assert.Pass();
                }
            }
        }

        [Test]
        [Category("Add")]
        public void Add_SingleReferenceObject_TrueFirst()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            if (!coll.IsReadOnly)
            {
                Assert.True(coll.Add(interval));
                Assert.True(coll.Add(interval) == coll.AllowsReferenceDuplicates);
            }
        }

        #region Events
        #endregion

        #endregion

        #region Add All

        [Test]
        [Category("Add All")]
        public void AddAll_IsReadOnly_Exception()
        {
            var intervals = new[] { ITH.RandomIntInterval() };
            var coll = CreateEmptyCollection<int>();

            if (coll.IsReadOnly)
            {
                try
                {
                    coll.AddAll(intervals);
                    Assert.Fail();
                }
                catch (ReadOnlyCollectionException)
                {
                    Assert.Pass();
                }
            }
        }

        #region Events
        #endregion

        #endregion

        #region Remove

        [Test]
        [Category("Remove")]
        public void Remove_IsReadOnly_Exception()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            if (coll.IsReadOnly)
            {
                try
                {
                    coll.Remove(interval);
                    Assert.Fail();
                }
                catch (ReadOnlyCollectionException)
                {
                    Assert.Pass();
                }
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_EmptyCollection_False()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            if (!coll.IsReadOnly)
            {
                Assert.False(coll.Remove(interval));
            }
        }

        #region Events
        #endregion

        #endregion

        #region Clear

        [Test]
        [Category("Clear")]
        public void Clear_IsReadOnly_Exception()
        {
            var coll = CreateEmptyCollection<int>();

            if (coll.IsReadOnly)
            {
                try
                {
                    coll.Clear();
                    Assert.Fail();
                }
                catch (ReadOnlyCollectionException)
                {
                    Assert.Pass();
                }
            }
        }

        [Test]
        [Category("Clear")]
        public void Clear_EmptyCollection_IsEmpty()
        {
            var coll = CreateEmptyCollection<int>();

            if (!coll.IsReadOnly)
            {
                coll.Clear();
                Assert.True(coll.IsEmpty);
            }
        }

        #region Events
        #endregion

        #endregion

        #endregion

        #endregion
    }
}
