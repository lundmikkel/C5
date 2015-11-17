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
    public class LayeredContainmentList<I, T> : SortedIntervalCollectionBase<I, T>
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

        private readonly I[] _intervals;
        private readonly int[] _pointers;
        private readonly I[] _intervalArray;

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
            Contract.Invariant(IsEmpty || _layerCount == _intervals.Count(x => x == null));
            // The first layer's count is non-negative and at most as big as count
            Contract.Invariant(0 <= _firstLayerCount && _firstLayerCount <= _count);
            // Either the collection is empty or there are one layer or more
            Contract.Invariant(IsEmpty || _layerCount >= 1);
            // Either all intervals are in the first layer, or there are more than one layer
            Contract.Invariant(_count == _firstLayerCount || _layerCount > 1);
            // The array is null if empty
            Contract.Invariant(!IsEmpty || _intervals == null);
            Contract.Invariant(!IsEmpty || _pointers == null);

            // No layer is empty
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _intervals.Length - 1, i => _intervals[i] != null || _intervals[i + 1] != null));
            // Each layer is sorted
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _intervals.Length - 1, i => _intervals[i] == null || _intervals[i + 1] == null || _intervals[i].CompareTo(_intervals[i + 1]) <= 0));
            // Each layer is sorted on both low and high endpoint
            Contract.Invariant(IsEmpty || Contract.ForAll(0, _intervals.Length - 1, i => _intervals[i] == null || _intervals[i + 1] == null || _intervals[i].CompareLow(_intervals[i + 1]) <= 0 && _intervals[i].CompareHigh(_intervals[i + 1]) <= 0));
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

            var lower = _pointers[previousLower];
            var upper = _pointers[previousUpper];

            while (lower < upper)
            {
                for (var i = lower; i < upper; ++i)
                    if (!_intervals.Skip(previousLower).Take(previousUpper - previousLower).Any(x => x.StrictlyContains(_intervals[i])))
                        return false;

                previousLower = lower;
                previousUpper = upper;
                lower = _pointers[previousLower];
                upper = _pointers[previousUpper];
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
        public LayeredContainmentList(IEnumerable<I> intervals)
        {
            // TODO: Calculate the maximum depth during construction based on argument

            // Make intervals to array to allow fast sorting and counting
            _intervalArray = intervals as I[] ?? intervals.ToArray();

            // Stop if we have no intervals
            if ((_count = _intervalArray.Length) == 0)
                return;

            constructLayers(ref _intervalArray, out _intervals, out _pointers, out _layerCount, out _firstLayerCount);

            // Cached values
            _span = new IntervalBase<T>(_intervals[0], _intervals[_firstLayerCount - 1]);
        }

        private void constructLayers(ref I[] intervalArray, out I[] intervals, out int[] pointers, out int layerCount, out int firstLayerCount)
        {
            // Sort intervals
            Sorting.Timsort(intervalArray, IntervalExtensions.CreateComparer<I, T>());

            // Pointer is used to store the layers length
            var layers = new ArrayList<Node> {
                // The main layer
                new Node {
                    Interval = intervalArray[0],
                    Length = 1
                },
                // An extra empty layer
                new Node()
            };

            var nodes = new Node[_count];
            // Add first interval to layers manually
            nodes[0] = new Node { Interval = intervalArray[0], Layer = 0, Pointer = 0 };

            // Figure out each intervals placement
            for (var i = 1; i < _count; ++i)
            {
                var interval = intervalArray[i];
                var layer = gallopLayerSearch(layers, interval);

                // Add a node if we are about to populate the last layer
                if (layer == layers.Count - 1)
                    layers.Add(new Node());

                // Store the layer's last interval and increment its size counter
                layers[layer].Interval = interval;
                layers[layer].Length++;

                // Slowly add each interval etc. to nodes array
                nodes[i] = new Node { Interval = interval, Layer = layer, Pointer = layers[layer + 1].Length };
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

            intervals = new I[_count + layerCount];
            pointers = new int[_count + layerCount];

            for (var i = 0; i < nodes.Length; ++i)
            {
                intervals[i] = nodes[i].Interval;
                pointers[i] = nodes[i].Pointer;
            }
        }

        private static int gallopLayerSearch(ArrayList<Node> layers, I query)
        {
            Contract.Requires(query != null);
            Contract.Requires(layers.Last.Interval == null);
            // The high endpoints of the last interval in each layer is sorted
            Contract.Requires(Contract.ForAll(1, layers.Count - 1, l => layers[l].Interval.CompareHigh(layers[l - 1].Interval) < 0));
            // Result is non-negative and less than layer count
            Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() < layers.Count);
            // The last interval in the layer has a high less than or equal to interval's high
            Contract.Ensures(layers[Contract.Result<int>()].Interval == null || layers[Contract.Result<int>()].Interval.CompareHigh(query) <= 0);
            // The last interval in the layer below has a high greater than interval's high
            Contract.Ensures(Contract.Result<int>() == 0 || layers[Contract.Result<int>() - 1].Interval.CompareHigh(query) > 0);

            var jump = 1;
            var lower = 0;
            var upper = layers.Count - 1;

            while (true)
            {
                var compare = layers[lower].Interval.CompareHigh(query);

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

        private static int binaryLayerSearch(ArrayList<Node> layers, I query, int lower, int upper)
        {
            int min = lower, max = upper;

            while (min < max)
            {
                var mid = min + (max - min >> 1);

                var compare = layers[mid].Interval.CompareHigh(query);

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

            return _intervals[0];
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
        public override I LowestInterval { get { return _intervals[0]; } }

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
                for (var i = 1; i < _firstLayerCount; i++)
                {
                    var interval = _intervals[i];
                    if (interval.CompareLow(lowestInterval) == 0)
                        yield return interval;
                    else
                        yield break;
                }
            }
        }

        /// <inheritdoc/>
        public override I HighestInterval { get { return _intervals[_firstLayerCount - 1]; } }

        /// <inheritdoc/>
        public override IEnumerable<I> HighestIntervals
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
                    var interval = _intervals[i];
                    if (interval.CompareHigh(highestInterval) == 0)
                        yield return interval;
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
                Contract.Ensures(Contract.Result<int>() >= _layerCount);

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
                Contract.Ensures(_maximumDepth >= _layerCount);

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
                foreach (var interval in _intervalArray)
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
            while (i < _count && _intervalArray[i].IntervalEquals(query))
                yield return _intervalArray[i++];
        }

        private int indexOf(IInterval<T> query)
        {
            int lower = 0, upper = _count - 1;

            while (lower <= upper)
            {
                var mid = lower + (upper - lower >> 1);
                var compareTo = _intervalArray[mid].CompareTo(query);

                if (compareTo < 0)
                    lower = mid + 1;
                else if (compareTo > 0)
                    upper = mid - 1;
                else if (lower != mid)
                    upper = mid;
                else
                    return mid;
            }

            return ~lower;
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            int lower = 0, upper = _firstLayerCount;

            // Make sure first and last don't point at the same interval (theorem 2)
            while (lower < upper)
            {
                var first = lower;

                // The first interval doesn't overlap we need to search for it
                if (!_intervals[first].Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    first = findFirst(query, ++first, upper);

                    // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
                    if (upper <= first || !(_intervals[first].CompareLowHigh(query) <= 0))
                        yield break;
                }

                // We can use first as lower to minimize search area
                var last = findLast(query, first, upper);

                // Save values for next iteration
                lower = _pointers[first];
                upper = _pointers[last];

                while (first < last)
                    yield return _intervals[first++];
            }
        }

        [Pure]
        private int findFirst(IInterval<T> query, int lower, int upper)
        {
            Contract.Requires(query != null);
            // Bounds must be in bounds
            Contract.Requires(0 <= lower && lower <= _intervals.Length);
            Contract.Requires(0 <= upper && upper <= _intervals.Length);
            // Lower and upper must be in the same layer
            Contract.Requires(Contract.ForAll(lower, upper, i => _intervals[i] != null));

            // Either no interval overlaps or the interval at index result is the first overlap
            Contract.Ensures(
                Contract.Result<int>() < lower ||
                upper <= Contract.Result<int>() ||
                Contract.ForAll(lower, upper, i => !_intervals[i].Overlaps(query)) ||
                _intervals[Contract.Result<int>()].Overlaps(query) && Contract.ForAll(lower, Contract.Result<int>(), i => !_intervals[i].Overlaps(query))
            );

            int min = lower, max = upper;

            while (min < max)
            {
                var mid = min + (max - min >> 1); // Divide by 2, by shifting one to the left

                if (_intervals[mid].CompareHighLow(query) < 0)
                    min = mid + 1;
                else
                    max = mid;
            }

            return min;
        }

        [Pure]
        private int findLast(IInterval<T> query, int lower, int upper)
        {
            Contract.Requires(query != null);
            // Bounds must be in bounds
            Contract.Requires(0 <= lower && lower <= _intervals.Length);
            Contract.Requires(0 <= upper && upper <= _intervals.Length);
            // Lower and upper must be in the same layer
            Contract.Requires(Contract.ForAll(lower, upper, i => _intervals[i] != null));

            // Either no interval overlaps or the interval at index result is the first overlap
            Contract.Ensures(
                Contract.Result<int>() < lower ||
                upper <= Contract.Result<int>() ||
                Contract.ForAll(lower, upper, i => !_intervals[i].Overlaps(query)) ||
                _intervals[Contract.Result<int>() - 1].Overlaps(query) && Contract.ForAll(Contract.Result<int>(), upper, i => !_intervals[i].Overlaps(query))
            );

            int min = lower, max = upper;

            while (min < max)
            {
                var mid = min + (max - min >> 1); // Divide by 2, by shifting one to the left

                if (query.CompareHighLow(_intervals[mid]) < 0)
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
            var i = findFirst(query, 0, _firstLayerCount);

            // Check if index is in bound and if the interval overlaps the query
            var result = 0 <= i && i < _firstLayerCount && _intervals[i].Overlaps(query);

            if (result)
                overlap = _intervals[i];

            return result;
        }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public override int CountOverlaps(T query)
        {
            return countOverlaps(new IntervalBase<T>(query));
        }

        /// <inheritdoc/>
        public override int CountOverlaps(IInterval<T> query)
        {
            return countOverlaps(query);
        }

        private int countOverlaps(IInterval<T> query)
        {
            Contract.Requires(query != null);

            if (IsEmpty)
                return 0;

            int first = 0, last = _firstLayerCount, count = 0;

            while (first < last)
            {
                // The first interval doesn't overlap we need to search for it
                if (!_intervals[first].Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    first = findFirst(query, ++first, last);

                    // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
                    if (last <= first || !(_intervals[first].CompareLowHigh(query) <= 0))
                        return count;
                }

                // We can use first as lower to speed up the search
                last = findLast(query, first, last);

                count += last - first;

                first = _pointers[first];
                last = _pointers[last];
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

                return _intervals.Take(_firstLayerCount).Gaps();
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
            var first = findFirst(query, 0, last);

            // Cache variables to speed up iteration
            I interval;
            while (first < last && (interval = _intervals[first++]).CompareLowHigh(query) <= 0)
                yield return interval;
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
                    l.Add(String.Format("<n{0}> {0}: {1}", i, _intervals[i]));

                    p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, i, layer + 1, _pointers[i]);
                }

                // Sentinel node
                l.Add(String.Format("<n{0}> {0}: *", upper));
                p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, upper, layer + 1, _pointers[upper - 1]);

                s += String.Format("\tlayer{0} [fontname=consola, label=\"{1}\"];\n\t{2}\n", layer, String.Join("|", l.ToArray()), p);


                lower = _pointers[lower];
                upper = _pointers[upper];
                layer++;
            }

            s += String.Format("\tlayer{0} [fontname=consola, label=\"<n0> 0: *\"];", layer);

            return s;
        }

        #endregion
    }
}