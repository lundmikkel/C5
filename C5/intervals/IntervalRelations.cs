using System;

namespace C5.intervals
{
    public class SymbolAttribute : Attribute
    {
        public string S;

        public SymbolAttribute(string s)
        {
            S = s;
        }

        public override string ToString()
        {
            return S;
        }
    }

    public static class ReflectionHelpers
    {
        public static string GetCustomDescription(object objEnum)
        {
            var fi = objEnum.GetType().GetField(objEnum.ToString());
            var attributes = (SymbolAttribute[]) fi.GetCustomAttributes(typeof(SymbolAttribute), false);
            return (attributes.Length > 0) ? attributes[0].ToString() : objEnum.ToString();
        }

        public static string Symbol(this IntervalRelation value)
        {
            return GetCustomDescription(value);
        }
    }

    /// <summary>
    /// The relationship between two intervals as described by James F. Allen in "Maintaining Knowledge about Temporal Intervals"
    /// </summary>
    public enum IntervalRelation
    {
        [Symbol(">")]
        After = 0,

        [Symbol("mi")]
        MetBy = 1,

        [Symbol("oi")]
        OverlappedBy = 2,

        [Symbol("f")]
        Finishes = 3,

        [Symbol("d")]
        During = 4,

        [Symbol("si")]
        StartedBy = 5,

        [Symbol("e")]
        Equals = 6,

        [Symbol("s")]
        Starts = 7,

        [Symbol("di")]
        Contains = 8,

        [Symbol("fi")]
        FinishedBy = 9,

        [Symbol("o")]
        Overlaps = 10,

        [Symbol("m")]
        Meets = 11,

        [Symbol("<")]
        Before = 12,
    };

    public static class IntervalRelations
    {
        /// <summary>
        /// Get the relationship between two intervals as described by James F. Allen in "Maintaining Knowledge about Temporal Intervals".
        /// The logic for points is described by Marc B. Vilain in "A System for Reasoning About Time", though terms are still Allen.
        /// </summary>
        /// <param name="x">The first interval</param>
        /// <param name="y">The second interval</param>
        /// <returns>How x relates to y</returns>
        public static IntervalRelation RelateTo<T>(this IInterval<T> x, IInterval<T> y) where T : IComparable<T>
        {
            // Check if x is before y
            var compareBefore = x.CompareHighLow(y);
            if (compareBefore < 0)
                return IntervalRelation.Before;

            // Check if x is after y
            var compareAfter = y.CompareHighLow(x);
            if (compareAfter < 0)
                return IntervalRelation.After;

            var compareLow = x.CompareLow(y);
            var compareHigh = x.CompareHigh(y);

            // Compare Low and High when dealing with points
            if (compareAfter == 0 && compareLow > 0 && compareHigh > 0)
                return IntervalRelation.MetBy;

            if (compareBefore == 0 && compareLow < 0 && compareHigh < 0)
                return IntervalRelation.Meets;

            // Convert the compared values to the integer values of the interval relations
            return (IntervalRelation) ((compareLow > 0 ? 2 : (compareLow == 0 ? 5 : 8)) + (compareHigh > 0 ? 0 : (compareHigh == 0 ? 1 : 2)));
        }
    }
}
