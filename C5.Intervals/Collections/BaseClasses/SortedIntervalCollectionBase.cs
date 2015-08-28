using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// Base class for classes implementing <see cref="ISortedIntervalCollection{I,T}"/> for
    /// sorted interval collections.
    /// </summary>
    /// <typeparam name="I">The interval type with endpoint type <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    /// <seealso cref="IntervalCollectionBase{I,T}"/>
    public abstract class SortedIntervalCollectionBase<I, T> : IntervalCollectionBase<I, T>, ISortedIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool IsFindOverlapsSorted { get { return true; } }

        #endregion

        #region Collection Properties
        
        /// <inheritdoc/>
        public override I LowestInterval
        {
            get
            {
                return Sorted.First();
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<I> LowestIntervals
        {
            get
            {
                var enumerator = Sorted.GetEnumerator();

                if (!enumerator.MoveNext())
                {
                    yield break;
                }

                var lowestInterval = enumerator.Current;
                yield return lowestInterval;

                while (enumerator.MoveNext())
                {
                    var interval = enumerator.Current;
                    if (interval.LowEquals(lowestInterval))
                        yield return interval;
                    else
                        yield break;
                }
            }
        }
        
        #endregion

        #endregion

        #region Sorted Enumeration

        /// <inheritdoc/>
        public abstract IEnumerable<I> Sorted { get; }

        #endregion

        #region Gaps

        /// <inheritdoc/>
        public override IEnumerable<IInterval<T>> Gaps
        {
            get
            {
                return Sorted.Gaps();
            }
        }

        #endregion
    }
}
