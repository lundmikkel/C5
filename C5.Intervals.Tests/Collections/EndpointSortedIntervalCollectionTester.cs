namespace C5.Intervals.Tests
{
    namespace EndpointSortedIntervalCollection
    {
        using System;
        using Interval = IntervalBase<int>;

        #region Black-box

        abstract class EndpointSortedIntervalCollectionTesterBlackBox : ContainmentFreeIntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(EndpointSortedIntervalCollection<,>);
            }

            protected override object[] AdditionalParameters()
            {
                return new object[] { IsReadOnly() };
            }

            protected abstract bool IsReadOnly();

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

        class EndpointSortedIntervalCollectionTesterBlackBox_IsReadOnly : EndpointSortedIntervalCollectionTesterBlackBox
        {
            protected override bool IsReadOnly()
            {
                return true;
            }
        }

        class EndpointSortedIntervalCollectionTesterBlackBox_IsNotReadOnly : EndpointSortedIntervalCollectionTesterBlackBox
        {
            protected override bool IsReadOnly()
            {
                return false;
            }
        }

        #endregion
    }
}
