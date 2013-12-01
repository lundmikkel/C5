using System;
using System.Linq;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals_new
{
    namespace DynamicIntervalTree
    {
        #region Black-box

        abstract class DynamicIntervalTreeTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(DynamicIntervalTree<,>);
            }

            // DIT's standard behavior where we set the ReferenceDuplicates to false
            protected override object[] AdditionalParameters()
            {
                return new object[] { AllowsReferenceDuplicates() };
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }
        }

        class DynamicIntervalTreeTester_BlackBox_ReferenceDuplicatesFalse : DynamicIntervalTreeTester_BlackBox
        {
            protected override bool AllowsReferenceDuplicates()
            {
                return false;
            }
        }

        class DynamicIntervalTreeTester_BlackBox_ReferenceDuplicatesTrue : DynamicIntervalTreeTester_BlackBox
        {
            protected override bool AllowsReferenceDuplicates()
            {
                return true;
            }
        }

        #endregion

        #region White-box

        // TODO: Hardcode examples from articles

        [TestFixture]
        class DynamicIntervalTreeTester
        {
            [SetUp]
            public void SetUp()
            {

            }

            [Test]
            public void Test() { }
        }

        [TestFixture]
        [Category("Former Bug")]
        class FormerBugs
        {
            [Test]
            public void Add_LowInsertedNodeButDidNotRotate_UnbalancedTree()
            {
                new DynamicIntervalTree<IInterval<int>, int>
                    {
                        new IntervalBase<int>(3, 5),
                        new IntervalBase<int>(1, 2),
                        new IntervalBase<int>(0, 4),
                        new IntervalBase<int>(-1, 5)
                    };
                Assert.Pass();
            }

            [Test]
            public void Remove_RotationDuringRemoveHighCausedUnupdatedSpan()
            {
                var interval1 = new IntervalBase<int>(6, 8);
                var interval2 = new IntervalBase<int>(3, 6);

                var collection = new DynamicIntervalTree<IInterval<int>, int>
                    {
                        interval1,
                        new IntervalBase<int>(4, 5),
                        new IntervalBase<int>(2, 5),
                        new IntervalBase<int>(7, 8),
                        new IntervalBase<int>(1, 4),
                        interval2
                    };

                collection.Remove(interval1);
                collection.Remove(interval2);
            }


            [Test]
            public void Add_SortedQueryingOutsideSpan()
            {
                var count = (int) Math.Pow(2.0, 5.0);
                var intervals = new IntervalBase<int>[count];

                for (var i = 0; i < count; i++)
                    intervals[i] = new IntervalBase<int>(i * 2, i * 2 + 1);

                var collection = new DynamicIntervalTree<IInterval<int>, int>();

                foreach (var interval in intervals)
                    collection.Add(interval);

                Console.Out.WriteLine(collection.QuickGraph);

                Assert.AreEqual(0, collection.FindOverlaps(-1).Count());
                Assert.AreEqual(0, collection.FindOverlaps(count * 2 + 2).Count());
            }
        }

        #endregion
    }
}
