using System;
using C5.intervals;
using C5.intervals.@static;

namespace C5.Tests.intervals_new
{
    namespace StaticIntervalTree
    {
        #region Black-box

        class StaticIntervalTreeTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(StaticIntervalTree<,>);
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

        #region Legacy Tests

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
                    Assert.That(StaticIntervalTree<IInterval<int>, int>.GetK(changedList, i) == sorted[i]);
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

        #endregion

        #endregion
    }
}
