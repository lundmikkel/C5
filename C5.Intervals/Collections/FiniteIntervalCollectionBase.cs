﻿using System;
using System.Collections.Generic;
using System.Linq;

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
    public abstract class FiniteIntervalCollectionBase<I, T> : IntervalCollectionBase<I, T>, IFiniteIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Interval Collection

        #region Properties

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool AllowsOverlaps { get { return false; } }

        /// <inheritdoc/>
        public override bool IsFindOverlapsSorted { get { return true; } }

        /// <inheritdoc/>
        public abstract Speed IndexingSpeed { get; }

        #endregion Data Structure Properties

        #region Collection Properties

        /// <inheritdoc/>
        public override I LowestInterval { get { return Sorted().First(); } }

        /// <inheritdoc/>
        public override IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                // Without overlaps we can only have one lowest interval
                yield return LowestInterval;
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                // Without overlaps we can only have one lowest interval
                yield return HighestInterval;
            }
        }

        /// <inheritdoc/>
        public override int MaximumDepth { get { return IsEmpty ? 0 : 1; } }

        #endregion Collection Properties

        #endregion Properties

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
        public abstract int IndexOf(I interval);

        /// <inheritdoc/>
        public abstract I this[int i] { get; }

        #endregion

        #region Neighbourhood

        /// <inheritdoc/>
        public abstract Neighbourhood<I, T> GetNeighbourhood(T query);

        /// <inheritdoc/>
        public virtual Neighbourhood<I, T> GetNeighbourhood(I query)
        {
            var i = IndexOf(query);

            // The interval was not found, return empty neighbourhood
            if (i < 0)
                return new Neighbourhood<I, T>();

            // Make sure we have a previous interval
            var previous = 0 < i ? this[i - 1] : null;
            // Overlap must be query interval
            var overlap = query;
            // Make sure we have a next interval
            var next = i + 1 < Count ? this[i + 1] : null;

            return new Neighbourhood<I, T>(previous, overlap, next);
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(T query)
        {
            I overlap;
            if (FindOverlap(query, out overlap))
                yield return overlap;
        }

        #endregion Find Overlaps

        #region Count Overlaps

        /// <inheritdoc/>
        public override int CountOverlaps(T query)
        {
            I overlap;
            return FindOverlap(query, out overlap) ? 1 : 0;
        }

        #endregion Count Overlaps

        #endregion Interval Collection
    }
}
