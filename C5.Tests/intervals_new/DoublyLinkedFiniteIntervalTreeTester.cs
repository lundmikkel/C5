using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using C5.intervals;
using C5.Tests.intervals;

namespace C5.Tests.intervals_new
{
    namespace DoublyLinkedFiniteIntervalTree
    {
        using Interval = IntervalBase<int>;

        #region Black-box

        class DoublyLinkedFiniteIntervalTreeTester_BlackBox : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(DoublyLinkedFiniteIntervalTree<,>);
            }

            protected override Speed CountSpeed()
            {
                return Speed.Constant;
            }

            protected override bool AllowsReferenceDuplicates()
            {
                return false;
            }
        }

        #endregion

        #region White-box

        [TestFixture]
        class DoublyLinkedFiniteIntervalTreeTester_WhiteBox
        {

            #region Code Contracts
            // TODO
            #endregion

            #region Inner Classes - Node

            #region Constructors
            // TODO
            #endregion

            #region Public Methods
            // TODO
            #endregion

            #endregion

            #region Constructors
            // TODO
            #endregion

            #region Collection Value
            // TODO
            #endregion

            #region Enumerable
            // TODO
            #endregion

            #region Interval Collection

            #region Properties

            #region Span
            // TODO
            #endregion

            #region Maximum Depth
            // TODO
            #endregion

            #region Allows Overlaps
            // TODO
            #endregion

            #region Allows Reference Duplicates
            // TODO
            #endregion

            #region Sorted
            // TODO
            #endregion

            #endregion

            #region Find Overlaps
            // TODO
            #endregion

            #region Find Overlap
            // TODO
            #endregion

            #region Count Overlaps
            // TODO
            #endregion

            #region Gaps
            // TODO
            #endregion

            #region Extensible

            #region Is Read Only
            // TODO
            #endregion

            #region Add
            // TODO
            #endregion

            #region Remove
            // TODO
            #endregion

            #region Clear
            // TODO
            #endregion

            #endregion

            #endregion

            #region QuickGraph
            // TODO
            #endregion
        }

        [TestFixture]
        class DoublyLinkedFiniteIntervalTreeTester
        {
            [Test]
            public void Add()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                const int count = 10;
                var numbers = new int[count];
                for (var i = 0; i < count; i++)
                    numbers[i] = i;
                numbers.Shuffle();

                foreach (var number in numbers)
                {
                    tree.Add(new IntervalBase<int>(number));
                }

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }

            [Test]
            public void AddSorted()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                const int count = 10;

                for (var i = 0; i < count; i++)
                    tree.Add(new IntervalBase<int>(i));

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }

            [Test]
            public void AddBalanced()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                foreach (var number in balancedNumbers(0, (int) Math.Pow(2, 4) - 2))
                    tree.Add(new IntervalBase<int>(number));

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }

            private IEnumerable<IntervalBase<int>> balancedNumbers(int lower, int upper)
            {
                if (lower > upper)
                    yield break;

                var mid = lower + (upper - lower >> 1);

                yield return new IntervalBase<int>(mid);

                foreach (var interval in balancedNumbers(lower, mid - 1))
                    yield return interval;
                foreach (var interval in balancedNumbers(mid + 1, upper))
                    yield return interval;
            }

            [Test]
            public void RemoveBalanced()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();
                int count = (int) Math.Pow(2, 4) - 2;
                var intervals = balancedNumbers(0, count).ToArray();

                foreach (var interval in intervals)
                    tree.Add(interval);

                tree.Remove(intervals[3]);
                tree.Remove(intervals[7]);

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }

            [Test]
            public void AddAndRemove()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                const int count = 10;

                var intervals = Enumerable.Range(0, count).Select(x => new IntervalBase<int>(x)).ToArray();

                foreach (var interval in intervals)
                    tree.Add(interval);

                Assert.AreEqual(count, tree.Count);

                foreach (var interval in intervals)
                    tree.Remove(interval);

                CollectionAssert.IsEmpty(tree);
                Assert.AreEqual(0, tree.Count);
            }

            [Test]
            public void DoubleAdd()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                const int count = 10;

                var intervals = Enumerable.Range(0, count).Select(x => new IntervalBase<int>(x)).ToArray();

                foreach (var interval in intervals)
                    tree.Add(interval);
                foreach (var interval in intervals)
                    tree.Add(interval);

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }
            [Test]
            public void Similar()
            {
                var tree = new DoublyLinkedFiniteIntervalTree<IInterval<int>, int>();

                var intervals = new[]
                    {
                        new Interval(0),
                        new Interval(1),
                        new Interval(2),
                        new Interval(3),
                        new Interval(3, 4),
                        new Interval(4),
                        new Interval(4, 5),
                        new Interval(4, 5, IntervalType.Open),
                        new Interval(4, 5, IntervalType.HighIncluded),
                        new Interval(6),
                        new Interval(7),
                    };

                foreach (var interval in intervals)
                    tree.Add(interval);


                tree.FindOverlaps(4);

#if DEBUG
                Console.Out.WriteLine(tree.QuickGraph);
#endif
            }
        }

        #endregion
    }
}
