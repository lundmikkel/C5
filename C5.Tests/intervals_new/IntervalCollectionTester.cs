using System;
using System.Linq;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals_new
{
    using Interval = IntervalBase<int>;
    using ITH = IntervalTestHelper;

    [TestFixture]
    abstract class IntervalCollectionTester
    {
        /*
         * Things to check for for each method:
         * [ ] Empty collection                                 (EmptyCollection)
         * [ ] Single interval collection                       (SingleInterval)
         * [ ] Many intervals collection - all same object      (SingleObject)
         * [ ] Many intervals collection - all same interval    (DuplicateIntervals)
         * [ ] Many intervals collection                        (ManyIntervals)
         */

        #region Meta

        private Random _random;

        private int Count { get; set; }

        [SetUp]
        public void SetUp()
        {
            var seed = new Random().Next(Int32.MinValue, Int32.MaxValue);
            _random = new Random(seed);
            Console.Out.WriteLine("Seed: {0}", seed);

            Count = _random.Next(10, 20);
        }

        private int randomInt()
        {
            return _random.Next(Int32.MinValue, Int32.MaxValue);
        }

        protected abstract Type GetCollectionType();

        /// <summary>
        /// Override to add additional parameters to constructors.
        /// </summary>
        /// <returns>An object array of the extra parameters.</returns>
        protected virtual object[] AdditionalParameters()
        {
            Console.Out.WriteLine("No additional parameters.");
            return new object[0];
        }

        protected IIntervalCollection<IInterval<T>, T> CreateCollection<T>(params IInterval<T>[] intervals) where T : IComparable<T>
        {
            var additionalParameters = AdditionalParameters();
            var parameters = new object[1 + additionalParameters.Length];
            parameters[0] = intervals;
            for (var i = 0; i < additionalParameters.Length; i++)
                parameters[i + 1] = additionalParameters[i];

            Type[] typeArgs = { typeof(IInterval<T>), typeof(T) };
            var genericType = GetCollectionType().MakeGenericType(typeArgs);
            return (IIntervalCollection<IInterval<T>, T>) Activator.CreateInstance(genericType, parameters);
        }

        protected IIntervalCollection<IInterval<T>, T> CreateEmptyCollection<T>() where T : IComparable<T>
        {
            return CreateCollection(new IInterval<T>[0]);
        }

        #endregion

        #region Test Methods

        #region Code Contracts

        [Test]
        [Category("Code Contracts")]
        public void CodeContracts_VerifyPreconditionsAreInAssembly()
        {
            const string contractExceptionName = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";
            var coll = CreateEmptyCollection<int>();

            try
            {
                var overlaps = coll.FindOverlaps(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                if (e.GetType().FullName != contractExceptionName)
                    throw;

                Assert.Pass();
            }
        }

        #endregion

        #region Collection Value

        #region IsEmpty
        #endregion

        #region Count

        [Test]
        [Category("Count")]
        public void Count_EmptyCollection_Zero()
        {
            var coll = CreateEmptyCollection<int>();

            Assert.AreEqual(0, coll.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_SingleInterval_One()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateCollection(interval);

            Assert.AreEqual(1, coll.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_SingleObject_CountOrOne()
        {
            var intervals = ITH.SingleObject(Count);
            var coll = CreateCollection(intervals);

            Assert.AreEqual(coll.AllowsReferenceDuplicates ? Count : 1, coll.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_DuplicateIntervals_Count()
        {
            var intervals = ITH.DuplicateIntervals(Count);
            var coll = CreateCollection(intervals);

            Assert.AreEqual(Count, coll.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_ManyIntervals_Count()
        {
            var intervals = ITH.ManyIntervals(Count);
            var coll = CreateCollection(intervals);

            Assert.AreEqual(Count, coll.Count);
        }

        #endregion

        #region CountSpeed
        #endregion

        #region Choose

        [Test]
        [Category("Choose")]
        [ExpectedException(typeof(NoSuchItemException))]
        public void Choose_EmptyCollection_Exception()
        {
            var interval = CreateEmptyCollection<int>().Choose();
        }

        [Test]
        [Category("Choose")]
        public void Choose_SingleInterval_IntervalIsSameAsChoose()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateCollection(interval);
            var choose = coll.Choose();

            Assert.AreSame(interval, choose);
        }

        [Test]
        [Category("Choose")]
        public void Choose_SingleObject_IntervalsAreSameAsChoose()
        {
            var intervals = ITH.SingleObject(Count);
            var coll = CreateCollection(intervals);
            var choose = coll.Choose();

            Assert.True(intervals.All(x => ReferenceEquals(x, choose)));
        }

        [Test]
        [Category("Choose")]
        public void Choose_DuplicateIntervals_OneIntervalIsSameAsChoose()
        {
            var intervals = ITH.ManyIntervals(Count);
            var coll = CreateCollection(intervals);
            var choose = coll.Choose();

            Assert.AreEqual(1, intervals.Count(x => ReferenceEquals(x, choose)));
        }

        [Test]
        [Category("Choose")]
        public void Choose_ManyIntervals_OneIntervalIsSameAsChoose()
        {
            var intervals = ITH.ManyIntervals(Count);
            var coll = CreateCollection(intervals);
            var choose = coll.Choose();

            Assert.AreEqual(1, intervals.Count(x => ReferenceEquals(x, choose)));
        }

        #endregion

        #endregion

        #region Enumerable

        [Test]
        [Category("Enumerable")]
        public void Enumerable_EmptyCollection_Empty()
        {
            var coll = CreateEmptyCollection<int>();

            CollectionAssert.IsEmpty(coll);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_SingleInterval_AreEqual()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateCollection(interval);

            CollectionAssert.AreEqual(new[] { interval }, coll);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_SingleObject_AreEquivalent()
        {
            var intervals = ITH.SingleObject(Count);
            var coll = CreateCollection(intervals);

            if (coll.AllowsReferenceDuplicates)
                CollectionAssert.AreEquivalent(intervals, coll);
            else
                CollectionAssert.AreEquivalent(new[] { intervals.First() }, coll);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_DuplicateIntervals_AreEquivalent()
        {
            var intervals = ITH.DuplicateIntervals(Count);
            var coll = CreateCollection(intervals);

            CollectionAssert.AreEquivalent(intervals, coll);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_ManyIntervals_AreEquivalent()
        {
            var intervals = ITH.ManyIntervals(Count);
            var coll = CreateCollection(intervals);

            CollectionAssert.AreEquivalent(intervals, coll);
        }

        #endregion

        #region Interval Collection

        #region Properties

        #region Span

        [Test]
        [Category("Span")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Span_EmptyCollection_Exception()
        {
            // ReSharper disable UnusedVariable
            var span = CreateEmptyCollection<int>().Span;
            // ReSharper restore UnusedVariable
        }

        [Test]
        [Category("Span")]
        public void Span_SingleInterval_IntervalEqualsSpan()
        {
            var interval = ITH.RandomIntInterval();
            var span = CreateCollection(interval).Span;
            Assert.True(span.IntervalEquals(interval));
        }

        [Test]
        [Category("Span")]
        public void Span_NonOverlappingIntervals_JoinedSpan()
        {
            // TODO: Refactor?
            IInterval<int> interval1, interval2;
            do
            {
                interval1 = ITH.RandomIntInterval();
                interval2 = ITH.RandomIntInterval();
            } while (interval1.Overlaps(interval2));

            var joinedSpan = interval1.JoinedSpan(interval2);
            var span = CreateCollection(
                    interval1,
                    interval2
                ).Span;

            Assert.True(joinedSpan.IntervalEquals(span));
        }

        [Test]
        [Category("Span")]
        public void Span_ContainedInterval_ContainerEqualsSpan()
        {
            // TODO: Refactor?
            IInterval<int> interval1, interval2;
            do
            {
                interval1 = ITH.RandomIntInterval();
                interval2 = ITH.RandomIntInterval();
            } while (!interval1.StrictlyContains(interval2));

            var span = CreateCollection(
                    interval1,
                    interval2
                ).Span;

            Assert.True(interval1.IntervalEquals(span));
        }

        #endregion

        #region Maximum Overlap

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_EmptyCollection_Zero()
        {
            var coll = CreateEmptyCollection<int>();

            Assert.AreEqual(0, coll.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_SingleInterval_One()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateCollection(interval);

            Assert.AreEqual(1, coll.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_SingleObject_CountOrOne()
        {
            var intervals = ITH.SingleObject(Count);
            var coll = CreateCollection(intervals);

            Assert.AreEqual(coll.AllowsReferenceDuplicates ? Count : 1, coll.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_DuplicateIntervals_Count()
        {
            var intervals = ITH.DuplicateIntervals(Count);
            var coll = CreateCollection(intervals);

            Assert.AreEqual(Count, coll.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_NonOverlappingIntervals_One()
        {
            var intervals = ITH.NonOverlappingIntervals(Count);
            var coll = CreateCollection(intervals);

            Assert.AreEqual(1, coll.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_BetweenDescreteValues_Two()
        {
            var intervals = new[]
                {
                    new Interval(1, 3, IntervalType.Open),
                    new Interval(2, 4, IntervalType.Open)
                };
            var coll = CreateCollection(intervals);

            Assert.AreEqual(2, coll.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_ManyOverlappingIntervals_Four()
        {
            // 0    5   10   15
            //             |
            //          (--]
            //          (--)
            //          |
            //        []
            //        |
            //      [---]
            //     |
            //    |
            //   |
            //  |
            // |
            // [--------------]
            var intervals = new[]
                {
                    new Interval(12),
                    new Interval(9, 12, IntervalType.HighIncluded),
                    new Interval(9, 12, IntervalType.Open),
                    new Interval(9),
                    new Interval(7, 8, IntervalType.Closed),
                    new Interval(7),
                    new Interval(5, 9, IntervalType.Closed),
                    new Interval(4),
                    new Interval(3),
                    new Interval(2),
                    new Interval(1),
                    new Interval(0),
                    new Interval(0, 15, IntervalType.Closed)
                };
            var coll = CreateCollection(intervals);

            Assert.AreEqual(4, coll.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_AllContainedIntervals_Count()
        {
            // 0    5   10   15   20
            // 
            //          []
            //         [--]
            //        [----]
            //       [------]
            //      [--------]
            //     [----------]
            //    [------------]
            //   [--------------]
            //  [----------------]
            // [------------------]
            var intervals = new[]
                {
                    new Interval(9, 10, IntervalType.Closed),
                    new Interval(8, 11, IntervalType.Closed),
                    new Interval(7, 12, IntervalType.Closed),
                    new Interval(6, 13, IntervalType.Closed),
                    new Interval(5, 14, IntervalType.Closed),
                    new Interval(4, 15, IntervalType.Closed),
                    new Interval(3, 16, IntervalType.Closed),
                    new Interval(2, 17, IntervalType.Closed),
                    new Interval(1, 18, IntervalType.Closed),
                    new Interval(0, 19, IntervalType.Closed)
                };
            var coll = CreateCollection(intervals);

            Assert.AreEqual(coll.Count, coll.MaximumOverlap);
        }

        #endregion

        #region Allows Reference Duplicates

        protected abstract bool AllowsReferenceDuplicates();

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_EmptyCollection_DefinedResult()
        {
            var coll = CreateEmptyCollection<int>();

            Assert.AreEqual(AllowsReferenceDuplicates(), coll.AllowsReferenceDuplicates);
        }

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_SingleInterval_DefinedResult()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateCollection(interval);

            Assert.AreEqual(AllowsReferenceDuplicates(), coll.AllowsReferenceDuplicates);
        }

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_ManyIntervals_DefinedResult()
        {
            var interval = ITH.ManyIntervals(Count);
            var coll = CreateCollection(interval);

            Assert.AreEqual(AllowsReferenceDuplicates(), coll.AllowsReferenceDuplicates);
        }

        #endregion

        #endregion

        #region Find Overlaps

        #region Stabbing

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_EmptyCollection_Empty()
        {
            var query = randomInt();
            var coll = CreateEmptyCollection<int>();

            CollectionAssert.IsEmpty(coll.FindOverlaps(query));
        }

        #region Endpoint Inclusion

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_SingleIntervalAllEndpointCombinations_Overlaps()
        {
            var interval = ITH.RandomIntInterval();
            var intervals = Enumerable.Range(0, 4).Select(i => new Interval(interval.Low, interval.High, (IntervalType) i)).ToArray();
            var coll = CreateCollection(intervals);

            CollectionAssert.AreEquivalent(intervals.Where(x => x.LowIncluded), coll.FindOverlaps(interval.Low));
            CollectionAssert.AreEquivalent(intervals.Where(x => x.HighIncluded), coll.FindOverlaps(interval.High));
            CollectionAssert.AreEquivalent(intervals, coll.FindOverlaps(interval.Low / 2 + interval.High / 2));
        }

        #endregion

        #endregion

        #region Range

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_EmptyCollection_Empty()
        {
            var query = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            CollectionAssert.IsEmpty(coll.FindOverlaps(query));
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void BensTest()
        {
            // ****************************************
            // | 0    5   10   15   20   25   30   35 |
            // | |    |    |    |    |    |    |    | |
            // | Container intervals:                 |
            // |                           [E  ]      |
            // |                     [D  ]            |
            // |                [C   ]                |
            // |            [B  ]                     |
            // |      [A  ]                           |
            // |                                      |
            // |                                      |
            // | Test intervals:                      |
            // |                                [   ] |
            // |                               [    ] |
            // |                            [ ]       |
            // |                           [        ] |
            // |                           [   ]      |
            // |                          []          |
            // |                         [ ]          |
            // |                     [         ]      |
            // |          [                ]          |
            // |          [ ]                         |
            // |          []                          |
            // |       [ ]                            |
            // |      [                        ]      |
            // |      [         )                     |
            // |      [   ]                           |
            // | [                                   ]|
            // | [        ]                           |
            // | [    ]                               |
            // | [   ]                                |
            // ****************************************
            var intervals = new[] {
                new IntervalBase<int>(5, 9, true, true),
                new IntervalBase<int>(11, 15, true, true),
                new IntervalBase<int>(15, 20, true, true),
                new IntervalBase<int>(20, 24, true, true),
                new IntervalBase<int>(26, 30, true, true)
            };

            var coll = CreateCollection(intervals);

            var ranges = new[] {
                    new Interval(  9,  26, IntervalType.Closed),
                    new Interval(  5,  30, IntervalType.Closed),
                    new Interval(  0,  35, IntervalType.Closed),
                    new Interval( 27,  29, IntervalType.Closed),
                    new Interval( 26,  30, IntervalType.Closed),
                    new Interval( 26,  26, IntervalType.Closed),
                    new Interval( 24,  26, IntervalType.Closed),
                    new Interval( 20,  30, IntervalType.Closed),
                    new Interval( 26,  35, IntervalType.Closed),
                    new Interval( 30,  35, IntervalType.Closed),
                    new Interval( 31,  35, IntervalType.Closed),
                    new Interval( 10,  11, IntervalType.Closed),
                    new Interval( 10,  10, IntervalType.Closed),
                    new Interval(  6,   8, IntervalType.Closed),
                    new Interval(  5,  15, IntervalType.LowIncluded),
                    new Interval(  5,   9, IntervalType.Closed),
                    new Interval(  0,  10, IntervalType.Closed),
                    new Interval(  0,   5, IntervalType.Closed),
                    new Interval(  0,   4, IntervalType.Closed)
                };

            foreach (var interval in ranges)
                CollectionAssert.AreEquivalent(intervals.Where(x => x.Overlaps(interval)), coll.FindOverlaps(interval));
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_ManyIntervals_ChooseOverlapsInCollection()
        {
            var intervals = ITH.ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = collection.Choose();
            var overlappingIntervals = collection.FindOverlaps(interval);
            Assert.True(overlappingIntervals.Count().CompareTo(0) == 1);
        }
        
        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_ManyIntervals_ChooseOverlapsNotInCollection()
        {
            var intervals = ITH.ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = ITH.RandomIntInterval();
            while (intervals.Any(x => x.Overlaps(interval)))
                interval = ITH.RandomIntInterval();
            var overlappingIntervals = collection.FindOverlaps(interval);
            Assert.True(overlappingIntervals.Count().Equals(0));
        }

        #endregion

        #endregion

        #region Find Overlap

        #region Stabbing

        [Test]
        [Category("Find Overlap Stabbing")]
        public void FindOverlapStabbing_EmptyCollection_False()
        {
            var query = randomInt();
            IInterval<int> interval = null;
            var coll = CreateEmptyCollection<int>();

            Assert.False(coll.FindOverlap(query, ref interval));
            Assert.IsNull(interval);
        }

        #endregion

        #region Range

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_EmptyCollection_False()
        {
            var query = ITH.RandomIntInterval();
            IInterval<int> interval = null;
            var coll = CreateEmptyCollection<int>();

            Assert.False(coll.FindOverlap(query, ref interval));
            Assert.IsNull(interval);
        }

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_ManyIntervals_ChooseOverlapsInCollection()
        {
            var intervals = ITH.ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = collection.Choose();
            IInterval<int> overlap = null;
            Assert.True(collection.FindOverlap(interval, ref overlap));
            Assert.True(interval.Overlaps(overlap));
        }

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_ManyIntervals_ChooseOverlapsNotInCollection()
        {
            var intervals = ITH.ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = ITH.RandomIntInterval();
            IInterval<int> overlap = null;
            while (intervals.Any(x => x.Overlaps(interval)))
                interval = ITH.RandomIntInterval();
            Assert.False(collection.FindOverlap(interval, ref overlap));
            Assert.IsNull(overlap);
        }

        #endregion

        #endregion

        #region Count Overlaps

        #region Stabbing

        [Test]
        [Category("Count Overlaps Stabbing")]
        public void CountOverlapsStabbing_EmptyCollection_Zero()
        {
            var query = randomInt();
            var coll = CreateEmptyCollection<int>();

            Assert.AreEqual(0, coll.CountOverlaps(query));
        }

        #endregion

        #region Range

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_EmptyCollection_Zero()
        {
            var query = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            Assert.AreEqual(0, coll.CountOverlaps(query));
        }

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_ManyIntervals_ChooseOverlapsInCollection()
        {
            var intervals = ITH.ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = collection.Choose();
            var numberOfoverlappingIntervals = collection.CountOverlaps(interval);
            Assert.True(numberOfoverlappingIntervals.CompareTo(0) == 1);
        }

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_ManyIntervals_ChooseOverlapsNotInCollection()
        {
            var intervals = ITH.ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = ITH.RandomIntInterval();
            while (intervals.Any(x => x.Overlaps(interval)))
                interval = ITH.RandomIntInterval();
            var numberOfoverlappingIntervals = collection.CountOverlaps(interval);
            Assert.True(numberOfoverlappingIntervals.Equals(0));
        }

        #endregion

        #endregion

        #region Extensible

        #region Is Read Only

        #endregion

        #region Add

        [Test]
        [Category("Add")]
        public void Add_IsReadOnly_Exception()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            if (coll.IsReadOnly)
            {
                try
                {
                    coll.Add(interval);
                    Assert.Fail();
                }
                catch (ReadOnlyCollectionException)
                {
                    Assert.Pass();
                }
            }
        }

        [Test]
        [Category("Add")]
        public void Add_SingleObject_FirstAdded()
        {
            var intervals = ITH.SingleObject(Count);
            var coll = CreateEmptyCollection<int>();

            if (!coll.IsReadOnly)
            {
                Assert.True(coll.Add(intervals.First()));

                foreach (var interval in intervals)
                    Assert.True(coll.Add(interval) == coll.AllowsReferenceDuplicates);
            }
        }

        [Test]
        [Category("Add")]
        public void Add_DuplicateIntervals_AllAdded()
        {
            var intervals = ITH.DuplicateIntervals(Count);
            var coll = CreateEmptyCollection<int>();

            if (!coll.IsReadOnly)
            {
                foreach (var interval in intervals)
                    Assert.True(coll.Add(interval));
            }
        }

        [Test]
        [Category("Add")]
        public void Add_ManyIntervals_AllAdded()
        {
            var intervals = ITH.ManyIntervals(Count);
            var coll = CreateEmptyCollection<int>();

            if (!coll.IsReadOnly)
            {
                foreach (var interval in intervals)
                    Assert.True(coll.Add(interval));
            }
        }

        #region Events
        #endregion

        #endregion

        #region Add All

        [Test]
        [Category("Add All")]
        public void AddAll_IsReadOnly_Exception()
        {
            var intervals = new[] { ITH.RandomIntInterval() };
            var coll = CreateEmptyCollection<int>();

            if (coll.IsReadOnly)
            {
                try
                {
                    coll.AddAll(intervals);
                    Assert.Fail();
                }
                catch (ReadOnlyCollectionException)
                {
                    Assert.Pass();
                }
            }
        }

        #region Events
        #endregion

        #endregion

        #region Remove

        [Test]
        [Category("Remove")]
        public void Remove_IsReadOnly_Exception()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            if (coll.IsReadOnly)
            {
                try
                {
                    coll.Remove(interval);
                    Assert.Fail();
                }
                catch (ReadOnlyCollectionException)
                {
                    Assert.Pass();
                }
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_EmptyCollection_False()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateEmptyCollection<int>();

            if (!coll.IsReadOnly)
            {
                Assert.False(coll.Remove(interval));
            }
        }

        #region Events
        #endregion

        #endregion

        #region Clear

        [Test]
        [Category("Clear")]
        public void Clear_IsReadOnly_Exception()
        {
            var coll = CreateEmptyCollection<int>();

            if (coll.IsReadOnly)
            {
                try
                {
                    coll.Clear();
                    Assert.Fail();
                }
                catch (ReadOnlyCollectionException)
                {
                    Assert.Pass();
                }
            }
        }

        [Test]
        [Category("Clear")]
        public void Clear_EmptyCollection_IsEmpty()
        {
            var coll = CreateEmptyCollection<int>();

            if (!coll.IsReadOnly)
            {
                Assert.True(coll.IsEmpty);
                coll.Clear();
                Assert.True(coll.IsEmpty);
            }
        }

        [Test]
        [Category("Clear")]
        public void Clear_SingleInterval_BecomesEmpty()
        {
            var interval = ITH.RandomIntInterval();
            var coll = CreateCollection(interval);

            if (!coll.IsReadOnly)
            {
                Assert.False(coll.IsEmpty);
                coll.Clear();
                Assert.True(coll.IsEmpty);
            }
        }

        [Test]
        [Category("Clear")]
        public void Clear_ManyIntervals_BecomesEmpty()
        {
            var interval = ITH.ManyIntervals(Count);
            var coll = CreateCollection(interval);

            if (!coll.IsReadOnly)
            {
                Assert.False(coll.IsEmpty);
                coll.Clear();
                Assert.True(coll.IsEmpty);
            }
        }

        #region Events
        // TODO: Should this be handled in the other tests or should we duplicate the construction parts and verify on them that the events were thrown?
        #endregion

        #endregion

        #endregion

        #endregion

        #region Example Cases

        #region IBS Article (Extended)

        [Test]
        [Category("Example Cases")]
        public void Stabbing_IntervalBinarySearchTreeArticleExampleExtended_OverlapsAndSpan()
        {
            //*****************************
            //   0     5    10    15    20
            //   |     |     |     |     |
            //             A[----------]
            //    B[-----]
            //   C[-)
            //                      D(---]
            //           E1[----]   
            //           E2[----]   
            //                        F|
            //...--------------------]G
            //        H(-----)
            //           
            //*****************************
            var intervals = new[]
                {
                    new Interval(9, 19, IntervalType.Closed),                   // A
                    new Interval(2, 7, IntervalType.Closed),                    // B
                    new Interval(1, 3, IntervalType.LowIncluded),               // C
                    new Interval(17, 20, IntervalType.HighIncluded),            // D
                    new Interval(8, 12, IntervalType.Closed),                   // E1
                    new Interval(8, 12, IntervalType.Closed),                   // E2
                    new Interval(18),                                           // F
                    new Interval(int.MinValue, 17, IntervalType.HighIncluded),  // G
                    new Interval(5, 10, IntervalType.Open)                      // H
                };

            var coll = CreateCollection(intervals);

            foreach (var point in intervals.UniqueEndpoints())
            {
                var expected = intervals.Where(x => x.Overlaps(point));
                CollectionAssert.AreEquivalent(expected, coll.FindOverlaps(point));
            }

            var span = new Interval(int.MinValue, 20, false, true);
            Assert.True(span.IntervalEquals(coll.Span));

            Assert.AreEqual(5, coll.MaximumOverlap);

            if (!coll.IsReadOnly)
            {
                // TODO: Finish. This is just the intervals from the old test being added and removed!
                var additionalIntervals = new[]
                {
                    new Interval(1, 2,  IntervalType.Closed),
                    new Interval(1, 4,  IntervalType.Closed),
                    new Interval(6, 12, IntervalType.Closed),
                    new Interval(5, 21, IntervalType.Closed)
                };

                foreach (var interval in additionalIntervals)
                {
                    coll.Add(interval);
                    coll.Remove(interval);
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
