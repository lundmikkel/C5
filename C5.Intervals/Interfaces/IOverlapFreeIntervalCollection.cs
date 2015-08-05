using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    // TODO: Split into something like IIntervalCollectionWithoutOverlaps and IOverlapFreeIntervalCollection. Maybe also Sorted and Indexed.
    /// <summary>
    /// An interval collection where intervals do not overlap
    /// and where they are sorted based on lowest low, then lowest high endpoint.
    /// All enumerators are be lazy.
    /// </summary>
    /// <typeparam name="I">The interval type in the collection. Especially used for return types for enumeration.</typeparam>
    /// <typeparam name="T">The interval's endpoint values.</typeparam>
    [ContractClass(typeof(OverlapFreeIntervalCollectionContract<,>))]
    public interface IOverlapFreeIntervalCollection<I, T> : IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        #region Data Structure Properties

        /// <summary>
        /// The value indicates the type of asymptotic complexity in terms of the indexer of
        /// this collection. This is to allow generic algorithms to alter their behaviour 
        /// for collections that provide good performance when applied to either random or
        /// sequencial access.
        /// </summary>
        /// <value>A characterization of the speed of lookup operations.</value>
        [Pure]
        Speed IndexingSpeed { get; }

        #endregion Data Structure Properties

        #endregion Properties

        #region Sorted Enumeration

        // TODO: Use IDirectedEnumerable?
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection sorted by lowest low, then lowest high endpoint.
        /// </summary>
        /// <remarks>
        /// The result is equal to a stable sorting of the collection using <see cref="IntervalExtensions.CompareTo{T}"/>.
        /// </remarks>
        /// <returns>An enumerable, enumerating all collection intervals in sorted order.</returns>
        [Pure]
        IEnumerable<I> Sorted();

        // TODO: Review documentation
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection sorted by highest high, then highest low.
        /// </summary>
        /// <remarks>
        /// The result is equal to <code>Sorted().Reverse()</code>.
        /// </remarks>
        /// <returns>An enumerable, enumerating the collection backwards in sorted order.</returns>
        [Pure]
        IEnumerable<I> SortedBackwards();

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the first interval that overlaps or follows <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// None of the intervals are guaranteed to overlap <paramref name="point"/>.
        /// This is equal to <code>Sorted().SkipWhile(x => x.CompareHigh(point) &lt; 0)</code>.
        /// </remarks>
        /// <param name="point">The comparable point from which to start the enumeration.</param>
        /// <param name="includeOverlaps">If false, any overlap is excluded.
        /// This is equal to <code>Sorted().SkipWhile(x => x.CompareLow(point) &lt;= 0)</code>.</param>
        /// <returns>An enumerable, enumerating all collection intervals not less than the point in sorted order.</returns>
        [Pure]
        IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true);

        // TODO: Review documentation
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection backwards in sorted order
        /// starting from the first interval that overlaps or comes before <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// None of the intervals are guaranteed to overlap <paramref name="point"/>.
        /// This is equal to <code>SortedBackwards().SkipWhile(x => x.CompareLow(point) &gt; 0)</code>.
        /// </remarks>
        /// <param name="point">The comparable point from which to start the enumeration.</param>
        /// <param name="includeOverlaps">If false, any overlap is excluded.
        /// This is equal to <code>SortedBackwards().SkipWhile(x => x.CompareHigh(point) &gt;= 0)</code>.</param>
        /// <returns>An enumerable, enumerating backwards all collection intervals not greater than the point in sorted order.</returns>
        [Pure]
        IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true);

        // TODO: Consider if this should be based on interval or reference equality
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the given interval object <paramref name="interval"/>.
        /// 
        /// If the interval object is not in the collection, the result will be empty.
        /// </summary>
        /// <remarks>
        /// This works on reference equality and not interval equality. If <see cref="IIntervalCollection{I,T}.AllowsReferenceDuplicates"/>
        /// is true, the enumeration will start from the first reference duplicate.
        /// This is equal to <code>Sorted().SkipWhile(x => !ReferenceEqual(interval, x))</code>.
        /// </remarks>
        /// <param name="interval">The interval object to locate in the collection.</param>
        /// <param name="includeInterval">If false, the interval object is excluded from the enumerator. This is equal to
        /// <code>Sorted().SkipWhile(x => !ReferenceEqual(interval, x)).SkipWhile(x => ReferenceEqual(interval, x))</code>.</param>
        /// <returns>An enumerable, enumerating all collection intervals starting from the given object.</returns>
        [Pure]
        IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true);

        // TODO: Document
        [Pure]
        IEnumerable<I> EnumerateBackwardsFrom(I interval, bool includeInterval = true);

        // TODO: Compare to definition of Skip, which returns the same collection for index <= 0 and an empty collection for index > count
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection in sorted order
        /// starting from the interval at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index to start from.</param>
        /// <remarks>
        /// This is equal to <code>Sorted().Skip(x)</code>.
        /// </remarks>
        /// <returns>An enumerable, enumerating all collection intervals starting from the given index.</returns>
        [Pure]
        IEnumerable<I> EnumerateFromIndex(int index);

        // TODO: Document
        [Pure]
        IEnumerable<I> EnumerateBackwardsFromIndex(int index);

        #endregion

        #region Indexed Access

        // TODO: Should this have a conditional flag as well? includeIndex
        // TODO: What is the behaviour when index is out of bounds? Think about LINQ and its Skip
        // TODO: Should this rather return a boolean value and have the index as an out?
        /// <summary>
        /// Determines the index of the interval object in the collection.
        /// </summary>
        /// <remarks>
        /// This works on reference equality and not interval equality.
        /// If the collection allows reference duplicates, the index of the first object will be returned.
        /// </remarks>
        /// <param name="interval">The interval object to locate in the collection.</param>
        /// <returns>The index of the interval object. A negative number if item not found,
        /// namely the one's complement of the index at which it should have been.</returns>
        [Pure]
        int IndexOf(I interval);

        /// <summary>
        /// Get the interval at index <paramref name="i"/>. First interval has index 0.
        /// 
        /// The result is equal to <code>Sorted().Skip(i).First()</code>.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The <code>i</code>'th interval.</returns>
        [Pure]
        I this[int i] { get; }

        #endregion Indexed Access

        #region Neighbourhood

        Neighbourhood<I, T> GetNeighbourhood(T query);

        Neighbourhood<I, T> GetNeighbourhood(I query);

        #endregion
    }

    public class Neighbourhood<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        public readonly I Previous;
        public readonly I Overlap;
        public readonly I Next;

        public Neighbourhood() { }

        public Neighbourhood(I previous, I overlap, I next)
        {
            Previous = previous;
            Overlap = overlap;
            Next = next;
        }

        public bool IsEmpty
        {
            get
            {
                return Previous == null && Overlap == null && Next == null;
            }
        }
    }

    [ContractClassFor(typeof(IOverlapFreeIntervalCollection<,>))]
    internal abstract class OverlapFreeIntervalCollectionContract<I, T> : IntervalCollectionBase<I, T>, IOverlapFreeIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        #region Data Structure Properties

        public abstract Speed IndexingSpeed { get; }

        #endregion Data Structure Properties

        #endregion Properties

        #region Sorted Enumeration

        public IEnumerable<I> Sorted()
        {
            Contract.Ensures(IsEmpty != Contract.Result<IEnumerable<I>>().Any());

            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // The enumerator is equal to the normal enumerator
            Contract.Ensures(Enumerable.SequenceEqual(
                this,
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>())
            );

            throw new NotImplementedException();
        }

        public IEnumerable<I> SortedBackwards()
        {
            Contract.Ensures(IsEmpty != Contract.Result<IEnumerable<I>>().Any());

            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // The backwards enumerator is equal to the sorted reversed
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted().Reverse(),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>())
            );

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true)
        {
            Contract.Requires(!ReferenceEquals(point, null));

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping as long as high is lower than point
            Contract.Ensures(Enumerable.SequenceEqual(
                includeOverlaps
                    ? Sorted().SkipWhile(x => x.CompareHigh(point) < 0)
                    : Sorted().SkipWhile(x => x.CompareLow(point) <= 0),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true)
        {
            Contract.Requires(!ReferenceEquals(point, null));

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping as long as high is lower than point
            Contract.Ensures(Enumerable.SequenceEqual(
                includeOverlaps
                    ? SortedBackwards().SkipWhile(x => x.CompareLow(point) > 0)
                    : SortedBackwards().SkipWhile(x => x.CompareHigh(point) >= 0),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);

            // If the collection doesn't contain the interval, the result is empty
            Contract.Ensures(this.Contains(interval) || !Contract.Result<IEnumerable<I>>().Any());
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping until interval is met
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted().SkipWhile(x => !ReferenceEquals(x, interval)).SkipWhile(x => !includeInterval && ReferenceEquals(x, interval)),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateBackwardsFrom(I interval, bool includeInterval = true)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // If the collection doesn't contain the interval, the result is empty
            Contract.Ensures(this.Contains(interval) || !Contract.Result<IEnumerable<I>>().Any());
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping until interval is met
            Contract.Ensures(Enumerable.SequenceEqual(
                SortedBackwards().SkipWhile(x => !ReferenceEquals(x, interval)).SkipWhile(x => !includeInterval && ReferenceEquals(x, interval)),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted().Skip(index),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<I> EnumerateBackwardsFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<I>>() != null);
            // The intervals are sorted
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I, T>());
            // Highs are sorted as well
            Contract.Ensures(Contract.Result<IEnumerable<I>>().Reverse().IsSorted<I>(IntervalExtensions.CreateHighComparer<I, T>()));
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                Sorted().Take(index + 1).Reverse(),
                Contract.Result<IEnumerable<I>>(),
                IntervalExtensions.CreateReferenceEqualityComparer<I, T>()
            ));

            throw new NotImplementedException();
        }

        #endregion

        #region Indexed Access
        public int IndexOf(I interval)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<int>() == Sorted().IndexOfSorted(interval, IntervalExtensions.CreateComparer<I, T>()));
            Contract.Ensures(!this.Any(x => ReferenceEquals(interval, x)) || 0 <= Contract.Result<int>() && Contract.Result<int>() < Count);
            Contract.Ensures(this.Any(x => ReferenceEquals(interval, x)) || 0 > Contract.Result<int>() && Contract.Result<int>() >= ~Count);

            throw new NotImplementedException();
        }

        public I this[int i]
        {
            get
            {
                // Requires collection is not empty
                Contract.Requires(!IsEmpty);
                // Requires index be in bounds
                Contract.Requires(0 <= i && i < Count);

                // If the index is correct, the output can never be null
                Contract.Ensures(Contract.Result<I>() != null);
                // Result is the same as skipping the first i elements
                Contract.Ensures(ReferenceEquals(Contract.Result<I>(), Sorted().Skip(i).First()));

                throw new NotImplementedException();
            }
        }

        #endregion

        #region Neighbourhood

        public Neighbourhood<I, T> GetNeighbourhood(T query)
        {
            // Query cannot be null
            Contract.Requires(!ReferenceEquals(query, null));

            // Result is never null
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>() != null);

            // The previous interval comes before the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Previous == null || Contract.Result<Neighbourhood<I, T>>().Previous.CompareHigh(query) < 0);
            // If there is an overlap, it must overlap the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Overlap == null || Contract.Result<Neighbourhood<I, T>>().Overlap.Overlaps(query));
            // The next interval comes after the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Next == null || Contract.Result<Neighbourhood<I, T>>().Next.CompareLow(query) > 0);

            // Previous is the last interval before the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Previous == null ||
                Contract.ForAll(
                    this.EnumerateBackwardsFrom(query, false),
                    interval => interval.CompareHighLow(Contract.Result<Neighbourhood<I, T>>().Previous) < 0 || ReferenceEquals(interval, Contract.Result<Neighbourhood<I, T>>().Previous)
                )
            );
            // Overlap is set, if there is an overlap
            Contract.Ensures((Contract.Result<Neighbourhood<I, T>>().Overlap != null) == IntervalCollectionContractHelper.GetReturnValue(() => { I overlap; return FindOverlap(query, out overlap); }));
            // Next is the first interval after the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Next == null ||
                Contract.ForAll(
                    this.EnumerateFrom(query, false),
                    interval => Contract.Result<Neighbourhood<I, T>>().Next.CompareHighLow(interval) < 0 || ReferenceEquals(interval, Contract.Result<Neighbourhood<I, T>>().Next)
                )
            );

            throw new NotImplementedException();
        }

        public Neighbourhood<I, T> GetNeighbourhood(I query)
        {
            // Query cannot be null
            Contract.Requires(query != null);

            // Result is never null
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>() != null);

            // If the collection doesn't contain the interval, the result is empty
            Contract.Ensures(this.Contains(query) || Contract.Result<Neighbourhood<I, T>>().Previous == null && Contract.Result<Neighbourhood<I, T>>().Overlap == null && Contract.Result<Neighbourhood<I, T>>().Next == null);
            // The previous interval comes before the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Previous == null || Contract.Result<Neighbourhood<I, T>>().Previous.CompareHighLow(query) < 0);
            // If there is an overlap, it must overlap the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Overlap == null || ReferenceEquals(Contract.Result<Neighbourhood<I, T>>().Overlap, query));
            // The next interval comes after the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Next == null || query.CompareHighLow(Contract.Result<Neighbourhood<I, T>>().Next) < 0);


            // Previous is the last interval before the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Previous == null ||
                Contract.ForAll(
                    this.EnumerateBackwardsFrom(query, false),
                    interval => interval.CompareHighLow(Contract.Result<Neighbourhood<I, T>>().Previous) < 0 || ReferenceEquals(interval, Contract.Result<Neighbourhood<I, T>>().Previous)
                )
            );
            // Overlap is set, if there is an overlap
            Contract.Ensures((Contract.Result<Neighbourhood<I, T>>().Overlap != null) == IntervalCollectionContractHelper.GetReturnValue(() => { I overlap; return FindOverlap(query, out overlap) && ReferenceEquals(overlap, query); }));
            // Next is the first interval after the query
            Contract.Ensures(Contract.Result<Neighbourhood<I, T>>().Next == null ||
                Contract.ForAll(
                    this.EnumerateFrom(query, false),
                    interval => Contract.Result<Neighbourhood<I, T>>().Next.CompareHighLow(interval) < 0 || ReferenceEquals(interval, Contract.Result<Neighbourhood<I, T>>().Next)
                )
            );

            throw new NotImplementedException();
        }

        #endregion
    }
}