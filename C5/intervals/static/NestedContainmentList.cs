using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace C5.intervals.@static
{
    /// <summary>
    /// An in-place implementation of Nested Containment List as described by Aleskeyenko et. al in "Nested
    /// Containment List (NCList): a new algorithm for accelerating interval query of genome alignment and interval databases"
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class NestedContainmentList<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly Node[] _list;
        private readonly IInterval<T> _span;
        private readonly Sublist _mainList;
        private readonly int _count;

        #endregion

        #region Inner Classes

        private struct Sublist
        {
            public Sublist(int start, int length)
                : this()
            {
                Start = start;
                Length = length;
            }

            public int Start { get; private set; }
            public int Length { get; private set; }
            public int End { get { return Start + Length; } }
        }

        private struct Node
        {
            internal I Interval { get; private set; }
            internal Sublist Sublist { get; private set; }

            internal Node(I interval, Sublist sublist)
                : this()
            {
                Interval = interval;
                Sublist = sublist;
            }

            public override string ToString()
            {
                return String.Format("{0} - {1}/{2}", Interval, Sublist.Length, Sublist.Start);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Nested Containment List with a enumerable of intervals
        /// </summary>
        /// <param name="intervals">A collection of intervals in arbitrary order</param>
        public NestedContainmentList(IEnumerable<I> intervals)
        {
            var intervalsArray = intervals as I[] ?? intervals.ToArray();

            if (intervalsArray.Any())
            {
                _count = intervalsArray.Count();

                Sorting.IntroSort(intervalsArray, 0, _count, IntervalExtensions.CreateComparer<I, T>());

                var totalSection = new Sublist(0, intervalsArray.Count());
                _list = new Node[totalSection.Length];

                // Build nested containment list recursively and save the upper-most list in the class
                _mainList = new Sublist(0, createList(intervalsArray, totalSection, totalSection));

                // Save span to allow for constant speeds on later requests
                _span = new IntervalBase<T>(_list[_mainList.Start].Interval, _list[_mainList.Length + _mainList.Start - 1].Interval);
            }
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

                _list[t++] = new Node(interval, new Sublist(end, length));
            }

            return t - target.Start;
        }

        #endregion

        #region Enumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Create an enumerator, enumerating the intervals in sorted order - sorted on low endpoint with shortest intervals first
        /// </summary>
        /// <returns>Enumerator</returns>
        // TODO: Test the order is still the same as when sorted with IntervalComparer. This should be that case!
        public override IEnumerator<I> GetEnumerator()
        {
            return getEnumerator(_mainList);
        }

        // TODO: Test the order is still the same as when sorted with IntervalComparer. This should be that case!

        private IEnumerator<I> getEnumerator(Sublist sublist)
        {
            // Just for good measures
            if (_list == null || sublist.Length == 0)
                yield break;

            for (var i = sublist.Start; i < sublist.End; i++)
            {
                var node = _list[i];

                // Yield the interval itself before the sublist to maintain sorting order
                yield return node.Interval;

                if (node.Sublist.Length > 0)
                {
                    var child = getEnumerator(node.Sublist);

                    while (child.MoveNext())
                        yield return child.Current;
                }
            }
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty { get { return _count == 0; } }
        /// <inheritdoc/>
        public override int Count { get { return _count; } }
        /// <inheritdoc/>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _list.First().Interval;
        }

        #endregion

        #region Interval Collection

        #region Properties

        /// <inheritdoc/>
        public IInterval<T> Span { get { return _span; } }

        /// <inheritdoc/>
        public int MaximumDepth
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public bool AllowsOverlaps { get { return true; } }

        /// <inheritdoc/>
        public bool AllowsReferenceDuplicates { get { return true; } }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            if (ReferenceEquals(query, null))
                return Enumerable.Empty<I>();

            return findOverlaps(_mainList, new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (query == null)
                return Enumerable.Empty<I>();

            return findOverlaps(_mainList, query);
        }

        private IEnumerable<I> findOverlaps(Sublist sublist, IInterval<T> query)
        {
            if (_list == null || sublist.Length == 0)
                yield break;

            // Find first overlapping interval
            var first = findFirst(sublist, query);

            // If index is out of bound, or interval doesn't overlap, we can just stop our search
            if (first < sublist.Start || sublist.End - 1 < first || !_list[first].Interval.Overlaps(query))
                yield break;

            var last = findLast(sublist, query);

            while (first <= last)
            {
                var node = _list[first++];

                yield return node.Interval;

                if (node.Sublist.Length > 0)
                    // If the interval is contained in the query, all intervals in the sublist must overlap the query
                    foreach (var interval in findOverlaps(node.Sublist, query))
                        yield return interval;
            }
        }

        /// <summary>
        /// Get the index of the first node with an interval the overlaps the query
        /// </summary>
        private int findFirst(Sublist sublist, IInterval<T> query)
        {
            if (query == null || sublist.Length == 0)
                return -1;

            int min = sublist.Start - 1, max = sublist.End;

            while (min + 1 < max)
            {
                var middle = min + (max - min >> 1);

                var interval = _list[middle].Interval;

                var compare = query.Low.CompareTo(interval.High);

                if (compare < 0 || compare == 0 && query.LowIncluded && interval.HighIncluded)
                    max = middle;
                else
                    min = middle;
            }

            // We return min so we know if the query was lower or higher than the list
            return max;
        }

        /// <summary>
        /// Get the index of the last node with an interval the overlaps the query
        /// </summary>
        private int findLast(Sublist sublist, IInterval<T> query)
        {
            if (query == null || sublist.Length == 0)
                return -1;

            int min = sublist.Start - 1, max = sublist.End;

            while (min + 1 < max)
            {
                var middle = min + (max - min >> 1); // Shift one is the same as dividing by 2

                var interval = _list[middle].Interval;

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return min;
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public bool FindOverlap(T query, ref I overlap)
        {
            return FindOverlap(new IntervalBase<T>(query), ref overlap);
        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, ref I overlap)
        {
            // Check if query overlaps the collection at all
            if (query == null || _list == null || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(_mainList, query);

            // Check if index is in bound and if the interval overlaps the query
            var result = _mainList.Start <= i && i < _mainList.End && _list[i].Interval.Overlaps(query);

            if (result)
                overlap = _list[i].Interval;

            return result;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public int CountOverlaps(T query)
        {
            return FindOverlaps(query).Count();
        }
        /// <inheritdoc/>
        public int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public void AddAll(IEnumerable<I> intervals)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            throw new ReadOnlyCollectionException();
        }

        #endregion

        #endregion
    }
}
