using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace C5.Intervals
{
    public class EndpointSortedIntervalList<I, T> : IEndpointSortedIntervalList<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly List<I> _list;
        private readonly Func<IInterval<T>, IInterval<T>, bool> _conflictsWithNeighbour;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariants()
        {
            // TODO
        }

        #endregion

        #region Constructors

        public EndpointSortedIntervalList(IEnumerable<I> intervals, Func<IInterval<T>, IInterval<T>, bool> conflictsWithNeighbour)
        {
            _list = new List<I>();
            _conflictsWithNeighbour = conflictsWithNeighbour;

            foreach (var interval in intervals)
                Add(interval);
        }

        #endregion
        
        #region IEndpointSortedIntervalList<I, T>

        #region Properties

        /// <inheritdoc/>
        public Speed IndexingSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public int Count { get { return _list.Count; } }

        /// <inheritdoc/>
        public I this[int i] { get { return _list[i]; } }

        /// <inheritdoc/>
        public I First { get { return _list[0]; } }

        /// <inheritdoc/>
        public I Last { get { return _list[Count - 1]; } }

        #endregion

        #region Find

        /// <inheritdoc/>
        public int Find(IInterval<T> query)
        {
            Contract.Ensures(Contract.Result<int>() == IntervalCollectionContractHelper.IndexOfSorted(_list, query, IntervalExtensions.CreateComparer<IInterval<T>, T>()));

            var low = 0;
            var high = _list.Count - 1;

            while (low <= high)
            {
                var mid = low + (high - low >> 1);
                var compareTo = _list[mid].CompareTo(query);

                if (compareTo < 0)
                    low = mid + 1;
                else if (compareTo > 0)
                    high = mid - 1;
                //Equal but range is not fully scanned
                else if (low != mid)
                    //Set upper bound to current number and rescan
                    high = mid;
                //Equal and full range is scanned
                else
                    return mid;
            }

            // key not found. return insertion point
            return ~low;
        }

        /// <inheritdoc/>
        public int FindFirst(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() < 0 || Count <= Contract.Result<int>() || _list[Contract.Result<int>()].Overlaps(query) || Contract.ForAll(0, Count, i => !_list[i].Overlaps(query)));
            // All intervals before index result do not overlap the query
            Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => !_list[i].Overlaps(query)));

            int min = -1, max = Count;

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1);
                if (query.CompareLowHigh(_list[middle]) <= 0)
                    max = middle;
                else
                    min = middle;
            }

            return max;
        }

        /// <inheritdoc/>
        public int FindLast(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() == 0 || _list[Contract.Result<int>() - 1].Overlaps(query) || Contract.ForAll(_list, x => !x.Overlaps(query)));
            // All intervals after index result do not overlap the query
            Contract.Ensures(Contract.ForAll(Contract.Result<int>(), Count, i => !_list[i].Overlaps(query)));

            int min = -1, max = Count;

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1);
                if (_list[middle].CompareLowHigh(query) <= 0)
                    min = middle;
                else
                    max = middle;
            }

            return max;
        }

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public IEnumerable<I> EnumerateFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            return EnumerateRange(index, Count);
        }

        /// <inheritdoc/>
        public IEnumerable<I> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            Contract.Requires(0 <= inclusiveFrom && inclusiveFrom < Count);
            Contract.Requires(1 <= exclusiveTo && exclusiveTo <= Count);
            Contract.Requires(inclusiveFrom < exclusiveTo);

            while (inclusiveFrom < exclusiveTo)
                yield return _list[inclusiveFrom++];
        }

        /// <inheritdoc/>
        public IEnumerable<I> EnumerateBackwardsFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            while (index >= 0)
                yield return _list[index--];
        }

        /// <inheritdoc/>
        public IEnumerator<I> GetEnumerator() { return _list.GetEnumerator(); }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            var index = Find(interval);

            if (index < 0)
                index = ~index;

            if (index > 0 && _conflictsWithNeighbour(interval, _list[index - 1])
                || index < Count && _conflictsWithNeighbour(interval, _list[index]))
                return false;

            _list.Insert(index, interval);
            return true;
        }

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            var index = Find(interval);

            if (index < 0 || Count <= index)
                return false;

            while (index < Count && _list[index].IntervalEquals(interval))
            {
                if (ReferenceEquals(_list[index], interval))
                {
                    _list.RemoveAt(index);
                    return true;
                }
                ++index;
            }

            return false;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _list.Clear();
        }

        #endregion

        #endregion

    }
}
