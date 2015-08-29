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

        private readonly EndpointSortedIntervalList _list;
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
            Contract.Invariant(_list.IsSorted(IntervalExtensions.CreateComparer<I, T>()));
            // Highs are sorted as well
            Contract.Invariant(_list.IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
        }

        #endregion

        #region Inner Class

        class EndpointSortedIntervalList : IEnumerable<I>
        {
            private readonly List<I> _intervals;
            private readonly Func<IInterval<T>, IInterval<T>, bool> _conflictsWithNeighbour;

            public EndpointSortedIntervalList(IEnumerable<I> intervals, Func<IInterval<T>, IInterval<T>, bool> conflictsWithNeighbour)
            {
                _intervals = new List<I>();
                _conflictsWithNeighbour = conflictsWithNeighbour;

                foreach (var interval in intervals)
                    Add(interval);
            }

            public int Count { get { return _intervals.Count; } }

            public I this[int i]
            {
                get { return _intervals[i]; }
            }

            public int Find(IInterval<T> query)
            {
                Contract.Ensures(Contract.Result<int>() == IntervalCollectionContractHelper.IndexOfSorted(_intervals, query, IntervalExtensions.CreateComparer<IInterval<T>, T>()));

                var low = 0;
                var high = _intervals.Count - 1;

                while (low <= high)
                {
                    var mid = low + (high - low >> 1);
                    var compareTo = _intervals[mid].CompareTo(query);

                    if (compareTo < 0)
                        low = mid + 1;
                    else if (compareTo > 0)
                        high = mid - 1;
                    //Equal but range is not fully scanned
                    else if (low != mid)
                        //Set upper bound to current number and rescan
                        high = mid;
                    //Equal and full range is scanned
                    else
                        return mid;
                }

                // key not found. return insertion point
                return ~low;
            }

            public int FindFirst(IInterval<T> query)
            {
                Contract.Requires(query != null);

                // Either the interval at index result overlaps or no intervals in the layer overlap
                Contract.Ensures(Contract.Result<int>() < 0 || Count <= Contract.Result<int>() || _intervals[Contract.Result<int>()].Overlaps(query) || Contract.ForAll(0, Count, i => !_intervals[i].Overlaps(query)));
                // All intervals before index result do not overlap the query
                Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => !_intervals[i].Overlaps(query)));

                int min = -1, max = Count;

                while (min + 1 < max)
                {
                    var middle = min + ((max - min) >> 1);
                    if (query.CompareLowHigh(_intervals[middle]) <= 0)
                        max = middle;
                    else
                        min = middle;
                }

                return max;
            }

            public int FindLast(IInterval<T> query)
            {
                Contract.Requires(query != null);

                // Either the interval at index result overlaps or no intervals in the layer overlap
                Contract.Ensures(Contract.Result<int>() == 0 || _intervals[Contract.Result<int>() - 1].Overlaps(query) || Contract.ForAll(_intervals, x => !x.Overlaps(query)));
                // All intervals after index result do not overlap the query
                Contract.Ensures(Contract.ForAll(Contract.Result<int>(), Count, i => !_intervals[i].Overlaps(query)));

                int min = -1, max = Count;

                while (min + 1 < max)
                {
                    var middle = min + ((max - min) >> 1);
                    if (_intervals[middle].CompareLowHigh(query) <= 0)
                        min = middle;
                    else
                        max = middle;
                }

                return max;
            }

            public IEnumerator<I> GetEnumerator()
            {
                return _intervals.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerable<I> EnumerateFromIndex(int index)
            {
                Contract.Requires(0 <= index && index < Count);

                return EnumerateRange(index, Count);
            }

            public IEnumerable<I> EnumerateRange(int inclusiveFrom, int exclusiveTo)
            {
                Contract.Requires(0 <= inclusiveFrom && inclusiveFrom < Count);
                Contract.Requires(1 <= exclusiveTo && exclusiveTo <= Count);
                Contract.Requires(inclusiveFrom < exclusiveTo);

                while (inclusiveFrom < exclusiveTo)
                    yield return _intervals[inclusiveFrom++];
            }

            public IEnumerable<I> EnumerateBackwardsFromIndex(int index)
            {
                Contract.Requires(0 <= index && index < Count);

                while (index >= 0)
                    yield return _intervals[index--];
            }

            public void Clear()
            {
                _intervals.Clear();
            }

            public bool Add(I interval)
            {
                var index = Find(interval);

                if (index < 0)
                    index = ~index;

                if (index > 0 && _conflictsWithNeighbour(interval, _intervals[index - 1])
                    || index < Count && _conflictsWithNeighbour(interval, _intervals[index]))
                    return false;

                _intervals.Insert(index, interval);
                return true;
            }

            public bool Remove(I interval)
            {
                var index = Find(interval);

                if (index < 0 || Count <= index)
                    return false;

                while (index < Count && _intervals[index].IntervalEquals(interval))
                {
                    if (ReferenceEquals(_intervals[index], interval))
                    {
                        _intervals.RemoveAt(index);
                        return true;
                    }
                    ++index;
                }

                return false;
            }
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

            _list = allowsOverlaps ? createList(intervals, ContainsNeighbour) : createList(intervals, OverlapsNeighbour);
        }

        private EndpointSortedIntervalList createList(IEnumerable<I> intervals, Func<IInterval<T>, IInterval<T>, bool> conflictsWithNeighbour)
        {
            return new EndpointSortedIntervalList(intervals, conflictsWithNeighbour);
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
            get { return Speed.Constant; }
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
            if (IsEmpty)
                return Enumerable.Empty<I>();

            var index = _list.Find(query);

            if (index < 0)
                return Enumerable.Empty<I>();

            return _list.EnumerateFromIndex(index).TakeWhile(interval => interval.IntervalEquals(query));
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
            if (Count <= first || !_list[first].Overlaps(query))
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

            // We know first doesn't overlap so we can increment it before searching
            var first = _list.FindFirst(query);

            // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
            if (Count <= first || !(_list[first].CompareLowHigh(query) <= 0))
                return 0;

            // We can use first as lower to speed up the search
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

        private bool ContainsNeighbour(IInterval<T> interval, IInterval<T> neighbour)
        {
            return interval.StrictlyContains(neighbour) || neighbour.StrictlyContains(interval);
        }

        private bool OverlapsNeighbour(IInterval<T> interval, IInterval<T> neighbour)
        {
            return interval.Overlaps(neighbour);
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
