using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using C5.intervaled;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace C5.Tests.intervaled
{
    using IntervalOfInt = IntervalBase<int>;

    [TestFixture]
    public class IntervalEquals
    {
        [Test]
        public void Intervals()
        {
            IInterval<int> interval1 = new IntervalOfInt(1, 2, true, true);
            IInterval<int> interval2 = new IntervalOfInt(1, 2, true, true);
            IInterval<int> interval3 = new IntervalOfInt(1, 2, true, false);
            IInterval<int> interval4 = new IntervalOfInt(1, 3, true, true);

            Assert.AreEqual(interval1, interval2);
            Assert.AreNotEqual(interval1, interval3);
            Assert.AreNotEqual(interval1, interval4);
        }
    }

    // ReSharper disable CoVariantArrayConversion
    /// <summary>
    /// I before J
    /// IIIIII    JJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalIBeforeJ
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(0, 5, false, false), // ()
                new IntervalOfInt(0, 5, false, true), // (]
                new IntervalOfInt(0, 5, true, false), // [)
                new IntervalOfInt(0, 5, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalOfInt(10, 15, false, false), // ()
                new IntervalOfInt(10, 15, false, true), // (]
                new IntervalOfInt(10, 15, true, false), // [)
                new IntervalOfInt(10, 15, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Before_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Before_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Less(i.CompareTo(j), 0);
            else
                Assert.Greater(j.CompareTo(i), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.Contains")]
        public void Contains_Before_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? i.Contains(j) : j.Contains(i));
        }
    }

    /// <summary>
    /// I equal J
    /// IIIIII
    /// JJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXEqualY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(0, 5, false, false), // ()
                new IntervalOfInt(0, 5, false, true), // (]
                new IntervalOfInt(0, 5, true, false), // [)
                new IntervalOfInt(0, 5, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalOfInt(0, 5, false, false), // ()
                new IntervalOfInt(0, 5, false, true), // (]
                new IntervalOfInt(0, 5, true, false), // [)
                new IntervalOfInt(0, 5, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlap_Equal_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [TestCaseSource("StabCases"), Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Equal_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expression)
        {
            Assert.That(i.CompareTo(j), expression);
        }

        public static readonly object[] StabCases = new object[] {
                new object[] { I[0], J[0], Is.EqualTo(0)},
                new object[] { I[0], J[1], Is.LessThan(0)},
                new object[] { I[0], J[2], Is.GreaterThan(0)},
                new object[] { I[0], J[3], Is.GreaterThan(0)},
                new object[] { I[1], J[0], Is.GreaterThan(0)},
                new object[] { I[1], J[1], Is.EqualTo(0)},
                new object[] { I[1], J[2], Is.GreaterThan(0)},
                new object[] { I[1], J[3], Is.GreaterThan(0)},
                new object[] { I[2], J[0], Is.LessThan(0)},
                new object[] { I[2], J[1], Is.LessThan(0)},
                new object[] { I[2], J[2], Is.EqualTo(0)},
                new object[] { I[2], J[3], Is.LessThan(0)},
                new object[] { I[3], J[0], Is.LessThan(0)},
                new object[] { I[3], J[1], Is.LessThan(0)},
                new object[] { I[3], J[2], Is.GreaterThan(0)},
                new object[] { I[3], J[3], Is.EqualTo(0)},

                new object[] { J[0], I[0], Is.EqualTo(0)},
                new object[] { J[0], I[1], Is.LessThan(0)},
                new object[] { J[0], I[2], Is.GreaterThan(0)},
                new object[] { J[0], I[3], Is.GreaterThan(0)},
                new object[] { J[1], I[0], Is.GreaterThan(0)},
                new object[] { J[1], I[1], Is.EqualTo(0)},
                new object[] { J[1], I[2], Is.GreaterThan(0)},
                new object[] { J[1], I[3], Is.GreaterThan(0)},
                new object[] { J[2], I[0], Is.LessThan(0)},
                new object[] { J[2], I[1], Is.LessThan(0)},
                new object[] { J[2], I[2], Is.EqualTo(0)},
                new object[] { J[2], I[3], Is.LessThan(0)},
                new object[] { J[3], I[0], Is.LessThan(0)},
                new object[] { J[3], I[1], Is.LessThan(0)},
                new object[] { J[3], I[2], Is.GreaterThan(0)},
                new object[] { J[3], I[3], Is.EqualTo(0)}
            };

        //Contains
        [TestCaseSource("ContainsCases"), Category("IntervalComparer<T>.Contains")]
        public void Contains_Equal_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Contains(j), expected);
        }

        public static object[] ContainsCases = new object[] {
                new object[] { I[0], J[0], Is.False},
                new object[] { I[0], J[1], Is.False},
                new object[] { I[0], J[2], Is.False},
                new object[] { I[0], J[3], Is.False},
                new object[] { I[1], J[0], Is.False},
                new object[] { I[1], J[1], Is.False},
                new object[] { I[1], J[2], Is.False},
                new object[] { I[1], J[3], Is.False},
                new object[] { I[2], J[0], Is.False},
                new object[] { I[2], J[1], Is.False},
                new object[] { I[2], J[2], Is.False},
                new object[] { I[2], J[3], Is.False},
                new object[] { I[3], J[0], Is.True},
                new object[] { I[3], J[1], Is.False},
                new object[] { I[3], J[2], Is.False},
                new object[] { I[3], J[3], Is.False},

                new object[] { J[0], I[0], Is.False},
                new object[] { J[0], I[1], Is.False},
                new object[] { J[0], I[2], Is.False},
                new object[] { J[0], I[3], Is.False},
                new object[] { J[1], I[0], Is.False},
                new object[] { J[1], I[1], Is.False},
                new object[] { J[1], I[2], Is.False},
                new object[] { J[1], I[3], Is.False},
                new object[] { J[2], I[0], Is.False},
                new object[] { J[2], I[1], Is.False},
                new object[] { J[2], I[2], Is.False},
                new object[] { J[2], I[3], Is.False},
                new object[] { J[3], I[0], Is.True},
                new object[] { J[3], I[1], Is.False},
                new object[] { J[3], I[2], Is.False},
                new object[] { J[3], I[3], Is.False}
            };
    }

    /// <summary>
    /// I meets J
    /// IIIIII
    ///      JJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXMeetsY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(0, 5, false, false), // ()
                new IntervalOfInt(0, 5, false, true), // (]
                new IntervalOfInt(0, 5, true, false), // [)
                new IntervalOfInt(0, 5, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalOfInt(5, 10, false, false), // ()
                new IntervalOfInt(5, 10, false, true), // (]
                new IntervalOfInt(5, 10, true, false), // [)
                new IntervalOfInt(5, 10, true, true) // []
            };

        [TestCaseSource("OverlapCases"), Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Meets_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Overlaps(j), expected);
        }

        public static object[] OverlapCases = new object[] {
                new object[] { I[0], J[0], Is.False},
                new object[] { I[0], J[1], Is.False},
                new object[] { I[0], J[2], Is.False},
                new object[] { I[0], J[3], Is.False},
                new object[] { I[1], J[0], Is.False},
                new object[] { I[1], J[1], Is.False},
                new object[] { I[1], J[2], Is.True},
                new object[] { I[1], J[3], Is.True},
                new object[] { I[2], J[0], Is.False},
                new object[] { I[2], J[1], Is.False},
                new object[] { I[2], J[2], Is.False},
                new object[] { I[2], J[3], Is.False},
                new object[] { I[3], J[0], Is.False},
                new object[] { I[3], J[1], Is.False},
                new object[] { I[3], J[2], Is.True},
                new object[] { I[3], J[3], Is.True},

                new object[] { J[0], I[0], Is.False},
                new object[] { J[0], I[1], Is.False},
                new object[] { J[0], I[2], Is.False},
                new object[] { J[0], I[3], Is.False},
                new object[] { J[1], I[0], Is.False},
                new object[] { J[1], I[1], Is.False},
                new object[] { J[1], I[2], Is.False},
                new object[] { J[1], I[3], Is.False},
                new object[] { J[2], I[0], Is.False},
                new object[] { J[2], I[1], Is.True},
                new object[] { J[2], I[2], Is.False},
                new object[] { J[2], I[3], Is.True},
                new object[] { J[3], I[0], Is.False},
                new object[] { J[3], I[1], Is.True},
                new object[] { J[3], I[2], Is.False},
                new object[] { J[3], I[3], Is.True}
            };


        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Meets_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Less(i.CompareTo(j), 0);
            else
                Assert.Greater(j.CompareTo(i), 0);
        }


        [Test, Combinatorial, Category("IntervalComparer<T>.Contains")]
        public void Contains_Meets_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? i.Contains(j) : j.Contains(i));
        }
    }

    /// <summary>
    /// I overlaps J
    /// IIIIIIIIIII
    ///      JJJJJJJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXOverlapsY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(0, 10, false, false), // ()
                new IntervalOfInt(0, 10, false, true), // (]
                new IntervalOfInt(0, 10, true, false), // [)
                new IntervalOfInt(0, 10, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalOfInt(5, 15, false, false), // ()
                new IntervalOfInt(5, 15, false, true), // (]
                new IntervalOfInt(5, 15, true, false), // [)
                new IntervalOfInt(5, 15, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Overlap_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Overlap_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Less(i.CompareTo(j), 0);
            else
                Assert.Greater(j.CompareTo(i), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.Contains")]
        public void Contains_Overlap_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? i.Contains(j) : j.Contains(i));
        }
    }

    /// <summary>
    /// I during J
    ///      IIIIII
    /// JJJJJJJJJJJJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXDuringY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(5, 10, false, false), // ()
                new IntervalOfInt(5, 10, false, true), // (]
                new IntervalOfInt(5, 10, true, false), // [)
                new IntervalOfInt(5, 10, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalOfInt(0, 15, false, false), // ()
                new IntervalOfInt(0, 15, false, true), // (]
                new IntervalOfInt(0, 15, true, false), // [)
                new IntervalOfInt(0, 15, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_During_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_During_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Greater(i.CompareTo(j), 0);
            else
                Assert.Less(j.CompareTo(i), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.Contains")]
        public void Contains_During_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.IsFalse(i.Contains(j));
            else
                Assert.That(j.Contains(i));
        }
    }

    /// <summary>
    /// I starts J
    /// IIIIII
    /// JJJJJJJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXStartsY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(0, 5, false, false), // ()
                new IntervalOfInt(0, 5, false, true), // (]
                new IntervalOfInt(0, 5, true, false), // [)
                new IntervalOfInt(0, 5, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalOfInt(0, 10,false, false), // ()
                new IntervalOfInt(0, 10,false, true), // (]
                new IntervalOfInt(0, 10,true, false), // [)
                new IntervalOfInt(0, 10,true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Starts_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [TestCaseSource("StabCases")]
        public void StaticComparer_Starts_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }

        public static object[] StabCases = new object[] {
                new object[] { I[0], J[0], Is.LessThan(0)},
                new object[] { I[0], J[1], Is.LessThan(0)},
                new object[] { I[0], J[2], Is.GreaterThan(0)},
                new object[] { I[0], J[3], Is.GreaterThan(0)},
                new object[] { I[1], J[0], Is.LessThan(0)},
                new object[] { I[1], J[1], Is.LessThan(0)},
                new object[] { I[1], J[2], Is.GreaterThan(0)},
                new object[] { I[1], J[3], Is.GreaterThan(0)},
                new object[] { I[2], J[0], Is.LessThan(0)},
                new object[] { I[2], J[1], Is.LessThan(0)},
                new object[] { I[2], J[2], Is.LessThan(0)},
                new object[] { I[2], J[3], Is.LessThan(0)},
                new object[] { I[3], J[0], Is.LessThan(0)},
                new object[] { I[3], J[1], Is.LessThan(0)},
                new object[] { I[3], J[2], Is.LessThan(0)},
                new object[] { I[3], J[3], Is.LessThan(0)},

                new object[] { J[0], I[0], Is.GreaterThan(0)},
                new object[] { J[0], I[1], Is.GreaterThan(0)},
                new object[] { J[0], I[2], Is.GreaterThan(0)},
                new object[] { J[0], I[3], Is.GreaterThan(0)},
                new object[] { J[1], I[0], Is.GreaterThan(0)},
                new object[] { J[1], I[1], Is.GreaterThan(0)},
                new object[] { J[1], I[2], Is.GreaterThan(0)},
                new object[] { J[1], I[3], Is.GreaterThan(0)},
                new object[] { J[2], I[0], Is.LessThan(0)},
                new object[] { J[2], I[1], Is.LessThan(0)},
                new object[] { J[2], I[2], Is.GreaterThan(0)},
                new object[] { J[2], I[3], Is.GreaterThan(0)},
                new object[] { J[3], I[0], Is.LessThan(0)},
                new object[] { J[3], I[1], Is.LessThan(0)},
                new object[] { J[3], I[2], Is.GreaterThan(0)},
                new object[] { J[3], I[3], Is.GreaterThan(0)}
            };


        [Test, Combinatorial, Category("IntervalComparer<T>.Contains")]
        public void Contains_Starts_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j)
        {
            Assert.IsFalse(i.Contains(j));
        }

        [TestCaseSource("ContainsCases"), Category("IntervalComparer<T>.Contains")]
        public void Contains_Starts_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Contains(j), expected);
        }

        public static object[] ContainsCases = new object[] {
                new object[] { J[0], I[0], Is.False},
                new object[] { J[0], I[1], Is.False},
                new object[] { J[0], I[2], Is.False},
                new object[] { J[0], I[3], Is.False},
                new object[] { J[1], I[0], Is.False},
                new object[] { J[1], I[1], Is.False},
                new object[] { J[1], I[2], Is.False},
                new object[] { J[1], I[3], Is.False},
                new object[] { J[2], I[0], Is.True},
                new object[] { J[2], I[1], Is.True},
                new object[] { J[2], I[2], Is.False},
                new object[] { J[2], I[3], Is.False},
                new object[] { J[3], I[0], Is.True},
                new object[] { J[3], I[1], Is.True},
                new object[] { J[3], I[2], Is.False},
                new object[] { J[3], I[3], Is.False}
            };
    }

    /// <summary>
    /// I finishes J
    ///      IIIIII
    /// JJJJJJJJJJJ
    /// </summary>
    [TestFixture]
    public class IntervalXFinishesY
    {
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(5, 10, false, false), // ()
                new IntervalOfInt(5, 10, false, true), // (]
                new IntervalOfInt(5, 10, true, false), // [)
                new IntervalOfInt(5, 10, true, true) // []
            };
        private static readonly IInterval<int>[] J = new[] {
                new IntervalOfInt(0, 10, false, false), // ()
                new IntervalOfInt(0, 10, false, true), // (]
                new IntervalOfInt(0, 10, true, false), // [)
                new IntervalOfInt(0, 10, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_Finishes_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            Assert.That(swap ? i.Overlaps(j) : j.Overlaps(i));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_Finishes_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Greater(i.CompareTo(j), 0);
            else
                Assert.Less(j.CompareTo(i), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.Contains")]
        public void Contains_Finishes_Combitorial([ValueSource("I")] IInterval<int> i, [ValueSource("J")] IInterval<int> j)
        {
            Assert.IsFalse(i.Contains(j));
        }

        // Contains
        [TestCaseSource("ContainsCases"), Category("IntervalComparer<T>.Contains")]
        public void Contains_Finishes_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Contains(j), expected);
        }

        public static object[] ContainsCases = new object[] {
                new object[] { J[0], I[0], Is.False},
                new object[] { J[0], I[1], Is.False},
                new object[] { J[0], I[2], Is.False},
                new object[] { J[0], I[3], Is.False},
                new object[] { J[1], I[0], Is.True},
                new object[] { J[1], I[1], Is.False},
                new object[] { J[1], I[2], Is.True},
                new object[] { J[1], I[3], Is.False},
                new object[] { J[2], I[0], Is.False},
                new object[] { J[2], I[1], Is.False},
                new object[] { J[2], I[2], Is.False},
                new object[] { J[2], I[3], Is.False},
                new object[] { J[3], I[0], Is.True},
                new object[] { J[3], I[1], Is.False},
                new object[] { J[3], I[2], Is.True},
                new object[] { J[3], I[3], Is.False}
            };
    }

    /// <summary>
    /// P before I
    /// P
    ///      IIIIII
    /// </summary>
    [TestFixture]
    public class IntervalPBeforeX
    {
        private static readonly IInterval<int> P = new IntervalOfInt(0);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(5, 10, false, false), // ()
                new IntervalOfInt(5, 10, false, true), // (]
                new IntervalOfInt(5, 10, true, false), // [)
                new IntervalOfInt(5, 10, true, true) // []
            };

        //FindOverlaps
        [TestCaseSource(typeof(IntervalPBeforeX), "OverlapCases")]
        public void Overlap_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Overlaps(j), expected);
        }

        public static object[] OverlapCases = new object[] {
                new object[] { I[0], P, Is.False},
                new object[] { I[1], P, Is.False},
                new object[] { I[2], P, Is.False},
                new object[] { I[3], P, Is.False},
                new object[] { P, I[0], Is.False},
                new object[] { P, I[1], Is.False},
                new object[] { P, I[2], Is.False},
                new object[] { P, I[3], Is.False}
            };

        //Sorting
        [TestCaseSource(typeof(IntervalPBeforeX), "SortingCases")]
        public void Sorting_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }

        public static object[] SortingCases = new object[] {
                new object[] { I[0], P, Is.GreaterThan(0)},
                new object[] { I[1], P, Is.GreaterThan(0)},
                new object[] { I[2], P, Is.GreaterThan(0)},
                new object[] { I[3], P, Is.GreaterThan(0)},
                new object[] { P, I[0], Is.LessThan(0)},
                new object[] { P, I[1], Is.LessThan(0)},
                new object[] { P, I[2], Is.LessThan(0)},
                new object[] { P, I[3], Is.LessThan(0)}
            };

        //Contains
        [TestCaseSource("ContainsCases"), Category("IntervalComparer<T>.Contains")]
        public void Contains_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Contains(j), expected);
        }

        public static object[] ContainsCases = new object[] {
                new object[] { P, I[0], Is.False},
                new object[] { P, I[1], Is.False},
                new object[] { P, I[2], Is.False},
                new object[] { P, I[3], Is.False},
                new object[] { I[0], P, Is.False},
                new object[] { I[1], P, Is.False},
                new object[] { I[2], P, Is.False},
                new object[] { I[3], P, Is.False}
            };
    }

    /// <summary>
    /// P starts I
    /// P
    /// IIIIII
    /// </summary>
    [TestFixture]
    public class IntervalPStartsX
    {
        private static readonly IInterval<int> P = new IntervalOfInt(0);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(0, 5, false, false), // ()
                new IntervalOfInt(0, 5, false, true), // (]
                new IntervalOfInt(0, 5, true, false), // [)
                new IntervalOfInt(0, 5, true, true) // []
            };

        [Test]
        public void Overlap()
        {
            Assert.False(I[0].Overlaps(P));
            Assert.False(I[1].Overlaps(P));
            Assert.That(I[2].Overlaps(P));
            Assert.That(I[3].Overlaps(P));

            Assert.False(P.Overlaps(I[0]));
            Assert.False(P.Overlaps(I[1]));
            Assert.That(P.Overlaps(I[2]));
            Assert.That(P.Overlaps(I[3]));
        }

        //Sorting
        [TestCaseSource(typeof(IntervalPStartsX), "StabCases")]
        public void Sorting_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }

        public static object[] StabCases = new object[] {
                new object[] { I[0], P, Is.GreaterThan(0)},
                new object[] { I[1], P, Is.GreaterThan(0)},
                new object[] { I[2], P, Is.GreaterThan(0)},
                new object[] { I[3], P, Is.GreaterThan(0)},
                new object[] { P, I[0], Is.LessThan(0)},
                new object[] { P, I[1], Is.LessThan(0)},
                new object[] { P, I[2], Is.LessThan(0)},
                new object[] { P, I[3], Is.LessThan(0)}
            };

        [Test]
        public void Contains()
        {
            Assert.False(P.Contains(I[0]));
            Assert.False(P.Contains(I[1]));
            Assert.False(P.Contains(I[2]));
            Assert.False(P.Contains(I[3]));

            Assert.False(I[0].Contains(P));
            Assert.False(I[1].Contains(P));
            Assert.False(I[2].Contains(P));
            Assert.False(I[3].Contains(P));
        }
    }

    /// <summary>
    /// P overlaps I
    ///      P
    /// IIIIIIIIIII
    /// </summary>
    [TestFixture]
    public class IntervalPOverlapsX
    {
        private static readonly IInterval<int> P = new IntervalOfInt(5);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(0, 10, false, false), // ()
                new IntervalOfInt(0, 10, false, true), // (]
                new IntervalOfInt(0, 10, true, false), // [)
                new IntervalOfInt(0, 10, true, true) // []
            };

        [Test]
        public void Overlap()
        {
            Assert.That(I[0].Overlaps(P));
            Assert.That(I[1].Overlaps(P));
            Assert.That(I[2].Overlaps(P));
            Assert.That(I[3].Overlaps(P));

            Assert.That(P.Overlaps(I[0]));
            Assert.That(P.Overlaps(I[1]));
            Assert.That(P.Overlaps(I[2]));
            Assert.That(P.Overlaps(I[3]));
        }

        //Sorting
        [TestCaseSource(typeof(IntervalPOverlapsX), "StabCases")]
        public void Sorting_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }

        public static object[] StabCases = new object[] {
                new object[] { I[0], P, Is.LessThan(0)},
                new object[] { I[1], P, Is.LessThan(0)},
                new object[] { I[2], P, Is.LessThan(0)},
                new object[] { I[3], P, Is.LessThan(0)},
                new object[] { P, I[0], Is.GreaterThan(0)},
                new object[] { P, I[1], Is.GreaterThan(0)},
                new object[] { P, I[2], Is.GreaterThan(0)},
                new object[] { P, I[3], Is.GreaterThan(0)}
            };

        [Test]
        public void Contains()
        {
            Assert.False(P.Contains(I[0]));
            Assert.False(P.Contains(I[1]));
            Assert.False(P.Contains(I[2]));
            Assert.False(P.Contains(I[3]));

            Assert.That(I[0].Contains(P));
            Assert.That(I[1].Contains(P));
            Assert.That(I[2].Contains(P));
            Assert.That(I[3].Contains(P));
        }
    }

    /// <summary>
    /// P finishes X
    ///      P
    /// XXXXXX
    /// </summary>
    [TestFixture]
    public class IntervalPFinishesX
    {
        private static readonly IInterval<int> P = new IntervalOfInt(5);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(0, 5, false, false), // ()
                new IntervalOfInt(0, 5, false, true), // (]
                new IntervalOfInt(0, 5, true, false), // [)
                new IntervalOfInt(0, 5, true, true), // []
            };

        [TestCaseSource("OverlapCases"), Category("IntervalComparer<T>.Overlaps")]
        public void Overlap_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.Overlaps(j), expected);
        }

        public static object[] OverlapCases = new object[] {
                new object[] { I[0], P, Is.False},
                new object[] { I[1], P, Is.True},
                new object[] { I[2], P, Is.False},
                new object[] { I[3], P, Is.True},
                new object[] { P, I[0], Is.False},
                new object[] { P, I[1], Is.True},
                new object[] { P, I[2], Is.False},
                new object[] { P, I[3], Is.True}
            };


        // Sorting
        [TestCaseSource(typeof(IntervalPFinishesX), "StabCases")]
        public void Sorting_TestCase(IInterval<int> i, IInterval<int> j, IResolveConstraint expected)
        {
            Assert.That(i.CompareTo(j), expected);
        }
        public static object[] StabCases = new object[] {
                new object[] { I[0], P, Is.LessThan(0)},
                new object[] { I[1], P, Is.LessThan(0)},
                new object[] { I[2], P, Is.LessThan(0)},
                new object[] { I[3], P, Is.LessThan(0)},
                new object[] { P, I[0], Is.GreaterThan(0)},
                new object[] { P, I[1], Is.GreaterThan(0)},
                new object[] { P, I[2], Is.GreaterThan(0)},
                new object[] { P, I[3], Is.GreaterThan(0)},
            };

        [Test]
        public void Contains()
        {
            Assert.False(P.Contains(I[0]));
            Assert.False(P.Contains(I[1]));
            Assert.False(P.Contains(I[2]));
            Assert.False(P.Contains(I[3]));
            Assert.False(I[0].Contains(P));
            Assert.False(I[1].Contains(P));
            Assert.False(I[2].Contains(P));
            Assert.False(I[3].Contains(P));
        }
    }

    /// <summary>
    /// P after I
    ///           P
    /// IIIIII
    /// </summary>
    [TestFixture]
    public class IntervalPAfterX
    {
        private static readonly IInterval<int> P = new IntervalOfInt(10);
        private static readonly IInterval<int>[] I = new[] {
                new IntervalOfInt(0, 5, false, false), // ()
                new IntervalOfInt(0, 5, false, true), // (]
                new IntervalOfInt(0, 5, true, false), // [)
                new IntervalOfInt(0, 5, true, true) // []
            };

        [Test, Combinatorial, Category("IntervalComparer<T>.Overlaps")]
        public void Overlaps_After_Combitorial([ValueSource("I")] IInterval<int> i, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? P.Overlaps(i) : i.Overlaps(P));
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.StaticCompare")]
        public void StaticCompare_After_Combitorial([ValueSource("I")] IInterval<int> i, [Values(true, false)] bool swap)
        {
            if (swap)
                Assert.Greater(P.CompareTo(i), 0);
            else
                Assert.Less(i.CompareTo(P), 0);
        }

        [Test, Combinatorial, Category("IntervalComparer<T>.Contains")]
        public void Contains_After_Combitorial([ValueSource("I")] IInterval<int> i, [Values(true, false)] bool swap)
        {
            Assert.IsFalse(swap ? P.Contains(i) : i.Contains(P));
        }


        private IInterval<int>[] BenchmarkIntervals()
        {
            var intervals = new ArrayList<IInterval<int>>();

            var size = 10;

            for (int low = 0; low < size; low++)
            {
                intervals.Add(new IntervalOfInt(low));

                for (int high = low + 1; high < size; high++)
                {
                    for (int lowInc = 0; lowInc < 2; lowInc++)
                    {
                        var lowIncluded = Convert.ToBoolean(lowInc);
                        for (int highInc = 0; highInc < 2; highInc++)
                        {
                            var highIncluded = Convert.ToBoolean(highInc);

                            intervals.Add(new IntervalOfInt(low, high, lowIncluded, highIncluded));
                        }
                    }
                }
            }

            return intervals.ToArray();
        }

        [Test]
        public void Benchmark()
        {
            var intervals = BenchmarkIntervals();
            var repetitions = 100000;

            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < repetitions; i++)
                foreach (var x in intervals)
                    foreach (var y in intervals)
                        x.Equals(y);

            sw.Stop();

            Console.WriteLine("Time for {0}: {1} ms",
                "Equals",
                sw.ElapsedMilliseconds
            );
        }
    }
    // ReSharper restore CoVariantArrayConversion

}
