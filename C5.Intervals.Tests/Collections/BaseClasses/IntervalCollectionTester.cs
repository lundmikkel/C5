using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

namespace C5.Intervals.Tests
{
    [TestFixture]
    abstract class IntervalCollectionTester
    {
        /*
         * Things to check for for each method:
         *  - Empty collection                                 (EmptyCollection)
         *  - Single interval collection                       (SingleInterval)
         *  - Many intervals collection - all same object      (SingleObject)
         *  - Many intervals collection - all same interval    (DuplicateIntervals)
         *  - Many intervals collection                        (ManyIntervals)
         */

        #region Inner Classes

        internal class Interval : TestInterval<int>
        {
            public Interval(int query)
                : base(query)
            {
            }

            public Interval(int low, int high, bool lowIncluded = true, bool highIncluded = false)
                : base(low, high, lowIncluded, highIncluded)
            {
            }

            public Interval(int low, int high, IntervalType type)
                : base(low, high, type)
            {
            }

            public Interval(IInterval<int> i)
                : base(i)
            {
            }

            public Interval(IInterval<int> low, IInterval<int> high)
                : base(low, high)
            {
            }

            public override string ToString()
            {
                return String.Format("{0}{1:#,0} : {2:#,0}{3}",
                    LowIncluded ? "[" : "(",
                    Low,
                    High,
                    HighIncluded ? "]" : ")");
            }
        }

        internal class TestInterval<T> : IntervalBase<T> where T : IComparable<T>
        {
            public int Id { get; private set; }

            private int randomInt()
            {
                var bytes = new byte[4];
                System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }

            public TestInterval(T query)
                : base(query)
            {
                Id = randomInt();
            }

            public TestInterval(T low, T high, bool lowIncluded = true, bool highIncluded = false)
                : base(low, high, lowIncluded, highIncluded)
            {
                Id = randomInt();
            }

            public TestInterval(T low, T high, IntervalType type)
                : base(low, high, type)
            {
                Id = randomInt();
            }

            public TestInterval(IInterval<T> i)
                : base(i)
            {
                Id = randomInt();
            }

            public TestInterval(IInterval<T> low, IInterval<T> high)
                : base(low, high)
            {
                Id = randomInt();
            }
        }

        #endregion

        #region Meta

        protected static Random Random { get; set; }

        private static int Count { get; set; }

        [SetUp]
        public static void SetUp()
        {
            var bytes = new byte[4];
            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(bytes);
            var seed = BitConverter.ToInt32(bytes, 0);

            updateRandom(seed);
        }

        // TODO: refactor this to a property on the tests
        protected static void updateRandom(int seed)
        {
            Random = new Random(seed);
            Console.Out.WriteLine("Seed: {0}", seed);

            Count = Random.Next(10, 20);
        }

        protected int randomInt()
        {
            return Random.Next(Int32.MinValue, Int32.MaxValue);
        }

        public static Interval SingleInterval(int maxLength = -1, int type = -1)
        {
            var low = Random.Next(Int32.MinValue + 3, Int32.MaxValue - 3);
            var max = maxLength <= 2 ? Int32.MaxValue : low + maxLength;
            if (max < low) max = Int32.MaxValue - 3;
            var high = Random.Next(low + 2, max);
            var intervalType = 0 <= type && type < 4 ? (IntervalType) type : (IntervalType) Random.Next(0, 4);
            return new Interval(low, high, intervalType);
        }

        public static Interval[] ManyIntervals(int count = -1, int maxLength = -1)
        {
            Contract.Ensures(Contract.Result<IEnumerable<IInterval<int>>>().Count() == Count);

            if (count < 0)
                count = Count;
            else
                Count = count;

            return Enumerable.Range(0, count).Select(i => SingleInterval(maxLength)).ToArray();
        }

        public static IEnumerable<Interval> InsertedIntervals(IIntervalCollection<Interval, int> collection, IEnumerable<Interval> intervals)
        {
            if (!collection.AllowsOverlaps)
                return NonOverlapping(intervals);

            var insertedInterval = !collection.AllowsContainments ? NonContained(intervals) : intervals;

            return !collection.AllowsReferenceDuplicates ? ReferenceDuplicateFree(insertedInterval) : insertedInterval;
        }

        public static IEnumerable<Interval> ReferenceDuplicateFree(IEnumerable<Interval> intervals)
        {
            var list = new List<Interval>();

            foreach (var interval in intervals)
                if (list.All(x => !ReferenceEquals(x, interval)))
                    list.Add(interval);

            return list;
        }

        public static IEnumerable<Interval> NonContained(IEnumerable<Interval> intervals)
        {
            var list = new List<Interval>();

            foreach (var interval in intervals)
                if (list.All(x => !interval.StrictlyContains(x) && !x.StrictlyContains(interval)))
                    list.Add(interval);

            return list;
        }

        public static Interval[] NonOverlapping(IEnumerable<Interval> intervals)
        {
            var list = new List<Interval>();

            foreach (var interval in intervals)
                if (!interval.OverlapsAny(list))
                    list.Add(interval);

            return list.ToArray();
        }

        private Interval[] MeetingIntervals(int count = 101, int length = 10)
        {
            var intervals = new Interval[count];

            var lastInterval = intervals[0] = new Interval(0, Random.Next(1, 1 + length), (IntervalType) Random.Next(0, 4));
            for (var i = 1; i < count; i++)
            {
                lastInterval = intervals[i] = new Interval(lastInterval.High, Random.Next(lastInterval.High + 1, lastInterval.High + 1 + length), !lastInterval.HighIncluded, Convert.ToBoolean(Random.Next(0, 2)));
            }

            return intervals;
        }

        private static void SingleIntervalEquals(Interval query, IEnumerable<IInterval<int>> gaps)
        {
            var enumerator = gaps.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.True(query.IntervalEquals(enumerator.Current));
            Assert.False(enumerator.MoveNext());
        }

        private Interval[] Normalize(Interval[] intervals)
        {
            var endpoints = intervals.UniqueEndpointValues();
            var map = new HashDictionary<int, int>();
            var counter = 0;
            foreach (var endpoint in endpoints)
                map.Add(endpoint, counter++);

            var normalizedIntervals = new Interval[intervals.Length];
            for (int i = 0; i < intervals.Length; i++)
            {
                var interval = intervals[i];

                normalizedIntervals[i] = new Interval(map[interval.Low], map[interval.High]);
            }
            return normalizedIntervals;
        }

        public Interval[] DuplicateIntervals(int type = -1)
        {
            var interval = SingleInterval(type);
            return Enumerable.Range(0, Count).Select(i => new Interval(interval)).ToArray();
        }

        public Interval[] SingleObject(int type = -1)
        {
            var interval = SingleInterval(type);
            return Enumerable.Range(0, Count).Select(i => interval).ToArray();
        }

        public Interval SinglePoint()
        {
            return new Interval(Random.Next(Int32.MinValue, Int32.MaxValue));
        }

        public static Interval[] NonOverlappingIntervals(int count, int length = 1, int space = 0)
        {
            // TODO: Contract.Ensures(Contract.ForAll(0, count - 1, i => Contract.ForAll(i + 1, count, j => !Contract.Result<IInterval<int>[]>()[i].Overlaps(Contract.Result<IInterval<int>[]>()[j]))));

            var intervals = new Interval[count];

            var low = 0;
            for (var i = 0; i < count; ++i)
            {
                intervals[i] = new Interval(low, low + length, IntervalType.LowIncluded);
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

        protected IIntervalCollection<I, T> CreateCollection<I, T>(params I[] intervals)
            where I : class, IInterval<T>
            where T : IComparable<T>
        {
            var additionalParameters = AdditionalParameters();
            var parameters = new object[1 + additionalParameters.Length];
            parameters[0] = intervals;
            for (var i = 0; i < additionalParameters.Length; i++)
                parameters[i + 1] = additionalParameters[i];

            // Check that test class name matches implementation
            var type = GetCollectionType();
            var className = GetType().Name.Substring(0, GetType().Name.IndexOf("Tester"));
            if (!type.Name.StartsWith(className))
                throw new ArgumentException("The class name does not match the type of the class!");

            Type[] typeArgs = { typeof(I), typeof(T) };
            var genericType = type.MakeGenericType(typeArgs);
            return (IIntervalCollection<I, T>) Activator.CreateInstance(genericType, parameters);
        }

        protected IIntervalCollection<I, T> CreateEmptyCollection<I, T>()
            where I : class, IInterval<T>
            where T : IComparable<T>
        {
            return CreateCollection<I, T>(new I[0]);
        }

        protected static void AssertThrowsContractException(Action function)
        {
            try
            {
                function();
            }
            catch (Exception e)
            {
                if (e.GetType().FullName != @"System.Diagnostics.Contracts.__ContractsRuntime+ContractException")
                    throw;

                Assert.Pass();
                return;
            }

            Assert.Fail();
        }

        #endregion

        #region Test Methods

        #region Code Contracts

        [Test]
        [Category("Code Contracts")]
        public void CodeContracts_VerifyPreconditionsAreInAssembly_ContractRuntimeContractException()
        {
            AssertThrowsContractException(() =>
            {
                var collection = CreateEmptyCollection<IInterval<int>, int>();
                collection.FindOverlaps(null);
            });
        }

        // TODO: Make this work in debug!
        [Test]
        [Category("Code Contracts")]
        public void CodeContracts_VerifyPostconditionsAreInDebugAssembly_ContractRuntimeContractException()
        {
#if DEBUG
            AssertThrowsContractException((() => CodeContract_EnsureFails()));
#else
            CodeContract_EnsureFails();
#endif
        }

        public int CodeContract_EnsureFails()
        {
            Contract.Ensures(false);
            return -1;
        }

        #endregion

        #region Collection Value

        #region IsEmpty

        [Test]
        [Category("IsEmpty")]
        public void IsEmpty_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<IInterval<int>, int>();
            Assert.That(collection.IsEmpty);
        }

        [Test]
        [Category("IsEmpty")]
        public void IsEmpty_SingleInterval_NotEmpty()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            Assert.That(!collection.IsEmpty);
        }

