using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using C5.Tests.intervals.Generic;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    using IntervalOfInt = IntervalBase<int>;

    /*
    // Removed as method is private.
    [TestFixture]
    public class Median
    {
        [Test]
        public void Empty()
        {
        }

        private void checkK(C5.IList<int> list)
        {
            var sorted = new int[list.Count];
            list.CopyTo(sorted, 0);
            Array.Sort(sorted);

            for (int i = 0; i < list.Count; i++)
            {
                var changedList = new ArrayList<int>();
                list.ToList().ForEach(n => changedList.Add(n));
                Assert.That(StaticIntervalTree<int>.GetK(changedList, i) == sorted[i]);
            }
        }

        [Test]
        public void AllSame()
        {
            checkK(new ArrayList<int> { 7, 7, 7, 7, 7 });
        }

        [Test]
        public void AllSameButOne()
        {
            checkK(new ArrayList<int> { 3, 7, 7, 7, 7 });
            checkK(new ArrayList<int> { 7, 3, 7, 7, 7 });
            checkK(new ArrayList<int> { 7, 7, 3, 7, 7 });
            checkK(new ArrayList<int> { 7, 7, 7, 3, 7 });
            checkK(new ArrayList<int> { 7, 7, 7, 7, 3 });
        }

        [Test]
        public void TwoDifferent()
        {
            checkK(new ArrayList<int> { 3, 3, 3, 7, 7 });
            checkK(new ArrayList<int> { 7, 7, 7, 3, 3 });
            checkK(new ArrayList<int> { 3, 7, 3, 7, 3 });
        }

        [Test]
        public void AllDifferent()
        {
            checkK(new ArrayList<int> { 11, 5, 9, 3, 7 });
            checkK(new ArrayList<int> { 3, 5, 7, 9, 11 });
        }

        [Test]
        public void Random()
        {
            Random randNum = new Random();
            var randoms = new C5.ArrayList<int>();
            Enumerable
                .Repeat(0, 100)
                .Select(i => randoms.Add(randNum.Next(0, 1000)));

            checkK(randoms);
        }
    }*/



    public class StaticIntervalTreeEndpointInclusion : IntervaledEndpointInclusion
    {
        internal override IIntervalCollection<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
    }

    public class StaticIntervalTreeNullCollection : IntervaledNullCollection
    {
        internal override IIntervalCollection<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
    }

    public class StaticIntervalTreeEmptyCollection : IntervaledEmptyCollection
    {
        internal override IIntervalCollection<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
    }

    public class StaticIntervalTreeIBS : IBS
    {
        internal override IIntervalCollection<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }

        [Test]
        public void Print()
        {
            File.WriteAllText(@"../../intervals/data/static_interval_tree.gv", ((StaticIntervalTree<int>) IntervalCollection).Graphviz());
        }
    }

    public class StaticIntervalSample100 : Sample100
    {
        protected override IIntervalCollection<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }


        [Test]
        public void Print()
        {
            File.WriteAllText(@"../../intervals/data/sit100.gv", ((StaticIntervalTree<int>) IntervalCollection).Graphviz());
        }
    }

    [TestFixture]
    public class BensTest : intervals.Generic.BensTest
    {
        protected override IIntervalCollection<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new NestedContainmentList<int>(intervals);
        }
    }

    [TestFixture]
    public class StaticIntervalPerfomance : Performance23333
    {
        protected override IIntervalCollection<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
    }

    [TestFixture]
    public class StaticIntervalTree_LargeTest : LargeTest_100000
    {
        protected override IStaticIntervalCollection<int> Factory(IEnumerable<IInterval<int>> intervals)
        {
            return new StaticIntervalTree<int>(intervals);
        }
    }

    // TODO: This was to test sorting order on RightList. Should this be kept in its current state?
    //************************************
    //   0     5    10    15    20    25
    //   |     |     |     |     |     |
    //                              
    //         [–––––––––––]        
    //               [–––––––––––]        
    //                     [–––––––––––]
    //                              
    //************************************
    [TestFixture]
    public class StabbingQueryMoreThanOneIntervals
    {
        private StaticIntervalTree<int> _intervalTree;

        [SetUp]
        public void Init()
        {
            _intervalTree = new StaticIntervalTree<int>(new[]
                {
                    new IntervalOfInt( 5, 15, true, true),
                    new IntervalOfInt(10, 20, true, true),
                    new IntervalOfInt(15, 25, true, true)
                });
        }

        [Test]
        public void StabbingQuery()
        {
            CollectionAssert.AreEquivalent(
                new[] { new IntervalOfInt(15, 25, true, true) },
                _intervalTree.FindOverlaps(23)
            );
        }
    }
}
