using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using C5.intervals;

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
        #region Properties

        /// <summary>
        /// The smallest interval that spans all intervals in the collection. The interval's low is the lowest low endpoint in the collection and the high is the highest high endpoint.
        /// <c>coll.FindOverlaps(coll.Span())</c> will by definition return all intervals in the collection.
        /// </summary>
        /// <remarks>Not defined for an empty collection.</remarks>
        /// <returns>The smallest spanning interval.</returns>
        /// <exception cref="InvalidOperationException">Thrown if called on an empty collection.</exception>
        [Pure]
        IInterval<T> Span { get; }

        /// <summary>
        /// The maximum number of intervals overlapping at a single point in the collection.
        /// <remarks>The point of maximum overlap may not be representable with an endpoint value, as it could be between two descrete values.</remarks>
        /// </summary>
        [Pure]
        int MaximumOverlap { get; }

        #endregion

        #region Find Overlaps

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
        [Pure]
        IEnumerable<I> FindOverlaps(IInterval<T> query);

        #endregion

        #region Find Overlap

        /// <summary>
        /// Check if there exists an interval that overlaps the query point.
        /// </summary>
        /// <remarks>There is no garanty of which interval will be returned, but implementations must assure that the interval returned is the fastest to retrieve.</remarks>
        /// <param name="query">The query point.</param>
        /// <param name="overlap">The overlapping interval found, if return is true.</param>
        /// <returns>True if an interval overlapped the query.</returns>
        [Pure]
        bool FindOverlap(T query, ref I overlap);

        /// <summary>
        /// Check if there exists an interval that overlaps the query interval.
        /// </summary>
        /// <remarks>There is no garanty of which interval will be returned, but implementations must assure that the interval returned is the fastest to retrieve.</remarks>
        /// <param name="query">The query interval.</param>
        /// <param name="overlap">The overlapping interval found, if return is true.</param>
        /// <returns>True if an interval overlapped the query.</returns>
        [Pure]
        bool FindOverlap(IInterval<T> query, ref I overlap);

        #endregion

        #region Count Overlaps

        /// <summary>
        /// Count the number of intervals overlapping the query point.
        /// </summary>
        /// <param name="query">The query interval</param>
        /// <remarks>Beware that not all data structure support this operation any faster than <c>FindOverlaps.Count()</c>!</remarks>
        /// <returns>The number of intervals that overlap the query point.</returns>
        [Pure]
        int CountOverlaps(T query);

        /// <summary>
        /// Count the number of intervals overlapping the query interval.
        /// Beware that not all data structure support this operation any faster than FindOverlaps.Count().
        /// </summary>
        /// <param name="query">The query interval</param>
        /// <returns>The number of intervals that overlap the query</returns>
        [Pure]
        int CountOverlaps(IInterval<T> query);

        #endregion

        #region Extensible

        /// <summary>
        /// If true any call of an updating operation will throw an
        /// <code>ReadOnlyCollectionException</code>
        /// </summary>
        /// <value>True if this collection is read-only.</value>
        [Pure]
        bool IsReadOnly { get; }

        /// <summary>
        /// Add an interval to the collection.
        /// </summary>
        /// <remarks>Different implementations may handle duplicates differently.</remarks>
        /// <param name="interval">The interval to add.</param>
        /// <returns>True if the interval was added.</returns>
        bool Add(I interval);

        /// <summary>
        /// Add a collection of intervals to the collection.
        /// </summary>
        /// <remarks>Different implementations may handle duplicates differently.</remarks>
        /// <param name="intervals">The intervals to add.</param>
        void AddAll(IEnumerable<I> intervals);

        /// <summary>
        /// Remove an interval from the collection.
        /// </summary>
        /// <remarks>Different implementations may remove duplicates differently.</remarks>
        /// <param name="interval">The interval to remove.</param>
        /// <returns>True if the interval was removed.</returns>
        bool Remove(I interval);

        /// <summary>
        /// Remove all intervals from the collection.
        /// </summary>
        void Clear();

        #endregion
    }

    [ContractClassFor(typeof(IIntervalCollection<,>))]
    abstract class IntervalCollectionContract<I, T> : IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        public IInterval<T> Span
        {
            get
            {
                // Span contains all intervals
                Contract.Ensures(IsEmpty || Contract.ForAll(this, i => Contract.Result<IInterval<T>>().Contains(i)));
                // Span has the lowest and highest endpoint of the collection
                Contract.Ensures(IsEmpty || Contract.ForAll(this, i => Contract.Result<IInterval<T>>().CompareLow(i) <= 0 && Contract.Result<IInterval<T>>().CompareHigh(i) >= 0));
                // There is an interval that has the same low as span
                Contract.Ensures(IsEmpty || Contract.Exists(this, i => Contract.Result<IInterval<T>>().CompareLow(i) == 0));
                // There is an interval that has the same high as span
                Contract.Ensures(IsEmpty || Contract.Exists(this, i => Contract.Result<IInterval<T>>().CompareHigh(i) == 0));

                throw new NotImplementedException();
            }
        }

        public int MaximumOverlap
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                //Contract.Ensures(Contract.ForAll(this, i => CountOverlaps(i.Low) <= Contract.Result<int>() && CountOverlaps(i.High) <= Contract.Result<int>()));

                throw new NotImplementedException();
            }
        }

        #endregion

        #region Find Overlaps

        public IEnumerable<I> FindOverlaps(T query)
        {
            Contract.Requires(!ReferenceEquals(query, null));
            // All intervals in collect that overlap query must be in the result
            Contract.Ensures(Contract.ForAll(this.Where(i => i.Overlaps(query)), i => Contract.Result<IEnumerable<I>>().Any(j => ReferenceEquals(i, j))));
            // All intervals in the collection that do not overlap cannot by in the result
            Contract.Ensures(Contract.ForAll(this.Where(i => !i.Overlaps(query)), i => Contract.ForAll(Contract.Result<IEnumerable<I>>(), j => !ReferenceEquals(i, j))));
            // The number of intervals in the collection that overlap the query must be equal to the result size
            Contract.Ensures(this.Count(i => i.Overlaps(query)) == Contract.Result<IEnumerable<I>>().Count());

            throw new NotImplementedException();
        }

        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            Contract.Requires(query != null);
            // All intervals in collect that overlap query must be in the result
            Contract.Ensures(Contract.ForAll(this.Where(i => i.Overlaps(query)), i => Contract.Result<IEnumerable<I>>().Any(j => ReferenceEquals(i, j))));
            // All intervals in the collection that do not overlap cannot by in the result
            Contract.Ensures(Contract.ForAll(this.Where(i => !i.Overlaps(query)), i => Contract.ForAll(Contract.Result<IEnumerable<I>>(), j => !ReferenceEquals(i, j))));
            // The number of intervals in the collection that overlap the query must be equal to the result size
            Contract.Ensures(this.Count(i => i.Overlaps(query)) == Contract.Result<IEnumerable<I>>().Count());

            throw new NotImplementedException();
        }

        #endregion

        #region Find Overlap

        public bool FindOverlap(T query, ref I overlap)
        {
            Contract.Requires(!ReferenceEquals(query, null));
            // A found overlap is not null and overlaps query
            Contract.Ensures(!Contract.Result<bool>() || !ReferenceEquals(overlap, null) && overlap.Overlaps(query));

            throw new NotImplementedException();
        }

        public bool FindOverlap(IInterval<T> query, ref I overlap)
        {
            Contract.Requires(query != null);
            // A found overlap is not null and overlaps query
            Contract.Ensures(!Contract.Result<bool>() || !ReferenceEquals(overlap, null) && overlap.Overlaps(query));

            throw new NotImplementedException();
        }

        #endregion

        #region Count Overlaps

        public int CountOverlaps(T query)
        {
            Contract.Requires(!ReferenceEquals(query, null));
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures(Contract.Result<int>() == this.Count(i => i.Overlaps(query)));

            throw new NotImplementedException();
        }

        public int CountOverlaps(IInterval<T> query)
        {
            Contract.Requires(!ReferenceEquals(query, null));
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures(Contract.Result<int>() == this.Count(i => i.Overlaps(query)));

            throw new NotImplementedException();
        }

        #endregion

        #region Extensible

        public abstract bool IsReadOnly { get; }

        public bool Add(I interval)
        {
            Contract.Requires(!ReferenceEquals(interval, null));
            // If the interval is added the count goes up by one
            Contract.Ensures(!Contract.Result<bool>() || Count == Contract.OldValue(Count) + 1);
            Contract.Ensures(Contract.Result<bool>() != Contract.OldValue(this.Any(i => ReferenceEquals(i, interval))));
            // The collection contains the interval
            Contract.Ensures(this.Any(i => ReferenceEquals(i, interval)));

            throw new NotImplementedException();
        }

        public void AddAll(IEnumerable<I> intervals)
        {
            Contract.Requires(intervals != null);
            // The collection contains all intervals
            Contract.Ensures(Contract.ForAll(intervals, i => this.Any(j => ReferenceEquals(i, j))));
        }

        public bool Remove(I interval)
        {
            Contract.Requires(!ReferenceEquals(interval, null));
            // If the interval is removed the count goes down by one
            Contract.Ensures(!Contract.Result<bool>() || Count == Contract.OldValue(Count) - 1);
            Contract.Ensures(Contract.Result<bool>() == Contract.OldValue(this.Any(i => ReferenceEquals(i, interval))));
            // The collection contains the interval
            Contract.Ensures(this.Any(i => !ReferenceEquals(i, interval)));

            throw new NotImplementedException();
        }

        public void Clear()
        {
            Contract.Ensures(IsEmpty);
            Contract.Ensures(Count == 0);

            throw new NotImplementedException();
        }

        #endregion

        #region Non-interval methods

        public abstract IEnumerator<I> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
        public abstract bool IsEmpty { get; }
        public abstract int Count { get; }
        public abstract I Choose();
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
