using System;
using System.Linq;
using C5.intervals;
using C5.Tests.intervals;
using C5.Tests.intervals.LayeredContainmentList;
using NUnit.Framework;

namespace C5.Tests.intervals_new
{
    namespace IntervalBinarySearchTree
    {
        #region Black-box

        class IntervalBinarySearchTreeTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(IntervalBinarySearchTreeAvl<,>);
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return false;
            }
        }

        #endregion

        #region White-box

        // TODO: Hardcode examples from articles

        [TestFixture]
        class IntervalBinarySearchTreeTester_WhiteBox
        {
            #region Helpers
            // TODO: Builder pattern

            private static IntervalBinarySearchTreeAvl<IInterval<int>, int> _createIBS(params IInterval<int>[] intervals)
            {
                return new IntervalBinarySearchTreeAvl<IInterval<int>, int>(intervals);
            }

            private static IInterval<int>[] Ø { get { return new IInterval<int>[0]; } }
            private static IInterval<int> A { get { return new IntervalBase<int>(1, 3); } }
            private static IInterval<int>[] B { get { return new IInterval<int>[]
            { new IntervalBase<int>(1,3), new IntervalBase<int>(3,5), };}}
            private static IInterval<int>[] C { get { return new IInterval<int>[]
            { new IntervalBase<int>(1,3), new IntervalBase<int>(2,5), new IntervalBase<int>(3,5), };}}
            private static IInterval<int>[] D { get { return new IInterval<int>[]
            { new IntervalBase<int>(1,3), new IntervalBase<int>(1,5), new IntervalBase<int>(3,5), };}}
            private static IInterval<int>[] E { get { return new IInterval<int>[]
            { new IntervalBase<int>(1,3), new IntervalBase<int>(1,5) };}}

            #endregion

            #region Inner Classes

            #region Node

            #region Constructor

            // TODO: Test Node(T)

            #endregion

            #region Public Methods

            #region Intervals
            #endregion

            #region UpdateMaximumOverlap
            #endregion

            #region CompareTo

            // TODO: Null values?

            #endregion

            #region ToString
            #endregion

            #region Swap
            #endregion

            #endregion

            #endregion

            #region Interval Set

            #region Constructor

            #region Set
            #endregion

            #region Empty
            #endregion

            #endregion

            #region ToString
            #endregion

            #region Minus Operator
            #endregion

            #endregion

            #endregion

            #region Constructors

            #region Non-empty Constructor and Privates
            #endregion

            #region Empty
            #endregion

            #endregion

            #region Collection Value

            #region IsEmpty
            #endregion

            #region Count
            #endregion

            #region CountSpeed
            #endregion

            #region Choose
            #endregion

            #endregion

            #region Enumerable

            #region GetEnumerator and Privates
            #endregion

            #endregion

            #region Events
            #endregion

            #region Interval Collection

            #region Properties

            #region Span

            #endregion

            #region Maximum Overlap

            #endregion

            #region Allows Reference Duplicates
            #endregion

            #endregion

            #region Find Overlaps

            #region Stabbing

            [Test]
            public void FindOverlapsStabbing_WhileRootNotNull_Zero()
            {
                CollectionAssert.IsEmpty(_createIBS(Ø).FindOverlaps(1));
            }
            
            [Test]
            public void FindOverlapsStabbing_WhileRootNotNull_One()
            {
                CollectionAssert.AreEquivalent(new[]{A},_createIBS(A).FindOverlaps(A.Low));
            }

            [Test]
            public void FindOverlapsStabbing_WhileRootNotNull_Many1()
            {
                CollectionAssert.AreEquivalent(new[] { B.First() }, _createIBS(B).FindOverlaps(2));
            }
            
            [Test]
            public void FindOverlapsStabbing_WhileRootNotNull_Many2()
            {
                CollectionAssert.AreEquivalent(new[] { B.Last() }, _createIBS(B).FindOverlaps(4));
            }
            
            [Test]
            public void FindOverlapsStabbing_RootLessIsEmpty()
            {
                var emptyLess = _createIBS(B);
                var success = emptyLess.Remove(emptyLess.Last());
                CollectionAssert.AreEquivalent(Ø, emptyLess.FindOverlaps(4));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootLessMany()
            {
                var resultSet = _createIBS(C);
                resultSet.Remove(resultSet.First());
                CollectionAssert.AreEquivalent(resultSet, _createIBS(C).FindOverlaps(4));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootGreaterOne()
            {
                var resultSet = _createIBS(B);
                resultSet.Remove(resultSet.Last());
                CollectionAssert.AreEquivalent(resultSet, _createIBS(B).FindOverlaps(2));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootGreaterMany()
            {
                var resultSet = _createIBS(D);
                resultSet.Remove(resultSet.Last());
                CollectionAssert.AreEquivalent(resultSet, _createIBS(D).FindOverlaps(2));
            }

            [Test]
            public void FindOverlapsStabbing_ForeachIntervalInRootequalMany()
            {
                CollectionAssert.AreEquivalent(E, _createIBS(E).FindOverlaps(1));
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

        #endregion
    }
}
