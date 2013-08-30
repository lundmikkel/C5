using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using C5.intervals;
using System.Linq;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    namespace Generic
    {
        // TODO: make one test with testcases
        // The class tests that the stabbing query catches included endpoints and doesn't catches excluded endpoints
        public abstract class IntervaledEndpointInclusion
        {
            private IIntervaled<int> _intervaled;

            internal abstract IIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

            [SetUp]
            public void Init()
            {
                // Create Intervaled
                _intervaled = Factory(new[]
                {
                    new IntervalBase<int>(5, 15, true, true),
                    new IntervalBase<int>(5, 15, true, false),
                    new IntervalBase<int>(5, 15, false, true),
                    new IntervalBase<int>(5, 15, false, false)
                });
            }

            [Test]
            public void Overlap_StabbingBefore_EmptyResult()
            {
                CollectionAssert.IsEmpty(_intervaled.FindOverlaps(4));
            }

            [Test]
            public void Overlap_StabbingLowEndpoint_IncludedLowEndpoints()
            {
                CollectionAssert.AreEquivalent(
                    new ArrayList{
                        new IntervalBase<int>(5, 15, true, true),
                        new IntervalBase<int>(5, 15, true, false),
                    },
                    _intervaled.FindOverlaps(5)
                );
            }

            [Test]
            public void Overlap_StabbingMiddle_All()
            {
                CollectionAssert.AreEquivalent(
                    new ArrayList{
                        new IntervalBase<int>(5, 15, false, true),
                        new IntervalBase<int>(5, 15, true, false),
                        new IntervalBase<int>(5, 15, false, false),
                        new IntervalBase<int>(5, 15, true, true),
                    },
                    _intervaled.FindOverlaps(10)
                );
            }

            [Test]
            public void Overlap_StabbingHighEndpoint_IncludedHighEndpoints()
            {
                CollectionAssert.AreEquivalent(
                    new ArrayList{
                        new IntervalBase<int>(5, 15, false, true),
                        new IntervalBase<int>(5, 15, true, true),
                    },
                    _intervaled.FindOverlaps(15)
                );
            }

            [Test]
            public void Overlap_StabbingAfter_EmptyResult()
            {
                CollectionAssert.IsEmpty(_intervaled.FindOverlaps(16));
            }
        }

        public abstract class IntervaledNullCollection
        {
            protected IIntervaled<int> _intervaled;

            internal abstract IIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

            [SetUp]
            public void Init()
            {
                _intervaled = Factory(Enumerable.Empty<IInterval<int>>());
            }

            [Test]
            public void Overlap_NullQuery_ThrowsException()
            {
                CollectionAssert.IsEmpty(_intervaled.FindOverlaps(null));
            }

            [Test]
            public void OverlapExists_NullQuery_ThrowsException()
            {
                Assert.IsFalse(_intervaled.OverlapExists(null));
            }
        }

        public abstract class IntervaledEmptyCollection
        {
            protected IIntervaled<int> _intervaled;

            internal abstract IIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

            [SetUp]
            public void Init()
            {
                // Create Intervaled
                _intervaled = Factory(Enumerable.Empty<IInterval<int>>());
            }

            [Test]
            public void Span_InvalidSpan_ThrowsException()
            {
                Assert.Throws<InvalidOperationException>(() => { var span = _intervaled.Span; });
            }

            [Test]
            public void Overlap_StabMin0Max_ReturnsEmpty()
            {
                CollectionAssert.IsEmpty(_intervaled.FindOverlaps(int.MinValue));
                CollectionAssert.IsEmpty(_intervaled.FindOverlaps(0));
                CollectionAssert.IsEmpty(_intervaled.FindOverlaps(int.MaxValue));
            }

            [Test]
            public void Overlap_InfiniteQuery_ReturnsEmpty()
            {
                CollectionAssert.IsEmpty(_intervaled.FindOverlaps(new IntervalBase<int>(int.MinValue, int.MaxValue, false, false)));
            }

            [Test]
            public void Overlap_RandomQuery_ReturnsEmpty()
            {
                CollectionAssert.IsEmpty(_intervaled.FindOverlaps(new IntervalBase<int>(0, 5)));
            }

            // TODO: Test with bad interval? Like (8:8)

            [Test]
            public void OverlapExists_InfiniteQuery_ReturnsFalse()
            {
                Assert.IsFalse(_intervaled.OverlapExists(new IntervalBase<int>(int.MinValue, int.MaxValue, false, false)));
            }

            [Test]
            public void OverlapExists_RandomQuery_ReturnsFalse()
            {
                Assert.IsFalse(_intervaled.OverlapExists(new IntervalBase<int>(0, 5)));
            }
        }

        public abstract class IBS
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
                    return _name;
                }
            }


            protected IIntervaled<int> Intervaled;

            private static readonly IInterval<int> A = new Interval("A", 9, 19, true, true);
            private static readonly IInterval<int> B = new Interval("B", 2, 7, true, true);
            private static readonly IInterval<int> C = new Interval("C", 1, 3);
            private static readonly IInterval<int> D = new Interval("D", 17, 20, false, true);
            private static readonly IInterval<int> E1 = new Interval("E1", 8, 12, true, true);
            private static readonly IInterval<int> E2 = new Interval("E2", 8, 12, true, true);
            private static readonly IInterval<int> F = new Interval("F", 18);
            private static readonly IInterval<int> G = new Interval("G", int.MinValue, 17, false, true);
            private static readonly IInterval<int> H = new Interval("H", 5, 10, false, false);


            internal abstract IIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

            [SetUp]
            public void Init()
            {
                Intervaled = Factory(new[] { A, B, C, D, E1, E2, F, G, H });
            }

            private void range(IInterval<int> query, System.Collections.Generic.IEnumerable<IInterval<int>> expected)
            {
                CollectionAssert.AreEquivalent(expected, Intervaled.FindOverlaps(query));
            }

            [TestCaseSource(typeof(IBS), "StabCases")]
            public void Overlap_StabbingAtKeyPoints_ReturnsSpecifiedIntervals_TestCase(int query, System.Collections.Generic.IEnumerable<IInterval<int>> expected)
            {
                CollectionAssert.AreEquivalent(expected, Intervaled.FindOverlaps(query));
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
                var span = Intervaled.Span;
                var expected = new IntervalBase<int>(int.MinValue, 20, false, true);
                Assert.That(expected.Equals(span));
            }
        }

        // TODO: Fix naming
        [TestFixture]
        public abstract class Sample100
        {
            protected IIntervaled<int> Intervaled;
            private IInterval<int>[] _intervals;

            protected abstract IIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

            [SetUp]
            public void Init()
            {
                var intervals = File.ReadAllLines(@"../../intervals/data/sample100.csv").Select(line => line.Split(','));

                _intervals = new IInterval<int>[intervals.Count()];

                foreach (var interval in intervals)
                {
                    var i = Convert.ToInt32(interval[0]);
                    var low = Convert.ToInt32(interval[1]);
                    var high = Convert.ToInt32(interval[2]);

                    _intervals[i] = new IntervalBase<int>(low, high, true, true);
                }

                Intervaled = Factory(_intervals);

            }

            private void stabbing(int query, System.Collections.Generic.IEnumerable<IInterval<int>> expected)
            {
                CollectionAssert.AreEquivalent(expected, Intervaled.FindOverlaps(query));
            }

            private void range(IInterval<int> query, System.Collections.Generic.IEnumerable<IInterval<int>> expected)
            {
                CollectionAssert.AreEquivalent(expected, Intervaled.FindOverlaps(query));
            }

            //TODO: Make all StabbingX one method

            [Test]
            public void Stabbing()
            {
                stabbing(2, new ArrayList<IInterval<int>> {
                        _intervals[0],
                        _intervals[1],
                    });

                stabbing(34, new ArrayList<IInterval<int>>());

                stabbing(78, new ArrayList<IInterval<int>>{
                    _intervals[39],
                    _intervals[40],
                });

                stabbing(164, new ArrayList<IInterval<int>>{
                    _intervals[83],
                });
            }

            [Test]
            public void Range()
            {
                range(new IntervalBase<int>(74, 80, true, false), new ArrayList<IInterval<int>>{
                    _intervals[37],
                    _intervals[38],
                    _intervals[39],
                    _intervals[40],
                });

                range(new IntervalBase<int>(97), new ArrayList<IInterval<int>>{
                    _intervals[49],
                });

                range(new IntervalBase<int>(74, 80, true, true), new ArrayList<IInterval<int>>{
                    _intervals[37],
                    _intervals[38],
                    _intervals[39],
                    _intervals[40],
                    _intervals[41],
                });
                range(new IntervalBase<int>(74, 80, false, true), new ArrayList<IInterval<int>>{
                    _intervals[38],
                    _intervals[39],
                    _intervals[40],
                    _intervals[41],
                });
                range(new IntervalBase<int>(74, 80, false, false), new ArrayList<IInterval<int>>{
                    _intervals[38],
                    _intervals[39],
                    _intervals[40],
                });
            }

            [Test]
            public void BigRange()
            {
                ArrayList<IInterval<int>> array;

                array = new ArrayList<IInterval<int>>();
                _intervals.ToList().ForEach(I => array.Add(I));
                range(Intervaled.Span, array);

                array = new ArrayList<IInterval<int>>();
                _intervals.Take(50).ToList().ForEach(I => array.Add(I));
                range(new IntervalBase<int>(0, 97), array);
            }
        }


        [TestFixture]
        public abstract class BensTest
        {
            // ****************************************
            // | X axis:                              |
            // | 0    5    10   15   20   25   30   35|
            // | |    |    |    |    |    |    |    | |
            // | Container intervals:                 |
            // |                [C---]                |
            // |            [B--]    [D--]            |
            // |      [A--]                [E--]      |
            // | Test intervals:                      |
            // | [---]                                |
            // | [----]                               |
            // | [--------]                           |
            // |      [---]                           |
            // |       [-]                            |
            // |          []                          |
            // |          [-]                         |
            // |      [---------)                     |
            // |                                [---] |
            // |                               [----] |
            // |                           [--------] |
            // |                           [---]      |
            // |                            [-]       |
            // |                          []          |
            // |                         [-]          |
            // |                     [---------]      |
            // |          [----------------]          |
            // |      [------------------------]      |
            // | [----------------------------------]|
            // | X axis:                              |
            // | |    |    |    |    |    |    |    | |
            // | 0    5    10   15   20   25   30   35|
            // ****************************************

            protected IIntervaled<int> _intervaled;

            // ReSharper disable InconsistentNaming
            private static readonly IInterval<int> A = new IntervalBase<int>(5, 9, true, true);
            private static readonly IInterval<int> B = new IntervalBase<int>(11, 15, true, true);
            private static readonly IInterval<int> C = new IntervalBase<int>(15, 20, true, true);
            private static readonly IInterval<int> D = new IntervalBase<int>(20, 24, true, true);
            private static readonly IInterval<int> E = new IntervalBase<int>(26, 30, true, true);
            // ReSharper restore InconsistentNaming

            protected abstract IIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

            [SetUp]
            public void Init()
            {
                _intervaled = Factory(new[] { A, B, C, D, E });
            }

            [TestCaseSource(typeof(BensTest), "StabCases")]
            public void Overlap_StabbingAtKeyRanges_ReturnsSpecifiedIntervals_TestCase(IntervalBase<int> range, System.Collections.Generic.IEnumerable<IInterval<int>> expected)
            {
                CollectionAssert.AreEquivalent(expected, _intervaled.FindOverlaps(range));
            }

            public static object[] StabCases()
            {
                return new object[] {
                    new object[] { new IntervalBase<int>(  0,  35, true, true), new[] { A, B, C, D, E }},
                    new object[] { new IntervalBase<int>(  5,  30, true, true), new[] { A, B, C, D, E }},
                    new object[] { new IntervalBase<int>(  9,  26, true, true), new[] { A, B, C, D, E }},
                    new object[] { new IntervalBase<int>( 20,  30, true, true), new[] { C, D, E }},
                    new object[] { new IntervalBase<int>( 24,  26, true, true), new[] { D, E }},
                    new object[] { new IntervalBase<int>( 26,  26, true, true), new[] { E }},
                    new object[] { new IntervalBase<int>( 27,  29, true, true), new[] { E }},
                    new object[] { new IntervalBase<int>( 26,  30, true, true), new[] { E }},
                    new object[] { new IntervalBase<int>( 26,  35, true, true), new[] { E }},
                    new object[] { new IntervalBase<int>( 30,  35, true, true), new[] { E }},
                    new object[] { new IntervalBase<int>( 31,  35, true, true), Enumerable.Empty<IntervalBase<int>>()},
                    new object[] { new IntervalBase<int>(  0,   4, true, true), Enumerable.Empty<IntervalBase<int>>()},
                    new object[] { new IntervalBase<int>(  0,   5, true, true), new[] { A }},
                    new object[] { new IntervalBase<int>(  0,  10, true, true), new[] { A }},
                    new object[] { new IntervalBase<int>(  5,   9, true, true), new[] { A }},
                    new object[] { new IntervalBase<int>(  6,   8, true, true), new[] { A }},
                    new object[] { new IntervalBase<int>( 10,  10, true, true), Enumerable.Empty<IntervalBase<int>>()},
                    new object[] { new IntervalBase<int>( 10,  11, true, true), new[] { B }},
                    new object[] { new IntervalBase<int>(  5,  15, true, false), new[] { A, B }}
                };
            }
        }

        public abstract class Performance23333
        {
            protected IIntervaled<int> Intervaled;

            protected abstract IIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

            [SetUp]
            public void Init()
            {
                var intervals = File.ReadAllLines(@"../../intervals/data/performance_23333.csv").Select(line => line.Split(','));
                var intervalList = new ArrayList<IInterval<int>>();

                foreach (var interval in intervals)
                {
                    var low = Convert.ToInt32(interval[1]);
                    var high = Convert.ToInt32(interval[2]);

                    intervalList.Add(low < high ? new IntervalBase<int>(low, high) : new IntervalBase<int>(low));
                }

                var sw = Stopwatch.StartNew();
                const int count = 1;
                for (var i = 0; i < count; i++)
                {
                    Intervaled = Factory(intervalList);
                }
                sw.Stop();
                Console.WriteLine("Creation time: " + (sw.ElapsedMilliseconds / count));
            }

            [Test, Category("Simple performance")]
            public void Range()
            {
                Assert.That(Intervaled.FindOverlaps(new IntervalBase<int>(1357516800, 1358121599)).Count() == 42);

                var sw = Stopwatch.StartNew();
                const int count = 1;
                for (var i = 0; i < count; i++)
                {
                    Intervaled.FindOverlaps(new IntervalBase<int>(1357516800, 1358121599)).Count();
                }
                sw.Stop();
                Console.WriteLine("Time: " + ((float) sw.ElapsedMilliseconds / count));
            }
        }

        public abstract class Performance100000
        {
            protected IIntervaled<int> Intervaled;

            protected abstract IIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

            [SetUp]
            public void Init()
            {
                var intervals = File.ReadAllLines(@"../../intervals/data/performance_100000.csv").Select(line => line.Split(','));
                var intervalList = new ArrayList<IInterval<int>>();

                foreach (var interval in intervals)
                {
                    var low = Convert.ToInt32(interval[1]);
                    var high = Convert.ToInt32(interval[2]);


                    intervalList.Add(new IntervalBase<int>(low, high, true, true));
                    //intervalList.Add(low < high ? new IntervalOfInt(low, high) : new IntervalOfInt(low));
                }

                var sw = Stopwatch.StartNew();
                const int count = 1;// 000;
                for (var i = 0; i < count; i++)
                {
                    Intervaled = Factory(intervalList);
                }
                sw.Stop();
                Console.WriteLine("Creation time: " + (sw.ElapsedMilliseconds / count));
            }

            [Test, Category("Simple performance")]
            public void Range()
            {
                Console.WriteLine(Intervaled.FindOverlaps(new IntervalBase<int>(9231, 24228, true, true)).Count());

                Assert.That(Intervaled.FindOverlaps(new IntervalBase<int>(9231, 24228)).Count() == 20931);

                var sw = Stopwatch.StartNew();
                const int count = 1;// 000;
                for (var i = 0; i < count; i++)
                {
                    Intervaled.FindOverlaps(new IntervalBase<int>(9231, 24228)).Count();
                }
                sw.Stop();
                Console.WriteLine("Time: " + ((float) sw.ElapsedMilliseconds / count));
            }
        }


        public abstract class LargeTest_100000
        {
            protected IStaticIntervaled<int> Intervaled;

            protected abstract IStaticIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

            [TestFixtureSetUp]
            public void SetUp()
            {
                var intervals = File.ReadAllLines(@"../../intervals/data/performance_100000.csv").Select(line => line.Split(','));
                var intervalList = new ArrayList<IInterval<int>>();

                foreach (var interval in intervals)
                {
                    var low = Convert.ToInt32(interval[1]);
                    var high = Convert.ToInt32(interval[2]);

                    intervalList.Add(low < high ? new IntervalBase<int>(low, high) : new IntervalBase<int>(low));
                }

                Intervaled = Factory(intervalList);
            }

            [TestCaseSource(typeof(LargeTest_100000), "CountCases"), Category("Large tests")]
            public void FindOverlaps(int expected, IntervalBase<int> query)
            {
                var sw = Stopwatch.StartNew();

                const int count = 1;

                for (var i = 0; i < count; i++)
                    Intervaled.FindOverlaps(query).Count();

                sw.Stop();
                Console.WriteLine("Query time: " + ((float) sw.ElapsedMilliseconds / count));

                var actual = Intervaled.FindOverlaps(query).Count();
                Assert.AreEqual(expected, actual);
            }

            [TestCaseSource(typeof(LargeTest_100000), "CountCases"), Category("Large tests")]
            public void CountOverlaps(int expected, IntervalBase<int> query)
            {
                var actual = Intervaled.CountOverlaps(query);
                Assert.AreEqual(expected, actual);
                var sw = Stopwatch.StartNew();

                const int count = 1;

                for (var i = 0; i < count; i++)
                    Intervaled.CountOverlaps(query);

                sw.Stop();
                Console.WriteLine("Query time: " + ((float) sw.ElapsedMilliseconds / count));

            }

            public static object[] CountCases()
            {
                return new object[] {
                    new object[] { 61, new IntervalBase<int>(98696, 98796)},
                    new object[] { 147, new IntervalBase<int>(4633, 4675)},
                    new object[] { 10000, new IntervalBase<int>(22514, 33893)},
                    new object[] { 20001, new IntervalBase<int>(374460, 525081)},
                    new object[] { 30000, new IntervalBase<int>(101517, 1658000)},
                    new object[] { 40000, new IntervalBase<int>(-1234, 21538)},
                    new object[] { 50000, new IntervalBase<int>(100, 32408)}
                };
            }
        }

        namespace Static
        {

            public abstract class StaticIntervaledEmptyCollection
            {
                private IStaticIntervaled<int> _intervaled;

                protected abstract IStaticIntervaled<int> Factory(System.Collections.Generic.IEnumerable<IInterval<int>> intervals);

                [SetUp]
                public void Init()
                {
                    _intervaled = Factory(Enumerable.Empty<IInterval<int>>());
                }

                [Test]
                public void OverlapCount_InfiniteQuery_ReturnsZero()
                {
                    Assert.AreEqual(0, _intervaled.CountOverlaps(new IntervalBase<int>(int.MinValue, int.MaxValue, false, false)));
                }

                [Test]
                public void OverlapCount_RandomQuery_ReturnsZero()
                {
                    Assert.AreEqual(0, _intervaled.CountOverlaps(new IntervalBase<int>(0, 5)));
                }
            }
        }
    }

    class ExtentionOutputs
    {
        [Test]
        public void Print()
        {
            var x = new IntervalBase<int>(1, 5);
            var y = new IntervalBase<int>(2, 3, true, true);

            x.Overlaps(y); // true
            y.Overlaps(x); // true

            x.Contains(y); // true
            y.Contains(x); // false

            x.CompareTo(y); // -1
            y.CompareTo(x); // 1

            x.Equals(y); // false

            x.GetHashCode(); //15734484
            y.GetHashCode(); //15762354

            x.ToString(); // [1:5)
            y.ToString(); // [2:3]
        }
    }

}
