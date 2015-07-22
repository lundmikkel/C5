using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.Intervals.Collections
{
    public abstract class SortedIntervalCollectionBase<I, T> : IntervalCollectionBase<I, T>, ISortedIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        #region Data Structure Properties

        /// <inheritdoc/>
        public abstract Speed IndexingSpeed { get; }

        #endregion Data Structure Properties

        #region Collection Properties

        /// <inheritdoc/>
        public override I LowestInterval
        {
            get
            {
                return Sorted().First();
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<I> LowestIntervals
        {
            get
            {
                using (var enumerator = Sorted().GetEnumerator())
                {
                    // Advance the enumerator to the first element
                    if (enumerator.MoveNext())
                        yield break;

                    var lowest = enumerator.Current;
                    yield return lowest;

                    I next;
                    // Only enumerate intervals with same low as the first interval
                    while (enumerator.MoveNext() && (next = enumerator.Current).LowEquals(lowest))
                        yield return next;
                }
            }
        }

        // /// <inheritdoc/>
        // public override I HighestInterval
        // {
        //     get
        //     {
        //         if (AllowsHighLowEnumeration)
        //             return Sorted(false).First();
        // 
        //         // Implement if needed
        //         throw new NotImplementedException();
        //     }
        // }
        // 
        // /// <inheritdoc/>
        // public override IEnumerable<I> HighestIntervals
        // {
        //     get
        //     {
        //         if (AllowsHighLowEnumeration)
        //             using (var enumerator = Sorted(false).GetEnumerator())
        //             {
        //                 // Advance the enumerator to the first element
        //                 if (enumerator.MoveNext())
        //                     yield break;
        // 
        //                 var highest = enumerator.Current;
        //                 yield return highest;
        // 
        //                 I next;
        //                 // Only enumerate intervals with same high as the first interval
        //                 while (enumerator.MoveNext() && (next = enumerator.Current).HighEquals(highest))
        //                     yield return next;
        //             }
        // 
        //         // Implement if needed
        //         throw new NotImplementedException();
        //     }
        // }

        #endregion

        #endregion

        #region Sorted Enumeration

        /// <inheritdoc/>
        public abstract IEnumerable<I> Sorted();

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateFromIndex(int index);

        #endregion Sorted Enumeration

        #region Indexed Access

        /// <inheritdoc/>
        public abstract int IndexOf(I interval);

        /// <inheritdoc/>
        public abstract I this[int i] { get; }

        #endregion Indexed Access

    }
}
