using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    using SCG = System.Collections.Generic;

    public class EndpointSortedIntervalCollection<I, T> : ContainmentFreeIntervalCollectionBase<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly IEndpointSortedIntervalList<I, T> _list;
        private readonly bool _allowsReferenceDuplicates;
        private readonly bool _allowsOverlaps;
        private readonly bool _isReadOnly;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariants()
        {
            // Interval list is never null
            Contract.Invariant(_list != null);

            // Intervals are sorted
            Contract.Invariant(_list.IsSorted<I>(IntervalExtensions.CreateComparer<I, T>()));
            // Highs are sorted as well
            Contract.Invariant(_list.IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
        }

        #endregion

        #region Constructors

        public EndpointSortedIntervalCollection(/*bool allowsReferenceDuplicates = false, */ bool allowsOverlaps = false, bool isReadOnly = false)
            : this(Enumerable.Empty<I>(), /*allowsReferenceDuplicates, */ allowsOverlaps, isReadOnly)
        { }

        public EndpointSortedIntervalCollection(IEnumerable<I> intervals, /*bool allowsReferenceDuplicates = false,*/ bool allowsOverlaps = false, bool isReadOnly = false)
        {
            _allowsReferenceDuplicates = allowsOverlaps; // allowsReferenceDuplicates;
            _allowsOverlaps = allowsOverlaps;
            _isReadOnly = isReadOnly;

            _list = allowsOverlaps ? CreateList(intervals, IntervalExtensions.EndpointsUnsortable) : CreateList(intervals, IntervalExtensions.Overlaps);
        }

        protected virtual IEndpointSortedIntervalList<I, T> CreateList(IEnumerable<I> intervals, Func<IInterval<T>, IInterval<T>, bool> conflictsWithNeighbour)
        {
            return new EndpointSortedIntervalList<I, T>(intervals, conflictsWithNeighbour);
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override int Count
        {
            get { return _list.Count; }
        }

        #endregion

        #region Interval Colletion

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool AllowsReferenceDuplicates
        {
            get { return _allowsReferenceDuplicates; }
        }

        /// <inheritdoc/>
        public override bool AllowsOverlaps
        {
            get { return _allowsOverlaps; }
        }

        /// <inheritdoc/>
        public override bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        /// <inheritdoc/>
        public override Speed IndexingSpeed
        {
            get { return _list.IndexingSpeed; }
        }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public override I LowestInterval
        {
            get { return _list.First; }
        }

        /// <inheritdoc/>
        public override IEnumerable<I> LowestIntervals
        {
            get
            {
                if (AllowsOverlaps)
                    return base.LowestIntervals;

                if (IsEmpty)
                    return Enumerable.Empty<I>();

                return _list.First.AsEnumerable();
            }
        }

        /// <inheritdoc/>
        public override I HighestInterval
        {
            get { return _list.Last; }
        }

        /// <inheritdoc/>
        public override IEnumerable<I> HighestIntervals
        {
            get
            {
                if (AllowsOverlaps)
                    return base.HighestIntervals;

                if (IsEmpty)
                    return Enumerable.Empty<I>();

                return _list.Last.AsEnumerable();
            }
        }

        /// <inheritdoc/>
        public override int MaximumDepth
        {
            get
            {
                if (AllowsOverlaps)
                    return base.MaximumDepth;

                return IsEmpty ? 0 : 1;
            }
        }

        #endregion

        #region Sorted Enumeration

        /// <inheritdoc/>
        public override IEnumerable<I> Sorted
        {
            get
            {
                foreach (var interval in _list)
                    yield return interval;
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<I> SortedBackwards()
        {
            return IsEmpty ? Enumerable.Empty<I>() : _list.EnumerateBackwardsFromIndex(Count - 1);
        }

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true)
        {
            var query = new IntervalBase<T>(point);
            var index = includeOverlaps ? _list.FindFirst(query) : _list.FindLast(query);
            return EnumerateFromIndex(index);
        }

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true)
        {
            var query = new IntervalBase<T>(point);
            var index = (includeOverlaps ? _list.FindLast(query) : _list.FindFirst(query)) - 1;
            return EnumerateBackwardsFromIndex(index);
        }

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateFrom(IInterval<T> interval, bool includeOverlaps = true)
        {
            if (IsEmpty)
                return Enumerable.Empty<I>();

            var index = includeOverlaps ? _list.FindFirst(interval) : _list.FindLast(interval);
            return EnumerateFromIndex(index);
        }

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateBackwardsFrom(IInterval<T> interval, bool includeOverlaps = true)
        {
            if (IsEmpty)
                return Enumerable.Empty<I>();

            var index = includeOverlaps ? _list.FindLast(interval) : _list.FindFirst(interval);
            return EnumerateBackwardsFromIndex(index - 1);
        }

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateFromIndex(int index)
        {
            if (Count <= index)
                return Enumerable.Empty<I>();
            if (index < 0)
                return Sorted;

            return _list.EnumerateFromIndex(index);
        }

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateBackwardsFromIndex(int index)
        {
            if (index < 0 || IsEmpty)
                return Enumerable.Empty<I>();
            if (Count <= index)
                return SortedBackwards();

            return _list.EnumerateBackwardsFromIndex(index);
        }

        #endregion

        #region Indexed Access

        /// <inheritdoc/>
        public override I this[int i]
        {
            get { return _list[i]; }
        }

        #endregion

        #region Find Equals

        public override IEnumerable<I> FindEquals(IInterval<T> query)
        {
            var index = _list.Find(query);
            return index < 0 ? Enumerable.Empty<I>() : _list.EnumerateFromIndex(index).TakeWhile(interval => interval.IntervalEquals(query));
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            // Break if we won't find any overlaps
            if (IsEmpty)
                return Enumerable.Empty<I>();

            // We know first doesn't overlap so we can increment it before searching
            var first = _list.FindFirst(query);

            // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
            if (Count <= first || !(_list[first].CompareLowHigh(query) <= 0))
                return Enumerable.Empty<I>();

            // We can use first as lower to minimize search area
            var last = _list.FindLast(query);

            return _list.EnumerateRange(first, last);
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public override int CountOverlaps(T query)
        {
            return countOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public override int CountOverlaps(IInterval<T> query)
        {
            return countOverlaps(query);
        }

        private int countOverlaps(IInterval<T> query)
        {
            Contract.Requires(query != null);

            if (IsEmpty)
                return 0;

            var first = _list.FindFirst(query);

            if (Count <= first || !(_list[first].CompareLowHigh(query) <= 0))
                return 0;

            var last = _list.FindLast(query);

            return last - first;
        }

        #endregion

        #region Extensible

        #region Add

        public override bool Add(I interval)
        {
            if (IsReadOnly)
                throw new ReadOnlyCollectionException();

            var intervalWasAdded = _list.Add(interval);

            if (intervalWasAdded)
                raiseForAdd(interval);

            return intervalWasAdded;
        }

        #endregion

        #region Remove

        public override bool Remove(I interval)
        {
            if (IsReadOnly)
                throw new ReadOnlyCollectionException();

            var intervalWasRemoved = _list.Remove(interval);

            if (intervalWasRemoved)
                raiseForRemove(interval);

            return intervalWasRemoved;
        }

        #endregion

        #region Clear

        protected override void clear()
        {
            _list.Clear();
        }

        #endregion

        #endregion

        #endregion
    }
}
