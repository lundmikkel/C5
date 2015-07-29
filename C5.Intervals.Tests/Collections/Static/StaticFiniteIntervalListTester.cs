using System;

namespace C5.Intervals.Tests
{
    namespace StaticFiniteIntervalList
    {
        #region Black-box

        class StaticFiniteIntervalListTesterBlackBox : IntervalCollectionWithoutContainmentsTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(StaticFiniteIntervalList<,>);
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }

            protected override bool AllowsOverlaps()
            {
                return false;
            }

            protected override bool AllowsContainments()
            {
                return false;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return false;
            }

            protected override object[] AdditionalParameters()
            {
                return new object[] { true };
            }
        }

        #endregion

        #region White-box

        #endregion
    }
}
