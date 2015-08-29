using System;

namespace C5.Intervals.Tests
{
    namespace BinaryIntervalSearch
    {
        #region Black-box

        class BinaryIntervalSearchTester_BlackBox : SortedIntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(BinaryIntervalSearch<,>);
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
