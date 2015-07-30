using System;
using System.Diagnostics.Contracts;

namespace C5.Intervals
{
    /// <summary>
    /// Basic interval class with immutable endpoints. Useful for return or query values or as base class for own implementation of <see cref="IInterval{T}"/>.
    /// </summary>
    /// <seealso cref="IInterval{T}"/>
    public class IntervalBase<T> : IInterval<T> where T : IComparable<T>
    {
        #region Fields

        // Use read-only fields to avoid breaking data structures, if values were changed
        /// <inheritdoc/>
        [ContractPublicPropertyName("Low")]
        private readonly T _low;
        /// <inheritdoc/>
        [ContractPublicPropertyName("High")]
        private readonly T _high;
        /// <inheritdoc/>
        [ContractPublicPropertyName("LowIncluded")]
        private readonly bool _lowIncluded;
        /// <inheritdoc/>
        [ContractPublicPropertyName("HighIncluded")]
        private readonly bool _highIncluded;

        /// <inheritdoc/>
        public T Low
        {
            get
            {
                Contract.Ensures(this.IsValidInterval());
                return _low;
            }
        }

        /// <inheritdoc/>
        public T High
        {
            get
            {
                Contract.Ensures(this.IsValidInterval());
                return _high;
            }
        }

        /// <inheritdoc/>
        public bool LowIncluded
        {
            get
            {
                Contract.Ensures(this.IsValidInterval());
                return _lowIncluded;
            }
        }

        /// <inheritdoc/>
        public bool HighIncluded
        {
            get
            {
                Contract.Ensures(this.IsValidInterval());
                return _highIncluded;
            }
        }

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            Contract.Invariant(this.IsValidInterval());
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Make a point with same Low and High and both endpoints included.
        /// </summary>
        /// <param name="query"></param>
        public IntervalBase(T query)
        {
            Contract.Requires(query != null);

            _low = _high = query;
            _lowIncluded = _highIncluded = true;
        }

        /// <summary>
        /// Create an interval. Default to time interval, which has low included and high excluded.
        /// </summary>
        /// <param name="low">Low endpoint.</param>
        /// <param name="high">High endpoint.</param>
        /// <param name="lowIncluded">True if low endpoint is included.</param>
        /// <param name="highIncluded">True if high endpoint is included.</param>
        /// <exception cref="ArgumentException">Thrown if interval is an empty point set.</exception>
        public IntervalBase(T low, T high, bool lowIncluded = true, bool highIncluded = false)
        {
            Contract.Requires(low.CompareTo(high) < 0 || low.CompareTo(high) == 0 && lowIncluded && highIncluded);

            _low = low;
            _high = high;
            _lowIncluded = lowIncluded;
            _highIncluded = highIncluded;
        }

        /// <summary>
        /// Create an interval using <see cref="IntervalType"/> to determine endpoint types. This
        /// can be useful for making code more readable.
        /// </summary>
        /// <param name="low">Low endpoint.</param>
        /// <param name="high">High endpoint.</param>
        /// <param name="type">The interval type.</param>
        /// <exception cref="ArgumentException">Thrown if interval is an empty point set.</exception>
        public IntervalBase(T low, T high, IntervalType type)
        {
            Contract.Requires(low.CompareTo(high) < 0 || low.CompareTo(high) == 0 && type == IntervalType.Closed);

            _low = low;
            _high = high;
            _lowIncluded = (type & IntervalType.LowIncluded) == IntervalType.LowIncluded;
            _highIncluded = (type & IntervalType.HighIncluded) == IntervalType.HighIncluded;
        }

        /// <summary>
        /// Copy the interval data from an <see cref="IInterval&lt;T&gt;"/> to a new interval. 
        /// </summary>
        /// <param name="i"><see cref="IInterval&lt;T&gt;"/> to copy the information from</param>
        public IntervalBase(IInterval<T> i)
        {
            Contract.Requires(i != null);
            Contract.Requires(i.IsValidInterval());

            _low = i.Low;
            _high = i.High;
            _lowIncluded = i.LowIncluded;
            _highIncluded = i.HighIncluded;
        }

        /// <summary>
        /// Create an interval using the low value from one interval, and the high from another interval.
        /// </summary>
        /// <param name="low">The interval from which the low endpoint should be used</param>
        /// <param name="high">The interval from which the high endpoint should be used</param>
        public IntervalBase(IInterval<T> low, IInterval<T> high)
        {
            Contract.Requires(low != null);
            Contract.Requires(high != null);
            Contract.Requires(low.Low.CompareTo(high.High) < 0 || low.Low.CompareTo(high.High) == 0 && low.LowIncluded && high.HighIncluded);

            _low = low.Low;
            _lowIncluded = low.LowIncluded;

            _high = high.High;
            _highIncluded = high.HighIncluded;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (GetType() != obj.GetType()) return false;
            var that = (IInterval<T>)obj;
            return this.IntervalEquals(that);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.GetIntervalHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.ToIntervalString();
        }

        #endregion
    }
}
