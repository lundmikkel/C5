using System;

namespace C5.intervals
{
    /// <summary>
    /// Basic interval useful for return or query values.
    /// </summary>
    public class IntervalBase<T> : IInterval<T> where T : IComparable<T>
    {
        // Use read-only fields to avoid breaking data structures, if values were changed
        private readonly T _low;
        private readonly T _high;
        private readonly bool _lowIncluded;
        private readonly bool _highIncluded;

        public T Low { get { return _low; } }
        public T High { get { return _high; } }
        public bool LowIncluded { get { return _lowIncluded; } }
        public bool HighIncluded { get { return _highIncluded; } }

        /// <summary>
        /// Make a point with same Low and High and both endpoints included.
        /// </summary>
        /// <param name="query"></param>
        public IntervalBase(T query)
        {
            _low = _high = query;
            _lowIncluded = _highIncluded = true;
        }

        /// <summary>
        /// Left-closed, right-open interval
        /// </summary>
        /// <param name="low">Low, included endpoint</param>
        /// <param name="high">High, non-included endpoint</param>
        /// <exception cref="ArgumentException">Thrown if interval is an empty point set</exception>
        public IntervalBase(T low, T high)
        {
            if (low.CompareTo(high) >= 0)
                throw new ArgumentException("Low must be smaller than high!");

            _low = low;
            _high = high;
            _lowIncluded = true;
            _highIncluded = false;
        }

        /// <summary>
        /// Make a full interval
        /// </summary>
        /// <param name="low">Low endpoint</param>
        /// <param name="high">High endpoint</param>
        /// <param name="lowIncluded">True if low endpoint is included</param>
        /// <param name="highIncluded">True if high endpoint is included</param>
        /// <exception cref="ArgumentException">Thrown if interval is an empty point set</exception>
        public IntervalBase(T low, T high, bool lowIncluded, bool highIncluded)
        {
            if (high.CompareTo(low) < 0 || (low.CompareTo(high) == 0 && !lowIncluded && !highIncluded))
                throw new ArgumentException("Low must be smaller than high. If low and high are equal, both lowIncluded and highIncluded should be true!");

            _low = low;
            _high = high;
            _lowIncluded = lowIncluded;
            _highIncluded = highIncluded;
        }

        public IntervalBase(IInterval<T> i)
        {
            _low = i.Low;
            _high = i.High;
            _lowIncluded = i.LowIncluded;
            _highIncluded = i.HighIncluded;
        }

        public IntervalBase(IInterval<T> low, IInterval<T> high)
        {
            if (low.CompareLowHigh(high) > 0)
                throw new ArgumentException("Low must be smaller than high. If low and high are equal, both lowIncluded and highIncluded should be true!");

            _low = low.Low;
            _lowIncluded = low.LowIncluded;

            _high = high.High;
            _highIncluded = high.HighIncluded;
        }

        public override bool Equals(object that)
        {
            // TODO: Is this done?
            if (ReferenceEquals(null, that)) return false;
            if (ReferenceEquals(this, that)) return true;
            if (GetType() != that.GetType()) return false;
            return IntervalExtensions.Equals(this, (IInterval<T>) that);
        }

        public override int GetHashCode()
        {
            return IntervalExtensions.GetHashCode(this);
        }

        public override string ToString()
        {
            return IntervalExtensions.ToString(this);
        }
    }
}
