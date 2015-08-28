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
    public abstract class OverlapFreeIntervalCollectionBase<I, T> : ContainmentFreeIntervalCollectionBase<I, T>, IOverlapFreeIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Interval Collection

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool AllowsOverlaps { get { return false; } }

        /// <inheritdoc/>
        public override bool AllowsReferenceDuplicates { get { return false; } }

        #endregion Data Structure Properties

        #region Collection Properties

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

        #region Indexed Access

        /// <inheritdoc/>
        public abstract int IndexOf(I interval);

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
