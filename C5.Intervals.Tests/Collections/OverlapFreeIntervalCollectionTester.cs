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
    abstract class OverlapFreeIntervalCollectionTester : ContainmentFreeIntervalCollectionTester
    {
        /*
         * Things to check for for each method:
         *  - Empty collection                                 (EmptyCollection)
         *  - Single interval collection                       (SingleInterval)
         *  - Many intervals collection                        (ManyIntervals)
         */

        #region Meta

        protected new IOverlapFreeIntervalCollection<I, T> CreateCollection<I, T>(params I[] intervals)
            where I : class, IInterval<T>
            where T : IComparable<T>
        {
            return (IOverlapFreeIntervalCollection<I, T>)base.CreateCollection<I, T>(intervals);
        }

        protected new IOverlapFreeIntervalCollection<I, T> CreateEmptyCollection<I, T>()
            where I : class, IInterval<T>
            where T : IComparable<T>
        {
            return (IOverlapFreeIntervalCollection<I, T>)base.CreateEmptyCollection<I, T>();
        }

        #endregion

        // TODO: Test explicitly that the collection is finite - doesn't allow overlaps, reference equals, or containments

        #region Test Methods

        #region Neighbourhood

        #region Get Neighbourhood Stabbing

        [Test]

        [Category("Get Neighbourhood Stabbing")]
        public void GetNeighbourhoodStabbing_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            var neighbourhood = collection.GetNeighbourhood(randomInt());
            Assert.That(neighbourhood.IsEmpty);
        }

        [Test]

        [Category("Get Neighbourhood Stabbing"), AdditionalSeeds(307850483)]
        public void GetNeighbourhoodStabbing_SingleInterval_Mixed()
        {
            var interval = SingleInterval();
            var before = interval.Low - 1;
            var middle = interval.Low / 2 + interval.High / 2;
            var after = interval.High + 1;
            var collection = CreateCollection<Interval, int>(interval);

            var beforeNeighbourhood = collection.GetNeighbourhood(before);
            Assert.Null(beforeNeighbourhood.Previous);
            Assert.Null(beforeNeighbourhood.Overlap);
            Assert.That(ReferenceEquals(beforeNeighbourhood.Next, interval));

            var middleNeighbourhood = collection.GetNeighbourhood(middle);
            Assert.Null(middleNeighbourhood.Previous);
            Assert.That(ReferenceEquals(middleNeighbourhood.Overlap, interval));
            Assert.Null(middleNeighbourhood.Next);

            var afterNeighbourhood = collection.GetNeighbourhood(after);
            Assert.That(ReferenceEquals(afterNeighbourhood.Previous, interval));
            Assert.Null(afterNeighbourhood.Overlap);
            Assert.Null(afterNeighbourhood.Next);
        }

        [Test]

        [Category("Get Neighbourhood Stabbing"), AdditionalSeeds(-255514596)]
        public void GetNeighbourhoodStabbing_ManyIntervals_Mixed()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            // Lowest value
            var minNeighbourhood = collection.GetNeighbourhood(Int32.MinValue);
            Assert.Null(minNeighbourhood.Previous);
            Assert.Null(minNeighbourhood.Overlap);
            Assert.That(ReferenceEquals(minNeighbourhood.Next, collection.LowestInterval));

            // Highest value
            var maxNeighbourhood = collection.GetNeighbourhood(Int32.MaxValue);
            Assert.That(ReferenceEquals(maxNeighbourhood.Previous, collection.HighestInterval));
            Assert.Null(maxNeighbourhood.Overlap);
            Assert.Null(maxNeighbourhood.Next);

            foreach (var interval in intervals)
            {
                Assert.NotNull(collection.GetNeighbourhood(interval.Low));
                Assert.NotNull(collection.GetNeighbourhood(interval.High));
            }
        }

        #endregion

        #region Get Neighbourhood Interval

        [Test]

        [Category("Get Neighbourhood Interval")]
        public void GetNeighbourhoodInterval_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            var neighbourhood = collection.GetNeighbourhood(SingleInterval());
            Assert.That(neighbourhood.IsEmpty);
        }

        [Test]

        [Category("Get Neighbourhood Interval")]
        public void GetNeighbourhoodInterval_SingleInterval_Mixed()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);

            var neighbourhood = collection.GetNeighbourhood(interval);
            Assert.Null(neighbourhood.Previous);
            Assert.That(ReferenceEquals(neighbourhood.Overlap, interval));
            Assert.Null(neighbourhood.Next);

            Assert.That(collection.GetNeighbourhood(SingleInterval()).IsEmpty);
        }

        [Test]

        [Category("Get Neighbourhood Interval"), AdditionalSeeds(-255514596)]
        public void GetNeighbourhoodInterval_ManyIntervals_Mixed()
        {
            var intervals = ManyIntervals(maxLength: 10000);
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = NonOverlapping(intervals);

            foreach (var interval in expected)
                Assert.That(ReferenceEquals(collection.GetNeighbourhood(interval).Overlap, interval));

            Assert.That(collection.GetNeighbourhood(SingleInterval()).IsEmpty);
        }

        #endregion

        #endregion

        #endregion
    }
}
