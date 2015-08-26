using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// <para>
    /// An interval collection that fast enumeration of the collection in sorted order.
    /// </para>
    /// </summary>
    /// <typeparam name="I">The interval type in the collection.</typeparam>
    /// <typeparam name="T">The interval's endpoint values.</typeparam>
    /// <remarks>
    /// <para>
    /// Sorting order is based on endpoints and their inclusion. Intervals are sorted first
    /// on lowest low endpoint, then lowest high endpoint.
    /// </para>
    /// </remarks>
    [ContractClass(typeof(SortedIntervalCollectionContract<,>))]
    public interface ISortedIntervalCollection<I, T> : IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Sorted Enumeration

        // TODO: Use IDirectedEnumerable?
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection sorted by 
        /// lowest low, then lowest high endpoint.
        /// </summary>
        /// <remarks>
        /// The enumerable must be lazy, while giving the same result as a stable sorting of
        /// the collection using <see cref="IntervalExtensions.CompareTo{T}"/>.
        /// </remarks>
        /// <value>An enumerable, enumerating all collection intervals in sorted order.</value>
        [Pure]
        IEnumerable<I> Sorted { get; }

        #endregion
    }

    [ContractClassFor(typeof(ISortedIntervalCollection<,>))]
    internal abstract class SortedIntervalCollectionContract<I, T> : IntervalCollectionBase<I, T>, ISortedIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        public IEnumerable<I> Sorted
        {
            get
            {
                Contract.Ensures(IsEmpty != Contract.Result<IEnumerable<I>>().Any());

                // The intervals are sorted
                Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());

                throw new NotImplementedException();
            }
        }
    }
}