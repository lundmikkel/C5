using System;
using System.Collections.Generic;
using C5.Tests.intervals.Generic;
using C5.Tests.intervals.Generic.Static;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    using SequencedIntervalsOfInt = TreeBag<IInterval<int>>;
    using IntervalOfInt = IntervalBase<int>;

    namespace NestedContainmentList
    {
        [TestFixture]
        public class EndpointInclusion : IntervaledEndpointInclusion
        {
            internal override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new NestedContainmentList<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture]
        public class NullCollection : IntervaledNullCollection
        {
            internal override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new NestedContainmentList<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture]
        public class Sample100 : intervals.Generic.Sample100
        {
            protected override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new NestedContainmentList<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture]
        public class BensTest : intervals.Generic.BensTest
        {
            protected override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new NestedContainmentList<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture]
        public class StaticEmptyCollection : StaticIntervaledEmptyCollection
        {
            protected override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new NestedContainmentList<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture]
        public class NestedContainmentListPerfomance : Performance23333
        {
            protected override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new NestedContainmentList<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture]
        public class NestedContainmentList100000Perfomance : Performance100000
        {
            protected override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new NestedContainmentList<IInterval<int>, int>(intervals);
            }
        }

        [TestFixture]
        public class NestedContainmentList_LargeTest : LargeTest_100000
        {
            protected override IIntervalCollection<IInterval<int>, int> Factory(IEnumerable<IInterval<int>> intervals)
            {
                return new NestedContainmentList<IInterval<int>, int>(intervals);
            }
        }

        //***************************************
        //   0     5    10    15    20    25   30
        //   |     |     |     |     |     |    |
        //               
        //   
        //                           [----------]
        //               [-----]   
        //         [----------------------]
        //   [----------------------]
        //
        //   
        //                        [****QUERY****]
        //
        //***************************************
        [TestFixture, Description("Test nested containment to make sure that it doesn't matter which interval the nested intervals are contained in, as lond as they are contained in the interval.")]
        public class OrderImportanceForNesting
        {
            private IIntervalCollection<IInterval<int>, int> _intervalCollection;

            [SetUp]
            public void Init()
            {
                _intervalCollection = new NestedContainmentList<IInterval<int>, int>(
                    new[]
                        {
                            new IntervalOfInt( 0, 20, true, true),
                            new IntervalOfInt( 5, 25, true, true),
                            new IntervalOfInt(10, 15, true, true),
                            new IntervalOfInt(20, 30, true, true),
                        }
                );
            }

            [Test]
            public void Overlap()
            {
                CollectionAssert.AreEquivalent(
                    new[] {
                            new IntervalOfInt( 0, 20, true, true),
                            new IntervalOfInt( 5, 25, true, true),
                            new IntervalOfInt(20, 30, true, true),
                    },
                    _intervalCollection.FindOverlaps(new IntervalOfInt(18, 30, true, true)));
            }

            [Test, Ignore("NCList doesn't work. So this test is failing as it should be. We just leave it out while testing SIT")]
            public void OverlapCount()
            {
                Assert.AreEqual(3, _intervalCollection.CountOverlaps(new IntervalOfInt(18, 30, true, true)));
            }
        }


        namespace Count
        {
            // TODO: Test with open intervals as well

            [TestFixture]
            public class NoContainments
            {
                private IIntervalCollection<IInterval<int>, int> _intervalCollection;

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
                    _intervalCollection = new NestedContainmentList<IInterval<int>, int>(new ArrayList<IInterval<int>>
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
                    Assert.AreEqual(0, _intervalCollection.CountOverlaps(new IntervalOfInt(9, 9, true, true)));
                    Assert.AreEqual(0, _intervalCollection.CountOverlaps(new IntervalOfInt(29, 30, true, true)));
                    Assert.AreEqual(0, _intervalCollection.CountOverlaps(new IntervalOfInt(int.MinValue, -2, true, true)));
                }

                [Test]
                public void CountSingle()
                {
                    Assert.AreEqual(1, _intervalCollection.CountOverlaps(new IntervalOfInt(0, 0, true, true)));
                    Assert.AreEqual(2, _intervalCollection.CountOverlaps(new IntervalOfInt(6, 9, true, true)));
                    Assert.AreEqual(1, _intervalCollection.CountOverlaps(new IntervalOfInt(21, 30, true, true)));
                }

                [Test]
                public void CountGroup()
                {
                    Assert.AreEqual(3, _intervalCollection.CountOverlaps(new IntervalOfInt(0, 6, true, true)));
                    Assert.AreEqual(5, _intervalCollection.CountOverlaps(new IntervalOfInt(12, 19, true, true)));
                    Assert.AreEqual(3, _intervalCollection.CountOverlaps(new IntervalOfInt(18, 19, true, true)));
                }
            }

            [TestFixture]
            public class OnlyContainments
            {
                private IIntervalCollection<IInterval<int>, int> _intervalCollection;

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
                    _intervalCollection = new NestedContainmentList<IInterval<int>, int>(new ArrayList<IInterval<int>>
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
                    Assert.AreEqual(0, _intervalCollection.CountOverlaps(new IntervalOfInt(31, int.MaxValue, true, true)));
                    Assert.AreEqual(0, _intervalCollection.CountOverlaps(new IntervalOfInt(int.MinValue, -2, true, true)));
                }

                [Test]
                public void CountSingle()
                {
                    Assert.AreEqual(1, _intervalCollection.CountOverlaps(new IntervalOfInt(0, 0, true, true)));
                    Assert.AreEqual(1, _intervalCollection.CountOverlaps(new IntervalOfInt(30, 35, true, true)));
                }

                [Test]
                public void CountGroup()
                {
                    Assert.AreEqual(2, _intervalCollection.CountOverlaps(new IntervalOfInt(23, 25, true, true)));
                    Assert.AreEqual(4, _intervalCollection.CountOverlaps(new IntervalOfInt(5, 8, true, true)));
                    Assert.AreEqual(9, _intervalCollection.CountOverlaps(new IntervalOfInt(13, 13, true, true)));
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
            public class MixedContainments
            {
                private IIntervalCollection<IInterval<int>, int> _intervalCollection;

                [SetUp]
                public void Init()
                {
                    _intervalCollection = new NestedContainmentList<IInterval<int>, int>(new ArrayList<IInterval<int>>
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
                public void Count()
                {
                    Assert.AreEqual(4, _intervalCollection.CountOverlaps(new IntervalOfInt(8, 10, true, true)));
                    Assert.AreEqual(3, _intervalCollection.CountOverlaps(new IntervalOfInt(2, 3, true, true)));
                    Assert.AreEqual(4, _intervalCollection.CountOverlaps(new IntervalOfInt(17, 19, true, true)));
                    Assert.AreEqual(2, _intervalCollection.CountOverlaps(new IntervalOfInt(14, 15, true, true)));
                    Assert.AreEqual(1, _intervalCollection.CountOverlaps(new IntervalOfInt(-5, -4, true, true)));
                }
            }
        }

        /*
        namespace BinarySearch
        {
            [TestFixture]
            public class BinarySearchHighInLows
            {
                private NestedContainmentList<IInterval<int>, int> _intervaled;

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
                    _intervaled = new NestedContainmentList<IInterval<int>, int>(new[] {A, B, C, D, E, F, G, H, I});
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
                private NestedContainmentList<IInterval<int>, int> _intervaled;

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
                    _intervaled = new NestedContainmentList<IInterval<int>, int>(new[] { A, B, C, D, E, F, G, H, I });
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
        public class StabbingQuery
        {
            private NestedContainmentList<IInterval<int>, int> _intervaled;

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
                _intervaled = new NestedContainmentList<IInterval<int>, int>(new[] { A, B, C, D, E, F, G });
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
    }
}
