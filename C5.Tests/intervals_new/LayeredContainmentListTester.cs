using System;
using C5.intervals;
using C5.intervals.@static;

namespace C5.Tests.intervals_new
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
