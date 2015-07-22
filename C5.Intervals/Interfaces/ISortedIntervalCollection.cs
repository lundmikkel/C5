using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace C5.Intervals
{
    /// <summary>
    /// A collection that allows fast overlap queries on collections of intervals.
    /// </summary>
    /// <remarks>The data structures do not support updates on its intervals' values.
    /// If you wish to change an interval's endpoints or their inclusion, the interval should 
    /// be removed from the data structure first, changed and then added again.</remarks>
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
        /// Indicates if collection allows enumeration ordered by highest high, then highest low.
        /// </summary>
        /// <remarks>If false, all methods relying on this will throw an <see cref="InvalidOperationException"/>, if called.</remarks>
        /// <value>True if this collection allows enumeration ordered by highest high,
        /// then highest low.</value>
        bool AllowsHighLowEnumeration { get; }

        /// <summary>
        /// The value indicates the type of asymptotic complexity in terms of the indexer of
        /// this collection. This is to allow generic algorithms to alter their behaviour 
        /// for collections that provide good performance when applied to either random or
        /// sequencial access.
        /// </summary>
        /// <value>A characterization of the speed of lookup operations.</value>
        Speed IndexingSpeed { get; }

        #endregion Data Structure Properties

        #endregion Properties

        #region Sorted Enumeration

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted ordered.
        /// 
        /// If <paramref name="lowHighSortedOrder"/> is true, the result is sorted using
        /// <see cref="IntervalExtensions.CreateComparer{I,T}"/>, otherwise using 
        /// <see cref="IntervalExtensions.CreateReversedComparer{I,T}"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="IIntervalCollection{I,T}.AllowsContainments"/> is false, the two enumerators
        /// are guaranteed to be backwards versions of each other.
        /// </remarks>
        /// <param name="lowHighSortedOrder">True if collection should be sorted on lowest low,
        /// then high. False if collection should be sorted on highest high, then low.</param>
        /// <returns>An enumerable, enumerating all collectionintervals in sorted order.</returns>
        [Pure]
        IEnumerable<I> Sorted(bool lowHighSortedOrder = true);

        // TODO: Add one using index
        // TODO: Should this take an I or IInterval?! Which interval should it enumerate from?
        // TODO: Update documentation as well
        /// <summary>
        /// Get a lazy enumerable of the intervals following the interval (or an equal interval) in the collection. If the interval is not in the 
        /// collection, the result will be empty.
        /// </summary>
        /// <param name="interval">The query interval. If not in the collection, the result 
        /// will be empty.</param>
        /// <param name="lowHighSortedOrder">True if order matches <see cref="LowHighSorted"/>,
        /// otherwise order matches <see cref="HighLowSorted"/>.</param>
        /// <returns>A lazy enumerable of the intervals following the interval in the 
        /// collection.</returns>
        [Pure]
        IEnumerable<I> NextIntervals(I interval, bool lowHighSortedOrder = true);

        // TODO: Should this take an I or IInterval?! Which interval should it enumerate from?
        // TODO: Update documentation as well
        /// <summary>
        /// Get a lazy enumerable of the intervals (in reverse lexicographical order) 
        /// preceding the interval (or an equal interval) in the collection. If the interval 
        /// is not in the collection, the result will be empty.
        /// </summary>
        /// <param name="interval">The query interval. If not in the collection, the result 
        /// will be empty.</param>
        /// <param name="lowHighSortedOrder">True if order matches <see cref="Sorted"/>,
        /// otherwise order matches <see cref="Sorted{true}"/>.</param>
        /// <returns>A lazy enumerable of the intervals following the interval in the 
        /// collection.</returns>
        [Pure]
        IEnumerable<I> PreviousIntervals(I interval, bool lowHighSortedOrder = true);

        #endregion Sorted Enumeration

        #region Indexed Access

        // TODO: Should this rather return a boolean value and have the index as an out?
        /// <summary>
        /// Determines the index of the interval in the collection.
        /// </summary>
        /// <param name="interval">The interval to locate in the collection.</param>
        /// <param name="lowHighSortedOrder">True if order matches <see cref="LowHighSorted"/>,
        /// otherwise order matches <see cref="HighLowSorted"/>.</param>
        /// <returns>The index of the interval. A negative number if item not found, namely
        /// the one's complement of the index at which the <code>Add</code> operation would put the item.</returns>
        int IndexOf(I interval, bool lowHighSortedOrder = true);

        /// <summary>
        /// Get the interval at index <paramref name="i"/>. First interval has index 0.
        /// 
        /// The result is equal to <code>Sorted(lowHighSortedOrder).Skip(i).First()</code>.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <param name="lowHighSortedOrder"></param>
        /// <returns>The <code>i</code>'th interval.</returns>
        I this[int i, bool lowHighSortedOrder = true] { get; }

        #endregion Indexed Access
    }

    [ContractClassFor(typeof(ISortedIntervalCollection<,>))]
    internal abstract class SortedIntervalCollectionContract<I, T> : IntervalCollectionBase<I, T>, ISortedIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        #region Data Structure Properties

        public abstract bool AllowsHighLowEnumeration { get; }

        public abstract Speed IndexingSpeed { get; }

        #endregion Data Structure Properties

        #endregion Properties

        #region Sorted Enumeration

        public IEnumerable<I> Sorted(bool lowHighSortedOrder = true)
        {
            Contract.Ensures(IsEmpty != Contract.Result<IEnumerable<I>>().Any());

            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(
                lowHighSortedOrder
                ? IntervalExtensions.CreateComparer<I, T>()
                : IntervalExtensions.CreateReversedComparer<I, T>())
            );
            // The enumerator is equal to the normal enumerator
            Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(this, Contract.Result<IEnumerable<I>>()));
            // Enumerator has same size as collection
            Contract.Ensures(Count == Contract.Result<IEnumerable<I>>().Count());

            throw new NotImplementedException();
        }

        public IEnumerable<I> NextIntervals(I interval, bool lowHighSortedOrder = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // Enumerable is the same as skipping until interval is met, and then skip one
            Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(
                Sorted(lowHighSortedOrder).SkipWhile(i => !ReferenceEquals(i, interval)).Skip(1),
                Contract.Result<IEnumerable<I>>())
            );

            throw new NotImplementedException();
        }

        public IEnumerable<I> PreviousIntervals(I interval, bool lowHighSortedOrder = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // Enumerable is the same as skipping until interval is met, and then skip one
            Contract.Ensures(IntervalCollectionContractHelper.CollectionEquals(
                Sorted(lowHighSortedOrder).Reverse().SkipWhile(i => !ReferenceEquals(i, interval)).Skip(1),
                Contract.Result<IEnumerable<I>>())
            );

            throw new NotImplementedException();
        }


        #endregion Sorted Enumeration

        public int IndexOf(I interval, bool lowHighSortedOrder = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<int>() == Sorted(lowHighSortedOrder).IndexOfSorted(interval, lowHighSortedOrder
                ? IntervalExtensions.CreateComparer<I, T>()
                : IntervalExtensions.CreateReversedComparer<I, T>()));

            Contract.Ensures(!this.Contains(interval) || 0 <= Contract.Result<int>() && Contract.Result<int>() < Count);
            // TODO: Is this true?
            Contract.Ensures(this.Contains(interval) || 0 > Contract.Result<int>() && Contract.Result<int>() >= ~Count);

            throw new NotImplementedException();
        }

        public I this[int i, bool lowHighSortedOrder = true]
        {
            get
            {
                // Requires collection is not empty
                Contract.Requires(!IsEmpty);
                // Requires index be in bounds
                Contract.Requires(0 <= i && i < Count);

                // TODO: This should be enforced by contracts instead of user code
                Contract.EnsuresOnThrow<InvalidOperationException>(!lowHighSortedOrder && AllowsHighLowEnumeration);

                // If the index is correct, the output can never be null
                Contract.Ensures(Contract.Result<I>() != null);
                // Result is the same as skipping the first i elements
                Contract.Ensures(ReferenceEquals(Contract.Result<I>(), Sorted(lowHighSortedOrder).Skip(i).First()));

                throw new NotImplementedException();
            }
        }
    }
}