using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    // TODO: Finish data structure
    public class BinaryIntervalSearch<I, T> : SortedIntervalCollectionBase<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly I[] _lowSorted, _highSorted;
        private readonly int _count;
        private readonly IInterval<T> _span;
        private int _maximumDepth = -1;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Create a Binary Interval Search using a collection of intervals.
        /// </summary>
        /// <param name="intervals"></param>
        public BinaryIntervalSearch(IEnumerable<I> intervals)
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
            Sorting.IntroSort(_lowSorted, 0, _count, lowComparer);

            var highComparer = ComparerFactory<I>.CreateComparer((x, y) => x.CompareHigh(y));
            Sorting.IntroSort(_highSorted, 0, _count, highComparer);

            _span = new IntervalBase<T>(_lowSorted.First(), _highSorted.Last());
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

            return _lowSorted.First();
        }

        #endregion Collection Value

        #region Interval Collection

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool AllowsOverlaps { get { return true; } }

        /// <inheritdoc/>
        public override bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        // TODO: Allow user to set
        public override bool IsFindOverlapsSorted { get { return false; } }

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

                var lowestInterval = _lowSorted[0];

                yield return lowestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = 1; i < _count; i++)
                {
                    if (_lowSorted[i].CompareLow(lowestInterval) == 0)
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

                var highestInterval = _highSorted[_count - 1];

                yield return highestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = _count - 2; i >= 0; i--)
                {
                    if (_highSorted[i].CompareHigh(highestInterval) == 0)
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

            // If we have fewer intervals to iterate in high sorted list, we do that
            if (_count - first < last)
            {
                lower = first;
                upper = _count;
                intervals = _highSorted;
            }

            // Enumerate collection until end is reached or all overlaps have been found
            for (var i = lower; 0 < overlapsRemaining && i < upper; ++i)
            {
                I interval;
                // Only return if it actually overlaps
                if ((interval = intervals[i]).Overlaps(query))
                {
                    yield return interval;
                    --overlapsRemaining;
                }
            }
        }

        private int findFirst(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // All intervals before index result do not overlap the query
            Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => !_highSorted[i].Overlaps(query)));

            int min = -1, max = _count;

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = _highSorted[middle];

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

            // All intervals after index result do not overlap the query
            Contract.Ensures(Contract.ForAll(Contract.Result<int>(), _lowSorted.Count(), i => !_lowSorted[i].Overlaps(query)));

            int min = -1, max = _count;

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = _lowSorted[middle];

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return max;
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