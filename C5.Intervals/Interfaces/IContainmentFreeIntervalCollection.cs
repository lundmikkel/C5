using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    // TODO: Rename to IEndpointSortedIntervalCollection?
    /// <summary>
    /// <para>
    /// An interval collection that does not allow contained intervals.
    /// </para>
    /// <para>
    /// An interval contained in another interval breaks the high (low) endpoint ordering,
    /// if a collection is sorted on low (high) endpoints. Collections disallowing
    /// containments can normally be optimized more than collections that allow them, giving
    /// speed improvements in cases where containments are non-existing, i.e. when all 
    /// intervals have equal length.
    /// </para>
    /// </summary>
    /// <typeparam name="I">The interval type in the collection. Especially used for return 
    /// types for enumeration.</typeparam>
    /// <typeparam name="T">The interval's endpoint values.</typeparam>
    /// <remarks>
    /// An interval is not considered contained, if is shares either endpoint with is the 
    /// containing interval.
    /// </remarks>
    [ContractClass(typeof(ContainmentFreeIntervalCollectionContract<,>))]
    public interface IContainmentFreeIntervalCollection<I, T> : ISortedIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
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

        #endregion

        #region Sorted Enumeration

        // TODO: Review documentation
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection sorted by 
        /// highest high, then highest low.
        /// </summary>
        /// <remarks>
        /// The result is equal to <c>coll.Sorted().Reverse()</c>.
        /// </remarks>
        /// <returns>
        /// An enumerable, enumerating the collection backwards in sorted order.
        /// </returns>
        [Pure]
        IEnumerable<I> SortedBackwards();

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted 
        /// order starting from the first interval that overlaps or follows 
        /// <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The result is equal to
        /// <c>coll.Sorted().SkipWhile(x => x.CompareHigh(point) &lt; 0)</c>.
        /// </para>
        /// <para>
        /// None of the intervals are guaranteed to overlap <paramref name="point"/>.
        /// </para>
        /// </remarks>
        /// <param name="point">The point from which to start the enumeration.</param>
        /// <param name="includeOverlaps">
        /// If false, any overlap is excluded. The result is equal to
        /// <c>coll.Sorted().SkipWhile(x => x.CompareLow(point) &lt;= 0)</c>.
        /// </param>
        /// <returns>An enumerable, enumerating all collection intervals not less than the 
        /// point in sorted order.</returns>
        [Pure]
        IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true);

        // TODO: Review documentation
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection backwards in 
        /// sorted order starting from the first interval that overlaps or comes before
        /// <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equal to
        /// <c>coll.SortedBackwards().SkipWhile(x => x.CompareLow(point) &gt; 0)</c>.
        /// </para>
        /// <para>
        /// None of the intervals are guaranteed to overlap <paramref name="point"/>.
        /// </para>
        /// </remarks>
        /// <param name="point">The point from which to start the backwards enumeration.</param>
        /// <param name="includeOverlaps">
        /// If false, any overlap is excluded. This is equal to
        /// <c>coll.SortedBackwards().SkipWhile(x => x.CompareHigh(point) &gt;= 0)</c>.
        /// </param>
        /// <returns>An enumerable, enumerating backwards all collection intervals not 
        /// greater than the point in sorted order.</returns>
        [Pure]
        IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true);

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted 
        /// order starting from the first interval that overlaps or follows 
        /// <paramref name="interval"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The result is equal to
        /// <c>coll.Sorted().SkipWhile(x => x.CompareHighLow(interval) &lt; 0)</c>.
        /// </para>
        /// <para>
        /// None of the intervals are guaranteed to overlap <paramref name="interval"/>.
        /// </para>
        /// </remarks>
        /// <param name="interval">The interval from which to start the enumeration.</param>
        /// <param name="includeOverlaps">
        /// If false, any overlap is excluded. The result is equal to
        /// <c>coll.Sorted().SkipWhile(x => x.CompareLowHigh(interval) &lt;= 0)</c>.
        /// </param>
        /// <returns>An enumerable, enumerating all collection intervals not less than the 
        /// point in sorted order.</returns>
        [Pure]
        IEnumerable<I> EnumerateFrom(IInterval<T> interval, bool includeOverlaps = true);

        // TODO: Document
        [Pure]
        IEnumerable<I> EnumerateBackwardsFrom(IInterval<T> interval, bool includeOverlaps = true);

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the interval at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index to start from.</param>
        /// <remarks>
        /// This is equal to <code>coll.Sorted().Skip(index)</code>.
        /// </remarks>
        /// <returns>An enumerable, enumerating all collection intervals starting from the given index.</returns>
        [Pure]
        IEnumerable<I> EnumerateFromIndex(int index);

        // TODO: Document
        [Pure]
        IEnumerable<I> EnumerateBackwardsFromIndex(int index);

        #endregion

        #region Indexed Access

        /// <summary>
        /// Get the interval at index <paramref name="i"/>. First interval has index 0.
        /// 
        /// The result is equal to <c>coll.Sorted().Skip(i).First()</c>.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The <c>i</c>'th interval.</returns>
        [Pure]
        I this[int i] { get; }

        #endregion
    }

    [ContractClassFor(typeof(IContainmentFreeIntervalCollection<,>))]
    internal abstract class ContainmentFreeIntervalCollectionContract<I, T> : SortedIntervalCollectionBase<I, T>, IContainmentFreeIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Data Structure Properties

        public override bool AllowsContainments
        {
            get
            {
                // Containments not allowed
                Contract.Ensures(!Contract.Result<bool>());

                throw new NotImplementedException();
            }
        }

        public Speed IndexingSpeed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Sorted Enumeration
        public override IEnumerable<I> Sorted
        {
            get
            {
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
                Sorted.Reverse(),
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
                    ? Sorted.SkipWhile(x => x.CompareHigh(point) < 0)
                    : Sorted.SkipWhile(x => x.CompareLow(point) <= 0),
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

        public IEnumerable<I> EnumerateFrom(IInterval<T> interval, bool includeOverlaps = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);

            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping as long as high is lower than point
            Contract.Ensures(Enumerable.SequenceEqual(
                includeOverlaps
                    ? Sorted.SkipWhile(x => x.CompareHighLow(interval) < 0)
                    : Sorted.SkipWhile(x => x.CompareLowHigh(interval) <= 0),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateBackwardsFrom(IInterval<T> interval, bool includeOverlaps = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping as long as high is lower than point
            Contract.Ensures(Enumerable.SequenceEqual(
                includeOverlaps
                    ? SortedBackwards().SkipWhile(x => x.CompareLowHigh(interval) > 0)
                    : SortedBackwards().SkipWhile(x => x.CompareHighLow(interval) >= 0),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateFromIndex(int index)
        {
            // Removes constraint to work like Skip()
            // Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted.Skip(index),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            // Negative indices is the same as Sorted
            Contract.Ensures(0 <= index ||
                Enumerable.SequenceEqual(
                    Sorted,
                    Contract.Result<IEnumerable<I>>(),
                    IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
                )
            );
            // Indices greater than or equal to count are empty
            Contract.Ensures(index < Count ||
                Enumerable.SequenceEqual(
                    Enumerable.Empty<I>(),
                    Contract.Result<IEnumerable<I>>(),
                    IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
                )
            );

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateBackwardsFromIndex(int index)
        {
            // Removes constraint to work like Skip()
            // Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted.Take(index + 1).Reverse(),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            // Negative indices is the same as empty
            Contract.Ensures(0 <= index ||
                Enumerable.SequenceEqual(
                    Enumerable.Empty<I>(),
                    Contract.Result<IEnumerable<I>>(),
                    IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
                )
            );
            // Indices greater than or equal to count are empty
            Contract.Ensures(index < Count ||
                Enumerable.SequenceEqual(
                    SortedBackwards(),
                    Contract.Result<IEnumerable<I>>(),
                    IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
                )
            );

            throw new NotImplementedException();
        }

        #endregion

        #region Indexed Access

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
                Contract.Ensures(ReferenceEquals(Contract.Result<I>(), Sorted.Skip(i).First()));

                throw new NotImplementedException();
            }
        }

        #endregion
    }
}