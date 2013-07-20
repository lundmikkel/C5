using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using C5.Tests.intervaled.Generic;
using C5.Tests.intervaled.Generic.Static;
using C5.intervaled;
using NUnit.Framework;

namespace C5.Tests.intervaled
{
    using SequencedIntervalsOfInt = TreeBag<IInterval<int>>;
    using IntervalOfInt = IntervalBase<int>;

    namespace LayeredContainmentList2
    {
        #region generic tests
        [TestFixture]
        public class LCListEndpointInclusion : IntervaledEndpointInclusion
        {
            internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }
        }

        [TestFixture]
        public class LCListNullCollection : IntervaledNullCollection
        {
            internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }
        }

        [TestFixture]
        public class LCListEmptyCollection : IntervaledEmptyCollection
        {
            internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }
        }

        [TestFixture]
        //************************************
        //   0     5    10    15    20    25
        //   |     |     |     |     |     |
        //                        F*
        //            E[----]   
        //        H(-----)
        //    B[-----]         
        //   C[-)
        //                      D(---]                 
        //             A[----------]                
        //...--------------------]G
        //           
        //************************************
        public class LCListIBS : Generic.IBS
        {
            internal override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }

            [Test]
            public void Print()
            {
                File.WriteAllText(@"../../intervaled/data/layered_containment_list.gv", ((LayeredContainmentList2<int>) Intervaled).Graphviz());
            }
        }

        [TestFixture]
        public class LCListSample100 : Generic.Sample100
        {
            protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }
        }

        [TestFixture]
        public class LCListBensTest : Generic.BensTest
        {
            protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }
        }

        [TestFixture]
        public class LCListStaticEmptyCollection : StaticIntervaledEmptyCollection
        {
            protected override IStaticIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }
        }

        [TestFixture]
        public class LCListPerfomance : Performance23333
        {
            protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }
        }

        [TestFixture]
        public class LayeredContainmentList2_LargeTest : LargeTest_100000
        {
            protected override IStaticIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }
        }

        [TestFixture]
        public class LCList100000Perfomance : Performance100000
        {
            protected override IIntervaled<int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new LayeredContainmentList2<int>(intervals);
            }

            [Test]
            public void CountTest()
            {
                var query = new IntervalBase<int>(9231, 24228);

                Console.WriteLine(Intervaled.FindOverlaps(query).Count());
                Console.WriteLine(((IStaticIntervaled<int>) Intervaled).CountOverlaps(query));

                var comparer = ComparerFactory<IInterval<int>>.CreateEqualityComparer(IntervalExtensions.Equals, IntervalExtensions.GetHashCode);

                var set = new HashSet<IInterval<int>>(comparer);

                foreach (var interval in Intervaled.FindOverlaps(query))
                    set.Add(interval);

                foreach (var interval in Intervaled.Where(interval => interval.Overlaps(query)))
                    set.Remove(interval);

                Console.WriteLine(set);

                Assert.AreEqual(20931, Intervaled.FindOverlaps(query).Count());
            }

            [Test, Ignore]
            public void Print()
            {
                File.WriteAllText(@"../../intervaled/data/lclist100000.gv", ((LayeredContainmentList2<int>) Intervaled).Graphviz());
            }
        }

        #endregion
        //*******************
        //   0    5   10 
        //   |    |    | 
        //   
        //   [---------) A
        //    [------) B
        //     [---) C
        //      [-----) D
        //       [) E
        //           
        //*******************
        [TestFixture]
        public class LCListContainmentInSameLayer
        {
            private IStaticIntervaled<int> _intervaled;

            private static readonly IInterval<int> A = new IntervalOfInt(0, 10);
            private static readonly IInterval<int> B = new IntervalOfInt(1, 8);
            private static readonly IInterval<int> C = new IntervalOfInt(2, 6);
            private static readonly IInterval<int> D = new IntervalOfInt(3, 9);
            private static readonly IInterval<int> E = new IntervalOfInt(4, 5);

            [SetUp]
            public void SetUp()
            {
                _intervaled = new LayeredContainmentList2<int>(new[] { A, B, C, D, E });
            }

            [TestCaseSource("StabCases")]
            public void Overlap_StabbingAtKeyPoints_ReturnsSpecifiedIntervals_TestCase(int low, int high, IEnumerable<IInterval<int>> expected)
            {
                var query = new IntervalOfInt(low, high, true, true);
                CollectionAssert.AreEquivalent(expected, _intervaled.FindOverlaps(query));
            }

            public static object[] StabCases = new object[] {
                new object[] {0, 0, new[] { A }},
                new object[] {0, 1, new[] { A, B }},
                new object[] {0, 2, new[] { A, B, C }},
                new object[] {0, 3, new[] { A, B, C, D }},
                new object[] {0, 4, new[] { A, B, C, D, E }},
                new object[] {0, 5, new[] { A, B, C, D, E }},
                new object[] {0, 6, new[] { A, B, C, D, E }},
                new object[] {0, 7, new[] { A, B, C, D, E }},
                new object[] {0, 8, new[] { A, B, C, D, E }},
                new object[] {0, 9, new[] { A, B, C, D, E }},
                new object[] {0, 10, new[] { A, B, C, D, E }}
            };
        }

        [TestFixture]
        public class LCListExample
        {
            private IIntervaled<int> _intervaled;

            private static readonly IInterval<int> A = new Interval("A", 0, 7);
            private static readonly IInterval<int> B = new Interval("B", 1, 8);
            private static readonly IInterval<int> C = new Interval("C", 2, 6);
            private static readonly IInterval<int> D = new Interval("D", 3, 5);
            private static readonly IInterval<int> E = new Interval("E", 4, 12);
            private static readonly IInterval<int> F = new Interval("F", 5, 7);
            private static readonly IInterval<int> G = new Interval("G", 6, 10);
            private static readonly IInterval<int> H = new Interval("H", 7, 12);
            private static readonly IInterval<int> I = new Interval("I", 10, 12);
            private static readonly IInterval<int> J = new Interval("J", 10, 12);

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

            [SetUp]
            public void SetUp()
            {
                _intervaled = new LayeredContainmentList2<int>(new[] { A, B, C, D, E, F, G, H, I, J });
            }

            [Test]
            public void GetEnumerator()
            {
                CollectionAssert.AreEqual(new[] { A, B, C, D, E, F, G, H, I, J }, _intervaled);
            }

            [Test]
            public void Empty()
            {
                Assert.Pass();
            }
        }


        namespace Count
        {
            // TODO: Test with open intervals as well

            [TestFixture]
            public class LCListNoContainments
            {
                private IStaticIntervaled<int> _intervaled;

                /**
                 * 0    5   10   15   20   25   30
                 * |    |    |    |    |    |    |
                 *                        [----] 9/0
                 *                  [--] 8/0
                 *                  [] 7/0
                 *            [------] 6/0
                 *            [-----] 5/0
                 *            [-----] 4/0
                 *       [-] 3/0
                 *   [---] 2/0
                 * [----] 1/0
                 */
                [SetUp]
                public void Init()
                {
                    _intervaled = new LayeredContainmentList2<int>(new ArrayList<IInterval<int>>
                    {
                        new IntervalOfInt(23, 28, true, true), // 8
                        new IntervalOfInt(17, 20, true, true), // 7
                        new IntervalOfInt(17, 18, true, true), // 6
                        new IntervalOfInt(11, 18, true, true), // 5
                        new IntervalOfInt(11, 17, true, true), // 4
                        new IntervalOfInt(11, 17, true, true), // 3
                        new IntervalOfInt( 6,  8, true, true), // 2
                        new IntervalOfInt( 2,  6, true, true), // 1
                        new IntervalOfInt( 0,  5, true, true), // 0
                    });
                }

                [Test]
                public void CountNone()
                {
                    Assert.AreEqual(0, _intervaled.CountOverlaps(new IntervalOfInt(9, 9, true, true)));
                    Assert.AreEqual(0, _intervaled.CountOverlaps(new IntervalOfInt(29, 30, true, true)));
                    Assert.AreEqual(0, _intervaled.CountOverlaps(new IntervalOfInt(int.MinValue, -2, true, true)));
                }

                [Test]
                public void CountSingle()
                {
                    Assert.AreEqual(1, _intervaled.CountOverlaps(new IntervalOfInt(0, 0, true, true)));
                    Assert.AreEqual(2, _intervaled.CountOverlaps(new IntervalOfInt(6, 9, true, true)));
                    Assert.AreEqual(1, _intervaled.CountOverlaps(new IntervalOfInt(21, 30, true, true)));
                }

                [Test]
                public void CountGroup()
                {
                    Assert.AreEqual(3, _intervaled.CountOverlaps(new IntervalOfInt(0, 6, true, true)));
                    Assert.AreEqual(5, _intervaled.CountOverlaps(new IntervalOfInt(12, 19, true, true)));
                    Assert.AreEqual(3, _intervaled.CountOverlaps(new IntervalOfInt(18, 19, true, true)));
                }
            }

            [TestFixture]
            public class LCListOnlyContainments
            {
                private IStaticIntervaled<int> _intervaled;

                /**
                 * 0    5   10   15   20   25   30
                 * |    |    |    |    |    |    |
                 *              [] 0/0
                 *             [--] 0/1
                 *            [----] 0/2
                 *           [------] 0/3
                 *          [--------] 0/4
                 *    [---------------] 0/5
                 *   [-----------------] 0/6
                 *  [---------------------------] 0/7
                 * [-----------------------------] 0/8
                 */
                [SetUp]
                public void Init()
                {
                    _intervaled = new LayeredContainmentList2<int>(new ArrayList<IInterval<int>>
                    {
                        new IntervalOfInt(13, 14, true, true),
                        new IntervalOfInt(12, 15, true, true),
                        new IntervalOfInt(11, 16, true, true),
                        new IntervalOfInt(10, 17, true, true),
                        new IntervalOfInt( 9, 18, true, true),
                        new IntervalOfInt( 3, 19, true, true),
                        new IntervalOfInt( 2, 20, true, true),
                        new IntervalOfInt( 1, 29, true, true),
                        new IntervalOfInt( 0, 30, true, true),
                    });
                }

                [Test]
                public void CountNone()
                {
                    Assert.AreEqual(0, _intervaled.CountOverlaps(new IntervalOfInt(31, int.MaxValue, true, true)));
                    Assert.AreEqual(0, _intervaled.CountOverlaps(new IntervalOfInt(int.MinValue, -2, true, true)));
                }

                [Test]
                public void CountSingle()
                {
                    Assert.AreEqual(1, _intervaled.CountOverlaps(new IntervalOfInt(0, 0, true, true)));
                    Assert.AreEqual(1, _intervaled.CountOverlaps(new IntervalOfInt(30, 35, true, true)));
                }

                [Test]
                public void CountGroup()
                {
                    Assert.AreEqual(2, _intervaled.CountOverlaps(new IntervalOfInt(23, 25, true, true)));
                    Assert.AreEqual(4, _intervaled.CountOverlaps(new IntervalOfInt(5, 8, true, true)));
                    Assert.AreEqual(9, _intervaled.CountOverlaps(new IntervalOfInt(13, 13, true, true)));
                }
            }


            //************************************
            //   0     5    10    15    20    25
            //   |     |     |     |     |     |
            //            E[----] 3/0
            //        H(-----) 2/0
            //    B[-----] 1/0
            //   C[-) 0/0              F* 1/0
            //    |                    |
            //    |                  D(---] 6/1
            //    |        A[----------] 5/0
            //...--------------------]G 0/4
            //************************************
            [TestFixture]
            public class LCListMixedContainments
            {
                private IStaticIntervaled<int> _intervaled;

                [SetUp]
                public void Init()
                {
                    _intervaled = new LayeredContainmentList2<int>(new ArrayList<IInterval<int>>
                    {
                        new IntervalOfInt( 9, 19, true, true),
                        new IntervalOfInt( 2,  7, true, true),
                        new IntervalOfInt( 1,  3, true, false),
                        new IntervalOfInt(17, 20, false, true),
                        new IntervalOfInt( 8, 12, true, true),
                        new IntervalOfInt(18),
                        new IntervalOfInt(int.MinValue, 17, false, true),
                        new IntervalOfInt(5, 10, false, false),
                    });
                }

                [Test]
                public void Print()
                {
                    Console.WriteLine(((LayeredContainmentList2<int>) _intervaled).Graphviz());
                }

                [Test]
                public void Count()
                {
                    Assert.AreEqual(4, _intervaled.CountOverlaps(new IntervalOfInt(8, 10, true, true)));
                    Assert.AreEqual(3, _intervaled.CountOverlaps(new IntervalOfInt(2, 3, true, true)));
                    Assert.AreEqual(4, _intervaled.CountOverlaps(new IntervalOfInt(17, 19, true, true)));
                    Assert.AreEqual(2, _intervaled.CountOverlaps(new IntervalOfInt(14, 15, true, true)));
                    Assert.AreEqual(1, _intervaled.CountOverlaps(new IntervalOfInt(-5, -4, true, true)));
                }

                [TestCaseSource(typeof(LCListMixedContainments), "StabCases")]
                public void Overlap_StabbingAtKeyPoints_ReturnsSpecifiedIntervals_TestCase(IInterval<int> range, bool expected)
                {
                    Assert.AreEqual(_intervaled.OverlapExists(range), expected);
                }

                public static object[] StabCases()
                {
                    return new object[]
                        {
                            new object[] {new IntervalOfInt( 1,  2, true, true), true},
                            new object[] {new IntervalOfInt( 3,  8, true, true), true},
                            new object[] {new IntervalOfInt( 5,  10, true, false), true},
                            new object[] {new IntervalOfInt( 11, 11, true, true), true},
                            new object[] {new IntervalOfInt( 17, 19, false, true), true},
                            new object[] {new IntervalOfInt( 20, 30, true, false), true},
                            new object[] {new IntervalOfInt( 20, 30, false, false), false},
                            new object[] {new IntervalOfInt( 21, 22, true, true), false},
                            new object[] {new IntervalOfInt( -5, -3, true, false), true}
                        };
                }
            }
        }

        /*
        namespace BinarySearch
        {
            [TestFixture]
            public class BinarySearchHighInLows
            {
                private LayeredContainmentList2<int> _intervaled;

                // ReSharper disable InconsistentNaming
                private static readonly IInterval<int> A = new IntervalOfInt(1, 4, true, true);
                private static readonly IInterval<int> B = new IntervalOfInt(3, 8, true, true);
                private static readonly IInterval<int> C = new IntervalOfInt(7, 12, true, true);
                private static readonly IInterval<int> D = new IntervalOfInt(10, 15, true, true);
                private static readonly IInterval<int> E = new IntervalOfInt(11, 17, true, true);
                private static readonly IInterval<int> F = new IntervalOfInt(20, 23, true, true);
                private static readonly IInterval<int> G = new IntervalOfInt(26, 32, false, false);
                private static readonly IInterval<int> H = new IntervalOfInt(30, 36, false, false);
                private static readonly IInterval<int> I = new IntervalOfInt(34, 40, false, false);
                // ReSharper restore InconsistentNaming

                [SetUp]
                public void Init()
                {
                    _intervaled = new LayeredContainmentList2<int>(new[] {A, B, C, D, E, F, G, H, I});
                }

                [TestCaseSource(typeof(BinarySearchHighInLows), "StabCases")]
                public void Overlap_StabbingAtKeyPoints_ReturnsSpecifiedIntervals_TestCase(IInterval<int> range, int expected)
                {
                    Assert.AreEqual(expected, _intervaled.searchHighInLows(_intervaled._list, range));
                }

                public static object[] StabCases()
                {
                    return new object[]
                        {
                            new object[] {new IntervalOfInt( 2,  5, true, true),    0},
                            new object[] {new IntervalOfInt( 3,  5, true, true),    0},
                            new object[] {new IntervalOfInt( 3,  7, true, false),   0},
                            new object[] {new IntervalOfInt( 3,  7, true, true),    0},
                            new object[] {new IntervalOfInt( 4,  7, false, true),   1},
                            new object[] {new IntervalOfInt( 8, 10, false, false),  2},
                            new object[] {new IntervalOfInt( 8, 10, true, true),    1},
                            new object[] {new IntervalOfInt( 8, 11, true, false),   1},
                            new object[] {new IntervalOfInt(12, 14, false, true),   3},
                            new object[] {new IntervalOfInt(17, 19, false, false),  5},
                            new object[] {new IntervalOfInt(17, 20, false, true),   5},
                            new object[] {new IntervalOfInt(17, 20, true, false),   4},
                            new object[] {new IntervalOfInt(18, 19, true, true),    5},
                            new object[] {new IntervalOfInt(19, 25, true, true),    5},
                            new object[] {new IntervalOfInt(23, 25, false, true),   6},
                            new object[] {new IntervalOfInt(23, 26, false, false),  6},
                            new object[] {new IntervalOfInt(23, 26, true, true),    5},
                            new object[] {new IntervalOfInt(26, 30, false, false),  6},
                            new object[] {new IntervalOfInt(26, 30, true, true),    6},
                            new object[] {new IntervalOfInt(26, 40, false, false),  6},
                            new object[] {new IntervalOfInt(32, 34, true, true),    7},
                            new object[] {new IntervalOfInt(32, 34, false, false),  7},
                            new object[] {new IntervalOfInt(34, 36, false, false),  7},
                            new object[] {new IntervalOfInt(34, 36, true, true),    7},
                            new object[] {new IntervalOfInt(36, 40, true, true),    8},
                            new object[] {new IntervalOfInt(40, 42, false, true),   9}
                        };
                }
            }

            [TestFixture]
            public class BinarySearchLowInHighs
            {
                private LayeredContainmentList2<int> _intervaled;

                // ReSharper disable InconsistentNaming
                private static readonly IInterval<int> A = new IntervalOfInt(1, 4, true, true);
                private static readonly IInterval<int> B = new IntervalOfInt(3, 8, true, true);
                private static readonly IInterval<int> C = new IntervalOfInt(7, 12, true, true);
                private static readonly IInterval<int> D = new IntervalOfInt(10, 15, true, true);
                private static readonly IInterval<int> E = new IntervalOfInt(11, 17, true, true);
                private static readonly IInterval<int> F = new IntervalOfInt(20, 23, true, true);
                private static readonly IInterval<int> G = new IntervalOfInt(26, 32, false, false);
                private static readonly IInterval<int> H = new IntervalOfInt(30, 36, false, false);
                private static readonly IInterval<int> I = new IntervalOfInt(34, 40, false, false);
                // ReSharper restore InconsistentNaming

                [SetUp]
                public void Init()
                {
                    _intervaled = new LayeredContainmentList2<int>(new[] { A, B, C, D, E, F, G, H, I });
                }

                [TestCaseSource(typeof(BinarySearchLowInHighs), "StabCases")]
                public void Overlap_StabbingAtKeyPoints_ReturnsSpecifiedIntervals_TestCase(IInterval<int> range, int expected)
                {
                    Assert.AreEqual(expected, _intervaled.searchLowInHighs(_intervaled._list, range));
                }

                public static object[] StabCases()
                {
                    return new object[]
                        {
                            new object[] {new IntervalOfInt( 2,  5, true, true),    1},
                            new object[] {new IntervalOfInt( 3,  5, true, true),    1},
                            new object[] {new IntervalOfInt( 3,  7, true, false),   1},
                            new object[] {new IntervalOfInt( 3,  7, true, true),    2},
                            new object[] {new IntervalOfInt( 4,  7, false, true),   2},
                            new object[] {new IntervalOfInt( 8, 10, false, false),  2},
                            new object[] {new IntervalOfInt( 8, 10, true, true),    3},
                            new object[] {new IntervalOfInt( 8, 11, true, false),   3},
                            new object[] {new IntervalOfInt(12, 14, false, true),   4},
                            new object[] {new IntervalOfInt(17, 19, false, false),  4},
                            new object[] {new IntervalOfInt(17, 20, false, true),   5},
                            new object[] {new IntervalOfInt(17, 20, true, false),   4},
                            new object[] {new IntervalOfInt(18, 19, true, true),    4},
                            new object[] {new IntervalOfInt(19, 25, true, true),    5},
                            new object[] {new IntervalOfInt(23, 25, false, true),   5},
                            new object[] {new IntervalOfInt(23, 26, false, false),  5},
                            new object[] {new IntervalOfInt(23, 26, true, true),    5},
                            new object[] {new IntervalOfInt(26, 30, false, false),  6},
                            new object[] {new IntervalOfInt(26, 30, true, true),    6},
                            new object[] {new IntervalOfInt(26, 40, false, false),  8},
                            new object[] {new IntervalOfInt(32, 34, true, true),    7},
                            new object[] {new IntervalOfInt(32, 34, false, false),  7},
                            new object[] {new IntervalOfInt(34, 36, false, false),  8},
                            new object[] {new IntervalOfInt(34, 36, true, true),    8},
                            new object[] {new IntervalOfInt(36, 40, true, true),    8},
                            new object[] {new IntervalOfInt(40, 42, false, true),   8}
                        };
                }
            }
        }
        */

        [TestFixture]
        public class enumeratorTester
        {
            private IInterval<int>[] _intervals;

            [SetUp]
            public void SetUp()
            {
                _intervals = BenchmarkTestCases.DataSetC(10);
                var comparer = ComparerFactory<IInterval<int>>.CreateComparer(IntervalExtensions.CompareTo);
                Sorting.IntroSort(_intervals, 0, _intervals.Count(), comparer);
            }

            [Test]
            public void Sorted()
            {
                _intervals.Shuffle();
                var intervaled = new LayeredContainmentList2<int>(_intervals);

                var lastInterval = intervaled.Choose();
                int count = 0;
                foreach (var interval in intervaled.Sorted)
                {
                    Assert.True(lastInterval.CompareTo(interval) <= 0);
                    lastInterval = interval;

                    count++;
                }

                Assert.AreEqual(intervaled.Count, count);
            }

            [Test]
            public void EnumeratesAll()
            {
                var intervaled = new LayeredContainmentList2<int>(_intervals);
                CollectionAssert.AreEqual(_intervals, intervaled.Sorted);
            }

            [Test]
            public void EnumeratesAllUnsorted()
            {
                var intervaled = new LayeredContainmentList2<int>(_intervals);
                CollectionAssert.AreEquivalent(_intervals, intervaled);
            }
        }

        [TestFixture]
        public class StabbingQuery
        {
            private LayeredContainmentList2<int> _intervaled;

            // ReSharper disable InconsistentNaming
            private static readonly IInterval<int> A = new IntervalOfInt(2, 7, true, true);
            private static readonly IInterval<int> B = new IntervalOfInt(4, 12, true, true);
            private static readonly IInterval<int> C = new IntervalOfInt(5, 7, true, true);
            private static readonly IInterval<int> D = new IntervalOfInt(6, 8, true, true);
            private static readonly IInterval<int> E = new IntervalOfInt(9, 11, true, true);
            private static readonly IInterval<int> F = new IntervalOfInt(11, 17, true, true);
            private static readonly IInterval<int> G = new IntervalOfInt(18, 21, true, true);
            // ReSharper restore InconsistentNaming

            [SetUp]
            public void Init()
            {
                _intervaled = new LayeredContainmentList2<int>(new[] { A, B, C, D, E, F, G });
            }

            [Test]
            public void PrintNcList()
            {
                Console.WriteLine(_intervaled);
            }

            [TestCaseSource(typeof(StabbingQuery), "StabCases")]
            public void Overlap_StabbingAtKeyPoints_ReturnsSpecifiedIntervals_TestCase(int query,
                                                                                       IEnumerable<IInterval<int>>
                                                                                           expected)
            {
                CollectionAssert.AreEquivalent(expected, _intervaled.FindOverlaps(query));
            }

            public static object[] StabCases()
            {
                return new object[]
                    {
                        new object[] {6, new[] {A, B, C, D}},
                        new object[] {9, new[] {B, E}},
                        new object[] {11, new[] {B, E, F}},
                        new object[] {13, new[] {F}}
                    };
            }
        }

        #region White box-testing

        #region Statement testing

        [TestFixture]
        public class Statement
        {
            // ReSharper disable InconsistentNaming
            private static readonly IInterval<int> A = new IntervalOfInt(1, 5, true, true);
            private static readonly IInterval<int> B = new IntervalOfInt(3, 8, true, true);
            private static readonly IInterval<int> C = new IntervalOfInt(9, 17, true, true);
            private static readonly IInterval<int> D = new IntervalOfInt(12, 20, true, true);
            private static readonly IInterval<int> E = new IntervalOfInt(2, 7, true, true);
            private static readonly IInterval<int> F = new IntervalOfInt(2, 16, true, true);
            private static readonly IInterval<int> G = new IntervalOfInt(3, 8, true, true);
            private static readonly IInterval<int> H = new IntervalOfInt(5, 12, true, true);
            private static readonly IInterval<int> I = new IntervalOfInt(11, 17, true, true);
            private static readonly IInterval<int> J = new IntervalOfInt(22, 25, true, true);
            private static readonly IInterval<int> K = new IntervalOfInt(23, 24, true, true);
            private static readonly IInterval<int> L = new IntervalOfInt(23, 25, true, true);
            private static readonly IInterval<int> M = new IntervalOfInt(3, 15, true, true);
            private static readonly IInterval<int> N = new IntervalOfInt(4, 6, true, true);
            private static readonly IInterval<int> O = new IntervalOfInt(7, 12, true, true);
            private static readonly IInterval<int> P = new IntervalOfInt(10, 11, true, true);
            private static readonly IInterval<int> Q = new IntervalOfInt(13, 14, true, true);

            private static readonly IEnumerable<IInterval<int>> dataSetA = new[] { E };
            private static readonly IEnumerable<IInterval<int>> dataSetB = new[] { A, B, C, D };
            private static readonly IEnumerable<IInterval<int>> dataSetC = new[] { F, G, H, I };
            private static readonly IEnumerable<IInterval<int>> dataSetD = new[] { F, M, N, O, C, P, Q };
            private static readonly IEnumerable<IInterval<int>> dataSetE = new[] { B, D };
            private static readonly IEnumerable<IInterval<int>> dataSetF = new[] { A, F, G, H, I };
            private static readonly IEnumerable<IInterval<int>> dataSetG = new[] { F, G, I };




            // ReSharper restore InconsistentNaming


            #region Constructor

            [Test]
            public void Constructor_Empty()
            {
                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>()));
            }

            [Test]
            public void Constructor_OneInterval()
            {
                CollectionAssert.AreEquivalent(dataSetA, new LayeredContainmentList2<int>(dataSetA));
            }

            [Test]
            public void Constructor_MoreThanOneIntervalAndOneContainmentLayer()
            {
                CollectionAssert.AreEquivalent(dataSetB, new LayeredContainmentList2<int>(dataSetB));
            }

            [Test]
            public void Constructor_MoreThanOneIntervalAndTwoContainmentLayers()
            {
                CollectionAssert.AreEquivalent(dataSetC, new LayeredContainmentList2<int>(dataSetC));
            }

            [Test]
            public void Constructor_MoreThanOneIntervalAndMoreThanTwoContainmentLayers()
            {
                CollectionAssert.AreEquivalent(dataSetD, new LayeredContainmentList2<int>(dataSetD));
            }

            [Test]
            public void Constructor_ThreeContainments()
            {
                var moreThanOne = new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(1, 7, true, true),
                        new IntervalOfInt(2, 6, true, true),
                        new IntervalOfInt(3, 8, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    });
                CollectionAssert.AreEquivalent(new[] 
                    { 
                        new IntervalOfInt(1, 7, true, true),
                        new IntervalOfInt(2, 6, true, true),
                        new IntervalOfInt(3, 8, true, true),
                        new IntervalOfInt(4, 5, true, true) }, moreThanOne);
            }
            /*
            [Test]
            public void Constructor_FirstContainssecondEndEqual()
            {
                var moreThanOne = new LayeredContainmentList2<int>(new[] { J, L });
                CollectionAssert.AreEquivalent(new[] { J, L }, moreThanOne);
            }
            */
            #endregion

            #region CountOverlap

            [Test]
            public void CountOverlap_Empty()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>()).
                    CountOverlaps(new IntervalOfInt(2, 7, true, true)));
            }

            [Test]
            public void CountOverlap_Empty_NullQuery()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>()).
                    CountOverlaps(null));
            }

            [Test]
            public void CountOverlap_OneInterval()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(dataSetA).
                    CountOverlaps(new IntervalOfInt(2, 7, true, true)));
            }

            [Test]
            public void CountOverlap_OneInterval2()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[] { new IntervalOfInt(2, 7, false, true) }).
                    CountOverlaps(new IntervalOfInt(2, 7, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals3()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, true),
                        new IntervalOfInt(1, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(0, 1, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals4()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, false, true),
                        new IntervalOfInt(0, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(-1, 1, true, false)));
            }

            [Test]
            public void CountOverlap_MoreIntervals5()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, false, true),
                        new IntervalOfInt(0, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(-1, 1, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals6()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, true),
                        new IntervalOfInt(0, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(-1, 2, true, false)));
            }

            [Test]
            public void CountOverlap_MoreIntervals61()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, false, true),
                        new IntervalOfInt(0, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(-1, 2, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals7()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, false, true),
                        new IntervalOfInt(0, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(-1, 2, true, false)));
            }

            [Test]
            public void CountOverlap_MoreIntervals8()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, true),
                        new IntervalOfInt(0, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(-1, 1, true, false)));
            }

            // ******************

            [Test]
            public void CountOverlap_MoreIntervals9()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, true),
                        new IntervalOfInt(1, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(7, 8, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals10()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, false),
                        new IntervalOfInt(1, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(7, 8, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals11()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, true),
                        new IntervalOfInt(1, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(7, 8, false, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals12()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, false),
                        new IntervalOfInt(1, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(7, 8, false, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals13()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, false),
                        new IntervalOfInt(1, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(8, 9, false, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals14()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, false),
                        new IntervalOfInt(1, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(8, 9, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals15()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, true),
                        new IntervalOfInt(1, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(8, 9, false, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals16()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, true),
                        new IntervalOfInt(1, 6, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(8, 9, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals17()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, true),
                        new IntervalOfInt(1, 5, true, true),
                        new IntervalOfInt(4, 5, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(6, 9, true, true)));
            }

            [Test]
            public void CountOverlap_MoreIntervals18()
            {
                Assert.AreEqual(1, new LayeredContainmentList2<int>(new[]
                    {
                        new IntervalOfInt(2, 7, true, true),
                        new IntervalOfInt(1, 5, true, true),
                        new IntervalOfInt(4, 5, true, true),
                        new IntervalOfInt(0, 1, true, true),
                        new IntervalOfInt(8, 12, true, true)
                    }).
                    CountOverlaps(new IntervalOfInt(6, 7, true, true)));
            }

            [Test]
            public void CountOverlap_MoreThanOneInterval()
            {
                Assert.AreEqual(5, new LayeredContainmentList2<int>(dataSetD).
                    CountOverlaps(new IntervalOfInt(6, 9, true, true)));
            }

            [Test]
            public void CountOverlap_MoreThanOneIntervalQueryNull()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(dataSetB).
                    CountOverlaps(null));
            }

            [Test]
            public void CountOverlap_ContainmentQueryHits()
            {
                Assert.AreEqual(3, new LayeredContainmentList2<int>(dataSetF).
                    CountOverlaps(new IntervalOfInt(6, 7, true, true)));
            }

            [Test]
            public void CountOverlap_MoreThanOneIntervalQueryBefore()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(dataSetE).
                    CountOverlaps(new IntervalOfInt(1, 2, true, true)));
            }

            [Test]
            public void CountOverlap_MoreThanOneIntervalQueryAfter()
            {
                Assert.AreEqual(0, new LayeredContainmentList2<int>(dataSetE).
                    CountOverlaps(new IntervalOfInt(21, 23, true, true)));
            }

            #endregion

            #region FindOverlaps

            [Test]
            public void FindOverlaps_NullQueryZeroIntervals()
            {
                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>()).FindOverlaps(null));
            }

            [Test]
            public void FindOverlaps_NullQueryOneOrMoreIntervals()
            {
                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), new LayeredContainmentList2<int>(dataSetB).FindOverlaps(null));
            }

            [Test]
            public void FindOverlapsStabbing_QueryZeroIntervals()
            {
                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>()).FindOverlaps(2));
            }

            [Test]
            public void FindOverlapsRange_QueryZeroIntervals()
            {
                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>()).FindOverlaps(new IntervalOfInt(21, 23, true, true)));
            }

            [Test]
            public void FindOverlapsStabbing_QueryPointOneOrMoreIntervals()
            {
                CollectionAssert.AreEquivalent(new[] { A }, new LayeredContainmentList2<int>(dataSetB).FindOverlaps(2));
            }

            [Test]
            public void FindOverlaps_MoreThanOneIntervalQueryAfter()
            {
                Assert.AreEqual(Enumerable.Empty<IInterval<int>>(), new LayeredContainmentList2<int>(dataSetE).FindOverlaps(new IntervalOfInt(21, 23, true, true)));
            }

            [Test]
            public void FindOverlaps_ContainmentQueryHits()
            {
                Assert.AreEqual(3, new LayeredContainmentList2<int>(dataSetF).FindOverlaps(new IntervalOfInt(6, 7, true, true)).Count());
            }

            [Test]
            public void FindOverlaps_MoreThanOneIntervalQueryBefore()
            {
                Assert.AreEqual(Enumerable.Empty<IInterval<int>>(), new LayeredContainmentList2<int>(dataSetE).FindOverlaps(new IntervalOfInt(1, 2, true, true)));
            }

            #endregion

            #region Enumerator

            [Test]
            public void Enumerator_Empty()
            {
                var result = new ArrayList<IInterval<int>>();
                var list = new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>());
                var enumerator = list.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }

                CollectionAssert.AreEquivalent(Enumerable.Empty<IInterval<int>>(), result);
            }

            [Test]
            public void Enumerator_OneInterval()
            {
                var result = new ArrayList<IInterval<int>>();
                var list = new LayeredContainmentList2<int>(dataSetA);
                var enumerator = list.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }

                CollectionAssert.AreEquivalent(dataSetA, result);
            }

            [Test]
            public void Enumerator_MoreContainments()
            {
                var result = new ArrayList<IInterval<int>>();
                var list = new LayeredContainmentList2<int>(dataSetC);
                var enumerator = list.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }

                CollectionAssert.AreEquivalent(dataSetC, result);
            }

            [Test]
            public void Enumerator_OneContainment()
            {
                var result = new ArrayList<IInterval<int>>();
                var list = new LayeredContainmentList2<int>(dataSetG);
                var enumerator = list.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }

                CollectionAssert.AreEquivalent(dataSetG, result);
            }

            #endregion

            #region Span

            [Test]
            public void Span_Empty()
            {
                Assert.Throws<InvalidOperationException>(() => { var span = new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>()).Span; });
            }

            [Test]
            public void Span_MoreThanZero()
            {
                Assert.That(new LayeredContainmentList2<int>(dataSetB).Span.Equals(new IntervalOfInt(1, 20, true, true)));
            }

            #endregion

            #region OverlapExists

            [Test]
            public void OverlapExists_NullQuery()
            {
                Assert.False(new LayeredContainmentList2<int>(dataSetB).OverlapExists(null));
            }

            [Test]
            public void OverlapExists_Empty()
            {
                Assert.False(new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>()).
                    OverlapExists(new IntervalOfInt(4, 5, true, true)));
            }

            [Test]
            public void OverlapExists_QueryOutOfSpan()
            {
                Assert.False(new LayeredContainmentList2<int>(dataSetB).OverlapExists(new IntervalOfInt(21, 23, true, true)));
            }

            [Test]
            public void OverlapExists_Hit()
            {
                Assert.True(new LayeredContainmentList2<int>(dataSetB).OverlapExists(new IntervalOfInt(4, 5, true, true)));
            }

            [Test]
            public void OverlapExists_NoHit()
            {
                Assert.False(new LayeredContainmentList2<int>(dataSetE).OverlapExists(new IntervalOfInt(9, 11, true, true)));
            }

            #endregion

            [Test]
            public void CountSpeed_NotEmpty()
            {
                Assert.That(new LayeredContainmentList2<int>(dataSetA).CountSpeed.Equals(Speed.Constant));
            }

            [Test]
            public void Choose_Empty()
            {
                Assert.Throws<NoSuchItemException>(() =>
                    { var choose = new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>()).Choose(); });
            }

            [Test]
            public void Choose_NotEmpty()
            {
                Assert.NotNull(new LayeredContainmentList2<int>(dataSetA).Choose());
            }

            [Test]
            public void ToString_Null()
            {
                var list = new LayeredContainmentList2<int>(Enumerable.Empty<IInterval<int>>());
                Assert.AreEqual("{  }", list.ToString());
            }

            [Test]
            public void ToString_NotNull()
            {
                var list = new LayeredContainmentList2<int>(dataSetA);
                Assert.AreEqual("{ [2:7] }", list.ToString());
            }

        }

        #endregion

        #endregion
    }
}
