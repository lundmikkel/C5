using System;
using NUnit.Framework;

namespace C5.Intervals.Tests
{
    namespace StaticFiniteIntervalList
    {
        #region Black-box

        [Ignore]
        class StaticFiniteIntervalListTesterBlackBox : OverlapFreeIntervalCollectionTester
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
        }

        #endregion
        
        #region White-box

        #endregion
    }
}
