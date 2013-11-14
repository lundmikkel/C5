using System;
using C5.Tests.intervals;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals_new.DynamicIntervalTree
{
    #region Black-box

    class DynamicIntervalTreeTester_BlackBox_ReferenceDuplicatesFalse : IntervalCollectionTester
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

    class DynamicIntervalTreeTester_BlackBox_ReferenceDuplicatesTrue : IntervalCollectionTester
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
