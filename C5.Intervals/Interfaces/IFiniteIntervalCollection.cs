using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    // TODO: Split into something like IIntervalCollectionWithoutOverlaps and IFiniteIntervalCollection. Maybe also Sorted and Indexed.
    /// <summary>
    /// An interval collection where intervals do not overlap
    /// and where they are sorted based on lowest low, then lowest high endpoint.
    /// All enumerators are be lazy.
    /// </summary>
    /// <typeparam name="I">The interval type in the collection. Especially used for return types for enumeration.</typeparam>
    /// <typeparam name="T">The interval's endpoint values.</typeparam>
    [ContractClass(typeof(FiniteIntervalCollectionContract<,>))]
    public interface IFiniteIntervalCollection<I, T> : IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {   
        #region Properties

        #region Data Structure Properties

        /// <summary>
        /// The value indicates the type of asymptotic complexity in terms of the indexer of
        /// this collection. This is to allow generic algorithms to alter their behaviour 
        /// for collections that provide good performance when applied to either random or
        /// sequencial access.
        /// </summary>
        /// <value>A characterization of the speed of lookup operations.</value>
        [Pure]
        Speed IndexingSpeed { get; }

        #endregion Data Structure Properties

        #endregion Properties

        #region Sorted Enumeration

        // TODO: Use IDirectedEnumerable?
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection sorted by lowest low, then lowest high endpoint.
        /// </summary>
        /// <remarks>
        /// The result is equal to a stable sorting of the collection using <see cref="IntervalExtensions.CompareTo{T}"/>.
        /// </remarks>
        /// <returns>An enumerable, enumerating all collection intervals in sorted order.</returns>
        [Pure]
        IEnumerable<I> Sorted();

        // TODO: Review documentation
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection sorted by highest high, then highest low.
        /// </summary>
        /// <remarks>
        /// The result is equal to <code>Sorted().Reverse()</code>.
        /// </remarks>
        /// <returns>An enumerable, enumerating the collection backwards in sorted order.</returns>
        [Pure]
        IEnumerable<I> SortedBackwards();

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the first interval that overlaps or follows <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// None of the intervals are guaranteed to overlap <paramref name="point"/>.
        /// This is equal to <code>Sorted().SkipWhile(x => x.CompareHigh(point) &lt; 0)</code>.
        /// </remarks>
        /// <param name="point">The comparable point from which to start the enumeration.</param>
        /// <param name="includeOverlaps">If false, any overlap is excluded.
        /// This is equal to <code>Sorted().SkipWhile(x => x.CompareLow(point) &lt;= 0)</code>.</param>
        /// <returns>An enumerable, enumerating all collection intervals not less than the point in sorted order.</returns>
        [Pure]
        IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true);

        // TODO: Review documentation
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection backwards in sorted order
        /// starting from the first interval that overlaps or comes before <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// None of the intervals are guaranteed to overlap <paramref name="point"/>.
        /// This is equal to <code>SortedBackwards().SkipWhile(x => x.CompareLow(point) &gt; 0)</code>.
        /// </remarks>
        /// <param name="point">The comparable point from which to start the enumeration.</param>
        /// <param name="includeOverlaps">If false, any overlap is excluded.
        /// This is equal to <code>SortedBackwards().SkipWhile(x => x.CompareHigh(point) &gt;= 0)</code>.</param>
        /// <returns>An enumerable, enumerating backwards all collection intervals not greater than the point in sorted order.</returns>
        [Pure]
        IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true);

        // TODO: Consider if this should be based on interval or reference equality
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the given interval object <paramref name="interval"/>.
        /// 
        /// If the interval object is not in the collection, the result will be empty.
        /// </summary>
        /// <remarks>
        /// This works on reference equality and not interval equality. If <see cref="IIntervalCollection{I,T}.AllowsReferenceDuplicates"/>
        /// is true, the enumeration will start from the first reference duplicate.
        /// This is equal to <code>Sorted().SkipWhile(x => !ReferenceEqual(interval, x))</code>.
        /// </remarks>
        /// <param name="interval">The interval object to locate in the collection.</param>
        /// <param name="includeInterval">If false, the interval object is excluded from the enumerator. This is equal to
        /// <code>Sorted().SkipWhile(x => !ReferenceEqual(interval, x)).SkipWhile(x => ReferenceEqual(interval, x))</code>.</param>
        /// <returns>An enumerable, enumerating all collection intervals starting from the given object.</returns>
        [Pure]
        IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true);

        // TODO: Document
        [Pure]
        IEnumerable<I> EnumerateBackwardsFrom(I interval, bool includeInterval = true);

        // TODO: Compare to definition of Skip, which returns the same collection for index <= 0 and an empty collection for index > count
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the interval at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index to start from.</param>
        /// <remarks>
        /// This is equal to <code>Sorted().Skip(x)</code>.
        /// </remarks>
        /// <returns>An enumerable, enumerating all collection intervals starting from the given index.</returns>
        [Pure]
        IEnumerable<I> EnumerateFromIndex(int index);

        // TODO: Document
        [Pure]
        IEnumerable<I> EnumerateBackwardsFromIndex(int index);

        #endregion

        #region Indexed Access

        // TODO: Should this have a conditional flag as well? includeIndex
        // TODO: What is the behaviour when index is out of bounds? Think about LINQ and its Skip
        // TODO: Should this rather return a boolean value and have the index as an out?
        /// <summary>
        /// Determines the index of the interval object in the collection.
        /// </summary>
        /// <remarks>
        /// This works on reference equality and not interval equality.
        /// If the collection allows reference duplicates, the index of the first object will be returned.
        /// </remarks>
        /// <param name="interval">The interval object to locate in the collection.</param>
        /// <returns>The index of the interval object. A negative number if item not found,
        /// namely the one's complement of the index at which it should have been.</returns>
        [Pure]
        int IndexOf(I interval);

        /// <summary>
        /// Get the interval at index <paramref name="i"/>. First interval has index 0.
        /// 
        /// The result is equal to <code>Sorted().Skip(i).First()</code>.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The <code>i</code>'th interval.</returns>
        [Pure]
        I this[int i] { get; }

        #endregion Indexed Access
    }

    [ContractClassFor(typeof(IFiniteIntervalCollection<,>))]
    internal abstract class FiniteIntervalCollectionContract<I, T> : IntervalCollectionBase<I, T>, IFiniteIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        #region Data Structure Properties

        public abstract Speed IndexingSpeed { get; }

        #endregion Data Structure Properties

        #endregion Properties

        #region Sorted Enumeration

        public IEnumerable<I> Sorted()
        {
            Contract.Ensures(IsEmpty != Contract.Result<IEnumerable<I>>().Any());

            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // The enumerator is equal to the normal enumerator
            Contract.Ensures(Enumerable.SequenceEqual(
                this,
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>())
            );

            throw new NotImplementedException();
        }

        public IEnumerable<I> SortedBackwards()
        {
            Contract.Ensures(IsEmpty != Contract.Result<IEnumerable<I>>().Any());

            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // The backwards enumerator is equal to the sorted reversed
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted().Reverse(),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>())
            );

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true)
        {
            Contract.Requires(!ReferenceEquals(point, null));

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping as long as high is lower than point
            Contract.Ensures(Enumerable.SequenceEqual(
                includeOverlaps
                    ? Sorted().SkipWhile(x => x.CompareHigh(point) < 0)
                    : Sorted().SkipWhile(x => x.CompareLow(point) <= 0),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true)
        {
            Contract.Requires(!ReferenceEquals(point, null));

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping as long as high is lower than point
            Contract.Ensures(Enumerable.SequenceEqual(
                includeOverlaps
                    ? SortedBackwards().SkipWhile(x => x.CompareLow(point) > 0)
                    : SortedBackwards().SkipWhile(x => x.CompareHigh(point) >= 0),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping until interval is met
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted().SkipWhile(x => !ReferenceEquals(x, interval)).SkipWhile(x => !includeInterval && ReferenceEquals(x, interval)),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateBackwardsFrom(I interval, bool includeInterval = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping until interval is met
            Contract.Ensures(Enumerable.SequenceEqual(
                SortedBackwards().SkipWhile(x => !ReferenceEquals(x, interval)).SkipWhile(x => !includeInterval && ReferenceEquals(x, interval)),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted().Skip(index),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateBackwardsFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted().Take(index + 1).Reverse(),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        #endregion

        public int IndexOf(I interval)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<int>() == Sorted().IndexOfSorted(interval, IntervalExtensions.CreateComparer<I, T>()));
            Contract.Ensures(!this.Any(x => ReferenceEquals(interval, x)) || 0 <= Contract.Result<int>() && Contract.Result<int>() < Count);
            Contract.Ensures(this.Any(x => ReferenceEquals(interval, x)) || 0 > Contract.Result<int>() && Contract.Result<int>() >= ~Count);

            throw new NotImplementedException();
        }

        public I this[int i]
        {
            get
            {
                // Requires collection is not empty
                Contract.Requires(!IsEmpty);
                // Requires index be in bounds
                Contract.Requires(0 <= i && i < Count);

                // If the index is correct, the output can never be null
                Contract.Ensures(Contract.Result<I>() != null);
                // Result is the same as skipping the first i elements
                Contract.Ensures(ReferenceEquals(Contract.Result<I>(), Sorted().Skip(i).First()));

                throw new NotImplementedException();
            }
        }
    }
}