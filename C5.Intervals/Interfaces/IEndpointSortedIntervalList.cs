using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    [ContractClass(typeof(EndpointSortedIntervalListContract<,>))]
    public interface IEndpointSortedIntervalList<I, T> : IEnumerable<I>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        /// <summary>
        /// Get the function that determines if the interval conflicts with it neighbours.
        /// </summary>
        /// <value>The function used to determine, if an interval conflicts with its 
        /// neighbours once inserted in the sorted order.</value>
        Func<IInterval<T>, IInterval<T>, bool> ConflictFunction { get; }

        /// <summary>
        /// The value indicates the type of asymptotic complexity in terms of the indexer of
        /// this collection. This is to allow generic algorithms to alter their behaviour 
        /// for collections that provide good performance when applied to either random or
        /// sequencial access.
        /// </summary>
        /// <value>A characterization of the speed of lookup operations.</value>
        [Pure]
        Speed IndexingSpeed { get; }

        /// <summary>
        /// Get the number of intervals in the list.
        /// </summary>
        /// <value>The number of intervals in the list.</value>
        [Pure]
        int Count { get; }

        /// <summary>
        /// <para>
        /// Get the interval at index <paramref name="i"/>. First interval has index 0.
        /// </para>
        /// <para>
        /// The result is equal to <c>coll.Skip(i).First()</c>.
        /// </para>
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The <c>i</c>'th interval.</returns>
        [Pure]
        I this[int i] { get; }

        /// <summary>
        /// Get the first interval in the list.
        /// </summary>
        /// <value>The first intervals in the list.</value>
        [Pure]
        I First { get; }

        /// <summary>
        /// Get the last interval in the list.
        /// </summary>
        /// <value>The last intervals in the list.</value>
        [Pure]
        I Last { get; }

        #endregion

        #region Enumerable

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection from the
        /// interval at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index to start from.</param>
        /// <remarks>
        /// This is equal to <code>coll.Skip(index)</code>.
        /// </remarks>
        /// <returns>An enumerable, enumerating all collection intervals starting from 
        /// the given index.</returns>
        [Pure]
        IEnumerable<I> EnumerateFromIndex(int index);

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection from the
        /// interval at <paramref name="inclusiveFrom"/> and ending just before the interval
        /// at <paramref name="exclusiveTo"/>.
        /// </summary>
        /// <param name="inclusiveFrom">The index to start from.</param>
        /// <param name="exclusiveTo">The index to stop before.</param>
        /// <remarks>
        /// This is equal to
        /// <code>coll.Skip(inclusiveFrom).Take(exclusiveTo - inclusiveFrom)</code>.
        /// </remarks>
        /// <returns>An enumerable, enumerating all collection intervals starting from 
        /// <c>inclusiveFrom</c> and ending before <c>exclusiveTo</c>.</returns>
        [Pure]
        IEnumerable<I> EnumerateRange(int inclusiveFrom, int exclusiveTo);

        // TODO: Document
        [Pure]
        IEnumerable<I> EnumerateBackwardsFromIndex(int index);

        #endregion

        #region Find

        /// <summary>
        /// Find the index of the first interval that is interval equal to the query 
        /// interval.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <returns>The index of the first interval equal to the query interval. A negative
        /// number if the interval was not found, namely the one's complement of the index 
        /// at which it should have been.</returns>
        int IndexOf(IInterval<T> query);

        /// <summary>
        /// Find the index of the first interval with a high higher than the query
        /// interval's low. This is the index of the first potential overlap.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <returns>The index of the first potential overlap.</returns>
        int FindFirstOverlap(IInterval<T> query);

        /// <summary>
        /// Find the index of the first interval with a low higher than the query interval's
        /// high. This is the index of the first interval after any potential overlaps.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <returns>The index of the first interval after any potential overlaps.</returns>
        int FindLastOverlap(IInterval<T> query);

        #endregion

        #region Extensible

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        bool Add(I interval);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        bool Remove(I interval);

        /// <summary>
        /// 
        /// </summary>
        void Clear();

        #endregion
    }

    [ContractClassFor(typeof (IEndpointSortedIntervalList<,>))]
    abstract class EndpointSortedIntervalListContract<I, T> : IEndpointSortedIntervalList<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        /// <inheritdoc/>
        public Func<IInterval<T>, IInterval<T>, bool> ConflictFunction
        {
            get
            {
                Contract.Ensures(Contract.Result<Func<IInterval<T>, IInterval<T>, bool>>() != null);

                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public Speed IndexingSpeed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(!this.Any() || Contract.Result<int>() > 0);
                Contract.Ensures(this.Any() || Contract.Result<int>() == 0);
                Contract.Ensures(Enumerable.Count(this) == Contract.Result<int>());

                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public I this[int i]
        {
            get
            {
                // Requires index be in bounds
                Contract.Requires(0 <= i && i < Count);

                // If the index is correct, the output can never be null
                Contract.Ensures(Contract.Result<I>() != null);
                // Result is the same as skipping the first i elements
                Contract.Ensures(ReferenceEquals(Contract.Result<I>(), this.Skip(i).First()));

                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public I First
        {
            get
            {
                Contract.Requires(Count > 0);

                // Result is the same 
                Contract.Ensures(ReferenceEquals(Contract.Result<I>(), this[0]));

                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public I Last
        {
            get
            {
                Contract.Requires(Count > 0);

                // Result is the same as this[Count - 1]
                Contract.Ensures(ReferenceEquals(Contract.Result<I>(), this[Count - 1]));

                throw new NotImplementedException();
            }
        }

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public IEnumerable<I> EnumerateFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);

            // The enumerator is endpoint sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsEndpointSorted<I, T>());
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                this.Skip(index),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<I> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            Contract.Requires(0 <= inclusiveFrom && inclusiveFrom < Count);
            Contract.Requires(1 <= exclusiveTo && exclusiveTo <= Count);
            Contract.Requires(inclusiveFrom < exclusiveTo);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);

            // The enumerator is endpoint sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsEndpointSorted<I, T>());
            // Enumerable is the same as skipping the first inclusiveFrom intervals and then tale t
            Contract.Ensures(Enumerable.SequenceEqual(
                this.Skip(inclusiveFrom).Take(exclusiveTo - inclusiveFrom),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<I> EnumerateBackwardsFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The enumerator is endpoint sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsEndpointBackwardsSorted<I, T>());
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                this.Take(index + 1).Reverse(),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerator<I> GetEnumerator()
        {
            Contract.Ensures(Count > 0 == Contract.Result<IEnumerator<I>>().ToEnumerable().Any());

            // Result is never null
            Contract.Ensures(Contract.Result<IEnumerator<I>>() != null);

            // The enumerator is endpoint sorted
            Contract.Ensures(Contract.Result<IEnumerator<I>>().ToEnumerable().IsEndpointSorted<I, T>());

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Find

        /// <inheritdoc/>
        public int IndexOf(IInterval<T> query)
        {
            Contract.Requires(query != null);

            Contract.Ensures(Contract.Result<int>() < 0 || this[Contract.Result<int>()].CompareTo(query) == 0 && (Contract.Result<int>() - 1 < 0 || this[Contract.Result<int>() - 1].CompareTo(query) < 0));
            Contract.Ensures(Contract.Result<int>() >= 0
                || (!(0 <= ~Contract.Result<int>() - 1 && ~Contract.Result<int>() - 1 < Count) || this[~Contract.Result<int>() - 1].CompareTo(query) < 0)
                && (!(0 <= ~Contract.Result<int>() && ~Contract.Result<int>() < Count) || query.CompareTo(this[~Contract.Result<int>()]) < 0));

            Contract.Ensures(Contract.Result<int>() == IntervalCollectionContractHelper.IndexOfSorted(this, query, IntervalExtensions.CreateComparer<IInterval<T>, T>()));

            throw new NotImplementedException();
        }

        // TODO: Maybe see if contracts from IndexOf can be reused somehow
        /// <inheritdoc/>
        public int FindFirstOverlap(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() < 0 || Count <= Contract.Result<int>() || this[Contract.Result<int>()].Overlaps(query) || Contract.ForAll(0, Count, i => !this[i].Overlaps(query)));
            // All intervals before index result do not overlap the query
            Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => !this[i].Overlaps(query)));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int FindLastOverlap(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() == 0 || this[Contract.Result<int>() - 1].Overlaps(query) || Contract.ForAll(this, x => !x.Overlaps(query)));
            // All intervals after index result do not overlap the query
            Contract.Ensures(Contract.ForAll(Contract.Result<int>(), Count, i => !this[i].Overlaps(query)));

            throw new NotImplementedException();
        }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            Contract.Requires(interval != null);

            // The collection cannot be empty afterwards
            Contract.Ensures(Count > 0);

            // The collection contains the interval
            Contract.Ensures(!Contract.Result<bool>() || Contract.Exists(this, x => ReferenceEquals(x, interval)));

            // If the interval was added, the number of object with the same reference goes up by one
            Contract.Ensures(Contract.Result<bool>() == (this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval))) + 1));
            // If the interval wasn't added, the number of object with the same reference stays the same
            Contract.Ensures(Contract.Result<bool>() != (this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval)))));

            // If the interval is added the count goes up by one
            Contract.Ensures(Contract.Result<bool>() == (Count == Contract.OldValue(Count) + 1));
            // If the interval is not added the count stays the same
            Contract.Ensures(Contract.Result<bool>() != (Count == Contract.OldValue(Count)));

            Contract.Ensures(Contract.Result<bool>() == Contract.OldValue(Contract.ForAll(this, x => !ConflictFunction(x, interval))));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            Contract.Requires(interval != null);
            
            // Nothing to remove if the collection is empty
            Contract.Ensures(!Contract.OldValue(Count == 0) || !Contract.Result<bool>());

            // If the interval is removed the count goes down by one
            Contract.Ensures(Contract.Result<bool>() == (Count == Contract.OldValue(Count) - 1));
            // If the interval isn't removed the count stays the same
            Contract.Ensures(Contract.Result<bool>() != (Count == Contract.OldValue(Count)));
            // If the interval was removed, the number of object with the same reference goes down by one
            Contract.Ensures(Contract.Result<bool>() == (this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval))) - 1));
            // If the interval wasn't removed, the number of object with the same reference stays the same
            Contract.Ensures(Contract.Result<bool>() != (this.Count(x => ReferenceEquals(x, interval)) == Contract.OldValue(this.Count(x => ReferenceEquals(x, interval)))));

            // The result is true if the collection contained the interval
            Contract.Ensures(Contract.Result<bool>() == Contract.OldValue(Contract.Exists(this, x => ReferenceEquals(x, interval))));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            // The collection must be empty afterwards
            Contract.Ensures(Count == 0);

            // Enumerator is empty
            Contract.Ensures(!this.Any());

            throw new NotImplementedException();
        }

        #endregion
    }
}
