using System;
using C5.Tests.intervals;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals_new
{
    #region Black-box

    class IntervalBinarySearchTreeBlackBoxTester : IntervalCollectionTester
    {
        protected override Type GetCollectionType()
        {
            return typeof(IntervalBinarySearchTreeAvl<,>);
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
