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
    public class LayeredContainmentListNewSplitGallop<I, T> : LayeredContainmentListNewSplit<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        public LayeredContainmentListNewSplitGallop(IEnumerable<I> intervals)
            : base(intervals)
        {
        }

        protected override int FindFirst(IInterval<T> query, int lower, int upper)
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
                var compare = query.Low.CompareTo(_intervals[lower].High);

                var next = lower + jump;

                // We are in a higher layer than needed, do binary search for the rest
                if (compare <= 0 || upper <= next)
                {
                    if (compare <= 0)
                        upper = lower + 1;

                    // Back up to previous value
                    lower -= (jump >> 1);

                    return base.FindFirst(query, lower, upper);
                }

                // Jump
                lower = next;

                // Double jump
                jump <<= 1;
            }
        }
    }
}