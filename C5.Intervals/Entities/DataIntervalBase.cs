using System;

namespace C5.Intervals
{
    /// <summary>
    /// Basic data interval class that uses <see cref="IntervalBase{T}"/>. Useful for return or query values or as base class for own implementation of <see cref="IDataInterval{T, D}"/>.
    /// </summary>
    /// <seealso cref="IDataInterval{T, D}"/>
    public class DataIntervalBase<T, D> : IntervalBase<T>, IDataInterval<T, D> where T : IComparable<T>
    {
        #region Fields

        protected readonly D _data;

        #endregion

        #region Constructors

        public DataIntervalBase(T query, D data)
            : base(query)
        {
            _data = data;
        }

        public DataIntervalBase(T low, T high, D data)
            : base(low, high, true, false)
        {
            _data = data;
        }

        public DataIntervalBase(T low, T high, bool lowIncluded, bool highIncluded, D data)
            : base(low, high, lowIncluded, highIncluded)
        {
            _data = data;
        }

        public DataIntervalBase(T low, T high, IntervalType type, D data)
            : base(low, high, type)
        {
            _data = data;
        }

        public DataIntervalBase(IInterval<T> i, D data)
            : base(i)
        {
            _data = data;
        }

        #endregion

        #region Properties

        public D Data { get { return _data; } }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (GetType() != obj.GetType()) return false;
            var that = (IDataInterval<T, D>)obj;
            return this.IntervalEquals(that) && _data.Equals(that.Data);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.GetIntervalHashCode() * 31 + _data.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0} - {1}", this.ToIntervalString(), _data);
        }

        #endregion
    }
}
