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
        /// <param name="i">First interval</param>
        /// <param name="j">Second interval</param>
        /// <returns>True if the intervals overlap, otherwise false</returns>
        public static bool Overlaps<T>(this IInterval<T> i, IInterval<T> j) where T : IComparable<T>
        {
            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int ijCompare = i.Low.CompareTo(j.High), jiCompare = j.Low.CompareTo(i.High);
            return (ijCompare < 0 || ijCompare == 0 && i.LowIncluded && j.HighIncluded)
                && (jiCompare < 0 || jiCompare == 0 && j.LowIncluded && i.HighIncluded);
        }

        /// <summary>
        /// Compare an interval with a point interval to see if they overlap
        /// </summary>
        /// <param name="i">Interval</param>
        /// <param name="j">Point interval</param>
        /// <returns>True if the intervals overlap, otherwise false</returns>
        public static bool Overlaps<T>(this IInterval<T> i, T j) where T : IComparable<T>
        {
            return Overlaps(i, new IntervalBase<T>(j));
        }

        /// <summary>
        /// Check if one interval contains another interval.
        /// i contains j if i.Low is lower than j.Low and j.High is lower than i.High.
        /// If endpoints are equal endpoint inclusion is also taken into account.
        /// </summary>
        /// <param name="i">Container interval</param>
        /// <param name="j">Contained interval</param>
        /// <returns>True if j is contained in i</returns>
        public static bool Contains<T>(this IInterval<T> i, IInterval<T> j) where T : IComparable<T>
        {
            // Save compare values to avoid comparing twice in case CompareTo() should be expensive
            int lowCompare = i.Low.CompareTo(j.Low), highCompare = j.High.CompareTo(i.High);
            return
                // Low
                (lowCompare < 0 || (lowCompare == 0 && i.LowIncluded && !j.LowIncluded))
                // High
                && (highCompare < 0 || (highCompare == 0 && !j.HighIncluded && i.HighIncluded));
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
        /// <param name="i">First interval</param>
        /// <param name="j">Second interval</param>
        /// <returns>Negative integer if i comes before j, 0 if they are equal, positive integer if i comes after j</returns>
        public static int CompareTo<T>(this IInterval<T> i, IInterval<T> j) where T : IComparable<T>
        {
            var lowCompare = i.Low.CompareTo(j.Low);

            // Check if i starts j - their lows are the same
            if (lowCompare == 0)
            {
                // If both include or exclude their low endpoint, we don't care which one is first
                if (i.LowIncluded == j.LowIncluded)
                {
                    return CompareHigh(i, j);
                }

                // i.LowIncluded and j.LowIncluded are different
                // So if i.LowIncluded is true it comes before j
                return i.LowIncluded ? -1 : 1;
            }

            // We can simply compare their low points
            return lowCompare;
        }

        public static int CompareHigh<T>(this IInterval<T> i, IInterval<T> j) where T : IComparable<T>
        {
            var highCompare = i.High.CompareTo(j.High);

            // Check if i finishes j - their highs are the same
            if (highCompare == 0)
            {
                // If both include or exclude the high endpoint, we don't care which one is first
                if (i.HighIncluded == j.HighIncluded)
                    return 0;

                // i.HighIncluded and j.HighIncluded are different
                // So if i.HighIncluded is true it comes before j in the sorting order - we don't know anything about their actual relation
                return !i.HighIncluded ? -1 : 1;
            }

            // We can simply compare their high points - but we swap the order as the highest endpoint has the lowest sorting order
            return highCompare;
        }

        /// <summary>
        /// Check if two intervals are equal, i.e. have the same low and high endpoint including endpoint inclusion
        /// </summary>
        /// <param name="i">First interval</param>
        /// <param name="j">Second interval</param>
        /// <returns>True if Low, High, LowIncluded, HighIncluded for both intervals are equal</returns>
        public static bool Equals<T>(this IInterval<T> i, IInterval<T> j) where T : IComparable<T>
        {
            return CompareTo(i, j) == 0;
        }

        /// <summary>
        /// Get the hashcode for an interval.
        /// Uses the low and high endpoints as well as endpoint inclusion.
        /// </summary>
        /// <param name="i">The interval</param>
        /// <typeparam name="T">The generic endpoint type</typeparam>
        /// <returns>The hashcode based on the interval</returns>
        public static int GetHashCode<T>(this IInterval<T> i) where T : IComparable<T>
        {
            // TODO: Check implement
            var hash = 17;
            hash = hash * 31 + i.Low.GetHashCode();
            hash = hash * 31 + i.High.GetHashCode();
            hash = hash * 31 + i.LowIncluded.GetHashCode();
            hash = hash * 31 + i.HighIncluded.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Create a string representing the interval.
        /// Closed intervals are represented with square brackets [a:b] and open with round brackets (a:b).
        /// </summary>
        /// <param name="i">The interval</param>
        /// <returns>The string representation</returns>
        public static string ToString<T>(this IInterval<T> i) where T : IComparable<T>
        {
            return String.Format("{0}{1}:{2}{3}",
                i.LowIncluded ? "[" : "(",
                i.Low,
                i.High,
                i.HighIncluded ? "]" : ")");
        }
    }
}