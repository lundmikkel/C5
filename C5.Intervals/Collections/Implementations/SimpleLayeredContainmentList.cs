using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using SCG = System.Collections.Generic;

namespace C5.Intervals
{
    /// <summary>
    /// A simple implementation of the Layered Containment List by Mikkel Riise Lund.
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class SimpleLayeredContainmentList<I, T> : SortedIntervalCollectionBase<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        // Number of intervals
        private readonly int _count;
        // Number of intervals in first layer for convenience/speed
        private readonly int _firstLayerCount;

        // Interval layers
        private readonly I[][] _layers;
        // First layer for convenience/speed
        private readonly I[] _firstLayer;

        // Sorted interval array
        private readonly I[] _sorted;

        // Collection span
        private readonly IInterval<T> _span;

        // Maximum Depth
        private int _maximumDepth = -1;
        private IInterval<T> _intervalOfMaximumDepth;

        #endregion

        #region Properties

        /// <summary>
        /// The degree of containment for the collection. This is the length of the longest 
        /// chain of intervals containing each other without any of them sharing endpoints.
        /// </summary>
        public int ContainmentDegree
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(IsEmpty || Contract.Result<int>() > 0);

                return _layers.Length;
            }
        }

        /// <summary>
        /// The number of intervals in the collection that are contained in another interval.
        /// </summary>
        public int ContainmentCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return _count - _firstLayerCount;
            }
        }

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // The first layer's count is non-negative and at most as big as count
            Contract.Invariant(0 <= _firstLayerCount && _firstLayerCount <= _count);
            // Either the collection is empty or there are one layer or more
            Contract.Invariant(IsEmpty || _layers.Length >= 1);
            // Either all intervals are in the first layer, or there are more than one layer
            Contract.Invariant(_count == _firstLayerCount || _layers.Length > 1);
            // The array is null if empty
            Contract.Invariant(!IsEmpty || _layers == null);

            // No layer is empty
            Contract.Invariant(IsEmpty || Contract.ForAll(_layers, layer => layer.Length > 0));
            // Each layer is sorted
            Contract.Invariant(IsEmpty || Contract.ForAll(_layers, layer => layer.IsSorted<I, T>()));
            // Each layer is sorted on both low and high endpoint
            Contract.Invariant(IsEmpty || Contract.ForAll(_layers, layer => layer.IsEndpointSorted<I, T>()));
            // Each interval in a higher layer is contained in some interval in the layer below
            Contract.Invariant(IsEmpty || _layers.ForAllConsecutiveElements((previousLayer, layer) => layer.All(x => previousLayer.Any(y => y.StrictlyContains(x)))));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Layered Containment List from a collection of intervals.
        /// </summary>
        /// <param name="intervals">The collection of intervals.</param>
        public SimpleLayeredContainmentList(IEnumerable<I> intervals)
        {
            // TODO: Calculate the maximum depth during construction based on argument

            // Make intervals to array to allow fast sorting and counting
            _sorted = intervals as I[] ?? intervals.ToArray();

            // Stop if we have no intervals
            if ((_count = _sorted.Length) == 0)
                return;

            // Sort intervals
            Sorting.Timsort(_sorted, IntervalExtensions.CreateComparer<I, T>());

            constructLayers(_sorted, out _layers);

            // Cache values for convenience/speed
            _firstLayer = _layers[0];
            _firstLayerCount = _firstLayer.Length;
            _span = new IntervalBase<T>(_firstLayer[0], _firstLayer[_firstLayerCount - 1]);
        }

        private void constructLayers(SCG.IList<I> sorted, out I[][] layers)
        {
            // TODO: Replace C5.ArrayList with SCG.List after testing
            var layerList = new ArrayList<SCG.IList<I>> {
                new ArrayList<I> { sorted[0] }
            };

            // Place intervals
            for (var i = 1; i < _count; ++i)
            {
                var interval = sorted[i];

                // Find layer using gallop/binary search
                var l = gallopLayerSearch(layerList, interval);

                // Add extra layer if necessary
                if (l == layerList.Count)
                    layerList.Add(new ArrayList<I>());

                // Add the interval to the correct layer
                layerList[l].Add(interval);
            }

            // Convert lists into static arrays
            layers = new I[layerList.Count][];
            for (var l = 0; l < layerList.Count; ++l)
                layers[l] = layerList[l].ToArray();
        }

        private static int gallopLayerSearch(SCG.IList<SCG.IList<I>> layers, I query)
        {
            Contract.Requires(query != null);
            // The high endpoints of the last interval in each layer is sorted
            Contract.Requires(layers.ForAllConsecutiveElements((l1, l2) => l1.Last().CompareHigh(l2.Last()) > 0));
            // Result is non-negative and less than layer count
            Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() <= layers.Count);
            // The last interval in the layer has a high less than or equal to interval's high
            Contract.Ensures(Contract.Result<int>() == layers.Count || layers[Contract.Result<int>()].Last().CompareHigh(query) <= 0);
            // The last interval in the layer below has a high greater than interval's high
            Contract.Ensures(Contract.Result<int>() == 0 || layers[Contract.Result<int>() - 1].Last().CompareHigh(query) > 0);

            var lower = 0;
            var upper = layers.Count;
            var jump = 1;

            while (true)
            {
                var layer = layers[lower];
                var compare = layer[layer.Count - 1].CompareHigh(query);

                // Endpoints match; we are in the right layer
                if (compare == 0)
                    return lower;

                var next = lower + jump;

                // We are in a higher layer than needed, do binary search for the rest
                if (compare < 0 || upper <= next)
                {
                    if (compare < 0)
                        upper = lower;

                    if (upper == 0)
                        return 0;

                    // Back up to previous value
                    lower = lower - (jump >> 1) + 1;

                    return binaryLayerSearch(layers, query, lower, upper);
                }

                // Jump
                lower = next;

                // Double jump
                jump <<= 1;
            }
        }

        private static int binaryLayerSearch(SCG.IList<SCG.IList<I>> layers, I query, int lower, int upper)
        {
            int min = lower, max = upper;

            while (min < max)
            {
                var mid = min + (max - min >> 1);
                var layer = layers[mid];
                var compare = layer[layer.Count - 1].CompareHigh(query);

                // interval contains the last interval
                if (compare < 0)
                    max = mid;
                // The last interval contains interval
                else if (compare > 0)
                    min = mid + 1;
                // We have found an interval with the same high
                else
                    return mid;
            }

            return min;
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
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _firstLayer[0];
        }

        #endregion

        #region Interval Collection

        #region Data Structure Properties

        // TODO: Support user setting the value (also for AllowsContainments and AllowsReferenceDuplicates)
        /// <inheritdoc/>
        public override bool AllowsOverlaps { get { return true; } }

        /// <inheritdoc/>
        public override bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        public override bool IsFindOverlapsSorted { get { return false; } }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public override IInterval<T> Span { get { return _span; } }

        /// <inheritdoc/>
        public override I LowestInterval { get { return _firstLayer[0]; } }

        /// <inheritdoc/>
        public override IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var lowestInterval = LowestInterval;
                yield return lowestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = 1; i < _firstLayerCount; ++i)
                {
                    if (_firstLayer[i].LowEquals(lowestInterval))
                        yield return _firstLayer[i];
                    else
                        yield break;
                }
            }
        }

        /// <inheritdoc/>
        public override I HighestInterval { get { return _firstLayer[_firstLayerCount - 1]; } }

        /// <inheritdoc/>
        public override IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var highestInterval = HighestInterval;
                yield return highestInterval;

                // Iterate through bottom layer as long as the intervals share a high
                for (var i = _firstLayerCount - 2; i >= 0; --i)
                {
                    if (_firstLayer[i].HighEquals(highestInterval))
                        yield return _firstLayer[i];
                    else
                        yield break;
                }
            }
        }

        #region Maximum Depth

        /// <inheritdoc/>
        public override int MaximumDepth
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= (IsEmpty ? 0 : _layers.Length));

                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(out _intervalOfMaximumDepth);

                return _maximumDepth;
            }
        }

        /// <summary>
        /// Get the interval in which the maximum depth is.
        /// </summary>
        public IInterval<T> IntervalOfMaximumDepth
        {
            get
            {
                Contract.Ensures(_maximumDepth >= _layers.Length);

                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(out _intervalOfMaximumDepth);

                return _intervalOfMaximumDepth;
            }
        }

        /// <summary>
        /// Find the maximum depth for all intervals that match the filter.
        /// </summary>
        /// <param name="filter">A filter.</param>
        /// <remarks>The query interval is not in the maximum depth. If only one interval
        /// overlaps the query, the result will therefore be 1.</remarks>
        /// <returns>The maximum depth.</returns>
        public int FindMaximumDepth(Func<I, bool> filter)
        {
            Contract.Requires(filter != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            IInterval<T> intervalOfMaximumDepth = null;
            return Sorted.Where(filter).MaximumDepth(out intervalOfMaximumDepth);
        }

        #endregion

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator() { return Sorted.GetEnumerator(); }

        /// <inheritdoc/>
        public override IEnumerable<I> Sorted
        {
            get
            {
                foreach (var interval in _sorted)
                    yield return interval;
            }
        }

        #endregion

        #region Find Equals

        public override IEnumerable<I> FindEquals(IInterval<T> query)
        {
            var i = indexOf(query);

            // No equal found
            if (i < 0)
                yield break;

            // Enumerate equals
            while (i < _count && _sorted[i].IntervalEquals(query))
                yield return _sorted[i++];
        }

        private int indexOf(IInterval<T> query)
        {
            int min = 0, max = _count - 1;

            while (min <= max)
            {
                var mid = min + (max - min >> 1);
                var compareTo = _sorted[mid].CompareTo(query);

                if (compareTo < 0)
                    min = mid + 1;
                else if (compareTo > 0)
                    max = mid - 1;
                else if (min != mid)
                    max = mid;
                else
                    return mid;
            }

            return ~min;
        }

        #endregion

        #region Find Overlaps

        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            foreach (var layer in _layers)
            {
                var first = findFirst(query, layer);

                // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
                if (first == layer.Length || layer[first].CompareLowHigh(query) > 0)
                    yield break;

                // We can use first as lower to minimize search area
                var last = findLast(query, layer, first);

                while (first < last)
                    yield return layer[first++];
            }
        }

        [Pure]
        private static int findFirst(IInterval<T> query, I[] layer)
        {
            Contract.Requires(query != null);
            Contract.Requires(layer != null);

            Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() <= layer.Length);
            // First potential overlap is found
            Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => layer[i].CompareHighLow(query) < 0)
                && Contract.ForAll(Contract.Result<int>(), layer.Length, i => layer[i].CompareHighLow(query) >= 0));

            int min = 0, max = layer.Length;

            while (min < max)
            {
                var mid = min + (max - min >> 1);

                if (layer[mid].CompareHighLow(query) < 0)
                    min = mid + 1;
                else
                    max = mid;
            }

            return min;
        }

        [Pure]
        private int findLast(IInterval<T> query, I[] layer, int first)
        {
            Contract.Requires(query != null);
            Contract.Requires(layer != null);

            Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() <= layer.Length);
            // Last potential overlap is found
            Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => layer[i].CompareLowHigh(query) <= 0)
                && Contract.ForAll(Contract.Result<int>(), layer.Length, i => layer[i].CompareLowHigh(query) > 0));

            int min = first, max = layer.Length;

            while (min < max)
            {
                var mid = min + (max - min >> 1);

                if (query.CompareHighLow(layer[mid]) < 0)
                    max = mid;
                else
                    min = mid + 1;
            }

            return min;
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public override bool FindOverlap(T query, out I overlap)
        {
            return FindOverlap(new IntervalBase<T>(query), out overlap);
        }

        /// <inheritdoc/>
        public override bool FindOverlap(IInterval<T> query, out I overlap)
        {
            overlap = null;

            // No overlap if query is null, collection is empty, or query doesn't overlap collection
            if (IsEmpty || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var first = findFirst(query, _firstLayer);

            // Check if index is in bound and if the interval overlaps the query
            var result = first < _firstLayerCount && _firstLayer[first].CompareLowHigh(query) <= 0;

            if (result)
                overlap = _firstLayer[first];

            return result;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public override int CountOverlaps(T query)
        {
            return CountOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public override int CountOverlaps(IInterval<T> query)
        {
            if (IsEmpty)
                return 0;

            var count = 0;

            foreach (var layer in _layers)
            {
                var first = findFirst(query, layer);

                // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
                if (first == layer.Length || layer[first].CompareLowHigh(query) > 0)
                    return count;

                // We can use first as lower to speed up the search
                var last = findLast(query, layer, first);

                count += last - first;
            }

            return count;
        }

        #endregion

        #region Gaps

        /// <inheritdoc/>
        public override IEnumerable<IInterval<T>> Gaps
        {
            get
            {
                if (IsEmpty)
                    return Enumerable.Empty<IInterval<T>>();

                return _firstLayer.Gaps();
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return findOverlapsInFirstLayer(query).Gaps(query);
        }

        private IEnumerable<I> findOverlapsInFirstLayer(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            var last = _firstLayerCount;
            var first = findFirst(query, _firstLayer);

            // Cache variables to speed up iteration
            I interval;
            while (first < last && (interval = _firstLayer[first++]).CompareLowHigh(query) <= 0)
                yield return interval;
        }

        #endregion

        #endregion
    }
}