using System;
using System.Diagnostics.Contracts;

namespace C5.Intervals
{
    /// <summary>
    /// An interval with associated data.
    /// </summary>
    /// <typeparam name="T">The generic type of the interval's endpoint values.</typeparam>
    /// <typeparam name="D">The generic type of the data.</typeparam>
    public interface IDataInterval<T, D> : IInterval<T> where T : IComparable<T>
    {
        /// <summary>
        /// The associated data.
        /// </summary>
        /// <value>The associated data.</value>
        [Pure]
        D Data { get; }
    }
}