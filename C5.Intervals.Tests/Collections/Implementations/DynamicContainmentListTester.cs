using System;

namespace C5.Intervals.Tests
{
    namespace DynamicContainmentList
    {
        #region Black-box

        class DynamicContainmentListTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(DynamicContainmentList<,>);
            }

            // DIT's standard behavior where we set the ReferenceDuplicates to false
            protected override object[] AdditionalParameters()
            {
                return new object[] { };
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }

            protected override bool AllowsOverlaps()
            {
                return true;
            }

            protected override bool AllowsContainments()
            {
                return true;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return true;
            }
        }

        #endregion
    }
}