﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    public class StaticFiniteIntervalList<I, T> : FiniteIntervalCollectionBase<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly int _count;
        private readonly I[] _intervals;
        private readonly IInterval<T> _span;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // Each layer is sorted
            Contract.Invariant(IsEmpty || this.IsSorted<I, T>());
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Layered Containment List with a collection of intervals.
        /// </summary>
        /// <param name="intervals">The collection of intervals.</param>
        public StaticFiniteIntervalList(IEnumerable<I> intervals)
        {
            Contract.Requires(intervals != null);

            var array = intervals as I[] ?? intervals.ToArray();

            // Stop if we have no intervals
            if (array.Length == 0)
                return;

            // TODO: Find a better solution - think BITS in dynamic sorted lists (SortedSplitList)
            var list = new List<I>(array.Length);
            foreach (var interval in array)
                if (!interval.OverlapsAny(list))
                    list.Add(interval);

            _intervals = list.ToArray();
            Sorting.Timsort(_intervals, IntervalExtensions.CreateComparer<I, T>());

            _count = list.Count;

            _span = new IntervalBase<T>(_intervals[0], _intervals[_count - 1]);
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (_count == 0));
                return _count == 0;
            }
        }

        /// <inheritdoc/>
        public override int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() == _count);
                return _count;
            }
        }

        /// <inheritdoc/>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _intervals[0];
        }

        #endregion

        #region Interval Collection

        #region Properties

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        public override bool IsFindOverlapsSorted { get { return true; } }

        public override Speed IndexingSpeed
        {
            get { return Speed.Constant; }
        }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public override IInterval<T> Span { get { return _span; } }

        /// <inheritdoc/>
        public override I LowestInterval { get { return _intervals[0]; } }

        /// <inheritdoc/>
        public override I HighestInterval { get { return _intervals[_count - 1]; } }

        #endregion

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator() { return Sorted().GetEnumerator(); }

        /// <inheritdoc/>
        public override IEnumerable<I> Sorted()
        {
            return enumerateFromIndex(0);
        }

        public override IEnumerable<I> SortedBackwards()
        {
            return enumerateBackwardsFromIndex(_count - 1);
        }

        public override IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true)
        {
            throw new NotImplementedException();

            var query = new IntervalBase<T>(point);
            var index = includeOverlaps ? findFirst(query) : findLast(query);
            for (var i = index; i < _count; ++i)
                yield return _intervals[i];
        }

        public override IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true)
        {
            var index = IndexOf(interval);
            return 0 <= index ? enumerateFromIndex(includeInterval ? index : index + 1) : Enumerable.Empty<I>();
        }

        public override IEnumerable<I> EnumerateBackwardsFrom(I interval, bool includeInterval = true)
        {
            var index = IndexOf(interval);
            return 0 <= index ? enumerateBackwardsFromIndex(includeInterval ? index : index - 1) : Enumerable.Empty<I>();
        }

        public override IEnumerable<I> EnumerateFromIndex(int index)
        {
            return enumerateFromIndex(index);
        }

        public override IEnumerable<I> EnumerateBackwardsFromIndex(int index)
        {
            return enumerateBackwardsFromIndex(index);
        }

        private IEnumerable<I> enumerateFromIndex(int i)
        {
            while (i < _count)
                yield return _intervals[i++];
        }

        private IEnumerable<I> enumerateBackwardsFromIndex(int i)
        {
            while (i >= 0)
                yield return _intervals[i--];
        }

        #endregion

        #region Indexed Access

        public override int IndexOf(I interval)
        {
            throw new NotImplementedException();
        }

        public override I this[int i] { get { return _intervals[i]; } }

        #endregion

        #region Neighbourhood

        /// <inheritdoc/>
        public override Neighbourhood<I, T> GetNeighbourhood(T query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            // Break if we won't find any overlaps
            if (IsEmpty)
                yield break;

            // We know first doesn't overlap so we can increment it before searching
            var first = findFirst(query);

            // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
            if (_count <= first || !_intervals[first].Overlaps(query))
                yield break;

            // We can use first as lower to minimize search area
            var last = findLast(query);

            while (first < last)
                yield return _intervals[first++];
        }

        private bool findOverlap(T query, out I overlap)
        {
            // Do a binary search among the low endpoints
            var min = 0;
            var max = _count;
            while (min < max)
            {
                var mid = min + (max - min >> 1);

                var compare = query.CompareTo(_intervals[mid].Low);

                if (compare < 0)
                    max = mid - 1;
                else if (compare > 0)
                    min = mid + 1;
                else
                {
                    min = mid;
                    break;
                }
            }

            // Make sure index is in bound
            if (min < _count)
            {
                var compare = query.CompareTo((overlap = _intervals[min]).Low);

                if (compare == 0)
                {
                    // Stabbing directly on low
                    if (overlap.LowIncluded)
                        return true;
                }
                else if (compare > 0)
                {
                    // Check high to check if we have an overlap
                    compare = query.CompareTo(overlap.High);
                    if (compare < 0 || compare == 0 && overlap.HighIncluded)
                        return true;

                    overlap = null;
                    return false;
                }
            }

            // Check if the interval before overlaps
            if (min > 0 && (overlap = _intervals[min - 1]).Overlaps(query))
                return true;

            overlap = null;
            return false;
        }

        private int findFirst(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() < 0 || _count <= Contract.Result<int>() || _intervals[Contract.Result<int>()].Overlaps(query) || Contract.ForAll(0, _count, i => !_intervals[i].Overlaps(query)));
            // All intervals before index result do not overlap the query
            Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => !_intervals[i].Overlaps(query)));

            int min = -1, max = _count;

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = _intervals[middle];

                var compare = query.Low.CompareTo(interval.High);

                if (compare < 0 || compare == 0 && query.LowIncluded && interval.HighIncluded)
                    max = middle;
                else
                    min = middle;
            }

            return max;
        }

        private int findLast(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() == 0 || _intervals[Contract.Result<int>() - 1].Overlaps(query) || Contract.ForAll(_intervals, x => !x.Overlaps(query)));
            // All intervals after index result do not overlap the query
            Contract.Ensures(Contract.ForAll(Contract.Result<int>(), _count, i => !_intervals[i].Overlaps(query)));

            int min = -1, max = _count;

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = _intervals[middle];

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return max;
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public override bool FindOverlap(T query, out I overlap)
        {
            return findOverlap(query, out overlap);
        }

        /// <inheritdoc/>
        public override bool FindOverlap(IInterval<T> query, out I overlap)
        {
            overlap = null;

            // No overlap if collection is empty, or query doesn't overlap collection
            if (IsEmpty || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(query);

            // Check if index is in bound and if the interval overlaps the query
            var result = 0 <= i && i < _count && _intervals[i].Overlaps(query);

            if (result)
                overlap = _intervals[i];

            return result;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public override int CountOverlaps(IInterval<T> query)
        {
            return IsEmpty ? 0 : countOverlaps(query);
        }

        private int countOverlaps(IInterval<T> query)
        {
            Contract.Requires(query != null);

            int lower, upper = _count;

            // We know first doesn't overlap so we can increment it before searching
            lower = findFirst(query);

            // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
            // TODO: Can we tighten the check here? Like i.low < q.high...
            if (upper <= lower || !_intervals[lower].Overlaps(query))
                return 0;

            // We can use first as lower to speed up the search
            upper = findLast(query);

            return upper - lower;
        }

        #endregion

        #endregion
    }
}