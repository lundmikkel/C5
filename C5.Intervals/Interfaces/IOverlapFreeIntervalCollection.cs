using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    // TODO: Introduce ISortedIntervalCollection and IIndexIntervalCollection?
    /// <summary>
    /// An interval collection where intervals do not overlap
    /// and where they are sorted based on lowest low, then lowest high endpoint.
    /// All enumerators are be lazy.
    /// </summary>
    /// <typeparam name="I">The interval type in the collection. Especially used for return types for enumeration.</typeparam>
    /// <typeparam name="T">The interval's endpoint values.</typeparam>
    [ContractClass(typeof(OverlapFreeIntervalCollectionContract<,>))]
    public interface IOverlapFreeIntervalCollection<I, T> : IContainmentFreeIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
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
    internal abstract class OverlapFreeIntervalCollectionContract<I, T> : ContainmentFreeIntervalCollectionBase<I, T>, IOverlapFreeIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {

        #region Indexed Access
        public int IndexOf(I interval)
        {
            Contract.Requires(interval != null);

            Contract.Ensures(Contract.Result<int>() == Sorted.IndexOfSorted(interval, IntervalExtensions.CreateComparer<I, T>()));
            Contract.Ensures(!this.Any(x => ReferenceEquals(interval, x)) || 0 <= Contract.Result<int>() && Contract.Result<int>() < Count);
            Contract.Ensures(this.Any(x => ReferenceEquals(interval, x)) || 0 > Contract.Result<int>() && Contract.Result<int>() >= ~Count);

            throw new NotImplementedException();
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