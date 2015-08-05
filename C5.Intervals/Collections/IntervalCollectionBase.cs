using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// Base class for classes implementing <see cref="IIntervalCollection{I,T}"/>.
    /// </summary>
    /// <remarks>
    /// When extending this class, it is recommended to consider reimplement the following methods,
    /// if better performance can be achieved:
    /// <list type="bullet">
    /// <item> 
    /// <description><see cref="Choose"/>: uses iterator to pick first item.</description> 
    /// </item>
    /// <item>
    /// <description><see cref="Span"/>: creates an interval from <see cref="LowestInterval"/> and
    /// <see cref="HighestInterval"/>.</description>
    /// </item>
    /// <item>
    /// <description><see cref="LowestInterval"/>, <see cref="LowestInterval"/>, <see cref="Gaps"/>: both use <see cref="Sorted"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description><see cref="FindOverlaps(T)"/>, <see cref="FindOverlap(C5.Intervals.IInterval{T},out I)"/>,
    /// <see cref="FindOverlap(T, out I)"/>, <see cref="CountOverlaps(C5.Intervals.IInterval{T})"/>,
    /// <see cref="CountOverlaps(T)"/>, <see cref="FindGaps"/>: use <see cref="FindOverlaps(C5.Intervals.IInterval{T})"/>.
    /// </description>
    /// </item>
    /// <item> 
    /// <description><see cref="Add"/>, <see cref="Remove"/>: throws
    /// appropriate error when <see cref="IsReadOnly"/> is true. Reimplement otherwise.</description> 
    /// </item>
    /// <item> 
    /// <description>
    /// <see cref="AddAll"/>: adds each interval seperately.
    /// Reimplement if implementation allows for bulk operations.
    /// </description> 
    /// </item>
    /// <item> 
    /// <description>
    /// <see cref="clear"/>: must be implemented when <see cref="IsReadOnly"/> is false.
    /// It should not be necesarry to reimplement <see cref="Clear"/>, which already handles empty collections and events.
    /// </description> 
    /// </item>
    /// </list>
    /// </remarks>
    /// <typeparam name="I">The interval type with endpoint type <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    /// <seealso cref="OverlapFreeIntervalCollectionBase{I,T}"/>
    public abstract class IntervalCollectionBase<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Collection Value

        /// <inheritdoc/>
        public override I Choose()
        {
            if (IsEmpty)
                // By specification:
                throw new NoSuchItemException();

            return this.First();
        }

        #endregion Collection Value

        #region Events

        /// <inheritdoc/>
        public override EventTypeEnum ListenableEvents
        {
            get
            {
                return IsReadOnly ? EventTypeEnum.None : EventTypeEnum.Basic;
            }
        }

        #endregion

        #region Interval Collection

        #region Properties

        #region Data Structure Properties

        // TODO: Assign default values?
        /// <inheritdoc/>
        public abstract bool AllowsOverlaps { get; }

        /// <inheritdoc/>
        public virtual bool AllowsContainments { get { return AllowsOverlaps; } }

        /// <inheritdoc/>
        public virtual bool AllowsReferenceDuplicates { get { return AllowsOverlaps; } }

        // TODO: Assign default values?
        /// <inheritdoc/>
        public abstract bool IsReadOnly { get; }

        /// <inheritdoc/>
        public virtual bool IsFindOverlapsSorted { get { return false; } }

        #endregion Data Structure Properties

        #region Collection Properties

        /// <inheritdoc/>
        public virtual IInterval<T> Span { get { return new IntervalBase<T>(LowestInterval, HighestInterval); } }

        /// <inheritdoc/>
        public abstract I LowestInterval { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<I> LowestIntervals { get; }

        /// <inheritdoc/>
        public abstract I HighestInterval { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<I> HighestIntervals { get; }

        /// <inheritdoc/>
        public abstract int MaximumDepth { get; }

        #endregion Collection Properties

        #endregion Properties

        #region Find Equals

        // TODO: Implement properly in subclasses!
        /// <inheritdoc/>
        public virtual IEnumerable<I> FindEquals(IInterval<T> query)
        {
            IEnumerable<I> overlaps;

            if (query.LowIncluded)
                overlaps = FindOverlaps(query.Low);
            else if (query.HighIncluded)
                overlaps = FindOverlaps(query.High);
            else
                overlaps = FindOverlaps(query);

            return overlaps.Where(interval => interval.IntervalEquals(query));
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public virtual IEnumerable<I> FindOverlaps(T query)
        {
            return FindOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public abstract IEnumerable<I> FindOverlaps(IInterval<T> query);

        #endregion Find Overlaps

        #region Find Overlap

        /// <inheritdoc/>
        // TODO: Use FindOverlap?
        public virtual bool FindOverlap(T query, out I overlap)
        {
            return (overlap = FindOverlaps(query).FirstOrDefault()) != null;

            // TODO: Compare this to:
            // bool result;
            // using (var enumerator = FindOverlaps(query).GetEnumerator())
            //     overlap = (result = enumerator.MoveNext()) ? enumerator.Current : null;
            // return result;
        }
        /// <inheritdoc/>
        public virtual bool FindOverlap(IInterval<T> query, out I overlap)
        {
            return (overlap = FindOverlaps(query).FirstOrDefault()) != null;
        }

        #endregion Find Overlap

        #region Count Overlaps

        /// <inheritdoc/>
        public virtual int CountOverlaps(T query)
        {
            return FindOverlaps(query).Count();
        }

        /// <inheritdoc/>
        public virtual int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }

        #endregion Count Overlaps

        #region Gaps

        /// <inheritdoc/>
        public virtual IEnumerable<IInterval<T>> Gaps { get { return this.Gaps(isSorted: false); } }

        /// <inheritdoc/>
        public virtual IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return FindOverlaps(query).Gaps(query, IsFindOverlapsSorted);
        }

        #endregion Gaps

        #region Extensible

        /// <inheritdoc/>
        public virtual bool Add(I interval)
        {
            if (IsReadOnly)
                throw new ReadOnlyCollectionException();

            // Implement if needed
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void AddAll(IEnumerable<I> intervals)
        {
            if (IsReadOnly)
                throw new ReadOnlyCollectionException();

            // Implement if bulk operation is possible
            foreach (var interval in intervals)
                Add(interval);
        }

        /// <inheritdoc/>
        public virtual bool Remove(I interval)
        {
            if (IsReadOnly)
                throw new ReadOnlyCollectionException();

            // Implement if needed
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            if (IsReadOnly)
                throw new ReadOnlyCollectionException();

            if (IsEmpty)
                return;

            var oldCount = Count;

            // Call user implementation to clear state
            clear();

            // Raise events
            if ((ActiveEvents & EventTypeEnum.Cleared) != 0)
                raiseCollectionCleared(true, oldCount);
            if ((ActiveEvents & EventTypeEnum.Changed) != 0)
                raiseCollectionChanged();
        }

        /// <summary>
        /// Clears the data structure's internal state, giving an empty, clean state.
        /// </summary>
        /// <remarks>
        /// Must be implemented if collection is not read-only!
        /// Otherwise it should simply be disregarded.
        /// </remarks>
        protected virtual void clear()
        {
            // Implement if needed
            throw new NotImplementedException();
        }

        #endregion Extensible

        #endregion Interval Collection
    }
}
