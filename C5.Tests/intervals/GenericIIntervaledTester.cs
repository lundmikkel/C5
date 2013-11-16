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
        using IntervalOfInt = IInterval<int>;

        // TODO: make one test with testcases
        // The class tests that the stabbing query catches included endpoints and doesn't catches excluded endpoints
        public abstract class IntervaledEndpointInclusion
        {
            private IIntervalCollection<IntervalOfInt, int> _intervalCollection;

            internal abstract IIntervalCollection<IntervalOfInt, int> Factory(System.Collections.Generic.IEnumerable<IntervalOfInt> intervals);

            [SetUp]
            public void Init()
            {
                // Create Intervaled
                _intervalCollection = Factory(new[]
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
                CollectionAssert.IsEmpty(_intervalCollection.FindOverlaps(4));
            }

            [Test]
            public void Overlap_StabbingLowEndpoint_IncludedLowEndpoints()
            {
                CollectionAssert.AreEquivalent(
                    new ArrayList{
                        new IntervalBase<int>(5, 15, true, true),
                        new IntervalBase<int>(5, 15, true, false),
                    },
                    _intervalCollection.FindOverlaps(5)
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
                    _intervalCollection.FindOverlaps(10)
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
                    _intervalCollection.FindOverlaps(15)
                );
            }

            [Test]
            public void Overlap_StabbingAfter_EmptyResult()
            {
                CollectionAssert.IsEmpty(_intervalCollection.FindOverlaps(16));
            }
        }

        public abstract class IntervaledNullCollection
        {
            protected IIntervalCollection<IntervalOfInt, int> _intervalCollection;

            internal abstract IIntervalCollection<IntervalOfInt, int> Factory(System.Collections.Generic.IEnumerable<IntervalOfInt> intervals);

            [SetUp]
            public void Init()
            {
                _intervalCollection = Factory(Enumerable.Empty<IntervalOfInt>());
            }

            [Test, Ignore]
            public void Overlap_NullQuery_ThrowsException()
            {
                CollectionAssert.IsEmpty(_intervalCollection.FindOverlaps(null));
            }

            [Test, Ignore]
            public void OverlapExists_NullQuery_ThrowsException()
            {
                IntervalOfInt overlap = null;
                Assert.IsFalse(_intervalCollection.FindOverlap(null, ref overlap));
            }
        }

        // TODO: Fix naming
        [TestFixture]
        public abstract class Sample100
        {
            protected IIntervalCollection<IntervalOfInt, int> IntervalCollection;
            private IntervalOfInt[] _intervals;

            protected abstract IIntervalCollection<IntervalOfInt, int> Factory(System.Collections.Generic.IEnumerable<IntervalOfInt> intervals);

            [SetUp]
            public void Init()
            {
                var intervals = File.ReadAllLines(@"../../intervals/data/sample100.csv").Select(line => line.Split(','));

                _intervals = new IntervalOfInt[intervals.Count()];

                foreach (var interval in intervals)
                {
                    var i = Convert.ToInt32(interval[0]);
                    var low = Convert.ToInt32(interval[1]);
                    var high = Convert.ToInt32(interval[2]);

                    _intervals[i] = new IntervalBase<int>(low, high, true, true);
                }

                IntervalCollection = Factory(_intervals);

            }

            private void stabbing(int query, System.Collections.Generic.IEnumerable<IntervalOfInt> expected)
            {
                CollectionAssert.AreEquivalent(expected, IntervalCollection.FindOverlaps(query));
            }

            private void range(IntervalOfInt query, System.Collections.Generic.IEnumerable<IntervalOfInt> expected)
            {
                CollectionAssert.AreEquivalent(expected, IntervalCollection.FindOverlaps(query));
            }

            //TODO: Make all StabbingX one method

            [Test]
            public void Stabbing()
            {
                stabbing(2, new ArrayList<IntervalOfInt> {
                        _intervals[0],
                        _intervals[1],
                    });

                stabbing(34, new ArrayList<IntervalOfInt>());

                stabbing(78, new ArrayList<IntervalOfInt>{
                    _intervals[39],
                    _intervals[40],
                });

                stabbing(164, new ArrayList<IntervalOfInt>{
                    _intervals[83],
                });
            }

            [Test]
            public void Range()
            {
                range(new IntervalBase<int>(74, 80, true, false), new ArrayList<IntervalOfInt>{
                    _intervals[37],
                    _intervals[38],
                    _intervals[39],
                    _intervals[40],
                });

                range(new IntervalBase<int>(97), new ArrayList<IntervalOfInt>{
                    _intervals[49],
                });

                range(new IntervalBase<int>(74, 80, true, true), new ArrayList<IntervalOfInt>{
                    _intervals[37],
                    _intervals[38],
                    _intervals[39],
                    _intervals[40],
                    _intervals[41],
                });
                range(new IntervalBase<int>(74, 80, false, true), new ArrayList<IntervalOfInt>{
                    _intervals[38],
                    _intervals[39],
                    _intervals[40],
                    _intervals[41],
                });
                range(new IntervalBase<int>(74, 80, false, false), new ArrayList<IntervalOfInt>{
                    _intervals[38],
                    _intervals[39],
                    _intervals[40],
                });
            }

            [Test]
            public void BigRange()
            {
                ArrayList<IntervalOfInt> array;

                array = new ArrayList<IntervalOfInt>();
                _intervals.ToList().ForEach(I => array.Add(I));
                range(IntervalCollection.Span, array);

                array = new ArrayList<IntervalOfInt>();
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

            protected IIntervalCollection<IntervalOfInt, int> _intervalCollection;

            // ReSharper disable InconsistentNaming
            private static readonly IntervalOfInt A = new IntervalBase<int>(5, 9, true, true);
            private static readonly IntervalOfInt B = new IntervalBase<int>(11, 15, true, true);
            private static readonly IntervalOfInt C = new IntervalBase<int>(15, 20, true, true);
            private static readonly IntervalOfInt D = new IntervalBase<int>(20, 24, true, true);
            private static readonly IntervalOfInt E = new IntervalBase<int>(26, 30, true, true);
            // ReSharper restore InconsistentNaming

            protected abstract IIntervalCollection<IntervalOfInt, int> Factory(System.Collections.Generic.IEnumerable<IntervalOfInt> intervals);

            [SetUp]
            public void Init()
            {
                _intervalCollection = Factory(new[] { A, B, C, D, E });
            }

            [TestCaseSource(typeof(BensTest), "StabCases")]
            public void Overlap_StabbingAtKeyRanges_ReturnsSpecifiedIntervals_TestCase(IntervalBase<int> range, System.Collections.Generic.IEnumerable<IntervalOfInt> expected)
            {
                CollectionAssert.AreEquivalent(expected, _intervalCollection.FindOverlaps(range));
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
            protected IIntervalCollection<IntervalOfInt, int> IntervalCollection;

            protected abstract IIntervalCollection<IntervalOfInt, int> Factory(System.Collections.Generic.IEnumerable<IntervalOfInt> intervals);

            [SetUp]
            public void Init()
            {
                var intervals = File.ReadAllLines(@"../../intervals/data/performance_23333.csv").Select(line => line.Split(','));
                var intervalList = new ArrayList<IntervalOfInt>();

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
                    IntervalCollection = Factory(intervalList);
                }
                sw.Stop();
                Console.WriteLine("Creation time: " + (sw.ElapsedMilliseconds / count));
            }

            [Test, Category("Simple performance")]
            public void Range()
            {
                Assert.That(IntervalCollection.FindOverlaps(new IntervalBase<int>(1357516800, 1358121599)).Count() == 42);

                var sw = Stopwatch.StartNew();
                const int count = 1;
                for (var i = 0; i < count; i++)
                {
                    IntervalCollection.FindOverlaps(new IntervalBase<int>(1357516800, 1358121599)).Count();
                }
                sw.Stop();
                Console.WriteLine("Time: " + ((float) sw.ElapsedMilliseconds / count));
            }
        }

        public abstract class Performance100000
        {
            protected IIntervalCollection<IntervalOfInt, int> IntervalCollection;

            protected abstract IIntervalCollection<IntervalOfInt, int> Factory(System.Collections.Generic.IEnumerable<IntervalOfInt> intervals);

            [SetUp]
            public void Init()
            {
                var intervals = File.ReadAllLines(@"../../intervals/data/performance_100000.csv").Select(line => line.Split(','));
                var intervalList = new ArrayList<IntervalOfInt>();

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
                    IntervalCollection = Factory(intervalList);
                }
                sw.Stop();
                Console.WriteLine("Creation time: " + (sw.ElapsedMilliseconds / count));
            }

            [Test, Category("Simple performance"), Ignore]
            public void Range()
            {
                Console.WriteLine(IntervalCollection.FindOverlaps(new IntervalBase<int>(9231, 24228, true, true)).Count());

                Assert.That(IntervalCollection.FindOverlaps(new IntervalBase<int>(9231, 24228)).Count() == 20931);

                var sw = Stopwatch.StartNew();
                const int count = 1;// 000;
                for (var i = 0; i < count; i++)
                {
                    IntervalCollection.FindOverlaps(new IntervalBase<int>(9231, 24228)).Count();
                }
                sw.Stop();
                Console.WriteLine("Time: " + ((float) sw.ElapsedMilliseconds / count));
            }
        }


        public abstract class LargeTest_100000
        {
            protected IIntervalCollection<IntervalOfInt, int> IntervalCollection;

            protected abstract IIntervalCollection<IntervalOfInt, int> Factory(System.Collections.Generic.IEnumerable<IntervalOfInt> intervals);

            [TestFixtureSetUp]
            public void SetUp()
            {
                var intervals = File.ReadAllLines(@"../../intervals/data/performance_100000.csv").Select(line => line.Split(','));
                var intervalList = new ArrayList<IntervalOfInt>();

                foreach (var interval in intervals)
                {
                    var low = Convert.ToInt32(interval[1]);
                    var high = Convert.ToInt32(interval[2]);

                    intervalList.Add(low < high ? new IntervalBase<int>(low, high) : new IntervalBase<int>(low));
                }

                IntervalCollection = Factory(intervalList);
            }

            [TestCaseSource(typeof(LargeTest_100000), "CountCases"), Category("Large tests"), Ignore]
            public void FindOverlaps(int expected, IntervalBase<int> query)
            {
                var sw = Stopwatch.StartNew();

                const int count = 1;

                for (var i = 0; i < count; i++)
                    IntervalCollection.FindOverlaps(query).Count();

                sw.Stop();
                Console.WriteLine("Query time: " + ((float) sw.ElapsedMilliseconds / count));

                var actual = IntervalCollection.FindOverlaps(query).Count();
                Assert.AreEqual(expected, actual);
            }

            [TestCaseSource(typeof(LargeTest_100000), "CountCases"), Category("Large tests"), Ignore]
            public void CountOverlaps(int expected, IntervalBase<int> query)
            {
                var actual = IntervalCollection.CountOverlaps(query);
                Assert.AreEqual(expected, actual);
                var sw = Stopwatch.StartNew();

                const int count = 1;

                for (var i = 0; i < count; i++)
                    IntervalCollection.CountOverlaps(query);

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
                private IIntervalCollection<IntervalOfInt, int> _intervalCollection;

                protected abstract IIntervalCollection<IntervalOfInt, int> Factory(System.Collections.Generic.IEnumerable<IntervalOfInt> intervals);

                [SetUp]
                public void Init()
                {
                    _intervalCollection = Factory(Enumerable.Empty<IntervalOfInt>());
                }

                [Test]
                public void OverlapCount_InfiniteQuery_ReturnsZero()
                {
                    Assert.AreEqual(0, _intervalCollection.CountOverlaps(new IntervalBase<int>(int.MinValue, int.MaxValue, false, false)));
                }

                [Test]
                public void OverlapCount_RandomQuery_ReturnsZero()
                {
                    Assert.AreEqual(0, _intervalCollection.CountOverlaps(new IntervalBase<int>(0, 5)));
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

            x.StrictlyContains(y); // true
            y.StrictlyContains(x); // false

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
