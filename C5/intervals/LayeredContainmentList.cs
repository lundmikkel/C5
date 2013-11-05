using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.intervals
{
    /// <summary>
    /// An implementation of the Layered Containment List by Mikkel Riise Lund using two seperate
    /// arrays for intervals and pointers.
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class LayeredContainmentList<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        // Number of intervals in the collection
        private readonly int _count;
        // Number of intervals in the first layer
        private readonly int _firstLayerCount;
        // Number of layers
        private readonly int _layerCount;

        private readonly I[][] _intervalLayers;
        private readonly int[][] _pointerLayers;

        private IInterval<T> _span;

        // MNO
        private int _maximumNumberOfOverlaps = -1;
        private IInterval<T> _intervalOfMaximumOverlap;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // Layer count is equal to the number of layers of intervals and pointers
            Contract.Invariant(IsEmpty || _layerCount == _intervalLayers.Length);
            Contract.Invariant(IsEmpty || _layerCount == _pointerLayers.Length);
            // The first layer's count is non-negative and at most as big as count
            Contract.Invariant(0 <= _firstLayerCount && _firstLayerCount <= _count);
            // Either the collection is empty or there are one layer or more
            Contract.Invariant(IsEmpty || 1 <= _layerCount);
            // Either all intervals are in the first layer, or there are more than one layer
            Contract.Invariant(_count == _firstLayerCount || 1 < _layerCount);
            Contract.Invariant(IsEmpty || _intervalLayers != null && _pointerLayers != null);

            // Each layer is sorted
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _layerCount, l => Contract.ForAll(0, _intervalLayers[l].Length - 1, i => _intervalLayers[l][i].CompareTo(_intervalLayers[l][i + 1]) <= 0)));
            // Each layer is sorted on both low and high endpoint
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _layerCount, l => Contract.ForAll(0, _intervalLayers[l].Length - 1, i => _intervalLayers[l][i].CompareLow(_intervalLayers[l][i + 1]) <= 0 && _intervalLayers[l][i].CompareHigh(_intervalLayers[l][i + 1]) <= 0)));
            // Each interval in a layer must be contained in at least one interval in each layer below
            Contract.Invariant(IsEmpty ||
                Contract.ForAll(1, _layerCount, ly =>
                    Contract.ForAll(0, ly, lx =>
                        Contract.ForAll(_intervalLayers[ly], y => _intervalLayers[lx].Any(x => x.StrictlyContains(y)))
                    )
                )
            );
        }

        #endregion

        #region Inner Classes

        struct Node
        {
            internal I Interval { get; private set; }
            internal int Pointer { get; private set; }

            internal Node(I interval, int pointer)
                : this()
            {
                Interval = interval;
                Pointer = pointer;
            }

            public override string ToString()
            {
                return Interval.ToString();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Layered Containment List with a collection of intervals.
        /// </summary>
        /// <param name="intervalEnumerable">The collection of intervals.</param>
        public LayeredContainmentList(IEnumerable<I> intervalEnumerable)
        {
            // Make intervals to array to allow fast sorting and counting
            var intervals = intervalEnumerable as I[] ?? intervalEnumerable.ToArray();

            // Stop if we have no intervals
            if (!intervals.Any()) return;

            _count = intervals.Length;

            var nodeLayers = generateLayers(ref intervals);

            _layerCount = nodeLayers.Count();
            _firstLayerCount = nodeLayers.First.Count;

            // Create the layers for intervals and pointers
            _intervalLayers = new I[_layerCount][];
            _pointerLayers = new int[_layerCount][];

            // Remember the count from the layer above
            var previousCount = 0;
            // Create each layer starting at the last layer
            for (var i = _layerCount - 1; i >= 0; i--)
            {
                var count = nodeLayers[i].Count;
                _intervalLayers[i] = new I[count];
                _pointerLayers[i] = new int[count + 1];

                for (var j = 0; j < count; j++)
                {
                    var node = nodeLayers[i][j];
                    _intervalLayers[i][j] = node.Interval;
                    _pointerLayers[i][j] = node.Pointer;
                }

                // Add sentinel pointer
                _pointerLayers[i][count] = previousCount;
                previousCount = count;
            }
        }

        private static ArrayList<ArrayList<Node>> generateLayers(ref I[] intervals)
        {
            Contract.Requires(intervals != null);

            // Used for tracking current layer
            var layer = 0;
            var layers = new ArrayList<ArrayList<Node>> { new ArrayList<Node>(), new ArrayList<Node>() };

            // Sort intervals
            var comparer = ComparerFactory<I>.CreateComparer((x, y) => x.CompareTo(y));
            Sorting.IntroSort(intervals, 0, intervals.Length, comparer);

            foreach (var interval in intervals)
            {
                while (layer > 0 && layers[layer - 1].Last.Interval.CompareHigh(interval) <= 0)
                    layer--;

                // Check if interval will be contained in the next layer
                while (!layers[layer].IsEmpty && interval.CompareHigh(layers[layer].Last.Interval) < 0)
                    layer++;

                // Add extra layer if needed
                if (layers.Count == layer + 1)
                    layers.Add(new ArrayList<Node>());

                // Add interval and pointer to list
                layers[layer].Add(new Node(interval, layers[layer + 1].Count));
            }

            // Remove empty layer
            layers.Remove();

            return layers;
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
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _intervalLayers.First().First();
        }

        #endregion

        #region Enumerable

        /// <summary>
        /// Fast enumeration of intervals in arbitrary order, not sorted. For sorted enumerator see <see cref="GetEnumeratorSorted"/> or better <see cref="Sorted"/>.
        /// </summary>
        /// <returns>Enumerator of all intervals in the data structure in arbitrary order</returns>
        public override IEnumerator<I> GetEnumerator()
        {
            if (IsEmpty)
                return (new I[] { }).Cast<I>().GetEnumerator();

            return getEnumerator();
        }

        /// <summary>
        /// Loops through each layer and yield its intervals
        /// </summary>
        /// <returns>Enumerator of all intervals in the data structure</returns>
        private IEnumerator<I> getEnumerator()
        {
            for (var i = 0; i < _layerCount; i++)
            {
                var intervalCount = _intervalLayers[i].Count();
                for (var j = 0; j < intervalCount; j++)
                    yield return _intervalLayers[i][j];
            }
        }

        /// <summary>
        /// Property exposing the method <see cref="GetEnumeratorSorted"/> as IEnumerable&lt;IInterval&lt;T&gt;&gt;.
        /// Usefull for loops: foreach (var interval in intervaled.Sorted) { }. 
        /// </summary>
        public IEnumerable<I> Sorted
        {
            get
            {
                var iterator = GetEnumeratorSorted();
                while (iterator.MoveNext())
                    yield return iterator.Current;
            }
        }

        /// <summary>
        /// Enumeration of intervals in sorted order according to <see cref="IntervalExtensions.CompareTo{T}"/>. For a faster, but unsorted, enumerator see <see cref="GetEnumerator"/>.
        /// </summary>
        /// <returns>Enumerator of all intervals in the data structure in sorted order</returns>
        public IEnumerator<I> GetEnumeratorSorted()
        {
            if (IsEmpty)
                return (new I[] { }).Cast<I>().GetEnumerator();

            return getEnumeratorSorted(0, _firstLayerCount);
        }

        /// <summary>
        /// Enumerate intervals in sorted order using the pointers
        /// </summary>
        /// <param name="start">The index of the first interval in the first layer</param>
        /// <param name="end">The index after the last interval in the first layer</param>
        /// <returns>Enumerator of all intervals in the data structure in sorted order</returns>
        private IEnumerator<I> getEnumeratorSorted(int start, int end)
        {
            // Create our own stack to avoid stack overflow and to speed up the enumerator
            var stack = new int[_layerCount << 1];
            var i = 0;
            // We stack both values consecutively instead of stacking pairs
            stack[i++] = start;
            stack[i++] = end;

            // Continue as long as we still have values on the stack
            while (i > 0)
            {
                // Get start and end from stack
                end = stack[--i];
                start = stack[--i];

                // Cache layers for speed
                var intervalLayer = _intervalLayers[i >> 1];
                var pointerLayer = _pointerLayers[i >> 1];

                while (start < end)
                {
                    yield return intervalLayer[start];

                    // If this and the next interval point to different intervals in the next layer, we need to swap layer
                    if (pointerLayer[start] < pointerLayer[start + 1])
                    {
                        // Push the current values
                        stack[i++] = start + 1;
                        stack[i++] = end;
                        // Push the values for the next layer
                        stack[i++] = pointerLayer[start];
                        stack[i++] = pointerLayer[start + 1];
                        break;
                    }

                    start++;
                }
            }
        }

        #endregion

        #region Interval Collection

        #region Properties

        #region Span

        /// <inheritdoc/>
        public IInterval<T> Span
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("An empty collection has no span");

                // Cache value for later requests
                if (_span == null)
                    _span = new IntervalBase<T>(_intervalLayers.First().First(), _intervalLayers.First()[_firstLayerCount - 1]);

                return _span;
            }
        }

        #endregion

        #region MNO

        /// <inheritdoc/>
        public int MaximumOverlap
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= _layerCount);

                if (_maximumNumberOfOverlaps < 0)
                    findMaximumOverlap(Sorted, true);

                return _maximumNumberOfOverlaps;
            }
        }

        /// <summary>
        /// Get the interval in which the maximum number of overlaps is.
        /// </summary>
        /// <exception cref="InvalidOperationException">If collection is empty.</exception>
        public IInterval<T> IntervalOfMaximumOverlap
        {
            get
            {
                Contract.Ensures(_maximumNumberOfOverlaps >= _layerCount);

                if (IsEmpty)
                    throw new InvalidOperationException("An empty collection has no interval of maximum overlap");

                if (_intervalOfMaximumOverlap == null)
                    findMaximumOverlap(Sorted, true);

                return _intervalOfMaximumOverlap;
            }
        }

        /// <summary>
        /// Find the maximum number of overlaps for all intervals overlapping the query interval.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <remarks>The query interval is not in the maximum number of overlaps. If only one interval
        /// overlaps the query, the result will therefore be 1.</remarks>
        /// <returns>The maximum number of overlaps</returns>
        public int FindMaximumOverlap(IInterval<T> query)
        {
            Contract.Requires(query != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            return findMaximumOverlap(FindOverlapsSorted(query));
        }

        /// <summary>
        /// Find the maximum number of overlaps for all intervals that match the filter and overlap the query interval.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <param name="filter">A filter.</param>
        /// <remarks>The query interval is not in the maximum number of overlaps. If only one interval
        /// overlaps the query, the result will therefore be 1.</remarks>
        /// <returns>The maximum number of overlaps</returns>
        public int FindMaximumOverlap(IInterval<T> query, Func<I, bool> filter)
        {
            Contract.Requires(query != null);
            Contract.Requires(filter != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            return findMaximumOverlap(FindOverlapsSorted(query).Where(filter));
        }

        /// <summary>
        /// Find the maximum number of overlaps for all intervals that match the filter.
        /// </summary>
        /// <param name="filter">A filter.</param>
        /// <remarks>The query interval is not in the maximum number of overlaps. If only one interval
        /// overlaps the query, the result will therefore be 1.</remarks>
        /// <returns>The maximum number of overlaps</returns>
        public int FindMaximumOverlap(Func<I, bool> filter)
        {
            Contract.Requires(filter != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            return findMaximumOverlap(Sorted.Where(filter));
        }

        /// <summary>
        /// Find the maximum number of overlaps and save the values in
        /// <see cref="_maximumNumberOfOverlaps"/> and <see cref="_intervalOfMaximumOverlap"/>.
        /// </summary>
        private int findMaximumOverlap(IEnumerable<I> sortedIntervals, bool setIntervalOfMaximumOverlap = false)
        {
            Contract.Requires(sortedIntervals != null);
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures(!setIntervalOfMaximumOverlap || !sortedIntervals.Any() || _intervalOfMaximumOverlap != null);

            var max = 0;

            // Create queue sorted on high intervals
            var comparer = ComparerFactory<IInterval<T>>.CreateComparer(IntervalExtensions.CompareHigh);
            var queue = new IntervalHeap<IInterval<T>>(comparer);

            // Set default values when we find MNO for the whole collection
            if (setIntervalOfMaximumOverlap)
            {
                // Set running maximum to the number of layers as that is the minimum number of overlaps
                max = _layerCount;
                // Set interval of maximum overlap to default value of the first interval in the last layer
                _intervalOfMaximumOverlap = new IntervalBase<T>(_intervalLayers.Last().First());
            }

            // Loop through intervals in sorted order
            foreach (var interval in sortedIntervals)
            {
                // Remove all intervals from the queue not overlapping the current interval
                while (!queue.IsEmpty && interval.CompareLowHigh(queue.FindMin()) > 0)
                    queue.DeleteMin();

                queue.Add(interval);

                if (queue.Count > max)
                {
                    max = queue.Count;
                    // Create a new interval when new maximum is found
                    // The low is the current intervals low due to the intervals being sorted
                    // The high is the smallest high in the queue
                    if (setIntervalOfMaximumOverlap)
                        _intervalOfMaximumOverlap = new IntervalBase<T>(interval, queue.FindMin());
                }
            }

            // Cache value for later requests
            _maximumNumberOfOverlaps = max;

            return max;
        }

        #endregion

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            // Check for null to avoid creating empty interval
            if (ReferenceEquals(query, null))
                return Enumerable.Empty<I>();

            return FindOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            // Break if we won't find any overlaps
            if (query == null || IsEmpty)
                yield break;

            int layer = 0, lower = 0, upper = _firstLayerCount;

            // Make sure first and last don't point at the same interval (theorem 2)
            while (lower < upper)
            {
                // Cache layer to speed up iteration
                var currentLayer = _intervalLayers[layer];

                var first = lower;

                // The first interval doesn't overlap we need to search for it
                if (!currentLayer[first].Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    first = findFirst(layer, ++first, upper, query);

                    // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
                    if (upper <= first || !currentLayer[first].Overlaps(query))
                        yield break;
                }

                // We can use first as lower to minimize search area
                var last = findLast(layer, first, upper, query);

                // Save values for next iteration
                lower = _pointerLayers[layer][first];
                upper = _pointerLayers[layer][last];
                layer++;

                while (first < last)
                    yield return currentLayer[first++];
            }
        }

        // TODO: Decide on using either start/end or lower/upper.
        private int findFirst(int layer, int lower, int upper, IInterval<T> query)
        {
            Contract.Requires(0 <= layer && layer < _intervalLayers.Length);
            Contract.Requires(0 <= lower && lower <= _intervalLayers[layer].Length);
            Contract.Requires(0 <= upper && upper <= _intervalLayers[layer].Length);
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() < lower || upper <= Contract.Result<int>() || _intervalLayers[layer][Contract.Result<int>()].Overlaps(query) || Contract.ForAll(lower, upper, i => !_intervalLayers[layer][i].Overlaps(query)));
            // All intervals before index result do not overlap the query
            Contract.Ensures(Contract.ForAll(0, Contract.Result<int>(), i => !_intervalLayers[layer][i].Overlaps(query)));

            int min = lower - 1, max = upper;

            var intervalLayer = _intervalLayers[layer];

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = intervalLayer[middle];

                var compare = query.Low.CompareTo(interval.High);

                if (compare < 0 || compare == 0 && query.LowIncluded && interval.HighIncluded)
                    max = middle;
                else
                    min = middle;
            }

            return max;
        }

        private int findLast(int layer, int lower, int upper, IInterval<T> query)
        {
            Contract.Requires(0 <= layer && layer < _intervalLayers.Length);
            Contract.Requires(0 <= lower && lower < _intervalLayers[layer].Length);
            Contract.Requires(0 <= upper && upper <= _intervalLayers[layer].Length);
            Contract.Requires(query != null);

            // Either the interval at index result overlaps or no intervals in the layer overlap
            Contract.Ensures(Contract.Result<int>() == 0 || _intervalLayers[layer][Contract.Result<int>() - 1].Overlaps(query) || Contract.ForAll(_intervalLayers[layer], x => !x.Overlaps(query)));
            // All intervals after index result do not overlap the query
            Contract.Ensures(Contract.ForAll(Contract.Result<int>(), _intervalLayers[layer].Count(), i => !_intervalLayers[layer][i].Overlaps(query)));

            int min = lower - 1, max = upper;
            var intervalLayer = _intervalLayers[layer];

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = intervalLayer[middle];

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return max;
        }

        #region Sorted

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection that overlap the query point in sorted order.
        /// </summary>
        /// <param name="query">The query point.</param>
        /// <returns>All intervals that overlap the query point.</returns>
        public IEnumerable<I> FindOverlapsSorted(T query)
        {
            Contract.Requires(!ReferenceEquals(query, null));

            var queryInterval = new IntervalBase<T>(query);

            // No overlap if collection is empty or query doesn't overlap collection
            if (IsEmpty || !queryInterval.Overlaps(Span))
                return Enumerable.Empty<I>();

            return findOverlapsSorted(queryInterval);
        }

        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection that overlap the query interval in sorted order.
        /// </summary>
        /// <param name="query">The query interval.</param>
        /// <returns>All intervals that overlap the query interval.</returns>
        public IEnumerable<I> FindOverlapsSorted(IInterval<T> query)
        {
            Contract.Requires(query != null);

            // No overlap if collection is empty or query doesn't overlap collection
            if (IsEmpty || !query.Overlaps(Span))
                return Enumerable.Empty<I>();

            return findOverlapsSorted(query);
        }

        private IEnumerable<I> findOverlapsSorted(IInterval<T> query)
        {
            // Create our own stack to avoid stack overflow and to speed up the enumerator
            var stack = new int[_layerCount << 1];
            var i = 0;

            // Keeps track of the index of the first overlap in each layer
            var firstOverlaps = new int[_layerCount];
            for (var l = 0; l < _layerCount; l++)
                firstOverlaps[l] = -1;

            // We stack both values consecutively instead of stacking pairs
            stack[i++] = 0;
            stack[i++] = _firstLayerCount;

            // Continue as long as we still have values on the stack
            while (i > 0)
            {
                // Get start and end from stack
                var end = stack[--i];
                var start = stack[--i];
                var layer = i >> 1;

                // Cache layers for speed
                var intervalLayer = _intervalLayers[layer];
                var pointerLayer = _pointerLayers[layer];

                if (firstOverlaps[layer] < 0)
                    firstOverlaps[layer] = findFirst(layer, start, end, query);

                if (firstOverlaps[layer] >= end)
                    continue;
                if (start < firstOverlaps[layer])
                    start = firstOverlaps[layer];

                // Iterate through all overlaps
                while (start < end && intervalLayer[start].CompareLowHigh(query) <= 0)
                {
                    yield return intervalLayer[start];

                    // If this and the next interval point to different intervals in the next layer, we need to swap layer
                    if (pointerLayer[start] < pointerLayer[start + 1])
                    {
                        // Push the current values
                        stack[i++] = start + 1;
                        stack[i++] = end;
                        // Push the values for the next layer
                        stack[i++] = pointerLayer[start];
                        stack[i++] = pointerLayer[start + 1];
                        break;
                    }

                    start++;
                }
            }
        }

        private IEnumerable<I> findOverlapsSortedRecursive(IInterval<T> query, int layer, int start, int end, int highestVisitedLayer)
        {
            if (layer > highestVisitedLayer)
            {
                start = findFirst(layer, start, end, query);
                highestVisitedLayer++;
            }

            var intervalLayer = _intervalLayers[layer];
            var pointerLayer = _pointerLayers[layer];

            // Iterate through all overlaps
            while (start < end && intervalLayer[start].CompareLowHigh(query) <= 0)
            {
                yield return intervalLayer[start];

                // If this and the next interval point to different intervals in the next layer, we need to swap layer
                if (pointerLayer[start] < pointerLayer[start + 1])
                {
                    foreach (var interval in findOverlapsSortedRecursive(query, layer + 1, pointerLayer[start], pointerLayer[start + 1], highestVisitedLayer))
                        yield return interval;
                }

                start++;
            }
        }

        #endregion

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public bool FindOverlap(T query, ref I overlap)
        {
            return FindOverlap(new IntervalBase<T>(query), ref overlap);
        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, ref I overlap)
        {
            // No overlap if query is null, collection is empty, or query doesn't overlap collection
            if (query == null || IsEmpty || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(0, 0, _firstLayerCount, query);

            // Check if index is in bound and if the interval overlaps the query
            var result = 0 <= i && i < _firstLayerCount && _intervalLayers[0][i].Overlaps(query);

            if (result)
                overlap = _intervalLayers[0][i];

            return result;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public int CountOverlaps(T query)
        {
            return countOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public int CountOverlaps(IInterval<T> query)
        {
            return !IsEmpty ? countOverlaps(query) : 0;
        }

        private int countOverlaps(IInterval<T> query)
        {
            Contract.Requires(query != null);

            int layer = 0, lower = 0, upper = _firstLayerCount, count = 0;

            while (lower < upper)
            {
                // The first interval doesn't overlap we need to search for it
                // TODO: Can we tighten the check here? Like i.low < q.high...
                if (!_intervalLayers[layer][lower].Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    lower = findFirst(layer, ++lower, upper, query);

                    // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
                    // TODO: Can we tighten the check here? Like i.low < q.high...
                    if (upper <= lower || !_intervalLayers[layer][lower].Overlaps(query))
                        return count;
                }

                // We can use first as lower to speed up the search
                upper = findLast(layer, lower, upper, query);

                count += upper - lower;

                lower = _pointerLayers[layer][lower];
                upper = _pointerLayers[layer][upper];
                layer++;
            }

            return count;
        }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public void AddAll(IEnumerable<I> intervals)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            throw new ReadOnlyCollectionException();
        }

        #endregion

        #endregion

        #region GraphViz

        /// <summary>
        /// Get a string representation of the tree in GraphViz dot format.
        /// </summary>
        /// <returns>GraphViz string.</returns>
        public string Graphviz()
        {
            return String.Format("digraph LayeredContainmentList {{\n\trankdir=BT;\n\tnode [shape=record];\n\n{0}\n}}", graphviz());
        }

        private string graphviz()
        {
            // Auto-generated contracts to shut static analysis up
            Contract.Requires(IsEmpty || 1 < _intervalLayers.Length);
            Contract.Requires(IsEmpty || _layerCount == _intervalLayers.Length || Count != _firstLayerCount || 0 <= (_firstLayerCount - 1));
            Contract.Requires(IsEmpty || _layerCount == _intervalLayers.Length || Count != _firstLayerCount || 0 >= _firstLayerCount || 0 >= _firstLayerCount || 0 < _intervalLayers.Length);

            var s = String.Empty;

            var layer = 0;
            int lower = 0, upper = _firstLayerCount;

            while (lower < upper)
            {
                var l = new ArrayList<string>();
                var p = String.Empty;
                for (var i = 0; i < upper; i++)
                {
                    l.Add(String.Format("<n{0}> {0}: {1}", i, _intervalLayers[layer][i]));

                    p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, i, layer + 1, _pointerLayers[layer][i]);
                }

                // Sentinel node
                l.Add(String.Format("<n{0}> {0}: *", upper));
                p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, upper, layer + 1, _pointerLayers[layer][upper - 1]);

                s += String.Format("\tlayer{0} [fontname=consola, label=\"{1}\"];\n\t{2}\n", layer, String.Join("|", l.ToArray()), p);


                lower = _pointerLayers[layer][lower];
                upper = _pointerLayers[layer][upper];
                layer++;
            }

            s += String.Format("\tlayer{0} [fontname=consola, label=\"<n0> 0: *\"];", layer);

            return s;
        }

        #endregion
    }
}