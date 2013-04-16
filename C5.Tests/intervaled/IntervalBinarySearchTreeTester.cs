using System.Collections.Generic;
using C5.Tests.intervaled.Generic;
using C5.intervaled;
using NUnit.Framework;

namespace C5.Tests.intervaled
{
    //using SequencedIntervalsOfInt = TreeBag<IInterval<int>>;
    using IntervalOfInt = IntervalBase<int>;

    namespace IntervalBinarySearchTree
    {

        [TestFixture]
        public class IntervalBinarySearchTreeIBS : Generic.IBS
        {
            internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new IntervalBinarySearchTree<int>(intervals);
            }

            [Test]
            public void MaximumOverlap_IBS_Returns5()
            {
                Assert.AreEqual(5, ((IntervalBinarySearchTree<int>) _intervaled).MaximumOverlap);
            }
        }

        [TestFixture]
        public class EndpointInclusion : IntervaledEndpointInclusion
        {
            internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new IntervalBinarySearchTree<int>(intervals);
            }
        }

        [TestFixture]
        public class NullCollection : IntervaledNullCollection
        {
            internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new IntervalBinarySearchTree<int>(intervals);
            }

            [Test]
            public void MaximumOverlap_EmptyCollection_Returns0()
            {
                Assert.AreEqual(0, ((IntervalBinarySearchTree<int>) _intervaled).MaximumOverlap);
            }
        }

        [TestFixture]
        public class EmptyCollection : IntervaledEmptyCollection
        {
            internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new IntervalBinarySearchTree<int>(intervals);
            }

            [Test]
            public void MaximumOverlap_EmptyCollection_Returns0()
            {
                Assert.AreEqual(0, ((IntervalBinarySearchTree<int>) _intervaled).MaximumOverlap);
            }
        }

        [TestFixture]
        public class Sample100 : Generic.Sample100
        {
            protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new IntervalBinarySearchTree<int>(intervals);
            }
        }

        [TestFixture]
        public class IBSPerformance : Generic.Performance23333
        {
            protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new IntervalBinarySearchTree<int>(intervals);
            }
        }

        [TestFixture]
        public class BensTest : Generic.BensTest
        {
            protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new IntervalBinarySearchTree<int>(intervals);
            }

            [Test]
            public void MaximumOverlap_BensCollection_Returns2()
            {
                Assert.AreEqual(2, ((IntervalBinarySearchTree<int>) _intervaled).MaximumOverlap);
            }
        }

        [TestFixture]
        public class MaximumOverlap
        {
            private IntervalBinarySearchTree<int> _intervaled;

            [SetUp]
            public void Init()
            {
                _intervaled = new IntervalBinarySearchTree<int>();
            }

            [Test]
            public void MaximumOverlap_EmptyCollection_ReturnZero()
            {
                Assert.AreEqual(0, _intervaled.MaximumOverlap);
            }

            [Test]
            public void MaximumOverlap_NonOverlappingIntervals_ReturnOne()
            {
                _intervaled.Add(new IntervalOfInt(1, 2));
                _intervaled.Add(new IntervalOfInt(2, 3));
                _intervaled.Add(new IntervalOfInt(3, 4));
                _intervaled.Add(new IntervalOfInt(4, 5));

                Assert.AreEqual(1, _intervaled.MaximumOverlap);
            }

            [Test]
            public void MaximumOverlap_MaximumOverlapBetweenDescreteValues_ReturnTwo()
            {
                _intervaled.Add(new IntervalOfInt(1, 3, false, false));
                _intervaled.Add(new IntervalOfInt(2, 4, false, false));

                Assert.AreEqual(2, _intervaled.MaximumOverlap);
            }
        }
    }
}
