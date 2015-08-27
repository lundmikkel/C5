namespace C5.Intervals.Tests
{
    namespace EndpointSortedIntervalCollection
    {
        using System;
        using Interval = IntervalBase<int>;

        #region Black-box

        class EndpointSortedIntervalCollectionTesterBlackBox : ContainmentFreeIntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(EndpointSortedIntervalCollection<,>);
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
    }
}
