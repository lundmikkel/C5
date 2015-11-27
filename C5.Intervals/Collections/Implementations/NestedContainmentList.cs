using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// An in-place implementation of Nested Containment List as described by Aleskeyenko et. al in "Nested
    /// Containment List (NCList): a new algorithm for accelerating interval query of genome alignment and interval databases"
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class NestedContainmentList<I, T> : SortedIntervalCollectionBase<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly int _count;

        private readonly Node[] _list;
        private readonly Sublist _mainSublist;

        private readonly IInterval<T> _span;

        private int _maximumDepth = -1;
        private IInterval<T> _intervalOfMaximumDepth;

        #endregion

        #region Inner Classes

        private struct Sublist
        {
            public int Start;
            public int Length;

            public Sublist(int start, int length)
                : this()
            {
                Start = start;
                Length = length;
            }
            public int End { get { return Start + Length; } }
        }

        [DebuggerDisplay("{Interval} - {Sublist.Start}/{Sublist.End}")]
        private struct Node
        {
            public I Interval;
            public Sublist Sublist;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Nested Containment List with a enumerable of intervals
        /// </summary>
        /// <param name="intervals">A collection of intervals in arbitrary order</param>
        public NestedContainmentList(IEnumerable<I> intervals)
        {
            var sorted = intervals as I[] ?? intervals.ToArray();

            if ((_count = sorted.Length) == 0)
                return;
            
            Sorting.IntroSort(sorted, 0, _count, IntervalExtensions.CreateComparer<I, T>());

            var totalSection = new Sublist(0, sorted.Length);
            _list = new Node[totalSection.Length];

            // Build nested containment list recursively and save the upper-most list in the class
            _mainSublist = new Sublist(0, createList(sorted, totalSection, totalSection));

            // Save span to allow for constant speeds on later requests
            _span = new IntervalBase<T>(_list[_mainSublist.Start].Interval, _list[_mainSublist.Length + _mainSublist.Start - 1].Interval);
        }

        /// <summary>
        /// A sorted list of IInterval&lt;T&gt; sorted with IntervalComparer&lt;T&gt;
        /// </summary>
        /// <param name="intervals">Sorted intervals</param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>A list of nodes</returns>
        private int createList(I[] intervals, Sublist source, Sublist target)
        {
            var end = target.End;
            var t = target.Start;

            for (var s = source.Start; s < source.End; s++)
            {
                var interval = intervals[s];
                var contained = 0;
                var length = 0;

                // Continue as long as we have more intervals
                while (s + 1 < source.End)
                {
                    var nextInterval = intervals[s + 1];

                    if (!interval.StrictlyContains(nextInterval))
                        break;

                    contained++;
                    s++;
                }

                if (contained > 0)
                {
                    end -= contained;
                    length = createList(intervals, new Sublist(s - contained + 1, contained), new Sublist(end, contained));
                }

                _list[t++] = new Node{Interval = interval, Sublist = new Sublist(end, length)};
            }

            return t - target.Start;
        }

        #endregion

        #region Enumerable

        /// <summary>
        /// Create an enumerator, enumerating the intervals in sorted order - sorted on low endpoint with shortest intervals first
        /// </summary>
        /// <returns>Enumerator</returns>
        public override IEnumerator<I> GetEnumerator() { return Sorted.GetEnumerator(); }

        /// <inheritdoc/>
        public override IEnumerable<I> Sorted { get { return sorted(_mainSublist); } }

        private IEnumerable<I> sorted(Sublist sublist)
        {
            for (var i = sublist.Start; i < sublist.End; i++)
            {
                var node = _list[i];
                yield return node.Interval;

                if (node.Sublist.Length > 0)
                    foreach (var interval in sorted(node.Sublist))
                        yield return interval;
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
                for (var i = 1; i < _mainSublist.Length; ++i)
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
        public override I HighestInterval { get { return _list[_mainSublist.Length - 1].Interval; } }

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
                for (var i = _mainSublist.Length - 2; i >= 0; i--)
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

            return findOverlaps(_mainSublist, new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (IsEmpty)
                return Enumerable.Empty<I>();

            return findOverlaps(_mainSublist, query);
        }

        private IEnumerable<I> findOverlaps(Sublist sublist, IInterval<T> query, bool takeAll = false)
        {
            // Find first overlapping interval
            var first = takeAll ? sublist.Start : findFirst(sublist, query);

            // If index is out of bound, or interval doesn't overlap, we can just stop our search
            if (!takeAll && (first >= sublist.End || !_list[first].Interval.Overlaps(query)))
                yield break;

            var last = takeAll ? sublist.End : findLast(sublist, query, first);

            while (first < last)
            {
                var node = _list[first++];
                yield return node.Interval;

                if (node.Sublist.Length > 0)
                    // If the interval is contained in the query, all intervals in the sublist must overlap the query
                    foreach (var interval in findOverlaps(node.Sublist, query))
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

        private int findLast(Sublist sublist, IInterval<T> query, int first)
        {
            int min = first, max = sublist.End;

            while (min < max)
            {
                var mid = min + (max - min >> 1);

                if (query.CompareHighLow(_list[mid].Interval) < 0)
                    max = mid;
                else
                    min = mid + 1;
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
            var first = findFirst(_mainSublist, query);

            // Check if index is in bound and if the interval overlaps the query
            var result = first < _mainSublist.End && _list[first].Interval.CompareLowHigh(query) <= 0;

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

                return _list.Take(_mainSublist.End).Select(x => x.Interval).Gaps();
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return findOverlapsInMainList(query).Gaps(query);
        }

        private IEnumerable<I> findOverlapsInMainList(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            var first = findFirst(_mainSublist, query);
            var last = findLast(_mainSublist, query, first);

            while (first < last)
                yield return _list[first++].Interval;
        }

        #endregion

        #endregion
    }
}
