using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace C5
{

    /// <summary>
    /// A collection that allows fast overlap queries on collections of intervals.
    /// </summary>
    /// <remarks>The data structures do not support updates on its intervals' values. If you wish to change an interval's endpoints or their inclusion, the interval should be removed from the data structure first, changed and then added agian.</remarks>
    /// <typeparam name="I">The interval type in the collection. Especially used for return types for enumeration.</typeparam>
    /// <typeparam name="T">The interval's endpoint values.</typeparam>
    [ContractClass(typeof(IntervalCollectionContract<,>))]
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
        IEnumerable<I> FindOverlaps(T query);

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection that overlap the query interval.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <returns>All intervals that overlap the query interval.</returns>
        /// <seealso cref="FindOverlaps(T)"/>
        IEnumerable<I> FindOverlaps(IInterval<T> query);


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

    [ContractClassFor(typeof(IIntervalCollection<,>))]
    abstract class IntervalCollectionContract<I, T> : IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        public abstract IEnumerator<I> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public abstract bool IsEmpty { get; }
        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public abstract I Choose();

        public abstract IInterval<T> Span { get; }
        public abstract int MaximumOverlap { get; }

        public abstract IEnumerable<I> FindOverlaps(T query);
        public abstract IEnumerable<I> FindOverlaps(IInterval<T> query);
        public abstract bool FindOverlap(T query, ref I overlap);
        public abstract bool FindOverlap(IInterval<T> query, ref I overlap);

        public int CountOverlaps(IInterval<T> query)
        {
            Contract.Requires(query != null);

            throw new NotImplementedException();
        }

        public abstract bool Add(I interval);
        public abstract bool Remove(I interval);

        #region Non-interval methods

        public abstract string ToString(string format, IFormatProvider formatProvider);
        public abstract bool Show(StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider);
        public abstract EventTypeEnum ListenableEvents { get; }
        public abstract EventTypeEnum ActiveEvents { get; }
        public abstract event CollectionChangedHandler<I> CollectionChanged;
        public abstract event CollectionClearedHandler<I> CollectionCleared;
        public abstract event ItemsAddedHandler<I> ItemsAdded;
        public abstract event ItemInsertedHandler<I> ItemInserted;
        public abstract event ItemsRemovedHandler<I> ItemsRemoved;
        public abstract event ItemRemovedAtHandler<I> ItemRemovedAt;
        public abstract Speed CountSpeed { get; }
        public abstract void CopyTo(I[] array, int index);
        public abstract I[] ToArray();
        public abstract void Apply(Action<I> action);
        public abstract bool Exists(Func<I, bool> predicate);
        public abstract bool Find(Func<I, bool> predicate, out I item);
        public abstract bool All(Func<I, bool> predicate);
        public abstract IEnumerable<I> Filter(Func<I, bool> filter);

        #endregion
    }
}
