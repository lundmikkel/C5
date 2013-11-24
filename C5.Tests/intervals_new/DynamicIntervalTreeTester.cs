using System;
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
}
