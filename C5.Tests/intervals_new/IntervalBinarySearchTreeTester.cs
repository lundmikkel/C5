using System;
using C5.intervals;
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
