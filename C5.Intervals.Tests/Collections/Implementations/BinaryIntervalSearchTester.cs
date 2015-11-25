using System;

namespace C5.Intervals.Tests
{
    namespace BinaryIntervalSearch
    {
        #region Black-box

        abstract class BinaryIntervalSearchTester_BlackBox : SortedIntervalCollectionTester
        {
            protected override object[] AdditionalParameters()
            {
                return new object[]{ IsFindOverlapsSorted() };
            }

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

            protected abstract bool IsFindOverlapsSorted();

            protected override bool AllowsContainments()
            {
                return true;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return true;
            }
        }

        class BinaryIntervalSearchTester_BlackBox_Sorted : BinaryIntervalSearchTester_BlackBox
        {
            protected override bool IsFindOverlapsSorted()
            {
                return true;
            }
        }

        class BinaryIntervalSearchTester_BlackBox_Unsorted : BinaryIntervalSearchTester_BlackBox
        {
            protected override bool IsFindOverlapsSorted()
            {
                return false;
            }
        }

        #endregion
    }
}
