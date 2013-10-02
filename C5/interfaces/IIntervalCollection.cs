using System;

namespace C5
{

    /// <summary>
    /// A collection that allows fast overlap queries on collections of intervals.
    /// </summary>
    /// <remarks>The data structures do not support updates on its intervals' values. If you wish to change an interval's endpoints or their inclusion, the interval should be removed from the data structure first, changed and then added agian.</remarks>
    /// <typeparam name="I">The interval type in the collection. Especially used for return types for enumeration.</typeparam>
    /// <typeparam name="T">The interval's endpoint values.</typeparam>
    public interface IIntervalCollection<I, T> : ICollectionValue<I>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        /// <summary>
        /// The smallest interval that spans all intervals in the collection. The interval's low is the lowest low endpoint in the collection and the high is the highest high endpoint.
        /// <c>coll.FindOverlaps(coll.Span())</c> will by definition return all intervals in the collection.
        /// </summary>
        /// <remarks>Not defined for an empty collection.</remarks>
        /// <returns>The smallest spanning interval.</returns>
        /// <exception cref="InvalidOperationException">Thrown if called on an empty collection.</exception>
        IInterval<T> Span { get; }

        /// <summary>
        /// The maximum number of intervals overlapping at a single point in the collection.
        /// <remarks>The point of maximum overlap may not be representable with an endpoint value, as it could be between two descrete values.</remarks>
        /// </summary>
        int MaximumOverlap { get; }


        // TODO: Check seealso in documentation. Does it actually reference the other overloaded method? Add it to FindOverlap as well
        // @design: made to spare the user of making a point interval [q:q] and allow for more effective implementations of the interface for some data structures
        /// <summary>
        /// Create an enumerable, enumerating all intervals that overlap the query point.
        /// </summary>
        /// <param name="query">The query point.</param>
        /// <returns>All intervals that overlap the query point.</returns>
        /// <seealso cref="FindOverlaps(IInterval{T})"/>
        System.Collections.Generic.IEnumerable<I> FindOverlaps(T query);

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection that overlap the query interval.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <returns>All intervals that overlap the query interval.</returns>
        /// <seealso cref="FindOverlaps(T)"/>
        System.Collections.Generic.IEnumerable<I> FindOverlaps(IInterval<T> query);


        /// <summary>
        /// Check if there exists an interval that overlaps the query point.
        /// </summary>
        /// <remarks>There is no garanty of which interval will be returned, but implementations must assure that the interval returned is the fastest to retrieve.</remarks>
        /// <param name="query">The query point.</param>
        /// <param name="overlap">The overlapping interval found, if return is true.</param>
        /// <returns>True if an interval overlapped the query.</returns>
        bool FindOverlap(T query, ref I overlap);

        /// <summary>
        /// Check if there exists an interval that overlaps the query interval.
        /// </summary>
        /// <remarks>There is no garanty of which interval will be returned, but implementations must assure that the interval returned is the fastest to retrieve.</remarks>
        /// <param name="query">The query interval.</param>
        /// <param name="overlap">The overlapping interval found, if return is true.</param>
        /// <returns>True if an interval overlapped the query.</returns>
        bool FindOverlap(IInterval<T> query, ref I overlap);


        /// <summary>
        /// Count the number of intervals overlapping the query interval.
        /// Beware that not all data structure support this operation any faster than FindOverlaps.Count().
        /// </summary>
        /// <param name="query">The query interval</param>
        /// <returns>The number of intervals that overlap the query</returns>
        int CountOverlaps(IInterval<T> query);


        /// <summary>
        /// Add an interval to the collection.
        /// </summary>
        /// <remarks>Different implementations may handle duplicates differently.</remarks>
        /// <param name="interval">The interval to add.</param>
        /// <returns>True if the interval was added.</returns>
        bool Add(I interval);

        /// <summary>
        /// Remove an interval from the collection.
        /// </summary>
        /// <remarks>Different implementations may remove duplicates differently.</remarks>
        /// <param name="interval">The interval to remove.</param>
        /// <returns>True if the interval was removed.</returns>
        bool Remove(I interval);
    }
}
