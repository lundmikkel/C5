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
                return new object[] { AllowsOverlaps(), IsReadOnly() };
            }

            protected abstract bool IsReadOnly();

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }

            protected override bool AllowsContainments()
            {
                return false;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return AllowsOverlaps();
            }
        }

        class EndpointSortedIntervalCollectionTesterBlackBox_StaticContainmentFree : EndpointSortedIntervalCollectionTesterBlackBox
        {
            protected override bool IsReadOnly()
            {
                return true;
            }

            protected override bool AllowsOverlaps()
            {
                return true;
            }
        }

        class EndpointSortedIntervalCollectionTesterBlackBox_DynamicContainmentFree : EndpointSortedIntervalCollectionTesterBlackBox
        {
            protected override bool IsReadOnly()
            {
                return false;
            }
            protected override bool AllowsOverlaps()
            {
                return true;
            }
        }

        class EndpointSortedIntervalCollectionTesterBlackBox_StaticOverlapFree : EndpointSortedIntervalCollectionTesterBlackBox
        {
            protected override bool IsReadOnly()
            {
                return true;
            }
            protected override bool AllowsOverlaps()
            {
                return false;
            }
        }

        class EndpointSortedIntervalCollectionTesterBlackBox_DynamicOverlapFree : EndpointSortedIntervalCollectionTesterBlackBox
        {
            protected override bool IsReadOnly()
            {
                return false;
            }
            protected override bool AllowsOverlaps()
            {
                return false;
            }
        }

        #endregion
    }
}
