using System;
using SCG = System.Collections.Generic;

namespace C5.intervaled
{
    /// <summary>
    /// Extends IIntervals with convenient methods for overlapping, containment, comparing, equality and hashcode, and string formatting.
    /// </summary>
    public static class IntervalExtensions
    {
        /// <summary>
        /// Compare two intervals to see if they overlap
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns>True if the intervals overlap, otherwise false</returns>
        public static bool Overlaps<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int xLowYHighCompare = x.Low.CompareTo(y.High), yLowXHighCompare = y.Low.CompareTo(x.High);
            return (xLowYHighCompare < 0 || xLowYHighCompare == 0 && x.LowIncluded && y.HighIncluded)
                && (yLowXHighCompare < 0 || yLowXHighCompare == 0 && y.LowIncluded && x.HighIncluded);

            // The same as (but faster than)
            return x.CompareLowHigh(y) <= 0 && y.CompareLowHigh(x) <= 0;
        }

        /// <summary>
        /// Compare an interval with a point interval to see if they overlap
        /// </summary>
        /// <param name="x">Interval</param>
        /// <param name="p">Point interval</param>
        /// <returns>True if the intervals overlap, otherwise false</returns>
        public static bool Overlaps<T>(this IInterval<T> x, T p) where T : IComparable<T>
        {
            return Overlaps(x, new IntervalBase<T>(p));
        }

        /// <summary>
        /// Check if one interval contains another interval.
        /// x contains y if x.Low is lower than y.Low and y.High is lower than x.High.
        /// If endpoints are equal endpoint inclusion is also taken into account.
        /// </summary>
        /// <param name="x">Container interval</param>
        /// <param name="y">Contained interval</param>
        /// <returns>True if y is contained in x</returns>
        public static bool Contains<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int lowCompare = x.Low.CompareTo(y.Low), highCompare = y.High.CompareTo(x.High);
            return
                (lowCompare < 0 || (lowCompare == 0 && x.LowIncluded && !y.LowIncluded))
                && (highCompare < 0 || (highCompare == 0 && !y.HighIncluded && x.HighIncluded));

            // The same as (but faster than)
            return x.CompareLow(y) < 0 && y.CompareHigh(x) < 0;
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
        /// <returns>Negative integer if x comes before y, 0 if they are equal, positive integer if x comes after y</returns>
        public static int CompareTo<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
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

            // The same as (but faster than)
            var compare = x.CompareLow(y);
            return compare != 0 ? compare : x.CompareHigh(y);
        }

        /// <summary>
        /// Compare the low endpoints of two intervals. If the low endpoint values are equal,
        /// an included endpoint precedes an excluded endpoint
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns>Negative integer if x's low endpoint comes before y's low endpoint, 0 if they are equal, otherwise positive</returns>
        public static int CompareLow<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
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
        /// <returns>Negative integer if x's high endpoint comes before y's high endpoint, 0 if they are equal, otherwise positive</returns>
        public static int CompareHigh<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
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
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns></returns>
        public static int CompareLowHigh<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            var compare = x.Low.CompareTo(y.High);

            // Check endpoint inclusion, if values are equal, but inclusion is different
            if (compare == 0)
                // Excluded high endpoints come before included
                return x.LowIncluded && y.HighIncluded ? 0 : 1;

            return compare;
        }

        /// <summary>
        /// Check if two intervals are equal, i.e. have the same low and high endpoint including endpoint inclusion
        /// </summary>
        /// <param name="x">First interval</param>
        /// <param name="y">Second interval</param>
        /// <returns>True if Low, High, LowIncluded, HighIncluded for both intervals are equal</returns>
        public static bool Equals<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            return x.Low.CompareTo(y.Low) == 0 && x.High.CompareTo(y.High) == 0 && x.LowIncluded == y.LowIncluded &&
                   x.HighIncluded == y.HighIncluded;

            // The same as (but faster than)
            return CompareTo(x, y) == 0;
        }

        /// <summary>
        /// Get the hashcode for an interval.
        /// Uses the low and high endpoints as well as endpoint inclusion.
        /// </summary>
        /// <param name="x">The interval</param>
        /// <typeparam name="T">The generic endpoint type</typeparam>
        /// <returns>The hashcode based on the interval</returns>
        public static int GetHashCode<T>(this IInterval<T> x) where T : IComparable<T>
        {
            // TODO: Check implement
            var hash = 17;
            hash = hash * 31 + x.Low.GetHashCode();
            hash = hash * 31 + x.High.GetHashCode();
            hash = hash * 31 + x.LowIncluded.GetHashCode();
            hash = hash * 31 + x.HighIncluded.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Create a string representing the interval.
        /// Closed intervals are represented with square brackets [a:b] and open with round brackets (a:b).
        /// </summary>
        /// <param name="x">The interval</param>
        /// <returns>The string representation</returns>
        public static string ToString<T>(this IInterval<T> x) where T : IComparable<T>
        {
            return String.Format("{0}{1}:{2}{3}",
                x.LowIncluded ? "[" : "(",
                x.Low,
                x.High,
                x.HighIncluded ? "]" : ")");
        }
    }
}