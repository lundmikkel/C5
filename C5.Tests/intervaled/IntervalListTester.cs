using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using C5.Tests.intervaled.Generic;
using C5.intervaled;
using NUnit.Framework;

namespace C5.Tests.intervaled
{
    /*using IntervalOfInt = IntervalBase<int>;

    public class IntervalListEndpointInclusion : IntervaledEndpointInclusion
    {
        internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new IntervalList<int>(intervals);
        }
    }

    public class IntervalListNullCollection : IntervaledNullCollection
    {
        internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new IntervalList<int>(intervals);
        }
    }

    public class IntervalListEmptyCollection : IntervaledEmptyCollection
    {
        internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new IntervalList<int>(intervals);
        }
    }

    public class IntervalListIBS : IBS
    {
        internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new IntervalList<int>(intervals);
        }
    }

    public class IntervalListSample100 : Sample100
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new IntervalList<int>(intervals);
        }
    }

    [TestFixture]
    public class IntervalListBensTest : Generic.BensTest
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList<int>(intervals);
        }
    }

    [TestFixture]
    public class IntervalListPerfomance : Performance23333
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new IntervalList<int>(intervals);
        }
    }

    [TestFixture]
    public class IntervalList100000Perfomance : Performance100000
    {
        protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new IntervalList<int>(intervals);
        }
    }

    [TestFixture]
    public class IntervalListLargeTest : LargeTest_100000
    {
        protected override IStaticIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new IntervalList<int>(intervals);
        }
    }
    */
}
