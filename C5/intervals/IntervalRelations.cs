using System;
using System.Diagnostics.Contracts;

namespace C5.intervals
{
    /// <summary>
    /// The relationship between two intervals as described by James F. Allen in "Maintaining Knowledge about Temporal Intervals"
    /// </summary>
    public enum IntervalRelation
    {
        /// <summary>
        /// The interval is after another interval
        /// </summary>
        [Symbol(">")]
        After = 0,

        /// <summary>
        /// The interval is met by another interval thereby sharing an endpoint
        /// </summary>
        [Symbol("mi")]
        MetBy = 1,

        /// <summary>
        /// The interval is overlaped by another interval but share no endpoint
        /// </summary>
        [Symbol("oi")]
        OverlappedBy = 2,

        /// <summary>
        /// The interval finishes another interval thereby sharing the high endpoint
        /// </summary>
        [Symbol("f")]
        Finishes = 3,

        /// <summary>
        /// The interval is during another interval
        /// </summary>
        [Symbol("d")]
        During = 4,

        /// <summary>
        /// The interval is started by another interval thereby sharing the low endpoint
        /// </summary>
        [Symbol("si")]
        StartedBy = 5,

        /// <summary>
        /// The interval is equal to another interval thereby sharing both endpoints
        /// </summary>
        [Symbol("e")]
        Equals = 6,

        /// <summary>
        /// The interval is starts another interval thereby sharing the low endpoint
        /// </summary>
        [Symbol("s")]
        Starts = 7,

        /// <summary>
        /// The interval is contained in another interval
        /// </summary>
        [Symbol("di")]
        Contains = 8,

        /// <summary>
        /// The interval finished by another interval thereby sharing the high endpoint
        /// </summary>
        [Symbol("fi")]
        FinishedBy = 9,

        /// <summary>
        /// The interval overlaps another interval but share no endpoint
        /// </summary>
        [Symbol("o")]
        Overlaps = 10,

        /// <summary>
        /// The interval meets another interval thereby sharing an endpoint
        /// </summary>
        [Symbol("m")]
        Meets = 11,

        /// <summary>
        /// The interval is before another interval
        /// </summary>
        [Symbol("<")]
        Before = 12,
    };

    /// <summary>
    /// An extension class for finding the relation between two intervals
    /// </summary>
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
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.After ||
                y.CompareLow(x) < 0 && y.CompareHighLow(x) < 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.MetBy ||
                y.CompareLow(x) < 0 && y.CompareHighLow(x) == 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.OverlappedBy ||
                y.CompareLow(x) < 0 && y.CompareHighLow(x) > 0 && y.CompareHigh(x) < 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Finishes ||
                y.CompareLow(x) < 0 && y.CompareHighLow(x) >= 0 && y.CompareHigh(x) == 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.During ||
                y.CompareLow(x) < 0 && y.CompareHighLow(x) > 0 && y.CompareHigh(x) > 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.StartedBy ||
                y.CompareLow(x) == 0 && y.CompareHighLow(x) >= 0 && y.CompareHigh(x) < 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Equals ||
                y.CompareLow(x) == 0 && y.CompareHigh(x) == 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Starts ||
                y.CompareLow(x) == 0 && y.CompareHigh(x) > 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Contains ||
                y.CompareLow(x) > 0 && y.CompareHigh(x) < 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.FinishedBy ||
                y.CompareLow(x) > 0 && y.CompareLowHigh(x) <= 0 && y.CompareHigh(x) == 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Overlaps ||
                y.CompareLow(x) > 0 && y.CompareHigh(x) > 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Meets ||
                y.CompareLow(x) > 0 && y.CompareHigh(x) > 0);
            Contract.Ensures(Contract.Result<IntervalRelation>() != IntervalRelation.Before ||
                y.CompareLow(x) > 0 && y.CompareHigh(x) > 0);


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

    /// <summary>
    /// Attribute class for the symbol annotation for interval relations
    /// </summary>
    public class SymbolAttribute : Attribute
    {
        /// <summary>
        /// The symbol name
        /// </summary>
        public string S;

        /// <summary>
        /// Create a symbol with the given string
        /// </summary>
        /// <param name="s">The symbol name.</param>
        public SymbolAttribute(string s)
        {
            Contract.Requires(s != null);

            S = s;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return S;
        }
    }

    /// <summary>
    /// A reflection helper class to get the symbol name for an interval relation.
    /// </summary>
    public static class ReflectionHelpers
    {
        /// <summary>
        /// Get the symbol name for a interval relation
        /// </summary>
        /// <param name="objEnum"></param>
        /// <returns></returns>
        public static string GetCustomDescription(object objEnum)
        {
            var attributes = (SymbolAttribute[]) objEnum.GetType().GetField(objEnum.ToString()).GetCustomAttributes(typeof(SymbolAttribute), false);
            return (attributes.Length > 0) ? attributes[0].ToString() : objEnum.ToString();
        }

        /// <summary>
        /// Get the symbol name for an interval relation.
        /// </summary>
        /// <param name="value">The interval relation.</param>
        /// <returns>A string representation of an interval relation.</returns>
        public static string Symbol(this IntervalRelation value)
        {
            return GetCustomDescription(value);
        }
    }
}
