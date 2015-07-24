using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace C5.Intervals
{
    // TODO: Split into two different interfaces: Sorted and Indexed?
    /// <summary>
    /// An interval collection where intervals are sorted based on lowest low, then lowest high endpoint.
    /// All enumerators must be lazy.
    /// </summary>
    /// <typeparam name="I">The interval type in the collection. Especially used for return types for enumeration.</typeparam>
    /// <typeparam name="T">The interval's endpoint values.</typeparam>
    [ContractClass(typeof(SortedIntervalCollectionContract<,>))]
    public interface ISortedIntervalCollection<I, T> : IIntervalCollection<I, T>
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

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection sorted by lowest low, then lowest high endpoint.
        /// </summary>
        /// <remarks>
        /// The result is equal to a stable sorting of the collection using <see cref="IntervalExtensions.CompareTo{T}"/>.
        /// </remarks>
        /// <returns>An enumerable, enumerating all collection intervals in sorted order.</returns>
        [Pure]
        IEnumerable<I> Sorted();

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the first interval that overlaps or follows <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// None of the intervals are guaranteed to overlap <paramref name="point"/>.
        /// This is equal to <code>Sorted().SkipWhile(i => i.CompareHigh(point) &lt; 0)</code>.
        /// </remarks>
        /// <param name="point">The point from which to start the enumeration.</param>
        /// <returns>An enumerable, enumerating all collection intervals not less than the point in sorted order.</returns>
        [Pure]
        IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true);

        // TODO: Document and implement
        // [Pure]
        // IEnumerable<I> EnumerateBackwardsFrom(T point);

        // TODO: Document .SkipWhile(i => ReferenceEqual(interval, i)
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the given interval object <paramref name="interval"/>.
        /// 
        /// If the interval object is not in the collection, the result will be empty.
        /// </summary>
        /// <remarks>
        /// This works on reference equality and not interval equality. If <see cref="IIntervalCollection{I,T}.AllowsReferenceDuplicates"/>
        /// is true, the enumeration will start from the first reference duplicate.
        /// This is equal to <code>Sorted().SkipWhile(i => !ReferenceEqual(interval, i))</code>.
        /// </remarks>
        /// <param name="interval">The interval object to locate in the collection.</param>
        /// <returns>An enumerable, enumerating all collection intervals starting from the given object.</returns>
        [Pure]
        IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true);

        // TODO: Document and implement
        // [Pure]
        // IEnumerable<I> EnumerateBackwardsFrom(I interval);

        // TODO: Compare to definition of Skip, which returns the same collection for index <= 0 and an empty collection for index > count
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the interval at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index to start from.</param>
        /// <remarks>
        /// This is equal to <code>Sorted().Skip(i)</code>.
        /// </remarks>
        /// <returns>An enumerable, enumerating all collection intervals starting from the given index.</returns>
        [Pure]
        IEnumerable<I> EnumerateFromIndex(int index);

        // TODO: Document and implement
        // [Pure]
        // IEnumerable<I> EnumerateBackwardsFrom(int index);

        #endregion

        #region Indexed Access

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

    [ContractClassFor(typeof(ISortedIntervalCollection<,>))]
    internal abstract class SortedIntervalCollectionContract<I, T> : IntervalCollectionBase<I, T>, ISortedIntervalCollection<I, T>
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
            // The enumerator is equal to the normal enumerator
            Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(this, Contract.Result<IEnumerable<I>>()));
            // Enumerator has same size as collection
            Contract.Ensures(Count == Contract.Result<IEnumerable<I>>().Count());

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true)
        {
            Contract.Requires(!ReferenceEquals(point, null));

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Enumerable is the same as skipping as long as high is lower than point
            Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(
                Sorted().SkipWhile(i => i.CompareHigh(point) < 0),
                Contract.Result<IEnumerable<I>>()
            ));

            throw new NotImplementedException();
        }

        // public IEnumerable<I> EnumerateBackwardsFrom(T point)
        // {
        //     Contract.Requires(!ReferenceEquals(point, null));
        // 
        //     Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
        //     // Enumerable is the same as skipping until interval overlaps
        //     Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(
        //         Sorted().Reverse().SkipWhile(i => !i.Overlaps(point)),
        //         Contract.Result<IEnumerable<I>>()
        //     ));
        // 
        //     throw new NotImplementedException();
        // }

        public IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Enumerable is the same as skipping until interval is met
            Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(
                Sorted().SkipWhile(i => !ReferenceEquals(i, interval)),
                Contract.Result<IEnumerable<I>>()
            ));

            throw new NotImplementedException();
        }

        // public IEnumerable<I> EnumerateBackwardsFrom(I interval)
        // {
        //     Contract.Requires(interval != null);
        // 
        //     Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
        //     // Enumerable is the same as skipping until interval is met, and then skip one
        //     Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(
        //         Sorted().Reverse().SkipWhile(i => !ReferenceEquals(i, interval)).Skip(1),
        //         Contract.Result<IEnumerable<I>>()
        //     ));
        // 
        //     throw new NotImplementedException();
        // }

        public IEnumerable<I> EnumerateFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(
                Sorted().Skip(index),
                Contract.Result<IEnumerable<I>>()
            ));

            throw new NotImplementedException();
        }

        // public IEnumerable<I> EnumerateBackwardsFrom(int index)
        // {
        //     Contract.Requires(0 <= index && index < Count);
        // 
        //     Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
        //     // Enumerable is the same as skipping the first index intervals
        //     Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(
        //         Sorted().Take(index).Reverse(),
        //         Contract.Result<IEnumerable<I>>()
        //     ));
        // 
        //     throw new NotImplementedException();
        // }

        #endregion

        public int IndexOf(I interval)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<int>() == Sorted().IndexOfSorted(interval, IntervalExtensions.CreateComparer<I, T>()));

            Contract.Ensures(!this.Any(i => ReferenceEquals(interval, i)) || 0 <= Contract.Result<int>() && Contract.Result<int>() < Count);
            // TODO: Is this true?
            Contract.Ensures(this.Any(i => ReferenceEquals(interval, i)) || 0 > Contract.Result<int>() && Contract.Result<int>() >= ~Count);

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