        [Test]
        [Category("IsEmpty")]
        public void IsEmpty_SingleObject_NotEmpty()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            Assert.That(!collection.IsEmpty);
        }

        [Test]
        [Category("IsEmpty")]
        public void IsEmpty_DuplicateIntervals_NotEmpty()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            Assert.That(!collection.IsEmpty);
        }

        [Test]
        [Category("IsEmpty")]
        public void IsEmpty_ManyIntervals_NotEmpty()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            Assert.That(!collection.IsEmpty);
        }

        #endregion

        #region Count

        [Test]
        [Category("Count")]
        public void Count_EmptyCollection_Zero()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_SingleInterval_One()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_SingleObject_CountOrOne()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? Count : 1;
            Assert.AreEqual(expected, collection.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_DuplicateIntervals_Count()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? Count : 1;
            Assert.AreEqual(expected, collection.Count);
        }

        [Test]
        [Category("Count")]
        public void Count_ManyIntervals_Count_FixedSeed()
        {
            updateRandom(-623960470);
            Count_ManyIntervals_Count();
        }

        [Test]
        [Category("Count")]
        public void Count_ManyIntervals_Count()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = InsertedIntervals(collection, intervals).Count();
            Assert.AreEqual(expected, collection.Count);
        }

        #endregion

        #region CountSpeed

        protected abstract Speed CountSpeed();

        [Test]
        [Category("Count Speed")]
        public void CountSpeed_VerifyCountSpeed_EqualToStated()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            Assert.AreEqual(CountSpeed(), collection.CountSpeed);
        }

        #endregion

        #region Choose

        [Test]
        [Category("Choose")]
        [ExpectedException(typeof(NoSuchItemException))]
        public void Choose_EmptyCollection_Exception()
        {
            var interval = CreateEmptyCollection<Interval, int>().Choose();
        }

        [Test]
        [Category("Choose")]
        public void Choose_SingleInterval_IntervalIsSameAsChoose()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            var choose = collection.Choose();
            Assert.AreSame(interval, choose);
        }

        [Test]
        [Category("Choose")]
        public void Choose_SingleObject_IntervalsAreSameAsChoose()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var choose = collection.Choose();
            Assert.True(intervals.All(x => ReferenceEquals(x, choose)));
        }

        [Test]
        [Category("Choose")]
        public void Choose_DuplicateIntervals_OneIntervalIsSameAsChoose()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var choose = collection.Choose();
            Assert.AreEqual(1, intervals.Count(x => ReferenceEquals(x, choose)));
        }

        [Test]
        [Category("Choose")]
        public void Choose_ManyIntervals_OneIntervalIsSameAsChoose()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var choose = collection.Choose();
            Assert.AreEqual(1, intervals.Count(x => ReferenceEquals(x, choose)));
        }

        #endregion

        #endregion

        #region Interval Collection
        
        #region Data Structure Properties

        #region Allows Overlaps

        protected abstract bool AllowsOverlaps();

        [Test]
        [Category("Allows Overlaps")]
        public void AllowsOverlaps_EmptyCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            Assert.AreEqual(AllowsOverlaps(), collection.AllowsOverlaps);
        }

        [Test]
        [Category("Allows Overlaps")]
        public void AllowsOverlaps_AddOverlappingIntervalToCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            if (!collection.IsReadOnly)
            {
                collection.Add(new Interval(0, 2));
                Assert.AreEqual(collection.AllowsOverlaps, collection.Add(new Interval(1, 3)));
            }
        }

        // TODO: Fix AddAll checker
        [Test]
        [Category("Allows Overlaps"), Ignore]
        public void AllowsOverlaps_AddAllContainedIntervalToCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            if (!collection.IsReadOnly)
            {
                collection.AddAll(new[] { new Interval(0, 2), new Interval(1, 3) });
                var expected = collection.AllowsOverlaps ? 2 : 1;
                Assert.AreEqual(expected, collection.Count);
            }
        }

        [Test]
        [Category("Allows Overlaps")]
        public void AllowsOverlaps_CreateCollectionWithOverlappingIntervals_DefinedResult()
        {
            var collection = CreateCollection<Interval, int>(new Interval(0, 2), new Interval(1, 3));
            var expected = collection.AllowsOverlaps ? 2 : 1;
            Assert.AreEqual(expected, collection.Count);
        }

        #endregion

        #region Allows Containments

        protected abstract bool AllowsContainments();

        [Test]
        [Category("Allows Containments")]
        public void AllowsContainments_EmptyCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            Assert.AreEqual(AllowsContainments(), collection.AllowsContainments);
        }

        [Test]
        [Category("Allows Containments")]
        public void AllowsContainments_AddContainedIntervalToCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            if (!collection.IsReadOnly)
            {
                collection.Add(new Interval(0, 3));
                Assert.AreEqual(collection.AllowsContainments, collection.Add(new Interval(1, 2)));
            }
        }

        // TODO: Fix AddAll checker
        [Test]
        [Category("Allows Containments"), Ignore]
        public void AllowsContainments_AddAllContainedIntervalToCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            if (!collection.IsReadOnly)
            {
                collection.AddAll(new[] { new Interval(0, 3), new Interval(1, 2) });
                var expected = collection.AllowsContainments ? 2 : 1;
                Assert.AreEqual(expected, collection.Count);
            }
        }

        [Test]
        [Category("Allows Containments")]
        public void AllowsContainments_CreateCollectionWithContainedIntervals_DefinedResult()
        {
            var collection = CreateCollection<Interval, int>(new Interval(0, 3), new Interval(1, 2));
            var expected = collection.AllowsContainments ? 2 : 1;
            Assert.AreEqual(expected, collection.Count);
        }

        #endregion

        #region Allows Reference Duplicates

        protected abstract bool AllowsReferenceDuplicates();

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_EmptyCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            Assert.AreEqual(AllowsReferenceDuplicates(), collection.AllowsReferenceDuplicates);
        }

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_AddSingleObjectToEmptyCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            if (!collection.IsReadOnly)
            {
                var interval = SingleInterval();
                collection.Add(interval);
                Assert.AreEqual(collection.AllowsReferenceDuplicates, collection.Add(interval));
            }
        }

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_AddAllSingleObjectToCollection_DefinedResult()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            if (!collection.IsReadOnly)
            {
                var interval = SingleInterval();
                collection.AddAll(new[] { interval, interval });
                var expected = collection.AllowsReferenceDuplicates ? 2 : 1;
                Assert.AreEqual(expected, collection.Count);
            }
        }

        [Test]
        [Category("Allows Reference Duplicates")]
        public void AllowsReferenceDuplicates_CreateCollectionWithSingleObject_DefinedResult()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval, interval);
            var expected = collection.AllowsReferenceDuplicates ? 2 : 1;
            Assert.AreEqual(expected, collection.Count);
        }

        #endregion

        #endregion

        #region Collection Properties

        #region Span

        [Test]
        [Category("Span"), AdditionalSeeds(-1724642727)]
        public void Span_EmptyCollection_Exception()
        {
            const string contractExceptionName = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";
            var collection = CreateEmptyCollection<IInterval<int>, int>();

            try
            {
                var span = CreateEmptyCollection<Interval, int>().Span;
            }
            catch (Exception e)
            {
                if (e.GetType().FullName != contractExceptionName)
                    throw;

                Assert.Pass();
                return;
            }

            Assert.Fail();
        }

        [Test]
        [Category("Span")]
        public void Span_SingleInterval_SpanEqualsInterval()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            var span = collection.Span;

            Assert.True(span.IntervalEquals(interval));
            Assert.True(collection.All(span.Contains));
        }

        [Test]
        [Category("Span")]
        public void Span_ManyIntervals_JoinedSpan()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var span = (InsertedIntervals(collection, intervals)).Span();

            Assert.True(span.IntervalEquals(collection.Span));
            Assert.True(collection.All(span.Contains));
        }

        [Test]
        [Category("Span")]
        public void Span_NonOverlappingIntervals_JoinedSpan()
        {
            // TODO: Refactor?
            Interval interval1, interval2;
            do
            {
                interval1 = SingleInterval();
                interval2 = SingleInterval();
            } while (interval1.Overlaps(interval2));

            var joinedSpan = interval1.JoinedSpan(interval2);
            var collection = CreateCollection<Interval, int>(interval1, interval2);
            var span = collection.Span;

            Assert.True(joinedSpan.IntervalEquals(span));
            Assert.True(collection.All(span.Contains));
        }

        [Test]
        [Category("Span")]
        public void Span_ContainedInterval_ContainerEqualsSpan_FixedSeed()
        {
            updateRandom(-1672943992);
            Span_ContainedInterval_ContainerEqualsSpan();
        }

        [Test]
        [Category("Span")]
        public void Span_ContainedInterval_ContainerEqualsSpan()
        {
            // TODO: Refactor?
            Interval interval1, interval2;
            do
            {
                interval1 = SingleInterval();
                interval2 = SingleInterval();
            } while (!interval1.StrictlyContains(interval2));

            var collection = CreateCollection<Interval, int>(interval1, interval2);
            var span = collection.Span;

            Assert.True(interval1.IntervalEquals(span));
        }

        #endregion

        #region Lowest Interval

        [Test]
        [Category("Lowest Interval"), AdditionalSeeds(153548762)]
        public void LowestInterval_EmptyCollection_Exception()
        {
            const string contractExceptionName = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";

            try
            {
                var lowestInterval = CreateEmptyCollection<Interval, int>().LowestInterval;
            }
            catch (Exception e)
            {
                if (e.GetType().FullName != contractExceptionName)
                    throw;

                Assert.Pass();
                return;
            }

            Assert.Fail();
        }

        [Test]
        [Category("Lowest Interval")]
        public void LowestInterval_SingleInterval_LowestIntervalEqualsInterval()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            var lowestInterval = collection.LowestInterval;

            Assert.True(ReferenceEquals(lowestInterval, interval));
        }

        [Test]
        [Category("Lowest Interval")]
        public void LowestInterval_SingleObject_LowestIntervalEqualsAnInterval()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var lowestInterval = collection.LowestInterval;
            Assert.That(ReferenceEquals(lowestInterval, intervals[0]));
        }

        [Test]
        [Category("Lowest Interval")]
        public void LowestInterval_DuplicateIntervals_LowestIntervalEqualsAnInterval()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var lowestInterval = collection.LowestInterval;
            Assert.That(lowestInterval.IntervalEquals(intervals[0]));
        }

        [Test]
        [Category("Lowest Interval")]
        public void LowestInterval_ManyIntervals_Lowest()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var lowestInterval = (InsertedIntervals(collection, intervals)).LowestInterval<Interval, int>();
            Assert.True(collection.LowestInterval.CompareLow(lowestInterval) == 0);
        }

        #endregion

        #region Lowest Intervals

        [Test]
        [Category("Lowest Intervals")]
        public void LowestIntervals_EmptyCollection_EmptyResult()
        {
            var collection = CreateEmptyCollection<IInterval<int>, int>();
            Assert.That(collection.LowestIntervals, Is.Empty);
        }

        [Test]
        [Category("Lowest Intervals")]
        public void LowestIntervals_SingleInterval_LowestIntervalsEqualInterval()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);

            Assert.That(collection.LowestIntervals.Count(), Is.EqualTo(1));
            Assert.That(ReferenceEquals(collection.LowestIntervals.First(), interval));
        }

        [Test]
        [Category("Lowest Intervals")]
        public void LowestIntervals_SingleObject_LowestIntervalsEqualsIntervals()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var lowestIntervals = collection.LowestIntervals;

            if (collection.AllowsReferenceDuplicates)
                Assert.That(lowestIntervals, Is.EqualTo(intervals));
            else
                Assert.That(lowestIntervals, Is.EqualTo(intervals.Take(1)));
        }

        [Test]
        [Category("Lowest Intervals")]
        public void LowestIntervals_DuplicateIntervals_LowestIntervalEqualsAnInterval()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var lowestIntervals = InsertedIntervals(collection, intervals);

            Assert.That(collection.LowestIntervals, Is.EqualTo(lowestIntervals));
        }

        [Test]
        [Category("Lowest Intervals")]
        public void LowestIntervals_ManyIntervals_Lowests()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var lowestIntervals = (InsertedIntervals(collection, intervals)).LowestIntervals<Interval, int>();

            Assert.That(collection.LowestIntervals, Is.EqualTo(lowestIntervals));
        }

        [Test]
        [Category("Lowest Intervals")]
        public void LowestIntervals_ManyIntervalsWithManyLowIntervals_LowestIntervals()
        {
            IEnumerable<Interval> intervals = ManyIntervals();
            var span = intervals.Span();
            var length = span.High / Count - span.Low / Count;

            var lowIntervals = new Interval[Count];

            for (var i = 0; i < Count; i++)
                lowIntervals[i] = new Interval(span.Low, span.Low + 1 + length * i, span.LowIncluded, span.HighIncluded);
            intervals = intervals.Concat(lowIntervals);

            var collection = CreateCollection<Interval, int>(intervals.ToArray());
            var lowestIntervals = (InsertedIntervals(collection, intervals)).LowestIntervals<Interval, int>();

            Assert.That(collection.LowestIntervals, Is.EquivalentTo(lowestIntervals));
        }

        #endregion

        #region Highest Interval

        [Test]
        [Category("Highest Interval"), AdditionalSeeds(309396561)]
        public void HighestInterval_EmptyCollection_Exception()
        {
            const string contractExceptionName = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";

            try
            {
                var highestInterval = CreateEmptyCollection<Interval, int>().HighestInterval;
            }
            catch (Exception e)
            {
                if (e.GetType().FullName != contractExceptionName)
                    throw;

                Assert.Pass();
                return;
            }

            Assert.Fail();
        }

        [Test]
        [Category("Highest Interval")]
        public void HighestInterval_SingleInterval_HighestIntervalEqualsInterval()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            var highestInterval = collection.HighestInterval;

            Assert.True(ReferenceEquals(highestInterval, interval));
        }

        [Test]
        [Category("Highest Interval")]
        public void HighestInterval_SingleObject_HighestIntervalEqualsAnInterval()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var highestInterval = collection.HighestInterval;

            Assert.That(ReferenceEquals(highestInterval, intervals[0]));
        }

        [Test]
        [Category("Highest Interval")]
        public void HighestInterval_DuplicateIntervals_HighestIntervalEqualsAnInterval()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var highestInterval = collection.HighestInterval;

            Assert.That(highestInterval.IntervalEquals(intervals[0]));
        }

        [Test]
        [Category("Highest Interval")]
        public void HighestInterval_ManyIntervals_Highest_FixedSeed()
        {
            updateRandom(497738314);
            HighestInterval_ManyIntervals_Highest();
        }

        [Test]
        [Category("Highest Interval")]
        public void HighestInterval_ManyIntervals_Highest()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var highestInterval = (InsertedIntervals(collection, intervals)).HighestInterval<Interval, int>();

            Assert.True(collection.HighestInterval.CompareHigh(highestInterval) == 0);
        }

        #endregion

        #region Highest Intervals

        [Test]
        [Category("Highest Intervals")]
        public void HighestIntervals_EmptyCollection_EmptyResult()
        {
            var collection = CreateEmptyCollection<IInterval<int>, int>();
            Assert.That(collection.HighestIntervals, Is.Empty);
        }

        [Test]
        [Category("Highest Intervals")]
        public void HighestIntervals_SingleInterval_HighestIntervalsEqualInterval()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);

            Assert.That(collection.HighestIntervals.Count(), Is.EqualTo(1));
            Assert.That(ReferenceEquals(collection.HighestIntervals.First(), interval));
        }

        [Test]
        [Category("Highest Intervals")]
        public void HighestIntervals_SingleObject_HighestIntervalsEqualIntervals()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);

            if (collection.AllowsReferenceDuplicates)
                Assert.That(collection.HighestIntervals, Is.EqualTo(intervals));
            else
                Assert.That(collection.HighestIntervals, Is.EqualTo(intervals.Take(1)));
        }

        [Test]
        [Category("Highest Intervals")]
        public void HighestIntervals_DuplicateIntervals_HighestIntervalsEqualIntervals()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var highestIntervals = InsertedIntervals(collection, intervals);

            Assert.That(collection.HighestIntervals, Is.EqualTo(highestIntervals));
        }

        [Test]
        [Category("Highest Intervals")]
        public void HighestIntervals_ManyIntervals_Highest()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var highestIntervals = (InsertedIntervals(collection, intervals)).HighestIntervals<Interval, int>();

            Assert.That(collection.HighestIntervals, Is.EqualTo(highestIntervals));
        }

        [Test]
        [Category("Highest Intervals")]
        public void HighestIntervals_ManyIntervalsWithManyHighIntervals_HighestIntervals()
        {
            IEnumerable<Interval> intervals = ManyIntervals();
            var span = intervals.Span();
            var length = span.High / Count - span.Low / Count;

            var highIntervals = new Interval[Count];

            for (var i = 0; i < Count; i++)
                highIntervals[i] = new Interval(span.High - 1 - length * 1, span.High, span.LowIncluded, span.HighIncluded);
            intervals = intervals.Concat(highIntervals);

            var collection = CreateCollection<Interval, int>(intervals.ToArray());
            var highestIntervals = (InsertedIntervals(collection, intervals)).HighestIntervals<Interval, int>();

            Assert.That(collection.HighestIntervals, Is.EquivalentTo(highestIntervals));
        }

        #endregion

        #region Maximum Depth

        [Test]
        [Category("Maximum Depth")]
        public void MaximumDepth_EmptyCollection_Zero()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            Assert.AreEqual(0, collection.MaximumDepth);
        }

        [Test]
        [Category("Maximum Depth")]
        public void MaximumDepth_SingleInterval_One()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            Assert.AreEqual(1, collection.MaximumDepth);
        }

        [Test]
        [Category("Maximum Depth")]
        public void MaximumDepth_SingleObject_CountOrOne()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            Assert.AreEqual(collection.AllowsReferenceDuplicates ? Count : 1, collection.MaximumDepth);
        }

        [Test]
        [Category("Maximum Depth")]
        public void MaximumDepth_DuplicateIntervals_Count()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = InsertedIntervals(collection, intervals).Count();
            Assert.AreEqual(expected, collection.MaximumDepth);
        }

        [Test]
        [Category("Maximum Depth")]
        // 82771978
        // -379836663
        public void MaximumDepth_NonOverlappingIntervals_One()
        {
            var intervals = NonOverlappingIntervals(Count);
            var collection = CreateCollection<Interval, int>(intervals);
            Assert.AreEqual(1, collection.MaximumDepth);
        }

        [Test]
        [Category("Maximum Depth")]
        public void MaximumDepth_BetweenDescreteValues_Two()
        {
            var collection = CreateCollection<Interval, int>(
                new Interval(1, 3, IntervalType.Open),
                new Interval(2, 4, IntervalType.Open)
            );
            var expected = collection.AllowsOverlaps ? 2 : 1;
            Assert.AreEqual(expected, collection.MaximumDepth);
        }

        [Test]
        [Category("Maximum Depth")]
        public void MaximumDepth_ManyOverlappingIntervals_Four()
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
            var collection = CreateCollection<Interval, int>(intervals);

            var expected = collection.AllowsOverlaps ? collection.AllowsContainments ? 4 : 2 : 1;
            Assert.AreEqual(expected, collection.MaximumDepth);
        }

        [Test]
        [Category("Maximum Depth")]
        public void MaximumDepth_AllContainedIntervals_Count()
        {
            // 0    5   10   15   20
            // 
            //          [-]
            //         [---]
            //        [-----]
            //       [-------]
            //      [---------]
            //     [-----------]
            //    [-------------]
            //   [---------------]
            //  [-----------------]
            // [-------------------]
            var intervals = Enumerable.Range(0, 10).Select(i => new Interval(i, 20 - i, IntervalType.Closed)).ToArray();
            var collection = CreateCollection<Interval, int>(intervals);
            Assert.AreEqual(collection.Count, collection.MaximumDepth);
        }

        #endregion

        #endregion
        
        #region Enumerable

        #region Get Enumerator

        [Test]
        [Category("Enumerable")]
        public void Enumerable_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            CollectionAssert.IsEmpty(collection);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_SingleInterval_AreEqual()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);
            CollectionAssert.AreEqual(new[] { interval }, collection);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_SingleObject_AreEquivalent()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsReferenceDuplicates ? intervals : new[] { intervals.First() };
            CollectionAssert.AreEqual(expected, collection);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_DuplicateIntervals_AreEquivalent()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = collection.AllowsOverlaps ? intervals : new[] { intervals.First() };
            CollectionAssert.AreEquivalent(expected, collection);
        }

        [Test]
        [Category("Enumerable")]
        public void Enumerable_ManyIntervals_AreEquivalent()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = InsertedIntervals(collection, intervals);
            CollectionAssert.AreEquivalent(expected, collection);
            CollectionAssert.AllItemsAreUnique(collection);
        }

        #endregion

        #endregion

        #region Find Equals

        [Test]
        [Category("Find Equals")]
        public void FindEquals_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();
            CollectionAssert.IsEmpty(collection.FindEquals(SingleInterval()));
        }

        [Test]
        [Category("Find Equals")]
        public void FindEquals_SingleInterval_SingleInterval()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);

            CollectionAssert.AreEqual(new[] { interval }, collection.FindEquals(interval));
        }

        [Test]
        [Category("Find Equals")]
        public void FindEquals_SingleObject_AllOrSingleInterval()
        {
            var intervals = SingleObject();
            var interval = intervals.First();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = InsertedIntervals(collection, intervals).Where(x => x.IntervalEquals(interval));

            var actual = collection.FindEquals(interval);
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        [Category("Find Equals")]
        public void FindEquals_DuplicateIntervals_AllOrSingleInterval()
        {
            var intervals = DuplicateIntervals();
            var interval = intervals.First();
            var collection = CreateCollection<Interval, int>(intervals);
            var expected = InsertedIntervals(collection, intervals).Where(x => x.IntervalEquals(interval));

            CollectionAssert.AreEquivalent(expected, collection.FindEquals(interval));
        }

        [Test]
        [Category("Find Equals")]
        public void FindEquals_ManyIntervals_EqualIntervals()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var insertedIntervals = InsertedIntervals(collection, intervals);

            foreach (var interval in insertedIntervals)
                CollectionAssert.AreEquivalent(insertedIntervals.Where(x => x.IntervalEquals(interval)), collection.FindEquals(interval));
        }

        #endregion

        #region Find Overlaps

        #region Stabbing

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_EmptyCollection_Empty()
        {
            var query = randomInt();
            var collection = CreateEmptyCollection<Interval, int>();

            CollectionAssert.IsEmpty(collection.FindOverlaps(query));
        }

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_SingleInterval_SingleOverlap()
        {
            var interval = SingleInterval();
            int query;
            do query = randomInt(); while (interval.Overlaps(query));
            var collection = CreateCollection<Interval, int>(interval);

            CollectionAssert.IsEmpty(collection.FindOverlaps(query));

            if (interval.LowIncluded)
                CollectionAssert.AreEqual(new[] { interval }, collection.FindOverlaps(interval.Low));
            else
                CollectionAssert.IsEmpty(collection.FindOverlaps(interval.Low));

            if (interval.HighIncluded)
                CollectionAssert.AreEqual(new[] { interval }, collection.FindOverlaps(interval.High));
            else
                CollectionAssert.IsEmpty(collection.FindOverlaps(interval.High));

            CollectionAssert.AreEquivalent(new[] { interval }, collection.FindOverlaps(interval.Low / 2 + interval.High / 2));
        }

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_SingleObject_CountOrSingleOverlap_FixedSeed()
        {
            updateRandom(32236203);
            FindOverlapsStabbing_SingleObject_CountOrSingleOverlap();
        }

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_SingleObject_CountOrSingleOverlap()
        {
            var intervals = SingleObject();
            var interval = intervals[0];
            int query;
            do query = randomInt(); while (interval.Overlaps(query));
            var collection = CreateCollection<Interval, int>(intervals);
            var overlaps = collection.AllowsReferenceDuplicates ? intervals : new[] { interval };

            CollectionAssert.IsEmpty(collection.FindOverlaps(query));

            if (interval.LowIncluded)
                CollectionAssert.AreEquivalent(overlaps, collection.FindOverlaps(interval.Low));
            else
                CollectionAssert.IsEmpty(collection.FindOverlaps(interval.Low));

            if (interval.HighIncluded)
                CollectionAssert.AreEquivalent(overlaps, collection.FindOverlaps(interval.High));
            else
                CollectionAssert.IsEmpty(collection.FindOverlaps(interval.High));

            CollectionAssert.AreEquivalent(overlaps, collection.FindOverlaps(interval.Low / 2 + interval.High / 2));
        }

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_DuplicateIntervals_CountOverlaps()
        {
            var intervals = DuplicateIntervals();
            var interval = intervals[0];
            int query;
            do query = randomInt(); while (interval.Overlaps(query));
            var collection = CreateCollection<Interval, int>(intervals);

            CollectionAssert.IsEmpty(collection.FindOverlaps(query));

            if (interval.LowIncluded)
                CollectionAssert.AreEquivalent(collection.AllowsOverlaps ? intervals : new[] { intervals.First() }, collection.FindOverlaps(interval.Low));
            else
                CollectionAssert.IsEmpty(collection.FindOverlaps(interval.Low));

            if (interval.HighIncluded)
                CollectionAssert.AreEquivalent(collection.AllowsOverlaps ? intervals : new[] { intervals.First() }, collection.FindOverlaps(interval.High));
            else
                CollectionAssert.IsEmpty(collection.FindOverlaps(interval.High));

            CollectionAssert.AreEquivalent(collection.AllowsOverlaps ? intervals : new[] { intervals.First() }, collection.FindOverlaps(interval.Low / 2 + interval.High / 2));
        }

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_ManyIntervals_AtLeastOneOverlap_FixedSeed()
        {
            updateRandom(1641746101);
            FindOverlapsStabbing_ManyIntervals_AtLeastOneOverlap();
        }

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_ManyIntervals_AtLeastOneOverlap()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            foreach (var interval in intervals)
            {
                CollectionAssert.AreEquivalent((InsertedIntervals(collection, intervals)).Where(x => x.Overlaps(interval.Low)), collection.FindOverlaps(interval.Low));
                CollectionAssert.AreEquivalent((InsertedIntervals(collection, intervals)).Where(x => x.Overlaps(interval.High)), collection.FindOverlaps(interval.High));
            }
        }

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapsStabbing_SingleIntervalAllEndpointCombinations_Overlaps()
        {
            var interval = SingleInterval();
            var intervals = Enumerable.Range(0, 4).Select(i => new Interval(interval.Low, interval.High, (IntervalType) i)).ToArray();
            var collection = CreateCollection<Interval, int>(intervals);
            var insertedIntervals = InsertedIntervals(collection, intervals).ToArray();

            CollectionAssert.AreEquivalent(insertedIntervals.Where(x => x.LowIncluded), collection.FindOverlaps(interval.Low));
            CollectionAssert.AreEquivalent(insertedIntervals.Where(x => x.HighIncluded), collection.FindOverlaps(interval.High));
            CollectionAssert.AreEquivalent(insertedIntervals, collection.FindOverlaps(interval.Low / 2 + interval.High / 2));
        }

        #endregion

        #region Range

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_EmptyCollection_Empty()
        {
            var query = SingleInterval();
            var collection = CreateEmptyCollection<Interval, int>();

            CollectionAssert.IsEmpty(collection.FindOverlaps(query));
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_BensTest_FixedSeed()
        {
            updateRandom(-1166356094);
            FindOverlapsRange_BensTest();
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_BensTest()
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
            // |           []                         |
            // |           |                          |
            // |          [                ]          |
            // |          [ ]                         |
            // |          []                          |
            // |       [ ]                            |
            // |      [                        ]      |
            // |      [         )                     |
            // |      [   ]                           |
            // | [                                  ] |
            // | [         ]                          |
            // | [    ]                               |
            // | [   ]                                |
            // ****************************************
            var intervals = new[] {
                new Interval(5, 9, IntervalType.Closed),
                new Interval(11, 15, IntervalType.Closed),
                new Interval(15, 20, IntervalType.Closed),
                new Interval(20, 24, IntervalType.Closed),
                new Interval(26, 30, IntervalType.Closed)
            };

            var collection = CreateCollection<Interval, int>(intervals);

            var ranges = new[] {
                    new Interval( 31,  35, IntervalType.Closed),
                    new Interval( 30,  35, IntervalType.Closed),
                    new Interval( 27,  29, IntervalType.Closed),
                    new Interval( 26,  35, IntervalType.Closed),
                    new Interval( 26,  30, IntervalType.Closed),
                    new Interval( 25,  26, IntervalType.Closed),
                    new Interval( 24,  26, IntervalType.Closed),
                    new Interval( 20,  30, IntervalType.Closed),
                    new Interval( 10,  11, IntervalType.Closed),
                    new Interval( 10,  10, IntervalType.Closed),
                    new Interval(  9,  26, IntervalType.Closed),
                    new Interval(  9,  11, IntervalType.Closed),
                    new Interval(  9,  10, IntervalType.Closed),
                    new Interval(  6,   8, IntervalType.Closed),
                    new Interval(  5,  30, IntervalType.Closed),
                    new Interval(  5,  15, IntervalType.LowIncluded),
                    new Interval(  5,   9, IntervalType.Closed),
                    new Interval(  0,  35, IntervalType.Closed),
                    new Interval(  0,  10, IntervalType.Closed),
                    new Interval(  0,   5, IntervalType.Closed),
                    new Interval(  0,   4, IntervalType.Closed)
                };

            if (!collection.AllowsOverlaps)
                intervals = NonOverlapping(intervals);

            foreach (var interval in ranges)
                CollectionAssert.AreEquivalent(intervals.Where(x => x.Overlaps(interval)), collection.FindOverlaps(interval));
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_ManyIntervals_ChooseOverlapsInCollection()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var interval = collection.Choose();
            var overlaps = collection.FindOverlaps(interval);
            Assert.True(overlaps.Count() > 0);
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_ManyIntervals_ChooseOverlapsNotInCollection()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            IInterval<int> interval;
            do { interval = SingleInterval(); } while (intervals.Any(x => x.Overlaps(interval)));

            CollectionAssert.IsEmpty(collection.FindOverlaps(interval));
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_ManyIntervals_ManyIntervals_FixedSeed()
        {
            updateRandom(-1585512131);
            FindOverlapsRange_ManyIntervals_ManyIntervals();
        }

        [Test]
        [Category("Find Overlaps Range")]
        public void FindOverlapsRange_ManyIntervals_ManyIntervals()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            foreach (var query in ManyIntervals())
                CollectionAssert.AreEquivalent((InsertedIntervals(collection, intervals)).Where(x => x.Overlaps(query)), collection.FindOverlaps(query));
        }

        #endregion

        #endregion

        #region Find Overlap

        #region Stabbing

        // TODO!!

        [Test]
        [Category("Find Overlap Stabbing")]
        public void FindOverlapStabbing_EmptyCollection_False()
        {
            var query = randomInt();
            Interval overlap;
            var collection = CreateEmptyCollection<Interval, int>();

            Assert.False(collection.FindOverlap(query, out overlap));
            Assert.IsNull(overlap);
        }

        [Test]
        [Category("Find Overlap Stabbing")]
        public void FindOverlapStabbing_SingleInterval_OverlapEqualsInterval()
        {
            var interval = SingleInterval();
            var query = randomInt();
            while (interval.Overlaps(query)) query = randomInt();
            var collection = CreateCollection<Interval, int>(interval);
            Interval overlap;

            Assert.That(collection.FindOverlap(query, out overlap), Is.False);
            Assert.That(overlap, Is.Null);

            if (interval.LowIncluded)
            {
                Assert.That(collection.FindOverlap(interval.Low, out overlap), Is.True);
                Assert.That(overlap, Is.EqualTo(interval));
            }
            else
            {
                Assert.That(collection.FindOverlap(interval.Low, out overlap), Is.False);
                Assert.That(overlap, Is.Null);
            }

            if (interval.HighIncluded)
            {
                Assert.That(collection.FindOverlap(interval.High, out overlap), Is.True);
                Assert.That(overlap, Is.EqualTo(interval));
            }
            else
            {
                Assert.That(collection.FindOverlap(interval.High, out overlap), Is.False);
                Assert.That(overlap, Is.Null);
            }

            Assert.That(collection.FindOverlap(interval.Low / 2 + interval.High / 2, out overlap), Is.True);
            Assert.That(overlap, Is.EqualTo(interval));
        }

        [Test]
        [Category("Find Overlap Stabbing")]
        public void FindOverlapStabbing_SingleObject_OverlapIntervalEqualsInterval()
        {
            var intervals = SingleObject();
            var interval = intervals[0];
            var query = randomInt();
            while (interval.Overlaps(query)) query = randomInt();
            var collection = CreateCollection<Interval, int>(intervals);
            Interval overlap;

            Assert.That(collection.FindOverlap(query, out overlap), Is.False);
            Assert.That(overlap, Is.Null);

            if (interval.LowIncluded)
            {
                Assert.That(collection.FindOverlap(interval.Low, out overlap), Is.True);
                Assert.That(overlap, Is.EqualTo(interval));
            }
            else
            {
                Assert.That(collection.FindOverlap(interval.Low, out overlap), Is.False);
                Assert.That(overlap, Is.Null);
            }

            if (interval.HighIncluded)
            {
                Assert.That(collection.FindOverlap(interval.High, out overlap), Is.True);
                Assert.That(overlap, Is.EqualTo(interval));
            }
            else
            {
                Assert.That(collection.FindOverlap(interval.High, out overlap), Is.False);
                Assert.That(overlap, Is.Null);
            }

            Assert.That(collection.FindOverlap(interval.Low / 2 + interval.High / 2, out overlap), Is.True);
            Assert.That(overlap, Is.EqualTo(interval));
        }

        [Test]
        [Category("Find Overlap Stabbing")]
        public void FindOverlapStabbing_DuplicateIntervals_CountOverlaps()
        {
            var intervals = DuplicateIntervals();
            var interval = intervals[0];
            var query = randomInt();
            while (interval.Overlaps(query)) query = randomInt();
            var collection = CreateCollection<Interval, int>(intervals);
            Interval overlap;

            Assert.That(collection.FindOverlap(query, out overlap), Is.False);
            Assert.That(overlap, Is.Null);

            if (interval.LowIncluded)
            {
                Assert.That(collection.FindOverlap(interval.Low, out overlap), Is.True);
                Assert.That(overlap.IntervalEquals(interval));
            }
            else
            {
                Assert.That(collection.FindOverlap(interval.Low, out overlap), Is.False);
                Assert.That(overlap, Is.Null);
            }

            if (interval.HighIncluded)
            {
                Assert.That(collection.FindOverlap(interval.High, out overlap), Is.True);
                Assert.That(overlap.IntervalEquals(interval));
            }
            else
            {
                Assert.That(collection.FindOverlap(interval.High, out overlap), Is.False);
                Assert.That(overlap, Is.Null);
            }

            Assert.That(collection.FindOverlap(interval.Low / 2 + interval.High / 2, out overlap), Is.True);
            Assert.That(overlap.IntervalEquals(interval));
        }

        [Test]
        [Category("Find Overlap Stabbing")]
        public void FindOverlapStabbing_ManyIntervals_AtLeastOneOverlap()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            // TODO!!
        }

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapStabbing_SingleIntervalAllEndpointCombinations_Overlaps()
        {
            var interval = SingleInterval();
            var intervals = Enumerable.Range(0, 4).Select(i => new Interval(interval.Low, interval.High, (IntervalType) i)).ToArray();
            var collection = CreateCollection<Interval, int>(intervals);
            Interval overlap;

            if (!collection.AllowsOverlaps)
                intervals = NonOverlapping(intervals);

            var hasOverlap = intervals.Any(x => x.Overlaps(interval.Low));
            Assert.That(collection.FindOverlap(interval.Low, out overlap), Is.EqualTo(hasOverlap));
            Assert.That(hasOverlap ? overlap.Overlaps(interval.Low) : overlap == null);

            hasOverlap = intervals.Any(x => x.Overlaps(interval.High));
            Assert.That(collection.FindOverlap(interval.High, out overlap), Is.EqualTo(hasOverlap));
            Assert.That(hasOverlap ? overlap.Overlaps(interval.High) : overlap == null);

            var query = interval.Low / 2 + interval.High / 2;
            Assert.That(collection.FindOverlap(query, out overlap), Is.True);
            Assert.That(overlap.Overlaps(query));
        }

        [Test]
        [Category("Find Overlaps Stabbing")]
        public void FindOverlapStabbing_TwoIntervalsSharingEndpointValue_NoOverlaps()
        {
            var intervals = new[]
            {
                new Interval(0, 1, IntervalType.LowIncluded),
                new Interval(1, 2, IntervalType.HighIncluded),
            };
            var collection = CreateCollection<Interval, int>(intervals);
            Interval overlap;

            Assert.That(collection.FindOverlap(1, out overlap), Is.False);
            Assert.That(overlap, Is.Null);
        }

        #endregion

        #region Range

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_EmptyCollection_False()
        {
            var query = SingleInterval();
            Interval interval;
            var collection = CreateEmptyCollection<Interval, int>();

            Assert.False(collection.FindOverlap(query, out interval));
            Assert.IsNull(interval);
        }

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_ManyIntervals_ChooseOverlapsInCollection_FixedSeed()
        {
            updateRandom(-1590477799);
            FindOverlapRange_ManyIntervals_ChooseOverlapsInCollection();
        }

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_ManyIntervals_ChooseOverlapsInCollection()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var interval = collection.Choose();
            Interval overlap;
            Assert.True(collection.FindOverlap(interval, out overlap));
            Assert.True(interval.Overlaps(overlap));
        }

        [Test]
        [Category("Find Overlap Range")]
        public void FindOverlapRange_ManyIntervals_ChooseOverlapsNotInCollection()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var interval = SingleInterval();
            Interval overlap;
            while (intervals.Any(x => x.Overlaps(interval)))
                interval = SingleInterval();
            Assert.False(collection.FindOverlap(interval, out overlap));
            Assert.IsNull(overlap);
        }

        #endregion

        #endregion

        #region Count Overlaps

        #region Stabbing

        // TODO: Finish testing CountOverlaps(T query)

        [Test]
        [Category("Count Overlaps Stabbing")]
        public void CountOverlapsStabbing_EmptyCollection_Zero()
        {
            var query = randomInt();
            var collection = CreateEmptyCollection<Interval, int>();

            Assert.AreEqual(0, collection.CountOverlaps(query));
        }

        [Test]
        [Category("Count Overlaps Stabbing")]
        public void CountOverlapsStabbing_FixedIntervals_NoOverlaps()
        {
            var intervals = new[] {
                new Interval( 0,  5, IntervalType.HighIncluded),
                new Interval( 2,  6, IntervalType.Closed),
                new Interval( 6,  8, IntervalType.Closed),
                new Interval(11, 17, IntervalType.Closed),
                new Interval(11, 17, IntervalType.Closed),
                new Interval(11, 18, IntervalType.Closed),
                new Interval(17, 18, IntervalType.Closed),
                new Interval(17, 20, IntervalType.LowIncluded),
                new Interval(18),
                new Interval(23, 28, IntervalType.Closed),
            };

            var collection = CreateCollection<Interval, int>(intervals);

            Assert.AreEqual(0, collection.CountOverlaps(0));
            Assert.AreEqual(0, collection.CountOverlaps(9));
            Assert.AreEqual(0, collection.CountOverlaps(20));
            Assert.AreEqual(0, collection.CountOverlaps(29));
        }

        #endregion

        #region Range

        // TODO: Finish testing CountOverlaps(IInterval<T> query)

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_EmptyCollection_Zero()
        {
            var query = SingleInterval();
            var collection = CreateEmptyCollection<Interval, int>();

            Assert.AreEqual(0, collection.CountOverlaps(query));
        }

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_ManyIntervals_ChooseOverlapsInCollection()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var interval = collection.Choose();
            Assert.Greater(collection.CountOverlaps(interval), 0);
        }

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_ManyIntervals_ChooseOverlapsNotInCollection()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            var interval = SingleInterval();
            while (intervals.Any(x => x.Overlaps(interval)))
                interval = SingleInterval();

            Assert.AreEqual(0, collection.CountOverlaps(interval));
        }

        [Test]
        [Category("Count Overlaps Range")]
        public void CountOverlapsRange_LayerExample()
        {
            // ****************************************
            // | 0    5   10   15   20   25   30   35 |
            // | |    |    |    |    |    |    |    | |
            // |                                      |
            // | intervals:                           |
            // | [    )                               |
            // |  [  )                                |
            // |   |                                  |
            // |       [     )                        |
            // |         [)                           |
            // |           [)                         |
            // |              [)                      |
            // |                                      |
            // | queries:                             |
            // |         [)                           |
            // |    [   )                             |
            // |                                      |
            // ****************************************

            var intervals = new[]
            {
                new Interval(0, 5),
                new Interval(1, 4),
                new Interval(2),
                new Interval(6, 12),
                new Interval(8, 9),
                new Interval(10, 11),
                new Interval(13, 14), 
            };

            var collection = CreateCollection<Interval, int>(intervals);

            var expected1 = collection.AllowsContainments ? 2 : 1;
            var actual1 = collection.CountOverlaps(new Interval(8, 9));
            Assert.AreEqual(expected1, actual1);

            var expected2 = collection.AllowsContainments ? 3 : 2;
            var actual2 = collection.CountOverlaps(new Interval(3, 7));
            Assert.AreEqual(expected2, actual2);
        }

        #endregion

        #region Legacy Examples

        [Test]
        public void CountOverlaps_NoContainments()
        {
            var intervals = new[]
            {
                new Interval( 0,  5, IntervalType.Closed),
                new Interval( 2,  6, IntervalType.Closed),
                new Interval( 6,  8, IntervalType.Closed),
                new Interval(11, 17, IntervalType.Closed),
                new Interval(11, 17, IntervalType.Closed),
                new Interval(11, 18, IntervalType.Closed),
                new Interval(17, 18, IntervalType.Closed),
                new Interval(17, 20, IntervalType.Closed),
                new Interval(23, 28, IntervalType.Closed),
            };

            var collection = CreateCollection<Interval, int>(intervals);

            // Non overlaps
            Assert.AreEqual(0, collection.CountOverlaps(new Interval(9)));
            Assert.AreEqual(0, collection.CountOverlaps(new Interval(29, 30, IntervalType.Closed)));
            Assert.AreEqual(0, collection.CountOverlaps(new Interval(int.MinValue, -2, IntervalType.Closed)));

            // Few overlaps
            Assert.AreEqual(1, collection.CountOverlaps(new Interval(0)));
            Assert.AreEqual(collection.AllowsOverlaps ? 2 : 1, collection.CountOverlaps(new Interval(6, 9, IntervalType.Closed)));
            Assert.AreEqual(1, collection.CountOverlaps(new Interval(21, 30, IntervalType.Closed)));

            // Many overlaps   
            Assert.AreEqual(collection.AllowsOverlaps ? 3 : 2, collection.CountOverlaps(new Interval(0, 6, IntervalType.Closed)));
            Assert.AreEqual(collection.AllowsOverlaps ? 5 : 1, collection.CountOverlaps(new Interval(12, 19, IntervalType.Closed)));
            Assert.AreEqual(collection.AllowsOverlaps ? 3 : 0, collection.CountOverlaps(new Interval(18, 19, IntervalType.Closed)));
        }

        [Test]
        public void CountOverlaps_OnlyContainments()
        {
            var intervals = new[]
            {
                new Interval( 0, 30, IntervalType.Closed),
                new Interval( 1, 29, IntervalType.Closed),
                new Interval( 2, 20, IntervalType.Closed),
                new Interval( 3, 19, IntervalType.Closed),
                new Interval( 9, 18, IntervalType.Closed),
                new Interval(10, 17, IntervalType.Closed),
                new Interval(11, 16, IntervalType.Closed),
                new Interval(12, 15, IntervalType.Closed),
                new Interval(13, 14, IntervalType.Closed),
            };

            var collection = CreateCollection<Interval, int>(intervals);

            // No overlaps
            Assert.AreEqual(0, collection.CountOverlaps(new Interval(31, int.MaxValue, IntervalType.Closed)));
            Assert.AreEqual(0, collection.CountOverlaps(new Interval(int.MinValue, -2, IntervalType.Closed)));

            // Single overlap
            Assert.AreEqual(1, collection.CountOverlaps(new Interval(0)));
            Assert.AreEqual(1, collection.CountOverlaps(new Interval(30, 35, IntervalType.Closed)));

            // Many overlaps
            Assert.AreEqual(collection.AllowsContainments ? 2 : 1, collection.CountOverlaps(new Interval(23, 25, IntervalType.Closed)));
            Assert.AreEqual(collection.AllowsContainments ? 4 : 1, collection.CountOverlaps(new Interval(5, 8, IntervalType.Closed)));
            Assert.AreEqual(collection.AllowsContainments ? 9 : 1, collection.CountOverlaps(new Interval(13, 13, IntervalType.Closed)));
        }

        #endregion

        #endregion

        #region Gaps

        #region Gaps

        [Test]
        [Category("Gaps")]
        public void Gaps_EmptyCollection_Empty()
        {
            var collection = CreateEmptyCollection<Interval, int>();

            CollectionAssert.IsEmpty(collection.Gaps);
        }

        [Test]
        [Category("Gaps")]
        public void Gaps_SingleInterval_Empty()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);

            CollectionAssert.IsEmpty(collection.Gaps);
        }

        [Test]
        [Category("Gaps")]
        public void Gaps_SingleObject_Empty()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);

            CollectionAssert.IsEmpty(collection.Gaps);
        }

        [Test]
        [Category("Gaps")]
        public void Gaps_DuplicateIntervals_Empty()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            CollectionAssert.IsEmpty(collection.Gaps);
        }

        [Test]
        [Category("Gaps")]
        public void Gaps_ManyIntervals_Gaps()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            var gaps = collection.Gaps;

            foreach (var gap in gaps)
            {
                Assert.False(collection.Any(x => x.Overlaps(gap)));
            }
        }

        [Test]
        [Category("Gaps")]
        public void Gaps_MeetingIntervals_Gaps_FixedSeed()
        {
            updateRandom(-1138109753);
            Gaps_MeetingIntervals_Gaps();
        }

        [Test]
        [Category("Gaps")]
        public void Gaps_MeetingIntervals_Gaps()
        {
            var count = Count;
            count += count % 2 + 1;
            var add = false;
            var intervals = MeetingIntervals(count);

            var expected = new ArrayList<Interval>();
            var input = new ArrayList<Interval>();

            foreach (var interval in intervals)
                ((add = !add) ? input : expected).Add(interval);

            var collection = CreateCollection<Interval, int>(input.ToArray());

            var gapsArray = collection.Gaps.ToArray();
            var expectedArray = expected.ToArray();

            for (int i = 0; i < count / 2; i++)
                Assert.True(expectedArray[i].IntervalEquals(gapsArray[i]));
        }

        [Test]
        [Category("Gaps")]
        public void Gaps_OverlappingIntervals_NoGaps()
        {
            var interval = SingleInterval();
            var interval2 = SingleInterval();
            while (!interval.Overlaps(interval2))
                interval2 = SingleInterval();
            Assert.True(interval.Overlaps(interval2));

            var collection = CreateCollection<Interval, int>(interval, interval2);

            CollectionAssert.IsEmpty(collection.Gaps);
        }

        [Test]
        [Category("Gaps")]
        public void Gaps_MeetingIntervals_NoGaps()
        {
            var interval = new Interval(0, 1, IntervalType.LowIncluded);
            var interval2 = new Interval(1, 2, IntervalType.LowIncluded);

            var collection = CreateCollection<Interval, int>(interval, interval2);

            CollectionAssert.IsEmpty(collection.Gaps);


            interval = new Interval(0, 1, IntervalType.HighIncluded);
            interval2 = new Interval(1, 2, IntervalType.HighIncluded);

            collection = CreateCollection<Interval, int>(interval, interval2);

            CollectionAssert.IsEmpty(collection.Gaps);
        }

        [Test]
        [Category("Gaps")]
        public void Gaps_MeetingIntervals_PointGap()
        {
            var interval = new Interval(0, 1, IntervalType.LowIncluded);
            var interval2 = new Interval(1, 2, IntervalType.HighIncluded);

            var collection = CreateCollection<Interval, int>(interval, interval2);

            var gaps = collection.Gaps;
            Assert.AreEqual(1, gaps.Count());
            Assert.True((new Interval(1)).IntervalEquals(gaps.First()));
        }


        [Test]
        [Category("Gaps")]
        public void Gaps_WeldingExample()
        {
            // ****************************************
            // | 0    5   10   15   20   25   30   35 |
            // | |    |    |    |    |    |    |    | |
            // | wel1d:                               |
            // | [  )                                 |
            // |      [)                              |
            // |           [    )                     |
            // |                     [)               |
            // |                                      |
            // |                                      |
            // | weld2:                               |
            // |  [)                                  |
            // |     [  )                             |
            // |                                      |
            // |                                      |
            // | paint:                               |
            // |   [ )                                |
            // |       [   )                          |
            // |             [)                       |
            // |                 [  )                 |
            // ****************************************

            var weld1 = new[]
            {
                new IntervalBase<int>( 0,  3),
                new IntervalBase<int>( 5,  6),
                new IntervalBase<int>(10, 15),
                new IntervalBase<int>(20, 21)
            };
            var weld2 = new[]
            {
                new IntervalBase<int>( 1,  2),
                new IntervalBase<int>( 4,  7)
            };
            var paint = new[]
            {
                new IntervalBase<int>( 2,  4),
                new IntervalBase<int>( 6, 10),
                new IntervalBase<int>(12, 13),
                new IntervalBase<int>(16, 19)
            };

            var weld1Paint = CreateCollection<IInterval<int>, int>(weld1.Concat(paint).ToArray());

            if (!weld1Paint.AllowsOverlaps)
                return;

            var weld1Result = new[]
            {
                new IntervalBase<int>( 4,  5),
                new IntervalBase<int>(15, 16),
                new IntervalBase<int>(19, 20),
            };
            var gaps = weld1Paint.Gaps;
            CollectionAssert.AreEquivalent(weld1Result, gaps);

            var weld2Paint = CreateCollection<IInterval<int>, int>(weld2.Concat(paint).ToArray());
            var weld2Result = new[]
            {
                new IntervalBase<int>(10, 12),
                new IntervalBase<int>(13, 16)
            };
            CollectionAssert.AreEquivalent(weld2Result, weld2Paint.Gaps);
        }

        #endregion

        #region Find Gaps

        [Test]
        [Category("Find Gaps")]
        public void FindGaps_EmptyCollection_GapMatchesQuery()
        {
            var query = SingleInterval();
            var collection = CreateEmptyCollection<Interval, int>();

            var gaps = collection.FindGaps(query);

            SingleIntervalEquals(query, gaps);
        }

        [Test]
        [Category("Find Gaps")]
        public void FindGaps_SingleInterval_QueryWithIntervalEmpty()
        {
            var interval = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);

            CollectionAssert.IsEmpty(collection.FindGaps(interval));
        }

        [Test]
        [Category("Find Gaps")]
        public void FindGaps_SingleInterval_QueryWithNoOverlap()
        {
            var interval = SingleInterval();
            var query = SingleInterval();
            while (interval.Overlaps(query)) query = SingleInterval();
            var collection = CreateCollection<Interval, int>(interval);

            SingleIntervalEquals(query, collection.FindGaps(query));
        }

        [Test]
        [Category("Find Gaps")]
        public void FindGaps_SingleObject_QueryWithIntervalEmpty()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);

            CollectionAssert.IsEmpty(collection.FindGaps(intervals.First()));
        }

        [Test]
        [Category("Find Gaps")]
        public void FindGaps_SingleObject_QueryWithNoOverlap()
        {
            var intervals = SingleObject();
            var query = SingleInterval();
            while (intervals.First().Overlaps(query)) query = SingleInterval();
            var collection = CreateCollection<Interval, int>(intervals);

            SingleIntervalEquals(query, collection.FindGaps(query));
        }

        [Test]
        [Category("Find Gaps")]
        public void FindGaps_DuplicateIntervals_QueryWithIntervalEmpty()
        {
            var interval = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(interval);

            CollectionAssert.IsEmpty(collection.FindGaps(interval.First()));
        }

        [Test]
        [Category("Find Gaps")]
        public void FindGaps_DuplicateIntervals_QueryWithNoOverlap()
        {
            var intervals = DuplicateIntervals();
            var query = SingleInterval();
            while (intervals.First().Overlaps(query)) query = SingleInterval();
            var collection = CreateCollection<Interval, int>(intervals);

            var gaps = collection.FindGaps(query);
            SingleIntervalEquals(query, gaps);
        }

        [Test]
        [Category("Find Gaps")]
        public void FindGaps_ManyIntervals_Gaps()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);
            var span = collection.Span;

            var gaps = collection.FindGaps(span);

            foreach (var gap in gaps)
            {
                Assert.False(collection.Any(x => x.Overlaps(gap)));
                Assert.True(span.Contains(gap));
            }
        }

        [Test]
        [Category("Find Gaps")]
        [Combinatorial]
        public void FindGaps_MeetingIntervals_Gaps(
            [Values(0, 1)] int count,
            [Values(true, false)] bool add)
        {
            var intervals = MeetingIntervals(100 + count);
            var span = intervals.Span();

            var expected = new ArrayList<Interval>();
            var input = new ArrayList<Interval>();

            foreach (var interval in intervals)
                ((add = !add) ? expected : input).Add(interval);

            var collection = CreateCollection<Interval, int>(input.ToArray());

            var gaps = collection.FindGaps(span);

            Assert.False(gaps.Any(x => x.OverlapsAny(collection)));

            Assert.True(expected.Zip(gaps, (x, y) => x.IntervalEquals(y)).All(b => b));
        }

        #endregion

        #region Find Gap
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
            var collection = CreateEmptyCollection<Interval, int>();

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
            var intervals = SingleObject();
            var collection = CreateEmptyCollection<Interval, int>();

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
            var intervals = DuplicateIntervals();
            var collection = CreateEmptyCollection<Interval, int>();

            if (!collection.IsReadOnly)
            {
                for (var i = 0; i < Count; i++)
                {
                    if (i == 0)
                        Assert.True(collection.Add(intervals[i]));
                    else
                        Assert.AreEqual(collection.AllowsOverlaps, collection.Add(intervals[i]));
                }
            }
        }

        [Test]
        [Category("Add")]
        public void Add_ManyIntervals_AllAdded()
        {
            var intervals = ManyIntervals();
            var collection = CreateEmptyCollection<Interval, int>();

            if (!collection.IsReadOnly)
            {
                for (var i = 0; i < Count; i++)
                {
                    if (i == 0)
                    {
                        Assert.True(collection.Add(intervals[i]));
                    }
                    else
                    {
                        Assert.AreEqual(
                            AddTest(collection, intervals, intervals[i]),
                            collection.Add(intervals[i])
                        );
                    }
                }
            }
        }

        private static bool AddTest(IIntervalCollection<Interval, int> collection, Interval[] intervals, Interval interval)
        {
            return collection.AllowsOverlaps ? collection.AllowsContainments || !collection.Any(x => x.StrictlyContains(interval) || interval.StrictlyContains(x)) : !interval.OverlapsAny(collection);
        }

        #region Events

        [Test]
        [Category("Add Event")]
        public void Add_ManyIntervals_EventThrown()
        {
            var intervals = ManyIntervals();
            var collection = CreateEmptyCollection<Interval, int>();

            if (!collection.IsReadOnly)
            {
                Interval eventInterval = null;
                IIntervalCollection<Interval, int> eventCollection = null;
                collection.ItemsAdded += (sender, args) => eventInterval = args.Item;
                collection.CollectionChanged += sender => eventCollection = (IIntervalCollection<Interval, int>) sender;

                foreach (var interval in (InsertedIntervals(collection, intervals)))
                {
                    Assert.True(collection.Add(interval));
                    Assert.AreSame(interval, eventInterval);
                    Assert.AreSame(collection, eventCollection);
                    eventInterval = null;
                    eventCollection = null;

                    Assert.AreEqual(collection.AllowsReferenceDuplicates, collection.Add(interval));
                    if (collection.AllowsReferenceDuplicates)
                    {
                        Assert.AreSame(interval, eventInterval);
                        Assert.AreSame(collection, eventCollection);
                    }
                    else
                    {
                        Assert.IsNull(eventInterval);
                        Assert.IsNull(eventCollection);
                    }
                    eventInterval = null;
                    eventCollection = null;
                }
            }
        }

        #endregion

        #endregion

        #region Add All

        // TODO: Make more generic tests for AddAll

        [Test]
        [Category("Add All")]
        public void AddAll_IsReadOnly_Exception()
        {
            var intervals = new[] { SingleInterval() };
            var collection = CreateEmptyCollection<Interval, int>();

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
            var collection = CreateEmptyCollection<Interval, int>();

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
            var collection = CreateEmptyCollection<Interval, int>();

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
            var collection = CreateCollection<Interval, int>(singleInterval);

            if (!collection.IsReadOnly)
            {
                Assert.False(collection.Remove(new Interval(singleInterval)));
                Assert.True(collection.Remove(singleInterval));
                Assert.False(collection.Remove(singleInterval));
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_SingleObject_Removed_FixedSeed()
        {
            updateRandom(1358768120);
            Remove_SingleObject_Removed();
        }

        [Test]
        [Category("Remove")]
        public void Remove_SingleObject_Removed()
        {
            var intervals = SingleObject();
            var collection = CreateCollection<Interval, int>(intervals);

            if (!collection.IsReadOnly)
            {
                for (var i = 0; i < Count; i++)
                {
                    if (i == 0)
                    {
                        Assert.False(collection.Remove(new Interval(intervals[i])));
                        Assert.True(collection.Remove(intervals[i]));
                    }
                    else
                        Assert.AreEqual(collection.AllowsReferenceDuplicates, collection.Remove(intervals[i]));
                }
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_DuplicateIntervals_True_FixedSeed()
        {
            updateRandom(1747493494);
            Remove_DuplicateIntervals_True();
            updateRandom(-356355912);
            Remove_DuplicateIntervals_True();
        }

        [Test]
        [Category("Remove")]
        public void Remove_DuplicateIntervals_True()
        {
            var intervals = DuplicateIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            if (!collection.IsReadOnly)
            {
                foreach (var interval in (InsertedIntervals(collection, intervals)))
                {
                    Assert.False(collection.Remove(new Interval(interval)));
                    Assert.True(collection.Remove(interval));
                    Assert.False(collection.Remove(interval));
                }
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_ManyIntervals_True_FixedSeed()
        {
            updateRandom(-284197963);
            Remove_ManyIntervals_True();
            updateRandom(873007789);
            Remove_ManyIntervals_True();
            updateRandom(-1204159134);
            Remove_ManyIntervals_True();
        }

        [Test]
        [Category("Remove")]
        public void Remove_ManyIntervals_True()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            if (!collection.IsReadOnly)
            {
                foreach (var interval in (InsertedIntervals(collection, intervals)))
                {
                    Assert.False(collection.Remove(new Interval(interval)));
                    Assert.True(collection.Remove(interval));
                    Assert.False(collection.Remove(interval));
                }
            }
        }

        [Test]
        [Category("Remove")]
        public void Remove_ManyIntervals_RemovingIntervalsNotInCollection()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            if (!collection.IsReadOnly)
            {
                foreach (var interval in ManyIntervals())
                    Assert.False(collection.Remove(interval));
            }

            Assert.AreEqual(InsertedIntervals(collection, intervals).Count(), collection.Count);
        }

        #region Events

        [Test]
        [Category("Remove Event")]
        public void Remove_ManyIntervals_EventThrown_FixedSeed()
        {
            updateRandom(1352270728);
            Remove_ManyIntervals_EventThrown();
            updateRandom(-904807620);
            Remove_ManyIntervals_EventThrown();
            updateRandom(-356355912);
            Remove_ManyIntervals_EventThrown();
            updateRandom(693772309);
            Remove_ManyIntervals_EventThrown();
            updateRandom(288655792);
            Remove_ManyIntervals_EventThrown();
        }

        [Test]
        [Category("Remove Event")]
        public void Remove_ManyIntervals_EventThrown()
        {
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals.Concat(intervals).ToArray());

            if (!collection.IsReadOnly)
            {
                IInterval<int> eventInterval = null;
                IIntervalCollection<Interval, int> eventCollection = null;
                collection.ItemsRemoved += (sender, args) => eventInterval = args.Item;
                collection.CollectionChanged += sender => eventCollection = (IIntervalCollection<Interval, int>) sender;

                foreach (var interval in (InsertedIntervals(collection, intervals)))
                {
                    Assert.True(collection.Remove(interval));
                    Assert.AreSame(interval, eventInterval);
                    Assert.AreSame(collection, eventCollection);
                    eventInterval = null;
                    eventCollection = null;

                    Assert.AreEqual(collection.AllowsReferenceDuplicates, collection.Remove(interval));
                    if (collection.AllowsReferenceDuplicates)
                    {
                        Assert.AreSame(interval, eventInterval);
                        Assert.AreSame(collection, eventCollection);
                    }
                    else
                    {
                        Assert.IsNull(eventInterval);
                        Assert.IsNull(eventCollection);
                    }
                    eventInterval = null;
                    eventCollection = null;

                    Assert.False(collection.Remove(interval));
                    Assert.IsNull(eventInterval);
                    Assert.IsNull(eventCollection);
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
            var collection = CreateEmptyCollection<Interval, int>();

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
            var collection = CreateEmptyCollection<Interval, int>();

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
            var collection = CreateCollection<Interval, int>(interval);

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
            var interval = ManyIntervals();
            var collection = CreateCollection<Interval, int>(interval);

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
            var intervals = ManyIntervals();
            var collection = CreateCollection<Interval, int>(intervals);

            if (!collection.IsReadOnly)
            {
                var eventThrown = false;
                IIntervalCollection<Interval, int> eventCollection = null;
                collection.CollectionCleared += (sender, args) => eventThrown = true;
                collection.CollectionChanged += sender => eventCollection = (IIntervalCollection<Interval, int>) sender;

                collection.Clear();
                Assert.True(eventThrown);
                Assert.AreSame(collection, eventCollection);
                Assert.True(collection.IsEmpty);
                eventThrown = false;
                eventCollection = null;

                collection.Clear();
                Assert.False(eventThrown);
                Assert.IsNull(eventCollection);
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

            var collection = CreateCollection<Interval, int>(intervals);

            foreach (var point in intervals.UniqueEndpointValues())
            {
                var expected = (InsertedIntervals(collection, intervals)).Where(x => x.Overlaps(point));
                CollectionAssert.AreEquivalent(expected, collection.FindOverlaps(point));
            }

            var span = InsertedIntervals(collection, intervals).Span();
            Assert.True(span.IntervalEquals(collection.Span));

            {
                var expected = collection.AllowsOverlaps ? collection.AllowsContainments ? 5 : 4 : 1;
                Assert.AreEqual(expected, collection.MaximumDepth);
            }

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

        #region Containment List Example

        [Test]
        [Category("Example Cases")]
        public void NestedContainmentListArticleExample()
        {
            var intervals = new[]
                {
                    new Interval( 0,  2),   // A
                    new Interval( 1,  6),   // B
                    new Interval( 3, 19),   // C
                    new Interval( 4,  9),   // D
                    new Interval( 5,  7),   // E
                    new Interval( 7, 11),   // F
                    new Interval( 8, 14),   // G
                    new Interval(10, 12),   // H
                    new Interval(15, 18),   // I
                    new Interval(16, 17),   // J
                    new Interval(20, 24),   // K
                    new Interval(22, 26),   // L
                    new Interval(23, 25),   // M
                    new Interval(25, 27),   // N
                    new Interval(26, 30),   // O
                    new Interval(28, 29)    // P
                };

            var collection = CreateCollection<Interval, int>(intervals);

            var query = new Interval(13, 21);
            var expected = InsertedIntervals(collection, intervals).Where(x => x.Overlaps(query));

            CollectionAssert.AreEquivalent(expected, collection.FindOverlaps(query));
        }

        [Test]
        [Category("Example Cases")]
        public void LayeredContainmentListArticleExample()
        {
            var intervals = new[]
                {
                    new Interval( 0,  9),   // A
                    new Interval( 3, 18),   // B
                    new Interval( 9, 52),   // C
                    new Interval(12, 25),   // D
                    new Interval(16, 20),   // E
                    new Interval(22, 28),   // F
                    new Interval(22, 48),   // G
                    new Interval(26, 32),   // H
                    new Interval(27, 36),   // I
                    new Interval(38, 53),   // J
                    new Interval(43, 47),   // K
                    new Interval(55, 71),   // L
                    new Interval(61, 70),   // M
                    new Interval(64, 68),   // N
                    new Interval(69, 76),   // O
                    new Interval(72, 80),   // P
                    new Interval(76, 80)    // Q
                };

            var collection = CreateCollection<Interval, int>(intervals);

            var query = new Interval(13, 21);
            var expected = InsertedIntervals(collection, intervals).Where(x => x.Overlaps(query));

            CollectionAssert.AreEquivalent(expected, collection.FindOverlaps(query));
        }

        #endregion

        #region Large Scale Random

        [Test]
        [Category("Large Scale")]
        public void Random_CallWholeInterface_FixedSeed()
        {
            updateRandom(550523346);
            Random_CallWholeInterface();
        }

        [Test]
        [Category("Large Scale")]
        public void Random_CallWholeInterface()
        {
            var collection = CreateEmptyCollection<Interval, int>();

            var sum = 0;

            if (!collection.IsReadOnly)
            {
                var count = Random.Next(50, 100);
                var set = new HashSet<Interval>();
                var intervals = ManyIntervals(count);

                for (var i = 0; i < count; i++)
                {
                    var interval = intervals[i];
                    if (collection.Add(interval))
                        set.Add(interval);

                    interval = set.Choose();
                    var span = collection.Span;
                    var mno = collection.MaximumDepth;
                    Assert.AreEqual(collection.FindOverlaps(interval).Count(), collection.CountOverlaps(interval));
                    Assert.AreEqual(collection.FindOverlaps(interval.Low).Count(), collection.CountOverlaps(interval.Low));
                    sum += collection.FindOverlaps(interval).Count();
                    Assert.True(collection.FindOverlap(interval, out interval));
                    collection.FindOverlap(interval.Low, out interval);

                    var remove = Random.Next(0, 2);
                    if (remove == 1)
                    {
                        interval = set.Choose();
                        collection.Remove(interval);
                        set.Remove(interval);
                    }
                }

                collection.Clear();
                CollectionAssert.IsEmpty(collection);
            }

            Console.Out.WriteLine("Sum: " + sum);
            Console.Out.WriteLine("Count: " + collection.Count);
        }

        #endregion

        #endregion

        #endregion
    }

    internal class AdditionalSeedsAttribute : Attribute
    {
        public AdditionalSeedsAttribute(params int[] seeds)
        {
            //throw new NotImplementedException();
        }
    }
}
