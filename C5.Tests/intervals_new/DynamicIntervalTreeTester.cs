using System;
using C5.Tests.intervals;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals_new
{
    #region Black-box

    class DynamicIntervalTreeBlackBoxReferenceDuplicatesFalseTester : IntervalCollectionTester
    {
        protected override Type GetCollectionType()
        {
            return typeof(DynamicIntervalTree<,>);
        }

        // DIT's standard behavior where we set the ReferenceDuplicates to false
        protected new object[] AdditionalParameters()
        {
            return new object[] { false };
        }
    }

    class DynamicIntervalTreeBlackBoxReferenceDuplicatesTrueTester : IntervalCollectionTester
    {
        protected override Type GetCollectionType()
        {
            return typeof(DynamicIntervalTree<,>);
        }

        // DIT where we set the ReferenceDuplicates to true
        protected new object[] AdditionalParameters()
        {
            return new object[] { true };
        }
    }

    #endregion

    #region White-box

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

    #endregion
}
