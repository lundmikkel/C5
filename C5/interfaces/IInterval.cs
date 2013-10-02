using System;

namespace C5
{
    /// <summary>
    /// An interval with included or excluded low and high endpoints.
    /// </summary>
    /// <remarks>None of the values should be changed once the interval is stored in an interval collection, as it most certainly will break the data structure.</remarks>
    /// <typeparam name="T">The generic type of the interval's endpoint values.</typeparam>
    public interface IInterval<T> where T : IComparable<T>
    {
        // @design: shortest and most generic terms for endpoints
        /// <summary>
        /// Get the low endpoint for the interval.
        /// </summary>
        /// <value>The low endpoint value.</value>
        /// <remarks>Also known as left, start, first, or lower endpoint.</remarks>
        T Low { get; }

        /// <summary>
        /// Get the high endpoint for the interval.
        /// </summary>
        /// <value>The high endpoint value.</value>
        /// <remarks>Also known as right, end, stop, last, or higher endpoint.</remarks>
        T High { get; }

        /// <summary>
        /// Get the low endpoint's inclusion.
        /// </summary>
        /// <value>True if the low endpoint is included in the interval, otherwise false.</value>
        /// <remarks>A stabbing query using the low endpoint value will not include the interval, if the value is false.</remarks>
        bool LowIncluded { get; }

        /// <summary>
        /// Get the high endpoint's inclusion.
        /// </summary>
        /// <value>True if the high endpoint is included in the interval, otherwise false.</value>
        /// <remarks>A stabbing query using the high endpoint value will not include the interval, if the value is false.</remarks>
        bool HighIncluded { get; }
    }
}
