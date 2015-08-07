using System;
using System.Collections.Generic;

namespace C5.Intervals
{
    /// <summary>
    /// Base class for classes implementing <see cref="IIntervalCollection{I,T}"/> for finite interval collections.
    /// </summary>
    /// <remarks>
    /// When extending this class, it should not be necessary reimplement any of the methods. It is however recommended
    /// to have a look at <see cref="IntervalCollectionBase{I,T}"/>, to see which of its methods are worth reimplementing.
    /// </remarks>
    /// <typeparam name="I">The interval type with endpoint type <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    /// <seealso cref="IntervalCollectionBase{I,T}"/>
    public abstract class ContainmentFreeIntervalCollectionBase<I, T> : IntervalCollectionBase<I, T>, IContainmentFreeIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {

        /// <inheritdoc/>
        #region Properties

        #region Data Structure Properties

        /// <summary>
        /// The value indicates the type of asymptotic complexity in terms of the indexer of
        /// this collection. This is to allow generic algorithms to alter their behaviour 
        /// for collections that provide good performance when applied to either random or
        /// sequencial access.
        /// </summary>
        /// <value>A characterization of the speed of lookup operations.</value>
        public abstract Speed IndexingSpeed { get; }

        #endregion

        #endregion

        #region Sorted Enumeration

        /// <inheritdoc/>
        public abstract IEnumerable<I> Sorted();

        /// <inheritdoc/>
        public abstract IEnumerable<I> SortedBackwards();

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateBackwardsFrom(I interval, bool includeInterval = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateFromIndex(int index);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateBackwardsFromIndex(int index);

        #endregion

        #region Indexed Access

        /// <inheritdoc/>
        public abstract I this[int i] { get; }

        #endregion
    }
}
