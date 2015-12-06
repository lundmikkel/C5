using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    public class BinaryIntervalSearch<I, T> : SortedIntervalCollectionBase<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly I[] _lowSorted, _highSorted;
        private readonly int _count;
        private readonly IInterval<T> _span;
        private int _maximumDepth = -1;
        private readonly bool _keepOverlapsSorted = true;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Create a Binary Interval Search using a collection of intervals.
        /// </summary>
        /// <param name="intervals"></param>
        /// <param name="keepOverlapsSorted">If true, overlaps will be sorted.</param>
        public BinaryIntervalSearch(IEnumerable<I> intervals, bool keepOverlapsSorted = false)
        {
            // Make intervals to array to allow fast sorting and counting
            var intervalArray = intervals as I[] ?? intervals.ToArray();

            // Stop if we have no intervals
            if (!intervalArray.Any())
                return;

            _count = intervalArray.Length;

            _lowSorted = new I[_count];
            _highSorted = new I[_count];

            for (var i = 0; i < _count; i++)
                _lowSorted[i] = _highSorted[i] = intervalArray[i];


            // Sort intervals
            var lowComparer = IntervalExtensions.CreateComparer<I, T>();
            Sorting.Timsort(_lowSorted, 0, _count, lowComparer);

            var highComparer = ComparerFactory<I>.CreateComparer((x, y) => x.CompareHigh(y));
            Sorting.Timsort(_highSorted, 0, _count, highComparer);

            _span = new IntervalBase<T>(_lowSorted[0], _highSorted[_count - 1]);
            _keepOverlapsSorted = keepOverlapsSorted;
        }

        #endregion Constructor

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

            return _lowSorted[0];
        }

        #endregion Collection Value

        #region Interval Collection

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool AllowsOverlaps { get { return true; } }

        /// <inheritdoc/>
        public override bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        public override bool IsFindOverlapsSorted { get { return _keepOverlapsSorted; } }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public override IInterval<T> Span { get { return _span; } }

        /// <inheritdoc/>
        public override I LowestInterval { get { return _lowSorted[0]; } }

        /// <inheritdoc/>
        public override IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var lowestInterval = LowestInterval;
                yield return lowestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = 1; i < _count; i++)
                {
                    if (_lowSorted[i].LowEquals(lowestInterval))
                        yield return _lowSorted[i];
                    else
                        yield break;
                }
            }
        }

        /// <inheritdoc/>
        public override I HighestInterval { get { return _highSorted[_count - 1]; } }

        /// <inheritdoc/>
        public override IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var highestInterval = HighestInterval;
                yield return highestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = _count - 2; i >= 0; i--)
                {
                    if (_highSorted[i].HighEquals(highestInterval))
                        yield return _highSorted[i];
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
                {
                    IInterval<T> intervalOfMaximumDepth;
                    _maximumDepth = Sorted.MaximumDepth(out intervalOfMaximumDepth);
                }

                return _maximumDepth;
            }
        }

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator() { return Sorted.GetEnumerator(); }

        /// <inheritdoc/>
        public override IEnumerable<I> Sorted
        {
            get
            {
                if (IsEmpty)
                    yield break;

                foreach (var interval in _lowSorted)
                    yield return interval;
            }
        }

        #endregion

        #region Find Equals

        public override IEnumerable<I> FindEquals(IInterval<T> query)
        {
            var i = indexOf(query);

            // No equal found
            if (i < 0)
                yield break;

            // Enumerate equals
            while (i < _count && _lowSorted[i].IntervalEquals(query))
                yield return _lowSorted[i++];
        }

        private int indexOf(IInterval<T> query)
        {
            int min = 0, max = _count - 1;

            while (min <= max)
            {
                var mid = min + (max - min >> 1);
                var compareTo = _lowSorted[mid].CompareTo(query);

                if (compareTo < 0)
                    min = mid + 1;
                else if (compareTo > 0)
                    max = mid - 1;
                else if (min != mid)
                    max = mid;
                else
                    return mid;
            }

            return ~min;
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(T query)
        {
            return FindOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (IsEmpty || !_span.Overlaps(query))
                yield break;

            // Search for the last overlap
            var first = findFirst(query);
            var last = findLast(query);

            var overlapsRemaining = last - first;

            if (overlapsRemaining == 0)
                yield break;

            var intervals = _lowSorted;
            var lower = 0;
            var upper = last;

            // If result must not necessarily be sorted, we iterate the shortest range
            if (!_keepOverlapsSorted && _count - first < last)
            {
                lower = first;
                upper = _count;
                intervals = _highSorted;
            }

            // Enumerate collection until end is reached or all overlaps have been found
            for (var i = lower; 0 < overlapsRemaining && i < upper; ++i)
            {
                // Only return if it actually overlaps
                if (intervals[i].Overlaps(query))
                {
                    yield return intervals[i];
                    --overlapsRemaining;
                }
            }
        }

        [Pure]
        private int findFirst(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // All intervals before index result do not overlap the query
            Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => !_highSorted[i].Overlaps(query)));

            int min = 0, max = _count;

            while (min < max)
            {
                var mid = min + (max - min >> 1);

                if (_highSorted[mid].CompareHighLow(query) < 0)
                    min = mid + 1;
                else
                    max = mid;
            }

            return min;
        }

        [Pure]
        private int findLast(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // All intervals after index result do not overlap the query
            Contract.Ensures(Contract.ForAll(Contract.Result<int>(), _lowSorted.Count(), i => !_lowSorted[i].Overlaps(query)));

            int min = 0, max = _count;

            while (min < max)
            {
                var mid = min + (max - min >> 1); // Divide by 2, by shifting one to the left

                if (query.CompareHighLow(_lowSorted[mid]) < 0)
                    max = mid;
                else
                    min = mid + 1;
            }

            return min;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public override int CountOverlaps(T query)
        {
            return CountOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public override int CountOverlaps(IInterval<T> query)
        {
            return IsEmpty || !_span.Overlaps(query) ? 0 : findLast(query) - findFirst(query);
        }

        #endregion

        #endregion
    }
}