using System;

namespace C5.Intervals.Tests
{
    namespace LayeredContainmentList
    {
        #region Black-box

        class LayeredContainmentListTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(LayeredContainmentList<,>);
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return true;
            }
        }

        #endregion

        #region White-box
        #endregion
    }
}
