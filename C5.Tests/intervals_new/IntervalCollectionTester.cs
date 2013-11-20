using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals_new
{
    using Interval = IntervalBase<int>;

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
            seed = 1352270728;
            _random = new Random(seed);
            Console.Out.WriteLine("Seed: {0}", seed);

            Count = _random.Next(10, 20);
        }

        private int randomInt()
        {
            return _random.Next(Int32.MinValue, Int32.MaxValue);
        }

        public IInterval<int> SingleInterval()
        {
            var low = _random.Next(Int32.MinValue, Int32.MaxValue);
            var high = _random.Next(low + 2, Int32.MaxValue);

            return new IntervalBase<int>(low, high, (IntervalType) _random.Next(0, 4));
        }

        public IInterval<int>[] ManyIntervals(int count)
        {
            Contract.Ensures(Contract.Result<IEnumerable<IInterval<int>>>().Count() == count);

            return Enumerable.Range(0, count).Select(i => SingleInterval()).ToArray();
        }

        public IInterval<int>[] DuplicateIntervals(int count)
        {
            var interval = SingleInterval();
            return Enumerable.Range(0, count).Select(i => new IntervalBase<int>(interval)).ToArray();
        }

        public IInterval<int>[] SingleObject(int count)
        {
            var interval = SingleInterval();
            return Enumerable.Range(0, count).Select(i => interval).ToArray();
        }

        public IInterval<int> SinglePoint()
        {
            return new IntervalBase<int>(_random.Next(Int32.MinValue, Int32.MaxValue));
        }

        public static IInterval<int>[] NonOverlappingIntervals(int count, int length = 1, int space = 0)
        {
            Contract.Ensures(Contract.ForAll(0, count, i => Contract.ForAll(i, count, j => !Contract.Result<IInterval<int>[]>()[i].Overlaps(Contract.Result<IInterval<int>[]>()[j]))));

            var intervals = new IInterval<int>[count];

            var low = 0;
            for (var i = 0; i < count; i++)
            {
                intervals[i] = new IntervalBase<int>(low, low + length, IntervalType.LowIncluded);
                low += length + space;
            }

            return intervals;
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
            var collection = CreateEmptyCollection<int>();

            try
            {
                var overlaps = collection.FindOverlaps(null);
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
            var collection = CreateEmptyCollection<int>();

            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_SingleInterval_One()
        {
            var interval = SingleInterval();
            var collection = CreateCollection(interval);

            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_SingleObject_CountOrOne()
        {
            var intervals = SingleObject(Count);
            var collection = CreateCollection(intervals);

            Assert.AreEqual(collection.AllowsReferenceDuplicates ? Count : 1, collection.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_DuplicateIntervals_Count()
        {
            var intervals = DuplicateIntervals(Count);
            var collection = CreateCollection(intervals);

            Assert.AreEqual(Count, collection.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_ManyIntervals_Count()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);

            Assert.AreEqual(Count, collection.Count);
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
            var interval = SingleInterval();
            var collection = CreateCollection(interval);
            var choose = collection.Choose();

            Assert.AreSame(interval, choose);
        }

        [Test]
        [Category("Choose")]
        public void Choose_SingleObject_IntervalsAreSameAsChoose()
        {
            var intervals = SingleObject(Count);
            var collection = CreateCollection(intervals);
            var choose = collection.Choose();

            Assert.True(intervals.All(x => ReferenceEquals(x, choose)));
        }

        [Test]
        [Category("Choose")]
        public void Choose_DuplicateIntervals_OneIntervalIsSameAsChoose()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var choose = collection.Choose();

            Assert.AreEqual(1, intervals.Count(x => ReferenceEquals(x, choose)));
        }

        [Test]
        [Category("Choose")]
        public void Choose_ManyIntervals_OneIntervalIsSameAsChoose()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var choose = collection.Choose();

            Assert.AreEqual(1, intervals.Count(x => ReferenceEquals(x, choose)));
        }

        #endregion

        #endregion

        #region Enumerable

        [Test]
        [Category("Enumerable")]
        public void Enumerable_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<int>();

            CollectionAssert.IsEmpty(collection);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_SingleInterval_AreEqual()
        {
            var interval = SingleInterval();
            var collection = CreateCollection(interval);

            CollectionAssert.AreEqual(new[] { interval }, collection);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_SingleObject_AreEquivalent()
        {
            var intervals = SingleObject(Count);
            var collection = CreateCollection(intervals);

            if (collection.AllowsReferenceDuplicates)
                CollectionAssert.AreEquivalent(intervals, collection);
            else
                CollectionAssert.AreEquivalent(new[] { intervals.First() }, collection);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_DuplicateIntervals_AreEquivalent()
        {
            var intervals = DuplicateIntervals(Count);
            var collection = CreateCollection(intervals);

            CollectionAssert.AreEquivalent(intervals, collection);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_ManyIntervals_AreEquivalent()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);

            CollectionAssert.AreEquivalent(intervals, collection);
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
            var interval = SingleInterval();
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
                interval1 = SingleInterval();
                interval2 = SingleInterval();
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
                interval1 = SingleInterval();
                interval2 = SingleInterval();
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
            var collection = CreateEmptyCollection<int>();

            Assert.AreEqual(0, collection.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_SingleInterval_One()
        {
            var interval = SingleInterval();
            var collection = CreateCollection(interval);

            Assert.AreEqual(1, collection.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_SingleObject_CountOrOne()
        {
            var intervals = SingleObject(Count);
            var collection = CreateCollection(intervals);

            Assert.AreEqual(collection.AllowsReferenceDuplicates ? Count : 1, collection.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_DuplicateIntervals_Count()
        {
            var intervals = DuplicateIntervals(Count);
            var collection = CreateCollection(intervals);

            Assert.AreEqual(Count, collection.MaximumOverlap);
        }

        [Test]
        [Category("Maximum Overlap")]
        public void MaximumOverlap_NonOverlappingIntervals_One()
        {
            var intervals = NonOverlappingIntervals(Count);
            var collection = CreateCollection(intervals);

            Assert.AreEqual(1, collection.MaximumOverlap);
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
            var collection = CreateCollection(intervals);

            Assert.AreEqual(2, collection.MaximumOverlap);
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
            var collection = CreateCollection(intervals);

            Assert.AreEqual(4, collection.MaximumOverlap);
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
            var collection = CreateCollection(intervals);

            Assert.AreEqual(collection.Count, collection.MaximumOverlap);
        }

        #endregion

        #region Allows Reference Duplicates

        protected abstract bool AllowsReferenceDuplicates();

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_EmptyCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<int>();

            Assert.AreEqual(AllowsReferenceDuplicates(), collection.AllowsReferenceDuplicates);
        }

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_SingleInterval_DefinedResult()
        {
            var interval = SingleInterval();
            var collection = CreateCollection(interval);

            Assert.AreEqual(AllowsReferenceDuplicates(), collection.AllowsReferenceDuplicates);
        }

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_ManyIntervals_DefinedResult()
        {
            var interval = ManyIntervals(Count);
            var collection = CreateCollection(interval);

            Assert.AreEqual(AllowsReferenceDuplicates(), collection.AllowsReferenceDuplicates);
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
            var collection = CreateEmptyCollection<int>();

            CollectionAssert.IsEmpty(collection.FindOverlaps(query));
        }

        #region Endpoint Inclusion

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_SingleIntervalAllEndpointCombinations_Overlaps()
        {
            var interval = SingleInterval();
            var intervals = Enumerable.Range(0, 4).Select(i => new Interval(interval.Low, interval.High, (IntervalType) i)).ToArray();
            var collection = CreateCollection(intervals);

            CollectionAssert.AreEquivalent(intervals.Where(x => x.LowIncluded), collection.FindOverlaps(interval.Low));
            CollectionAssert.AreEquivalent(intervals.Where(x => x.HighIncluded), collection.FindOverlaps(interval.High));
            CollectionAssert.AreEquivalent(intervals, collection.FindOverlaps(interval.Low / 2 + interval.High / 2));
        }

        #endregion

        #endregion

        #region Range

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_EmptyCollection_Empty()
        {
            var query = SingleInterval();
            var collection = CreateEmptyCollection<int>();

            CollectionAssert.IsEmpty(collection.FindOverlaps(query));
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

            var collection = CreateCollection(intervals);

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
                CollectionAssert.AreEquivalent(intervals.Where(x => x.Overlaps(interval)), collection.FindOverlaps(interval));
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_ManyIntervals_ChooseOverlapsInCollection()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = collection.Choose();
            var overlaps = collection.FindOverlaps(interval);
            Assert.True(overlaps.Any());
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_ManyIntervals_ChooseOverlapsNotInCollection()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = SingleInterval();
            while (intervals.Any(x => x.Overlaps(interval)))
                interval = SingleInterval();

            var overlaps = collection.FindOverlaps(interval);
            Assert.True(!overlaps.Any());
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
            var collection = CreateEmptyCollection<int>();

            Assert.False(collection.FindOverlap(query, ref interval));
            Assert.IsNull(interval);
        }

        #endregion

        #region Range

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_EmptyCollection_False()
        {
            var query = SingleInterval();
            IInterval<int> interval = null;
            var collection = CreateEmptyCollection<int>();

            Assert.False(collection.FindOverlap(query, ref interval));
            Assert.IsNull(interval);
        }

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_ManyIntervals_ChooseOverlapsInCollection()
        {
            var intervals = ManyIntervals(Count);
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
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = SingleInterval();
            IInterval<int> overlap = null;
            while (intervals.Any(x => x.Overlaps(interval)))
                interval = SingleInterval();
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
            var collection = CreateEmptyCollection<int>();

            Assert.AreEqual(0, collection.CountOverlaps(query));
        }

        #endregion

        #region Range

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_EmptyCollection_Zero()
        {
            var query = SingleInterval();
            var collection = CreateEmptyCollection<int>();

            Assert.AreEqual(0, collection.CountOverlaps(query));
        }

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_ManyIntervals_ChooseOverlapsInCollection()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);
            var interval = collection.Choose();
            Assert.Greater(collection.CountOverlaps(interval), 0);
        }

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_ManyIntervals_ChooseOverlapsNotInCollection()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);

            var interval = SingleInterval();
            while (intervals.Any(x => x.Overlaps(interval)))
                interval = SingleInterval();

            Assert.AreEqual(0, collection.CountOverlaps(interval));
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
            var interval = SingleInterval();
            var collection = CreateEmptyCollection<int>();

            if (collection.IsReadOnly)
            {
                try
                {
                    collection.Add(interval);
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
            var intervals = SingleObject(Count);
            var collection = CreateEmptyCollection<int>();

            if (!collection.IsReadOnly)
            {
                Assert.True(collection.Add(intervals.First()));

                foreach (var interval in intervals)
                    Assert.True(collection.Add(interval) == collection.AllowsReferenceDuplicates);
            }
        }

        [Test]
        [Category("Add")]
        public void Add_DuplicateIntervals_AllAdded()
        {
            var intervals = DuplicateIntervals(Count);
            var collection = CreateEmptyCollection<int>();

            if (!collection.IsReadOnly)
            {
                foreach (var interval in intervals)
                    Assert.True(collection.Add(interval));
            }
        }

        [Test]
        [Category("Add")]
        public void Add_ManyIntervals_AllAdded()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateEmptyCollection<int>();

            if (!collection.IsReadOnly)
            {
                foreach (var interval in intervals)
                    Assert.True(collection.Add(interval));
            }
        }

        #region Events

        [Test]
        [Category("Add Event")]
        public void Add_ManyIntervals_EventThrown()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateEmptyCollection<int>();

            if (!collection.IsReadOnly)
            {
                IInterval<int> eventInterval = null;
                collection.ItemsAdded += (c, args) => eventInterval = args.Item;

                foreach (var interval in intervals)
                {
                    Assert.True(collection.Add(interval));
                    Assert.AreSame(interval, eventInterval);
                    eventInterval = null;

                    Assert.AreEqual(collection.AllowsReferenceDuplicates, collection.Add(interval));
                    if (collection.AllowsReferenceDuplicates)
                        Assert.AreSame(interval, eventInterval);
                    else
                        Assert.IsNull(eventInterval);
                    eventInterval = null;
                }
            }
        }

        #endregion

        #endregion

        #region Add All

        [Test]
        [Category("Add All")]
        public void AddAll_IsReadOnly_Exception()
        {
            var intervals = new[] { SingleInterval() };
            var collection = CreateEmptyCollection<int>();

            if (collection.IsReadOnly)
            {
                try
                {
                    collection.AddAll(intervals);
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
            var interval = SingleInterval();
            var collection = CreateEmptyCollection<int>();

            if (collection.IsReadOnly)
            {
                try
                {
                    collection.Remove(interval);
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
            var interval = SingleInterval();
            var collection = CreateEmptyCollection<int>();

            if (!collection.IsReadOnly)
            {
                Assert.False(collection.Remove(interval));
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_SingleInterval_Removed()
        {
            var singleInterval = SingleInterval();
            var collection = CreateCollection(singleInterval);

            if (!collection.IsReadOnly)
            {
                Assert.True(collection.Remove(singleInterval));
                Assert.False(collection.Remove(singleInterval));
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_SingleObject_Removed()
        {
            var intervals = SingleObject(Count);
            var collection = CreateCollection(intervals);

            if (!collection.IsReadOnly)
            {
                for (var i = 0; i < Count; i++)
                {
                    if (i == 0)
                        Assert.True(collection.Remove(intervals[i]));
                    else
                        Assert.AreEqual(collection.AllowsReferenceDuplicates, collection.Remove(intervals[i]));
                }
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_DuplicateIntervals_True()
        {
            var intervals = DuplicateIntervals(Count);
            var collection = CreateCollection(intervals);

            if (!collection.IsReadOnly)
            {
                foreach (var interval in intervals)
                {
                    Assert.True(collection.Remove(interval));
                    Assert.False(collection.Remove(interval));
                }
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_ManyIntervals_True()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);

            if (!collection.IsReadOnly)
            {
                foreach (var interval in intervals)
                {
                    Assert.True(collection.Remove(interval));
                    Assert.False(collection.Remove(interval));
                }
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_ManyIntervals_RemovingIntervalsNotInCollection()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);

            if (!collection.IsReadOnly)
            {
                foreach (var interval in ManyIntervals(Count))
                    Assert.False(collection.Remove(interval));
            }
        }

        #region Events

        [Test]
        [Category("Remove Event")]
        public void Remove_ManyIntervals_EventThrown()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals.Concat(intervals).ToArray());

            if (!collection.IsReadOnly)
            {
                IInterval<int> eventInterval = null;
                collection.ItemsRemoved += (c, args) => eventInterval = args.Item;

                foreach (var interval in intervals)
                {
                    Assert.True(collection.Remove(interval));
                    Assert.AreSame(interval, eventInterval);
                    eventInterval = null;

                    Assert.AreEqual(collection.AllowsReferenceDuplicates, collection.Remove(interval));
                    if (collection.AllowsReferenceDuplicates)
                        Assert.AreSame(interval, eventInterval);
                    else
                        Assert.IsNull(eventInterval);
                    eventInterval = null;

                    Assert.False(collection.Remove(interval));
                    Assert.IsNull(eventInterval);
                }
            }
        }

        #endregion

        #endregion

        #region Clear

        [Test]
        [Category("Clear")]
        public void Clear_IsReadOnly_Exception()
        {
            var collection = CreateEmptyCollection<int>();

            if (collection.IsReadOnly)
            {
                try
                {
                    collection.Clear();
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
            var collection = CreateEmptyCollection<int>();

            if (!collection.IsReadOnly)
            {
                Assert.True(collection.IsEmpty);
                collection.Clear();
                Assert.True(collection.IsEmpty);
            }
        }

        [Test]
        [Category("Clear")]
        public void Clear_SingleInterval_BecomesEmpty()
        {
            var interval = SingleInterval();
            var collection = CreateCollection(interval);

            if (!collection.IsReadOnly)
            {
                Assert.False(collection.IsEmpty);
                collection.Clear();
                Assert.True(collection.IsEmpty);
            }
        }

        [Test]
        [Category("Clear")]
        public void Clear_ManyIntervals_BecomesEmpty()
        {
            var interval = ManyIntervals(Count);
            var collection = CreateCollection(interval);

            if (!collection.IsReadOnly)
            {
                Assert.False(collection.IsEmpty);
                collection.Clear();
                Assert.True(collection.IsEmpty);
            }
        }

        #region Events

        [Test]
        [Category("Clear Event")]
        public void Clear_ManyIntervals_EventThrown()
        {
            var intervals = ManyIntervals(Count);
            var collection = CreateCollection(intervals);

            if (!collection.IsReadOnly)
            {
                var eventThrown = false;
                collection.CollectionCleared += (sender, args) => eventThrown = true;

                collection.Clear();
                Assert.True(eventThrown);
                Assert.True(collection.IsEmpty);
                eventThrown = false;

                collection.Clear();
                Assert.False(eventThrown);
                Assert.True(collection.IsEmpty);
            }
        }

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

            var collection = CreateCollection(intervals);

            foreach (var point in intervals.UniqueEndpoints())
            {
                var expected = intervals.Where(x => x.Overlaps(point));
                CollectionAssert.AreEquivalent(expected, collection.FindOverlaps(point));
            }

            var span = new Interval(int.MinValue, 20, false, true);
            Assert.True(span.IntervalEquals(collection.Span));

            Assert.AreEqual(5, collection.MaximumOverlap);

            if (!collection.IsReadOnly)
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
                    collection.Add(interval);
                    collection.Remove(interval);
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
