using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace C5.intervals
{
    /// <summary>
    /// Extends IIntervals with convenient methods for overlapping, containment, comparing, equality and hashcode, and string formatting.
    /// </summary>
    public static class IntervalExtensions
    {
        /// <summary>
        /// Check if two intervals overlap.
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <remarks>True if their intersection is not empty. The meaning should not be confused with <see cref="IntervalRelation.Overlaps"/> and <see cref="IntervalRelation.OverlappedBy"/>!</remarks>
        /// <returns>True if the intervals overlap.</returns>
        [Pure]
        public static bool Overlaps<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == (x.CompareLowHigh(y) <= 0 && y.CompareLowHigh(x) <= 0));

            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int xLowYHighCompare = x.Low.CompareTo(y.High), yLowXHighCompare = y.Low.CompareTo(x.High);
            return (xLowYHighCompare < 0 || xLowYHighCompare == 0 && x.LowIncluded && y.HighIncluded)
                && (yLowXHighCompare < 0 || yLowXHighCompare == 0 && y.LowIncluded && x.HighIncluded);
        }

        /// <summary>
        /// Compare an interval with a point interval to see if they overlap
        /// </summary>
        /// <param name="x">Interval.</param>
        /// <param name="p">Point interval.</param>
        /// <returns>True if the intervals overlap.</returns>
        [Pure]
        public static bool Overlaps<T>(this IInterval<T> x, T p) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(!ReferenceEquals(p, null));

            return Overlaps(x, new IntervalBase<T>(p));
        }

        /// <summary>
        /// Check if one interval strictly contains another interval. The container interval
        /// contains all of the contained interval without sharing endpoints.
        /// </summary>
        /// <param name="x">Container interval.</param>
        /// <param name="y">Contained interval.</param>
        /// <returns>True if y is strictly contained in x.</returns>
        [Pure]
        public static bool StrictlyContains<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == (x.CompareLow(y) < 0 && y.CompareHigh(x) < 0));

            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int lowCompare = x.Low.CompareTo(y.Low), highCompare = y.High.CompareTo(x.High);
            return
                (lowCompare < 0 || (lowCompare == 0 && x.LowIncluded && !y.LowIncluded))
                && (highCompare < 0 || (highCompare == 0 && !y.HighIncluded && x.HighIncluded));
        }

        /// <summary>
        /// Check if one interval contains another interval. The container interval
        /// contains all of the contained interval possibly sharing endpoints.
        /// </summary>
        /// <param name="x">Container interval.</param>
        /// <param name="y">Contained interval.</param>
        /// <returns>True if y is contained in x.</returns>
        [Pure]
        public static bool Contains<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == (x.CompareLow(y) <= 0 && y.CompareHigh(x) <= 0));

            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int lowCompare = x.Low.CompareTo(y.Low), highCompare = y.High.CompareTo(x.High);
            return
                (lowCompare < 0 || lowCompare == 0 && (x.LowIncluded || !y.LowIncluded))
                && (highCompare < 0 || highCompare == 0 && (!y.HighIncluded || x.HighIncluded));
        }

        /// <summary>
        /// Compare two intervals to determine which one is first in a sorting order.
        /// The interval that starts first and is shortest will come first.
        /// 
        /// Sorting order is determined as follows:
        /// The interval with the lowest low endpoint comes first.
        /// If the low endpoints are equal, the interval with included low endpoint comes first.
        /// If the low endpoint inclusions are equal, the interval with the lowest high endpoint comes first.
        /// If the high endpoints are equal, the interval with excluded high endpoint comes first.
        /// If the high endpoint inclusions are equal, the interval are equal.
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns>Negative if first interval is before the second, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareTo<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<int>() == (x.CompareLow(y) != 0 ? x.CompareLow(y) : x.CompareHigh(y)));

            var lowCompare = x.Low.CompareTo(y.Low);

            // Check if x starts y - their lows are the same
            if (lowCompare == 0)
            {
                // If both include or exclude their low endpoint, we don't care which one is first
                if (x.LowIncluded == y.LowIncluded)
                {
                    var highCompare = x.High.CompareTo(y.High);

                    // Check endpoint inclusion, if values are equal, but inclusion is different
                    if (highCompare == 0 && x.HighIncluded != y.HighIncluded)
                        // Excluded high endpoints come before included
                        return !x.HighIncluded ? -1 : 1;

                    return highCompare;
                }

                // x.LowIncluded and y.LowIncluded are different
                // So if x.LowIncluded is true it comes before y
                return x.LowIncluded ? -1 : 1;

            }

            // We can simply compare their low points
            return lowCompare;
        }

        /// <summary>
        /// Compare the low endpoints of two intervals. If the low endpoint values are equal,
        /// an included endpoint precedes an excluded endpoint
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns>Negative if first interval's low is less than second's low endpoint, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareLow<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var compare = x.Low.CompareTo(y.Low);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0 && x.LowIncluded != y.LowIncluded)
                // Included low endpoints come before excluded
                return x.LowIncluded ? -1 : 1;

            return compare;
        }

        /// <summary>
        /// Compare the high endpoints of two intervals. If the high endpoint values are equal,
        /// an excluded endpoint precedes an included endpoint
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns>Negative if first interval's high is less than second's high endpoint, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareHigh<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var compare = x.High.CompareTo(y.High);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0 && x.HighIncluded != y.HighIncluded)
                // Excluded high endpoints come before included
                return !x.HighIncluded ? -1 : 1;

            return compare;
        }

        /// <summary>
        /// Compare the low endpoint of first interval to the high endpoint of the second interval.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <returns>Negative if first interval's low is less than second's high endpoint, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareLowHigh<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var compare = x.Low.CompareTo(y.High);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0)
                // Excluded high endpoints come before included
                return x.LowIncluded && y.HighIncluded ? 0 : 1;

            return compare;
        }
        /// <summary>
        /// Compare the high endpoint of first interval to the low endpoint of the second interval.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <returns>Negative if first interval's high is less than second's low endpoint, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareHighLow<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var compare = x.High.CompareTo(y.Low);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0)
                // Excluded high endpoints come before included
                return x.HighIncluded && y.LowIncluded ? 0 : -1;

            return compare;
        }

        /// <summary>
        /// Compare the endpoint values of an interval with each other.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <returns>Negative if the high endpoint value is less than the low endpoint value, 0 if they are equal, otherwise positive.</returns>
        [Pure]
        public static int CompareEndpointsValues<T>(this IInterval<T> x) where T : IComparable<T>
        {
            Contract.Requires(x != null);

            return x.Low.CompareTo(x.High);
        }

        /// <summary>
        /// Check if two intervals are equal, i.e. have the same low and high endpoint including endpoint inclusion.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <returns>True if both endpoints are equal.</returns>
        [Pure]
        public static bool IntervalEquals<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Ensures(Contract.Result<bool>() == (CompareTo(x, y) == 0));

            return x.Low.CompareTo(y.Low) == 0 && x.High.CompareTo(y.High) == 0 && x.LowIncluded == y.LowIncluded &&
                   x.HighIncluded == y.HighIncluded;
        }

        /// <summary>
        /// Get the intersection between two intervals.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>An interval of intersection.</returns>
        /// <exception cref="ArgumentException">If intervals do not overlap.</exception>
        [Pure]
        public static IInterval<T> IntersectionWith<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Requires(x.Overlaps(y));
            // The intersection will be contained in both intervals
            Contract.Ensures(x.Contains(Contract.Result<IInterval<T>>()));
            Contract.Ensures(y.Contains(Contract.Result<IInterval<T>>()));

            var low = x.CompareLow(y) > 0 ? x : y;
            var high = x.CompareHigh(y) < 0 ? x : y;

            return new IntervalBase<T>(low, high);
        }

        /// <summary>
        /// Creates an interval from the lowest low and the highest high of the two intervals. The joined span contains both intervals.
        /// </summary>
        /// <param name="x">The first interval.</param>
        /// <param name="y">The second interval.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>The joined span.</returns>
        public static IInterval<T> JoinedSpan<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            // The intersection will be contained in both intervals
            Contract.Ensures(Contract.Result<IInterval<T>>().Contains(x));
            Contract.Ensures(Contract.Result<IInterval<T>>().Contains(y));
            // The endpoints of the joined span is each equal to one of the endpoints of the two intervals
            Contract.Ensures(x.CompareLow(Contract.Result<IInterval<T>>()) == 0 || y.CompareLow(Contract.Result<IInterval<T>>()) == 0);
            Contract.Ensures(x.CompareHigh(Contract.Result<IInterval<T>>()) == 0 || y.CompareHigh(Contract.Result<IInterval<T>>()) == 0);

            return new IntervalBase<T>(x.LowestLow(y), x.HighestHigh(y));
        }

        /// <summary>
        /// Returns the interval with the highest high endpoint. If equal the first interval is favored.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <typeparam name="T">Endpoint value type</typeparam>
        /// <returns>The interval with the highest high endpoint.</returns>
        [Pure]
        public static IInterval<T> HighestHigh<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            return x.CompareHigh(y) >= 0 ? x : y;
        }

        /// <summary>
        /// Returns the interval with the lowest low endpoint. If equal the first interval is favored.
        /// </summary>
        /// <param name="x">First interval.</param>
        /// <param name="y">Second interval.</param>
        /// <typeparam name="T">Endpoint value type</typeparam>
        /// <returns>The interval with the lowest low endpoint.</returns>
        [Pure]
        public static IInterval<T> LowestLow<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            return x.CompareLow(y) <= 0 ? x : y;
        }

        /// <summary>
        /// Check if the interval has the value as its low or high endpoint value.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <param name="endpoint">The endpoint value.</param>
        /// <typeparam name="T">The interval endpoint value type.</typeparam>
        /// <returns>True if either Low or High is equal to the endpoint.</returns>
        [Pure]
        public static bool HasEndpoint<T>(this IInterval<T> x, T endpoint) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Requires(!ReferenceEquals(endpoint, null));

            return x.Low.CompareTo(endpoint) == 0 || x.High.CompareTo(endpoint) == 0;
        }

        /// <summary>
        /// Check if an interval is a point.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>True if Low and High are equal and both endpoints are included, false otherwise.</returns>
        [Pure]
        public static bool IsPoint<T>(this IInterval<T> x) where T : IComparable<T>
        {
            Contract.Requires(x != null);

            return x.Low.CompareTo(x.High) == 0 && x.LowIncluded && x.HighIncluded;
        }

        /// <summary>
        /// Get the hashcode for an interval based on endpoints.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <typeparam name="T">The endpoint type.</typeparam>
        /// <returns>The hashcode based on the interval.</returns>
        [Pure]
        public static int GetHashCode<T>(this IInterval<T> x) where T : IComparable<T>
        {
            Contract.Requires(x != null);

            unchecked
            {
                return (((527
                    + x.Low.GetHashCode()) * 31
                    + x.LowIncluded.GetHashCode()) * 31
                    + x.High.GetHashCode()) * 31
                    + x.HighIncluded.GetHashCode();
            }
        }

        /// <summary>
        /// Create a string representing the interval.
        /// </summary>
        /// <param name="x">The interval.</param>
        /// <remarks>Closed intervals are represented with square brackets [a:b] and open with round brackets (a:b).</remarks>
        /// <returns>The string representation.</returns>
        [Pure]
        public static string ToString<T>(this IInterval<T> x) where T : IComparable<T>
        {
            Contract.Requires(x != null);
            Contract.Ensures(Contract.Result<string>().Length > 0);

            return String.Format("{0}{1}:{2}{3}",
                x.LowIncluded ? "[" : "(",
                x.Low,
                x.High,
                x.HighIncluded ? "]" : ")");
        }
    }

    /// <summary>
    /// Convenient extensions.
    /// </summary>
    // TODO: Find the proper file for the extension class
    public static class C5Extensions
    {
        /// <summary>
        /// Check if an IEnumerable is sorted in non-descending order.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>True if sorted.</returns>
        [Pure]
        public static bool IsSorted<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            Contract.Requires(collection != null);

            using (var enumerator = collection.GetEnumerator())
            {

            if (enumerator.MoveNext())
            {
                var previous = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;

                    if (previous.CompareTo(current) > 0)
                        return false;

                    previous = current;
                }
            }
            }

            return true;
        }

        /// <summary>
        /// Check if an IEnumerable is sorted in non-descending order using a comparer.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="comparer">The comparer for the collection.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>True if sorted.</returns>
        [Pure]
        public static bool IsSorted<T>(this IEnumerable<T> collection, IComparer<T> comparer)
        {
            Contract.Requires(collection != null);
            Contract.Requires(comparer != null);

            var enumerator = collection.GetEnumerator();

            if (enumerator.MoveNext())
            {
                var previous = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;

                    if (comparer.Compare(previous, current) > 0)
                        return false;

                    previous = current;
                }
            }

            return true;
        }
    }
}