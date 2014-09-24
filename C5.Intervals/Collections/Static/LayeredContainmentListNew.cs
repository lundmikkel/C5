using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace C5.Intervals
{
    /// <summary>
    /// An implementation of the Layered Containment List by Mikkel Riise Lund using two separate
    /// arrays for intervals and pointers.
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class LayeredContainmentListNew<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        // Number of intervals in the collection
        private readonly int _count;
        // Number of intervals in the first layer
        private readonly int _firstLayerCount;
        // Number of layers
        private readonly int _layerCount;

        private readonly Node[] _nodes;
        private readonly I[] intervalArray;

        private readonly IInterval<T> _span;

        // Maximum Depth
        private int _maximumDepth = -1;
        private IInterval<T> _intervalOfMaximumDepth;

        #endregion

        #region Properties

        /// <summary>
        /// The degree of containment for a collection. This is the length of the longest chain of
        /// intervals strictly contained in each other.
        /// </summary>
        public int ContainmentDegree
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(IsEmpty || Contract.Result<int>() > 0);

                return _layerCount;
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
            // Layer count is equal to the number of layers of intervals and pointers
            Contract.Invariant(IsEmpty || _layerCount == _nodes.Count(node => node.Interval == null));
            // The first layer's count is non-negative and at most as big as count
            Contract.Invariant(0 <= _firstLayerCount && _firstLayerCount <= _count);
            // Either the collection is empty or there are one layer or more
            Contract.Invariant(IsEmpty || _layerCount >= 1);
            // Either all intervals are in the first layer, or there are more than one layer
            Contract.Invariant(_count == _firstLayerCount || _layerCount > 1);
            // The array is null if empty
            Contract.Invariant(!IsEmpty || _nodes == null);

            // No layer is empty
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _nodes.Length - 1, i => _nodes[i].Interval != null || _nodes[i + 1].Interval != null));
            // Each layer is sorted
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _nodes.Length - 1, i => _nodes[i].Interval == null || _nodes[i + 1].Interval == null || _nodes[i].Interval.CompareTo(_nodes[i + 1].Interval) <= 0));
            // Each layer is sorted on both low and high endpoint
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _nodes.Length - 1, i => _nodes[i].Interval == null || _nodes[i + 1].Interval == null || _nodes[i].Interval.CompareLow(_nodes[i + 1].Interval) <= 0 && _nodes[i].Interval.CompareHigh(_nodes[i + 1].Interval) <= 0));
            Contract.Invariant(checkContainmentInvariant());
        }

        [Pure]
        private bool checkContainmentInvariant()
        {
            if (IsEmpty)
                return true;

            var previousLower = 0;
            var previousUpper = _firstLayerCount;

            if (previousLower == previousUpper)
                return true;

            var lower = _nodes[previousLower].Pointer;
            var upper = _nodes[previousUpper].Pointer;

            while (lower < upper)
            {
                for (var i = lower; i < upper; ++i)
                    if (!_nodes.Skip(previousLower).Take(previousUpper - previousLower).Any(node => node.Interval.StrictlyContains(_nodes[i].Interval)))
                        return false;

                previousLower = lower;
                previousUpper = upper;
                lower = _nodes[previousLower].Pointer;
                upper = _nodes[previousUpper].Pointer;
            }

            return true;
        }

        #endregion

        #region Inner Classes

        private class Node
        {
            public I Interval;
            public int Pointer;
            public int Layer;

            // TODO: Remove
            public int Length { get { return Pointer; } set { Pointer = value; } }

            public override string ToString()
            {
                return Interval + " / " + Pointer;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Layered Containment List with a collection of intervals.
        /// </summary>
        /// <param name="intervals">The collection of intervals.</param>
        public LayeredContainmentListNew(IEnumerable<I> intervals)
        {
            // TODO: Calculate the maximum depth during construction based on argument

            // Make intervals to array to allow fast sorting and counting
            intervalArray = intervals as I[] ?? intervals.ToArray();

            // Stop if we have no intervals
            if ((_count = intervalArray.Length) == 0)
                return;

            constructLayers(ref intervalArray, out _nodes, out _layerCount, out _firstLayerCount);

            // Cache span value
            _span = new IntervalBase<T>(_nodes[0].Interval, _nodes[_firstLayerCount - 1].Interval);

            return;
        }

        private void constructLayers(ref I[] intervals, out Node[] nodes, out int layerCount, out int firstLayerCount)
        {
            // Sort intervals
            Sorting.Timsort(intervalArray, IntervalExtensions.CreateComparer<I, T>());

            // Pointer is used to store the layers length
            var layers = new ArrayList<Node> {
                // The main layer
                new Node {
                    Interval = intervals[0],
                    Length = 1
                },
                // An extra empty layer
                new Node()
            };

            nodes = new Node[_count];
            // Add first interval to layers manually
            nodes[0] = new Node{ Interval = intervals[0], Layer = 0, Pointer = 0 };
            
            var layer = 0;

            // Figure out each intervals placement
            for (var i = 1; i < _count; ++i)
            {
                var interval = intervals[i];
                layer = binaryLayerSearch(layers, interval, layer);

                // Add a node if we are about to populate the last layer
                if (layer == layers.Count - 1)
                    layers.Add(new Node());

                // Store the layer's last interval and increment its size counter
                layers[layer].Interval = interval;
                layers[layer].Length++;

                // Slowly add each interval etc. to nodes array
                nodes[i] = new Node { Interval = interval, Layer = layer, Pointer = layers[layer + 1].Length};
            }

            // Minus the one extra empty layer
            layerCount = layers.Count - 1;

            firstLayerCount = layers[0].Length;

            // Create a new array for all interval nodes plus one sentinel node for each layer
            Array.Resize(ref nodes, _count + layerCount);

            // Add sentinels
            for (var l = 0; l < layerCount; ++l)
                nodes[_count + l] = new Node { Layer = l, Pointer = layers[l + 1].Length };

            // Stable sort intervals according to layer
            var comparer = ComparerFactory<Node>.CreateComparer((x, y) => x.Layer - y.Layer);
            Sorting.Timsort(nodes, comparer);

            // Fix pointers
            for (int l = 0, offset = 0; l < layerCount; ++l)
            {
                var start = offset;
                // Plus the sentinel node
                offset += layers[l].Length + 1;

                for (var i = start; i < offset; ++i)
                    nodes[i].Length += offset;
            }
        }

        private static int binaryLayerSearch(ArrayList<Node> layers, I interval, int current)
        {
            Contract.Requires(layers != null);
            Contract.Requires(interval != null);
            Contract.Requires(layers.Last.Interval == null);
            // The high endpoints of the last interval in each layer is sorted
            Contract.Requires(Contract.ForAll(1, layers.Count - 1, l => layers[l].Interval.CompareHigh(layers[l - 1].Interval) < 0));
            // Result is non-negative and less than layer count
            Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() < layers.Count);
            // The last interval in the layer has a high less than or equal to interval's high
            Contract.Ensures(layers[Contract.Result<int>()].Interval == null || layers[Contract.Result<int>()].Interval.CompareHigh(interval) <= 0);
            // The last interval in the layer below has a high greater than interval's high
            Contract.Ensures(Contract.Result<int>() == 0 || layers[Contract.Result<int>() - 1].Interval.CompareHigh(interval) > 0);

            var lower = 0;
            var upper = layers.Count - 1;

            do
            {
                var compare = layers[current].Interval.CompareHigh(interval);

                // interval contains the last interval
                if (compare < 0)
                    upper = current;
                // The last interval contains interval
                else if (compare > 0)
                    lower = current + 1;
                // We have found an interval with the same high
                else
                    return current;

                // Binarily pick the next layer to check
                current = lower + (upper - lower >> 1);
            } while (lower < upper);

            return lower;
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

            return _nodes[0].Interval;
        }

        #endregion

        #region Interval Collection

        #region Properties

        #region Data Structure Properties

        // TODO: Support user setting the value
        /// <inheritdoc/>
        public bool AllowsOverlaps { get { return true; } }

        // TODO: Support user setting the value
        /// <inheritdoc/>
        public bool AllowsReferenceDuplicates { get { return true; } }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public IInterval<T> Span { get { return _span; } }

        /// <inheritdoc/>
        public I LowestInterval { get { return _nodes[0].Interval; } }

        /// <inheritdoc/>
        public IEnumerable<I> LowestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var lowestInterval = LowestInterval;
                yield return lowestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = 1; i < _firstLayerCount; i++)
                {
                    var interval = _nodes[i].Interval;
                    if (interval.CompareLow(lowestInterval) == 0)
                        yield return interval;
                    else
                        yield break;
                }
            }
        }

        /// <inheritdoc/>
        public I HighestInterval { get { return _nodes[_firstLayerCount - 1].Interval; } }

        /// <inheritdoc/>
        public IEnumerable<I> HighestIntervals
        {
            get
            {
                if (IsEmpty)
                    yield break;

                var highestInterval = HighestInterval;
                yield return highestInterval;

                // Iterate through bottom layer as long as the intervals share a low
                for (var i = _firstLayerCount - 2; i >= 0; i--)
                {
                    var interval = _nodes[i].Interval;
                    if (interval.CompareHigh(highestInterval) == 0)
                        yield return interval;
                    else
                        yield break;
                }
            }
        }

        #region Maximum Depth

        /// <inheritdoc/>
        public int MaximumDepth
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= _layerCount);

                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(ref _intervalOfMaximumDepth);

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
                Contract.Ensures(_maximumDepth >= _layerCount);

                // If the Maximum Depth is below 0, then the interval of maximum depth has not been set yet
                Contract.Assert(_maximumDepth >= 0 || _intervalOfMaximumDepth == null);

                if (_maximumDepth < 0)
                    _maximumDepth = Sorted.MaximumDepth(ref _intervalOfMaximumDepth);

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
            return Sorted.Where(filter).MaximumDepth(ref intervalOfMaximumDepth);
        }

        #endregion

        #endregion

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator() { return Sorted.GetEnumerator(); }

        /// <inheritdoc/>
        public IEnumerable<I> Sorted { get { return intervalArray; } }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            return FindOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            int lower = 0, upper = _firstLayerCount;

            // Make sure first and last don't point at the same interval (theorem 2)
            while (lower < upper)
            {
                var first = lower;

                // The first interval doesn't overlap we need to search for it
                if (!_nodes[first].Interval.Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    first = findFirst(query, ++first, upper);

                    // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
                    if (upper <= first || !_nodes[first].Interval.Overlaps(query))
                        yield break;
                }

                // We can use first as lower to minimize search area
                var last = findLast(query, first, upper);

                // Save values for next iteration
                lower = _nodes[first].Pointer;
                upper = _nodes[last].Pointer;

                while (first < last)
                    yield return _nodes[first++].Interval;
            }
        }

        // TODO: Decide on using either start/end or lower/upper.
        private int findFirst(IInterval<T> query, int lower, int upper)
        {
            Contract.Requires(query != null);
            // Bounds must be in bounds
            Contract.Requires(0 <= lower && lower <= _nodes.Length);
            Contract.Requires(0 <= upper && upper <= _nodes.Length);
            // Lower and upper must be in the same layer
            Contract.Requires(Contract.ForAll(lower, upper, i => _nodes[i].Interval != null));
            
            // Either no interval overlaps or the interval at index result is the first overlap
            Contract.Ensures(
                Contract.Result<int>() < lower ||
                upper <= Contract.Result<int>() ||
                Contract.ForAll(lower, upper, i => !_nodes[i].Interval.Overlaps(query)) ||
                _nodes[Contract.Result<int>()].Interval.Overlaps(query) && Contract.ForAll(lower, Contract.Result<int>(), i => !_nodes[i].Interval.Overlaps(query))
            );

            int min = lower - 1, max = upper;

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1); // Divide by 2, by shifting one to the left

                var interval = _nodes[middle].Interval;

                var compare = query.Low.CompareTo(interval.High);

                if (compare < 0 || compare == 0 && query.LowIncluded && interval.HighIncluded)
                    max = middle;
                else
                    min = middle;
            }

            return max;
        }

        private int findLast(IInterval<T> query, int lower, int upper)
        {
            Contract.Requires(query != null);
            // Bounds must be in bounds
            Contract.Requires(0 <= lower && lower <= _nodes.Length);
            Contract.Requires(0 <= upper && upper <= _nodes.Length);
            // Lower and upper must be in the same layer
            Contract.Requires(Contract.ForAll(lower, upper, i => _nodes[i].Interval != null));

            // Either no interval overlaps or the interval at index result is the first overlap
            Contract.Ensures(
                Contract.Result<int>() < lower ||
                upper <= Contract.Result<int>() ||
                Contract.ForAll(lower, upper, i => !_nodes[i].Interval.Overlaps(query)) ||
                _nodes[Contract.Result<int>() - 1].Interval.Overlaps(query) && Contract.ForAll(Contract.Result<int>(), upper, i => !_nodes[i].Interval.Overlaps(query))
            );

            int min = lower - 1, max = upper;

            while (min + 1 < max)
            {
                var middle = min + ((max - min) >> 1); // Divide by 2, by shifting one to the left

                var interval = _nodes[middle].Interval;

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return max;
        }

        #region Sorted

        
        // TODO: Solve the sorted enumeration problem!
        /// <summary>
        /// Create an enumerable, enumerating all intervals in the collection that overlap the query point in sorted order.
        /// </summary>
        /// <param name="query">The query point.</param>
        /// <returns>All intervals that overlap the query point.</returns>
        public IEnumerable<I> FindOverlapsSorted(T query)
        {
            Contract.Requires(query != null);

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
            // TODO: Fix! See Sorted enumerator

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

                if (firstOverlaps[layer] < 0)
                    firstOverlaps[layer] = findFirst(query, start, end);

                if (firstOverlaps[layer] >= end)
                    continue;
                if (start < firstOverlaps[layer])
                    start = firstOverlaps[layer];

                // Iterate through all overlaps
                while (start < end && _nodes[start].Interval.CompareLowHigh(query) <= 0)
                {
                    yield return _nodes[start].Interval;

                    // If this and the next interval point to different intervals in the next layer, we need to swap layer
                    if (_nodes[start].Pointer < _nodes[start + 1].Pointer)
                    {
                        // Push the current values
                        stack[i++] = start + 1;
                        stack[i++] = end;
                        // Push the values for the next layer
                        stack[i++] = _nodes[start].Pointer;
                        stack[i++] = _nodes[start + 1].Pointer;
                        break;
                    }

                    start++;
                }
            }
        }

        #endregion

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public bool FindOverlap(T query, out I overlap)
        {
            return FindOverlap(new IntervalBase<T>(query), out overlap);
        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, out I overlap)
        {
            overlap = null;

            // No overlap if query is null, collection is empty, or query doesn't overlap collection
            if (IsEmpty || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(query, 0, _firstLayerCount);

            // Check if index is in bound and if the interval overlaps the query
            var result = 0 <= i && i < _firstLayerCount && _nodes[i].Interval.Overlaps(query);

            if (result)
                overlap = _nodes[i].Interval;

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
            return countOverlaps(query);
        }

        private int countOverlaps(IInterval<T> query)
        {
            Contract.Requires(query != null);

            if (IsEmpty)
                return 0;

            int lower = 0, upper = _firstLayerCount, count = 0;

            while (lower < upper)
            {
                // The first interval doesn't overlap we need to search for it
                // TODO: Can we tighten the check here? Like i.low < q.high...
                if (!_nodes[lower].Interval.Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    lower = findFirst(query, ++lower, upper);

                    // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
                    // TODO: Can we tighten the check here? Like i.low < q.high...
                    if (upper <= lower || !_nodes[lower].Interval.Overlaps(query))
                        return count;
                }

                // We can use first as lower to speed up the search
                upper = findLast(query, lower, upper);

                count += upper - lower;

                lower = _nodes[lower].Pointer;
                upper = _nodes[upper].Pointer;
            }

            return count;
        }

        #endregion

        #region Gaps

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> Gaps
        {
            get
            {
                if (IsEmpty)
                    return Enumerable.Empty<IInterval<T>>();

                return _nodes.Select(n => n.Interval).Take(_firstLayerCount).Gaps();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IInterval<T>> FindGaps(IInterval<T> query)
        {
            return findOverlapsInFirstLayer(query).Gaps(query);
        }

        private IEnumerable<I> findOverlapsInFirstLayer(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            var last = _firstLayerCount;
            var first = findFirst(query, 0, last);

            // Cache variables to speed up iteration
            I interval;
            while (first < last && (interval = _nodes[first++].Interval).CompareLowHigh(query) <= 0)
                yield return interval;
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
        public string Graphviz
        {
            get
            {
                return String.Format("digraph LayeredContainmentList {{\n\trankdir=BT;\n\tnode [shape=record];\n\n{0}\n}}", graphviz());
            }
        }

        private string graphviz()
        {
            // TODO
            // Auto-generated contracts to shut static analysis up
            //Contract.Requires(IsEmpty || 1 < _intervalLayers.Length);
            //Contract.Requires(IsEmpty || _layerCount == _intervalLayers.Length || Count != _firstLayerCount || 0 <= (_firstLayerCount - 1));
            //Contract.Requires(IsEmpty || _layerCount == _intervalLayers.Length || Count != _firstLayerCount || 0 >= _firstLayerCount || 0 >= _firstLayerCount || 0 < _intervalLayers.Length);

            var s = String.Empty;

            var layer = 0;
            int lower = 0, upper = _firstLayerCount;

            while (lower < upper)
            {
                var l = new ArrayList<string>();
                var p = String.Empty;
                for (var i = 0; i < upper; i++)
                {
                    l.Add(String.Format("<n{0}> {0}: {1}", i, _nodes[i].Interval));

                    p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, i, layer + 1, _nodes[i].Pointer);
                }

                // Sentinel node
                l.Add(String.Format("<n{0}> {0}: *", upper));
                p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, upper, layer + 1, _nodes[upper - 1].Pointer);

                s += String.Format("\tlayer{0} [fontname=consola, label=\"{1}\"];\n\t{2}\n", layer, String.Join("|", l.ToArray()), p);


                lower = _nodes[lower].Pointer;
                upper = _nodes[upper].Pointer;
                layer++;
            }

            s += String.Format("\tlayer{0} [fontname=consola, label=\"<n0> 0: *\"];", layer);

            return s;
        }

        #endregion
    }
}