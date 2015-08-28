using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// Base class for classes implementing <see cref="IIntervalCollection{I,T}"/> for finite interval collections.
    /// </summary>
    /// <remarks>
    /// When extending this class, it should not be necessary reimplement any of the methods. It is however recommended
    /// to have a look at <see cref="IntervalCollectionBase{I,T}"/>, to see which of its methods are worth reimplementing.
    /// </remarks>
    /// <typeparam name="I">The interval type with endpoint type <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    /// <seealso cref="IntervalCollectionBase{I,T}"/>
    public abstract class ContainmentFreeIntervalCollectionBase<I, T> : SortedIntervalCollectionBase<I, T>, IContainmentFreeIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Data Structure Properties

        public override bool AllowsContainments { get { return false; } }

        /// <inheritdoc/>
        public abstract Speed IndexingSpeed { get; }

        #endregion

        #region Collection Property

        public override I HighestInterval
        {
            get
            {
                return SortedBackwards().First();
            }
        }

        public override IEnumerable<I> HighestIntervals
        {
            get
            {
                var enumerator = SortedBackwards().GetEnumerator();

                if (!enumerator.MoveNext())
                {
                    yield break;
                }

                var highestInterval = enumerator.Current;
                yield return highestInterval;

                while (enumerator.MoveNext())
                {
                    var interval = enumerator.Current;
                    if (interval.HighEquals(highestInterval))
                        yield return interval;
                    else
                        yield break;
                }
            }
        }

        public override int MaximumDepth
        {
            get
            {
                var intervals = Sorted;
                var max = 0;
                var queue = new CircularQueue<I>();

                // Loop through intervals in sorted order
                foreach (var interval in intervals)
                {
                    // Remove all intervals from the queue not overlapping the current interval
                    while (!queue.IsEmpty && interval.CompareLowHigh(queue[0]) > 0)
                        queue.Dequeue();

                    queue.Enqueue(interval);

                    if (queue.Count > max)
                        max = queue.Count;
                }

                return max;
            }
        }

        #endregion

        #region Sorted Enumeration

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator()
        {
            // It is expected that the default enumerator _is_ the sorted enumerator!
            return Sorted.GetEnumerator();
        }

        /// <inheritdoc/>
        public abstract IEnumerable<I> SortedBackwards();

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateBackwardsFrom(I interval, bool includeInterval = true);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateFromIndex(int index);

        /// <inheritdoc/>
        public abstract IEnumerable<I> EnumerateBackwardsFromIndex(int index);

        #endregion

        #region Indexed Access

        /// <inheritdoc/>
        public abstract I this[int i] { get; }

        #endregion
    }
}
