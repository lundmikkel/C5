using System;
using C5.Tests.intervals;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals_new.IntervalBinarySearchTree
{
    #region Black-box

    class IntervalBinarySearchTreeTester_BlackBox : IntervalCollectionTester
    {
        protected override Type GetCollectionType()
        {
            return typeof(IntervalBinarySearchTreeAvl<,>);
        }

        protected override bool AllowsReferenceDuplicates()
        {
            return false;
        }
    }

    #endregion

    #region White-box

    [TestFixture]
    class IntervalBinarySearchTree
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void Test()
        {

        }
    }

    #endregion
}
