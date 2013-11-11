using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using C5.Tests.intervals.Generic;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    namespace DynamicIntervalTree
    {

        [TestFixture]
        public class OrcompIntervalTreeIbs : IBS
        {
            internal override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new DynamicIntervalTree<IInterval<int>, int>(intervals);
            }

            [Test]
            public void MaximumOverlap_IBS_Returns5()
            {
                Assert.AreEqual(5, ((DynamicIntervalTree<IInterval<int>, int>) IntervalCollection).MaximumOverlap);
            }
        }

        [TestFixture]
        public class EndpointInclusion : IntervaledEndpointInclusion
        {
            internal override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new DynamicIntervalTree<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture]
        public class NullCollection : IntervaledNullCollection
        {
            internal override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new DynamicIntervalTree<IInterval<int>, int>(intervals);
            }

            [Test]
            public void MaximumOverlap_EmptyCollection_Returns0()
            {
                Assert.AreEqual(0, ((DynamicIntervalTree<IInterval<int>, int>) _intervalCollection).MaximumOverlap);
            }
        }

        [TestFixture]
        public class EmptyCollection : IntervaledEmptyCollection
        {
            internal override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new DynamicIntervalTree<IInterval<int>, int>(intervals);
            }

            [Test]
            public void MaximumOverlap_EmptyCollection_Returns0()
            {
                Assert.AreEqual(0, ((DynamicIntervalTree<IInterval<int>, int>) _intervalCollection).MaximumOverlap);
            }
        }

        [TestFixture]
        public class Sample100 : intervals.Generic.Sample100
        {
            protected override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new DynamicIntervalTree<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture, Ignore]
        public class IBSPerformance : Performance23333
        {
            protected override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new DynamicIntervalTree<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture]
        public class BensTest : intervals.Generic.BensTest
        {
            protected override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new DynamicIntervalTree<IInterval<int>, int>(intervals);
            }

            [Test]
            public void MaximumOverlap_BensCollection_Returns2()
            {
                Assert.AreEqual(2, ((DynamicIntervalTree<IInterval<int>, int>) _intervalCollection).MaximumOverlap);
            }
        }

        [TestFixture]
        public class RandomRemove
        {
            private IList<IInterval<int>> _intervals;
            private readonly Random _random = new Random(0);

            [SetUp]
            public void SetUp()
            {
                const int count = 100000;
                _intervals = new ArrayList<IInterval<int>>(count);
                _intervals.AddAll(BenchmarkTestCases.DataSetB(count));
                _intervals.Shuffle(_random);
            }

            [Test]
            public void AddAndRemove()
            {
                var intervalCollection = new DynamicIntervalTree<IInterval<int>, int>();

                foreach (var interval in _intervals)
                    intervalCollection.Add(interval);

                foreach (var interval in _intervals)
                {
                    Assert.True(intervalCollection.Remove(interval));
                    Assert.False(intervalCollection.Remove(interval));
                }
            }
        }

        [TestFixture]
        public class IBSRemove
        {
            //************************************
            //   0     5    10    15    20    25
            //   |     |     |     |     |     |
            //             A[----------]                
            //    B[-----]         
            //   C[-)
            //                      D(---]                 
            //            E[----]   
            //                        F*
            //...--------------------]G
            //        H(-----)
            //           
            //************************************

            private DynamicIntervalTree<IInterval<int>, int> _intervales;

            public class Interval : IntervalBase<int>
            {
                private readonly string _name;

                public Interval(string name, int query)
                    : base(query)
                {
                    _name = name;
                }

                public Interval(string name, int low, int high)
                    : base(low, high)
                {
                    _name = name;
                }

                public Interval(string name, int low, int high, bool lowIncluded, bool highIncluded)
                    : base(low, high, lowIncluded, highIncluded)
                {
                    _name = name;
                }

                public Interval(string name, IInterval<int> i)
                    : base(i)
                {
                    _name = name;
                }

                public Interval(string name, IInterval<int> low, IInterval<int> high)
                    : base(low, high)
                {
                    _name = name;
                }

                public override string ToString()
                {
                    return base.ToString(); // _name;
                }
            }


            private static readonly IInterval<int> A = new Interval("A", 9, 19, true, true);
            private static readonly IInterval<int> B = new Interval("B", 2, 7, true, true);
            private static readonly IInterval<int> C = new Interval("C", 1, 3);
            private static readonly IInterval<int> D = new Interval("D", 17, 20, false, true);
            private static readonly IInterval<int> E1 = new Interval("E1", 8, 12, true, true);
            private static readonly IInterval<int> E2 = new Interval("E2", 8, 12, true, true);
            private static readonly IInterval<int> F = new Interval("F", 18);
            private static readonly IInterval<int> G = new Interval("G", int.MinValue, 17, false, true);
            private static readonly IInterval<int> H = new Interval("H", 5, 10, false, false);
            private static readonly IInterval<int> A1 = new Interval("A1", 1, 2, true, true);
            private static readonly IInterval<int> A2 = new Interval("A2", 1, 4, true, true);
            private static readonly IInterval<int> A3 = new Interval("A3", 6, 12, true, true);
            private static readonly IInterval<int> A4 = new Interval("A4", 5, 21, true, true);

            [SetUp]
            public void Init()
            {
                _intervales = new DynamicIntervalTree<IInterval<int>, int>
                    {
                        A,
                        B,
                        C,
                        D,
                        E1,
                        E2,
                        F,
                        G,
                        H,
                        A1
                    };

                _intervales.Remove(A1);
                _intervales.Add(A2);
                _intervales.Remove(A2);
                _intervales.Add(A3);
                _intervales.Remove(A3);
                _intervales.Add(A4);
                _intervales.Remove(A4);
            }

            private void range(IInterval<int> query, IEnumerable<IInterval<int>> expected)
            {
                CollectionAssert.AreEquivalent(expected, _intervales.FindOverlaps(query));
            }

            [TestCaseSource(typeof(IBSRemove), "StabCases")]
            public void Overlap_StabbingAtKeyPoints_ReturnsSpecifiedIntervals_TestCase(int query, IEnumerable<IInterval<int>> expected)
            {
                CollectionAssert.AreEquivalent(expected, _intervales.FindOverlaps(query));
            }

            public static object[] StabCases = new object[] {
                new object[] { 0, new[] { G }},
                new object[] { 1, new[] { C, G }},
                new object[] { 2, new[] { B, C, G }},
                new object[] { 3, new[] { B, G }},
                new object[] { 5, new[] { B, G }},
                new object[] { 7, new[] { B, G, H }},
                new object[] { 8, new[] { E1, E2, G, H }},
                new object[] { 9, new[] { A, E1, E2, G, H }},
                new object[] {10, new[] { A, E1, E2, G }},
                new object[] {12, new[] { A, E1, E2, G }},
                new object[] {15, new[] { A, G }},
                new object[] {17, new[] { A, G }},
                new object[] {18, new[] { A, D, F }},
                new object[] {19, new[] { A, D }},
                new object[] {20, new[] { D }},
                new object[] {21, Enumerable.Empty<IInterval<int>>()},
            };

            // TODO: Finish
            [Test]
            public void Overlap_Range_ReturnsSpecifiedIntervals()
            {
                range(new IntervalBase<int>(0, 2, true, true), new[] { B, C, G });
            }

            [Test]
            public void Span_IBS_ReturnSpan()
            {
                var span = _intervales.Span;
                var expected = new IntervalBase<int>(int.MinValue, 20, false, true);
                Assert.That(expected.Equals(span));
            }

            [Test]
            public void MaximumOverlap_IBS_Returns5()
            {
                Assert.AreEqual(5, _intervales.MaximumOverlap);
            }
        }

        [TestFixture]
        public class MaximumOverlap
        {
            private DynamicIntervalTree<IInterval<int>, int> _intervaled;

            [SetUp]
            public void Init()
            {
                _intervaled = new DynamicIntervalTree<IInterval<int>, int>();
            }

            [Test]
            public void MaximumOverlap_EmptyCollection_ReturnZero()
            {
                Assert.AreEqual(0, _intervaled.MaximumOverlap);
            }

            [Test]
            public void MaximumOverlap_NonOverlappingIntervals_ReturnOne()
            {
                _intervaled.Add(new IntervalBase<int>(1, 2));
                _intervaled.Add(new IntervalBase<int>(2, 3));
                _intervaled.Add(new IntervalBase<int>(3, 4));
                _intervaled.Add(new IntervalBase<int>(4, 5));

                Assert.AreEqual(1, _intervaled.MaximumOverlap);
            }

            [Test]
            public void MaximumOverlap_MaximumOverlapBetweenDescreteValues_ReturnTwo()
            {
                _intervaled.Add(new IntervalBase<int>(1, 3, false, false));
                _intervaled.Add(new IntervalBase<int>(2, 4, false, false));

                Assert.AreEqual(2, _intervaled.MaximumOverlap);
            }
        }


        [TestFixture]
        public class DuplicateIntervals
        {
            private DynamicIntervalTree<IntervalBase<int>, int> intervalCollection;

            [SetUp]
            public void SetUp()
            {
                intervalCollection = new DynamicIntervalTree<IntervalBase<int>, int>();
            }

            [Test]
            public void AllSameLow()
            {
                var random = new Random(0);
                const int count = 15;

                for (var i = 0; i < count; i++)
                    intervalCollection.Add(new IntervalBase<int>(0, random.Next(10) + 1));
            }

            [Test]
            public void AddAll()
            {
                var random = new Random(0);
                const int count = 15;

                var intervals = new ArrayList<IntervalBase<int>>();
                for (var i = 0; i < count; i++)
                {
                    var interval = new IntervalBase<int>(0, random.Next(10) + 1);
                    intervals.Add(interval);
                }
                intervalCollection.AddAll(intervals);
            }
        }
    }
}
