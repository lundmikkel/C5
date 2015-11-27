using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    public class NestedContainmentListArticle<I, T> : SortedIntervalCollectionBase<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly Node[] _list;
        private readonly Sublist[] _header;
        private readonly int _count;
        private readonly IInterval<T> _span;

        private int _maximumDepth = -1;
        private IInterval<T> _intervalOfMaximumDepth;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // Count is equal to the number of intervals
            Contract.Invariant(IsEmpty || _count == _list.Length);
            // The first list's count is non-negative and at most as big as count
            Contract.Invariant(IsEmpty || 0 <= _header[0].End && _header[0].End <= _count);
            // Either the collection is empty or there is one list or more
            Contract.Invariant(IsEmpty || _header.Length >= 1);
            // Either all intervals are in the first list, or there are more than one list
            Contract.Invariant(IsEmpty || _count == _header[0].End || _header.Length > 1);
            // The lists are null if empty
            Contract.Invariant(!IsEmpty || _list == null && _header == null);

            // No layer is empty
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _header.Length, sublist => _header[sublist].End - _header[sublist].Start > 0));
            // Each list is sorted
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _header.Length, j => Contract.ForAll(_header[j].Start, _header[j].End - 1, i => _list[i].Interval.CompareTo(_list[i + 1].Interval) <= 0)));
            // Each list is sorted on both low and high endpoint
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _header.Length, j => Contract.ForAll(_header[j].Start, _header[j].End - 1, i => _list[i].Interval.CompareLow(_list[i + 1].Interval) <= 0 && _list[i].Interval.CompareHigh(_list[i + 1].Interval) <= 0)));
            // Each interval in a layer must be contained in at least one interval in each layer below
            Contract.Invariant(IsEmpty ||
                Contract.ForAll(0, _count, i =>
                    {
                        var sublist = _list[i].Sublist;
                        var parent = _list[i].Interval;
                        return sublist < 0 || Contract.ForAll(_header[sublist].Start, _header[sublist].End, isub => parent.StrictlyContains(_list[isub].Interval));
                    }
                )
            );
        }

        #endregion

        #region Inner Classes

        [DebuggerDisplay("{Interval} / {Sublist}")]
        private struct Node
        {
            public I Interval;
            public int Sublist;

            public bool HasSublist { get { return Sublist >= 0; } }
        }

        [DebuggerDisplay("{Start} / {End}")]
        private struct Sublist
        {
            public int Start, End;
        }

        #endregion

        #region Constructors

        // TODO: Make an out-of-place constructor
        public NestedContainmentListArticle(IEnumerable<I> intervals)
        {
            var sorted = intervals as I[] ?? intervals.ToArray();

            // Stop if we have no intervals
            if ((_count = sorted.Length) == 0)
                return;

            // Sort intervals in place
            Sorting.HeapSort(sorted, 0, _count, IntervalExtensions.CreateComparer<I, T>());

            // Create list with intervals
            _list = new Node[_count];
            for (var i = 0; i < _count; ++i)
                _list[i].Interval = sorted[i];

            var listCount = initSublists(sorted);

            // If no intervals are contained, we can skip the list construction
            if (listCount == 1)
            {
                // Initialize header to contain only main list
                _header = new[] { new Sublist{Start = 0, End = _count} };

                for (var i = 0; i < _count; ++i)
                    _list[i].Sublist = -1;
            }
            else
            {
                _header = new Sublist[listCount];

                labelSublists();
                computeSubStart(listCount);
                computeAbsPos();

                // Sort sublists
                var sublistComparer = ComparerFactory<Node>.CreateComparer((i, j) =>
                {
                    var compareTo = i.Sublist.CompareTo(j.Sublist);
                    return compareTo != 0 ? compareTo : i.Interval.CompareTo(j.Interval);
                });
                Sorting.HeapSort(_list, 0, _count, sublistComparer);

                sublistInvert();
            }

            _span = new IntervalBase<T>(_list[0].Interval, _list[_header[0].End - 1].Interval);
        }

        /// <summary>
        /// Returns the number of sublists in the data structure
        /// </summary>
        /// <param name="intervals">The intervals.</param>
        /// <returns>The number of sublists</returns>
        private int initSublists(I[] intervals)
        {
            var sublistCount = 1;

            for (var i = 1; i < _count; ++i)
                if (intervals[i - 1].StrictlyContains(intervals[i]))
                    ++sublistCount;

            return sublistCount;
        }

        /// <summary>
        /// The function stores in _list an interval and the id of the sublist it belongs to,
        /// and in _header the id of the sublist's parent and its length.
        /// 
        /// The algorithm works by taking the last interval and check if it contains the next interval.
        /// If not, it works its way from sublist to sublist down to the main list, looking for the right sublist.
        /// When found it sets the intervals sublist and moves on to the next interval.
        /// </summary>
        private void labelSublists()
        {
            _header[0] = new Sublist{Start = -1, End = 1};
            var parent = 0;
            var parentList = 1;
            var currentList = 1;

            for (var i = 1; i < _count; /**/)
            {
                if (parentList == 0 || _list[parent].Interval.StrictlyContains(_list[i].Interval))
                {
                    if (_header[parentList].End == 0)
                    {
                        currentList++;
                        _header[parentList].Start = parent;
                    }

                    _header[parentList].End++;
                    _list[i].Sublist = parentList;
                    parent = i;
                    parentList = currentList;
                    ++i;
                }
                else
                {
                    if (parentList < _header.Length)
                        _header[parentList].Start = _header[_list[parent].Sublist].End - 1;

                    parentList = _list[parent].Sublist;
                    parent = _header[_list[parent].Sublist].Start;
                }
            }

            // Pop remaining stack
            while (parentList > 0)
            {
                if (parentList < _header.Length)
                    _header[parentList].Start = _header[_list[parent].Sublist].End - 1;

                parentList = _list[parent].Sublist;
                parent = _header[_list[parent].Sublist].Start;
            }
        }

        private void computeSubStart(int listCount)
        {
            for (int i = 0, total = 0, temp; i < listCount; ++i, total += temp)
            {
                temp = _header[i].End;
                _header[i].End = total;
            }
        }

        private void computeAbsPos()
        {
            for (var i = 1; i < _count; ++i)
                if (_list[i - 1].Sublist < _list[i].Sublist)
                    _header[_list[i].Sublist].Start += _header[_list[i - 1].Sublist].End;
        }

        private void sublistInvert()
        {
            var parentList = 0;
            _header[0].Start = 0;

            for (var i = 0; i < _count; ++i)
            {
                if (_list[i].Sublist > parentList)
                {
                    parentList = _list[i].Sublist;
                    var parent = _header[parentList].Start;
                    _list[parent].Sublist = parentList;
                    _header[parentList].End = _header[parentList].Start = i;
                }

                _header[parentList].End++;
                _list[i].Sublist = -1;
            }
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
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _list[0].Interval;
        }

        #endregion

        #region Interval Collection

        #region Properties

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool AllowsOverlaps { get { return true; } }

        /// <inheritdoc/>
        public override bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        public override bool IsFindOverlapsSorted { get { return true; } }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public override IInterval<T> Span { get { return _span; } }

        /// <inheritdoc/>
        public override I LowestInterval { get { return _list[0].Interval; } }

        /// <inheritdoc/>
        public override IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var lowestInterval = LowestInterval;
                yield return lowestInterval;

                // Iterate through main sublist as long as the intervals share a low
                for (var i = 1; i < _header[0].End; ++i)
                {
                    var interval = _list[i].Interval;
                    if (interval.LowEquals(lowestInterval))
                        yield return interval;
                    else
                        yield break;
                }
            }
        }

        /// <inheritdoc/>
        public override I HighestInterval { get { return _list[_header[0].End - 1].Interval; } }

        /// <inheritdoc/>
        public override IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var highestInterval = HighestInterval;
                yield return highestInterval;

                // Iterate through main sublist as long as the intervals share a high
                for (var i = _header[0].End - 2; i >= 0; --i)
                {
                    var interval = _list[i].Interval;
                    if (interval.HighEquals(highestInterval))
                        yield return interval;
                    else
                        yield break;
                }
            }
        }

        /// <inheritdoc/>
        public override int MaximumDepth
        {
            get
            {
                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(out _intervalOfMaximumDepth);

                return _maximumDepth;
            }
        }

        /// <summary>
        /// Get the interval in which the maximum depth is.
        /// </summary>
        public IInterval<T> IntervalOfMaximumDepth
        {
            get
            {
                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(out _intervalOfMaximumDepth);

                return _intervalOfMaximumDepth;
            }
        }

        #endregion

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator()
        {
            if (IsEmpty)
                yield break;

            foreach (var node in _list)
                yield return node.Interval;
        }

        /// <inheritdoc/>
        public override IEnumerable<I> Sorted
        {
            get
            {
                if (IsEmpty)
                    return Enumerable.Empty<I>();

                return sorted(_header[0]);
            }
        }

        private IEnumerable<I> sorted(Sublist sublist)
        {
            for (var i = sublist.Start; i < sublist.End; ++i)
            {
                var node = _list[i];
                yield return node.Interval;

                if (node.HasSublist)
                    foreach (var interval in sorted(_header[node.Sublist]))
                        yield return interval;
            }
        }

        #endregion

        #region Find Equals

        public override IEnumerable<I> FindEquals(IInterval<T> query)
        {
            // TODO: Replace!

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
        public override IEnumerable<I> FindOverlaps(T query)
        {
            if (IsEmpty)
                return Enumerable.Empty<I>();

            return findOverlaps(new IntervalBase<T>(query), _header[0]);
        }

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (IsEmpty)
                return Enumerable.Empty<I>();

            return findOverlaps(query, _header[0]);
        }

        private IEnumerable<I> findOverlaps(IInterval<T> query, Sublist sublist, bool takeAll = false)
        {
            Contract.Requires(!IsEmpty);
            Contract.Requires(sublist.Start != sublist.End);

            // Find first overlapping interval
            var first = takeAll ? sublist.Start : findFirst(sublist, query);
            
            while (first < sublist.End && (takeAll || _list[first].Interval.CompareLowHigh(query) <= 0))
            {
                var node = _list[first++];
                yield return node.Interval;

                if (node.HasSublist)
                    // If the interval is contained in the query, all intervals in the sublist must overlap the query
                    foreach (var interval in findOverlaps(query, _header[node.Sublist], takeAll || query.Contains(node.Interval)))
                        yield return interval;
            }
        }

        private int findFirst(Sublist sublist, IInterval<T> query)
        {
            int min = sublist.Start, max = sublist.End;

            while (min < max)
            {
                var middle = min + (max - min >> 1);

                if (_list[middle].Interval.CompareHighLow(query) < 0)
                    min = middle + 1;
                else
                    max = middle;
            }

            return min;
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public override bool FindOverlap(T query, out I overlap)
        {
            return FindOverlap(new IntervalBase<T>(query), out overlap);
        }

        /// <inheritdoc/>
        public override bool FindOverlap(IInterval<T> query, out I overlap)
        {
            overlap = null;

            // Check if query overlaps the collection at all
            if (IsEmpty || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var first = findFirst(_header[0], query);

            // Check if index is in bound and if the interval overlaps the query
            var result = first < _header[0].End && _list[first].Interval.CompareLowHigh(query) <= 0;

            if (result)
                overlap = _list[first].Interval;

            return result;
        }

        #endregion

        #region Gaps

        /// <inheritdoc/>
        public override IEnumerable<IInterval<T>> Gaps
        {
            get
            {
                if (IsEmpty)
                    return Enumerable.Empty<IInterval<T>>();

                return _list.Take(_header[0].End).Select(node => node.Interval).Gaps();
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return findOverlapsInFirstLayer(query).Gaps(query);
        }

        private IEnumerable<I> findOverlapsInFirstLayer(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            var first = findFirst(_header[0], query);
            var last = _header[0].End;
            I interval;

            while (first < last && (interval = _list[first++].Interval).CompareLowHigh(query) <= 0)
                yield return interval;
        }

        #endregion

        #endregion
    }
}