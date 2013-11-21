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

        [TestFixture]
        class IntervalBinarySearchTreeTester_WhiteBox
        {
            [SetUp]
            public void SetUp()
            {

            }

            #region Interval Collection

            #region Properties

            #region Span

            [Test]
            public void Test()
            {

            }

            #endregion

            #endregion

            #endregion

        }

        #endregion
    }
}
