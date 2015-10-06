using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    // TODO: Make sorted using a binary heap of enumerators
    public class DynamicContainmentList<I, T> : IntervalCollectionBase<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private int _count;

        private readonly IList<IContainmentFreeIntervalCollection<I, T>> _collections;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
        }

        #endregion

        #region Constructors

        public DynamicContainmentList()
        {
            _collections = new ArrayList<IContainmentFreeIntervalCollection<I, T>>();
        }

        public DynamicContainmentList(IEnumerable<I> intervals)
            : this()
        {
            AddAll(intervals);
        }

        protected virtual IContainmentFreeIntervalCollection<I, T> CreateCollection()
        {
            //return new EndpointSortedIntervalCollection<I, T>(true);
            //return new EndpointSortedIntervalCollection<I, T>(false);
            return new DoublyLinkedFiniteIntervalTree<I, T>();
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (_count == 0));
                return _count == 0;
            }
        }

        /// <inheritdoc/>
        public override int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() == _count);
                return _count;
            }
        }

        /// <inheritdoc/>
        public override Speed CountSpeed
        {
            get { return Speed.Constant; }
        }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _collections[0].Choose();
        }

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator()
        {
            foreach (var collection in _collections)
            {
                foreach (var interval in collection)
                {
                    yield return interval;
                }
            }
        }

        #endregion

        #region Properties

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool AllowsOverlaps { get { return true; } }

        /// <inheritdoc/>
        public override bool IsReadOnly { get { return false; } }

        /// <inheritdoc/>
        public override bool IsFindOverlapsSorted { get { return false; } }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public override IInterval<T> Span
        {
            get
            {
                return _collections.Select(collection => collection.Span).Span();
            }
        }

        /// <inheritdoc/>
        public override I LowestInterval
        {
            get
            {
                // TODO: Incorrect when collection is containment free - more low intervals could be in the same collection
                return _collections.Select(collection => collection.LowestInterval).LowestInterval<I, T>();
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<I> LowestIntervals
        {
            get
            {
                // TODO: Incorrect when collection is containment free - more low intervals could be in the same collection
                return !IsEmpty ? _collections.Select(collection => collection.LowestInterval).LowestIntervals<I, T>() : Enumerable.Empty<I>();
            }
        }

        /// <inheritdoc/>
        public override I HighestInterval
        {
            get
            {
                // TODO: Incorrect when collection is containment free - more low intervals could be in the same collection
                return _collections.Select(collection => collection.HighestInterval).HighestInterval<I, T>();
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<I> HighestIntervals
        {
            get
            {
                // TODO: Incorrect when collection is containment free - more low intervals could be in the same collection
                return !IsEmpty ? _collections.Select(collection => collection.HighestInterval).HighestIntervals<I, T>() : Enumerable.Empty<I>();
            }
        }

        /// <inheritdoc/>
        public override int MaximumDepth
        {
            get
            {
                IInterval<T> interval;
                return IntervalExtensions.MaximumDepth(this, out interval, false);
            }
        }

        #endregion

        #endregion

        #region Find Equals

        /// <inheritdoc/>
        public override IEnumerable<I> FindEquals(IInterval<T> query)
        {
            foreach (var collection in _collections)
            {
                foreach (var interval in collection.FindEquals(query))
                {
                    yield return interval;
                }
            }
        }

        #region Contains

        public override bool Contains(I interval)
        {
            foreach (var collection in _collections)
            {
                if (collection.Contains(interval))
                    return true;
            }

            return false;
        }

        #endregion

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(T query)
        {
            foreach (var collection in _collections)
            {
                foreach (var interval in collection.FindOverlaps(query))
                {
                    yield return interval;
                }
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            foreach (var collection in _collections)
            {
                foreach (var interval in collection.FindOverlaps(query))
                {
                    yield return interval;
                }
            }
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public override bool FindOverlap(T query, out I overlap)
        {
            foreach (var collection in _collections)
            {
                if (collection.FindOverlap(query, out overlap))
                    return true;
            }

            overlap = null;
            return false;
        }

        /// <inheritdoc/>
        public override bool FindOverlap(IInterval<T> query, out I overlap)
        {
            foreach (var collection in _collections)
            {
                if (collection.FindOverlap(query, out overlap))
                    return true;
            }

            overlap = null;
            return false;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public override int CountOverlaps(T query)
        {
            var sum = 0;

            foreach (var collection in _collections)
            {
                sum += collection.CountOverlaps(query);
            }

            return sum;
        }

        public override int CountOverlaps(IInterval<T> query)
        {
            var sum = 0;

            foreach (var collection in _collections)
            {
                sum += collection.CountOverlaps(query);
            }

            return sum;
        }

        #endregion

        #region Extensible

        #region Add

        /// <inheritdoc/>
        public override bool Add(I interval)
        {
            add(interval);
            _count++;
            raiseForAdd(interval);
            return true;
        }

        private void add(I interval)
        {
            foreach (var collection in _collections)
            {
                if (collection.Add(interval))
                    return;
            }

            var newCollection = CreateCollection();
            newCollection.Add(interval);
            _collections.Add(newCollection);
        }

        #endregion

        #region Remove

        /// <inheritdoc/>
        public override bool Remove(I interval)
        {
            if (remove(interval))
            {
                _count--;
                raiseForRemove(interval);
                return true;
            }

            return false;
        }

        private bool remove(I interval)
        {
            foreach (var collection in _collections)
            {
                if (collection.Remove(interval))
                {
                    // TODO: When and how should we remove the collection?
                    if (collection.IsEmpty)
                        _collections.Remove(collection);

                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Clear

        protected override void clear()
        {
            _count = 0;
            _collections.Clear();
        }

        #endregion

        #endregion
    }
}