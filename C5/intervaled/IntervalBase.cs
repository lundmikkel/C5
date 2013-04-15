using System;

namespace C5.intervaled
{
    /// <summary>
    /// Basic interval useful for return or query values.
    /// </summary>
    public class IntervalBase<T> : IInterval<T> where T : IComparable<T>
    {
        public T Low { get; protected set; }
        public T High { get; protected set; }
        public bool LowIncluded { get; protected set; }
        public bool HighIncluded { get; protected set; }

        /// <summary>
        /// Make a point with same Low and High and both endpoints included.
        /// </summary>
        /// <param name="query"></param>
        public IntervalBase(T query)
        {
            Low = High = query;
            LowIncluded = HighIncluded = true;
        }

        /// <summary>
        /// Left-closed, right-open interval
        /// </summary>
        /// <param name="low">Low, included endpoint</param>
        /// <param name="high">High, non-included endpoint</param>
        /// <exception cref="ArgumentException">Thrown if interval is an empty point set</exception>
        public IntervalBase(T low, T high)
        {
            if (high.CompareTo(low) < 0)
                throw new ArgumentException("Low must be smaller than high!");

            Low = low;
            High = high;
            LowIncluded = true;
            HighIncluded = false;
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
            if (high.CompareTo(low) < 0 || (low.CompareTo(high) == 0 && !lowIncluded && !HighIncluded))
                throw new ArgumentException("Low must be smaller than high. If low and high are equal, both lowIncluded and highIncluded should be true!");

            Low = low;
            High = high;
            LowIncluded = lowIncluded;
            HighIncluded = highIncluded;
        }

        public IntervalBase(IInterval<T> i)
        {
            Low = i.Low;
            High = i.High;
            LowIncluded = i.LowIncluded;
            HighIncluded = i.HighIncluded;
        }

        public IntervalBase(IInterval<T> low, IInterval<T> high)
        {
            Low = low.Low;
            LowIncluded = low.LowIncluded;

            High = high.High;
            HighIncluded = high.HighIncluded;
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
