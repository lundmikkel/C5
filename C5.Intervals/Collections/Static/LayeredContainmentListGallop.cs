using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// An implementation of the Layered Containment List by Mikkel Riise Lund using two separate
    /// arrays for intervals and pointers.
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class LayeredContainmentListGallop<I, T> : LayeredContainmentList<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        public LayeredContainmentListGallop(IEnumerable<I> intervals)
            : base(intervals)
        {
        }

        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            int lower = 0,
                upper = _firstLayerCount;

            var first = findFirst(query, lower, upper);

            if (first < lower || upper <= first || !_intervals[first].Overlaps(query))
                yield break;

            var last = findLastForwardsGallop(query, first, upper);

            while (first < last)
            {
                lower = _pointers[first];
                upper = _pointers[last];

                while (first < last)
                    yield return _intervals[first++];

                first = findFirstGallop(query, lower, upper);
                if (first < lower || upper <= first || !_intervals[first].Overlaps(query))
                    yield break;

                last = findLastBackwardsGallop(query, first, upper);
            }
        }

        private int findFirstGallop(IInterval<T> query, int lower, int upper)
        {
            Contract.Requires(query != null);
            // Bounds must be in bounds
            Contract.Requires(0 <= lower && lower <= _intervals.Length);
            Contract.Requires(0 <= upper && upper <= _intervals.Length);
            // Lower and upper must be in the same layer
            Contract.Requires(Contract.ForAll(lower, upper, i => _intervals[i] != null));

            // Either no interval overlaps or the interval at index result is the first overlap
            Contract.Ensures(
                Contract.Result<int>() < lower ||
                upper <= Contract.Result<int>() ||
                Contract.ForAll(lower, upper, i => !_intervals[i].Overlaps(query)) ||
                _intervals[Contract.Result<int>()].Overlaps(query) && Contract.ForAll(lower, Contract.Result<int>(), i => !_intervals[i].Overlaps(query))
            );

            var jump = 1;

            if (upper <= lower)
                return lower;

            while (true)
            {
                var compare = query.CompareLowHigh(_intervals[lower]);

                var next = lower + jump;

                // We are in a higher layer than needed, do binary search for the rest
                if (compare <= 0 || upper <= next)
                {
                    if (compare <= 0)
                        upper = lower + 1;

                    // Back up to previous value
                    lower -= (jump >> 1);

                    return findFirst(query, lower, upper);
                }

                // Jump
                lower = next;

                // Double jump
                jump <<= 1;
            }
        }

        private int findLastForwardsGallop(IInterval<T> query, int lower, int upper)
        {
            Contract.Requires(query != null);
            // Bounds must be in bounds
            Contract.Requires(0 <= lower && lower <= _intervals.Length);
            Contract.Requires(0 <= upper && upper <= _intervals.Length);
            // Lower and upper must be in the same layer
            Contract.Requires(Contract.ForAll(lower, upper, i => _intervals[i] != null));

            // Either no interval overlaps or the interval at index result is the first overlap
            Contract.Ensures(
                Contract.Result<int>() < lower ||
                upper <= Contract.Result<int>() ||
                Contract.ForAll(lower, upper, i => !_intervals[i].Overlaps(query)) ||
                _intervals[Contract.Result<int>() - 1].Overlaps(query) && Contract.ForAll(Contract.Result<int>(), upper, i => !_intervals[i].Overlaps(query))
            );

            var jump = 1;

            if (upper <= lower)
                return upper;

            while (true)
            {
                var compare = _intervals[lower].CompareLowHigh(query);

                var next = lower + jump;

                // We are in a higher layer than needed, do binary search for the rest
                if (compare > 0 || upper <= next)
                {
                    if (compare > 0)
                        upper = lower + 1;

                    // Back up to previous value
                    lower -= (jump >> 1);

                    return findLast(query, lower, upper);
                }

                // Jump
                lower = next;

                // Double jump
                jump <<= 1;
            }
        }

        private int findLastBackwardsGallop(IInterval<T> query, int lower, int upper)
        {
            Contract.Requires(query != null);
            // Bounds must be in bounds
            Contract.Requires(0 <= lower && lower <= _intervals.Length);
            Contract.Requires(0 <= upper && upper <= _intervals.Length);
            // Lower and upper must be in the same layer
            Contract.Requires(Contract.ForAll(lower, upper, i => _intervals[i] != null));

            // Either no interval overlaps or the interval at index result is the first overlap
            Contract.Ensures(
                Contract.Result<int>() < lower ||
                upper <= Contract.Result<int>() ||
                Contract.ForAll(lower, upper, i => !_intervals[i].Overlaps(query)) ||
                _intervals[Contract.Result<int>() - 1].Overlaps(query) && Contract.ForAll(Contract.Result<int>(), upper, i => !_intervals[i].Overlaps(query))
            );

            var jump = 1;

            if (upper <= lower)
                return upper;

            while (true)
            {
                var compare = query.CompareHighLow(_intervals[upper - 1]);

                var next = upper - jump;

                // We are in a higher layer than needed, do binary search for the rest
                if (compare > 0 || next - 1 < lower)
                {
                    if (compare > 0)
                        lower = upper - 1;

                    // Back up to previous value
                    upper += (jump >> 1);

                    return findLast(query, lower, upper);
                }

                // Jump
                upper = next;

                // Double jump
                jump <<= 1;
            }
        }
    }
}