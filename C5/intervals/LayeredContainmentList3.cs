using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.intervals
{
    public class LayeredContainmentList3<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        private readonly int _count;
        private readonly Node[] _intervals;
        private readonly int _firstLayerCount;
        private readonly IInterval<T> _span;

        #region Node nested classes

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

            internal Node(int pointer)
                : this()
            {
                Pointer = pointer;
            }

            public override string ToString()
            {
                return String.Format("{0}/{1}", Interval != null ? Interval.ToString() : "*", Pointer);
            }
        }

        #endregion

        #region Constructor

        public LayeredContainmentList3(IEnumerable<I> intervals)
        {
            // Make intervals to array to allow fast sorting and counting
            var intervalArray = intervals as I[] ?? intervals.ToArray();

            // Only do the work if we have something to work with
            if (!intervalArray.IsEmpty())
            {
                // Count intervals so we can use it later on
                _count = intervalArray.Length;

                // Sort intervals
                var comparer = ComparerFactory<I>.CreateComparer((x, y) => x.CompareTo(y));
                Sorting.IntroSort(intervalArray, 0, _count, comparer);

                // Put intervals in the arrays
                var layers = createLayers(intervalArray);

                // Create the list that contains the containment layers
                _intervals = new Node[intervalArray.Length + layers.Count];
                _firstLayerCount = layers[0].Count - 1;

                var i = 0;
                var offset = 0;

                // Create each containment layer
                for (var l = 0; l < layers.Count - 1; l++)
                {
                    offset += layers[l].Count;
                    foreach (var node in layers[l])
                    {
                        _intervals[i++] = new Node(node.Interval, node.Pointer + offset);
                    }
                }

                // Save the span once
                _span = new IntervalBase<T>(_intervals[0].Interval, _intervals[_firstLayerCount - 1].Interval);
            }
        }

        private ArrayList<ArrayList<Node>> createLayers(I[] intervals)
        {
            // Use a stack to keep track of current containment
            var layer = 0;
            var layers = new ArrayList<ArrayList<Node>> { new ArrayList<Node>() };

            foreach (var interval in intervals)
            {
                // Track containment
                while (layer > 0)
                {
                    // If the interval is contained in the top of the stack, leave it...
                    if (layers[layer - 1].Last.Interval.StrictlyContains(interval))
                        break;

                    layer--;
                }

                // Check if interval will be contained in the next layer
                while (!layers[layer].IsEmpty && layers[layer].Last.Interval.StrictlyContains(interval))
                    layer++;

                layer++;

                while (layers.Count < layer + 1)
                    layers.Add(new ArrayList<Node>());

                layers[layer - 1].Add(new Node(interval, layers[layer].Count));
            }

            var lastCount = 0;
            for (var i = layers.Count - 1; i >= 0; i--)
            {
                layers[i].Add(new Node(lastCount));
                lastCount = layers[i].Count - 1;
            }

            return layers;
        }

        #endregion

        #region CollectionValue

        public override bool IsEmpty
        {
            get { return Count == 0; }
        }

        public override int Count { get { return _count; } }

        public override Speed CountSpeed
        {
            get { return Speed.Constant; }
        }

        public override I Choose()
        {
            if (Count == 0)
                throw new NoSuchItemException();

            return _intervals.First().Interval;
        }

        #endregion

        public int CountOverlaps(IInterval<T> query)
        {
            // Break if we won't find any overlaps
            if (ReferenceEquals(query, null) || IsEmpty)
                return 0;

            return countOverlaps(0, _firstLayerCount, query);
        }

        public bool Add(I interval)
        {
            throw new NotSupportedException();
        }

        public bool Remove(I interval)
        {
            throw new NotSupportedException();
        }

        private int countOverlaps(int lower, int upper, IInterval<T> query)
        {
            var count = 0;

            while (lower < upper)
            {
                var first = lower;

                // The first interval doesn't overlap we need to search for it
                if (!_intervals[first].Interval.Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    first = findFirst(++first, upper, query);

                    // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
                    if (upper <= first || !_intervals[first].Interval.Overlaps(query))
                        return count;
                }

                // We can use first as lower to speed up the search
                var last = findLast(first, upper, query);

                lower = _intervals[first].Pointer;
                upper = _intervals[last].Pointer;

                count += last - first;

            }
            return count;
        }

        /// <summary>
        /// Will return the index of the first interval that overlaps the query
        /// </summary>
        private int findFirst(int lower, int upper, IInterval<T> query)
        {
            int min = lower - 1, max = upper;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = _intervals[middle].Interval;

                var compare = query.Low.CompareTo(interval.High);

                if (compare < 0 || compare == 0 && query.LowIncluded && interval.HighIncluded)
                    max = middle;
                else
                    min = middle;
            }

            return max;
        }

        /// <summary>
        /// Will return the index of the last interval that overlaps the query
        /// </summary>
        private int findLast(int lower, int upper, IInterval<T> query)
        {
            int min = lower - 1, max = upper;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = _intervals[middle].Interval;

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return max;
        }

        public override IEnumerator<I> GetEnumerator()
        {
            if (IsEmpty)
                return (new I[] { }).Cast<I>().GetEnumerator();

            return getEnumerator(0, _firstLayerCount);
        }

        private IEnumerator<I> getEnumerator(int start, int end)
        {
            while (start < end)
            {
                var node = _intervals[start];

                yield return node.Interval;

                // Check if we are at the last node
                if (node.Pointer < _intervals[start + 1].Pointer)
                {
                    var child = getEnumerator(node.Pointer, _intervals[start + 1].Pointer);

                    while (child.MoveNext())
                        yield return child.Current;
                }

                start++;
            }
        }

        public IInterval<T> Span
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("An empty collection has no span");

                return _span;
            }
        }

        public int MaximumOverlap
        {
            get { throw new NotSupportedException(); }
        }

        public IEnumerable<I> FindOverlaps(T query)
        {
            // Break if we won't find any overlaps
            if (ReferenceEquals(query, null) || IsEmpty)
                return Enumerable.Empty<I>();

            return FindOverlaps(new IntervalBase<T>(query));
        }

        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            // Break if we won't find any overlaps
            if (ReferenceEquals(query, null) || IsEmpty)
                yield break;

            int lower = 0, upper = _firstLayerCount;

            // Make sure first and last don't point at the same interval (theorem 2)
            while (lower < upper)
            {
                var first = lower;

                // The first interval doesn't overlap we need to search for it
                if (!_intervals[first].Interval.Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    first = findFirst(++first, upper, query);

                    // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
                    if (upper <= first || !_intervals[first].Interval.Overlaps(query))
                        yield break;
                }

                // We can use first as lower to speed up the search
                var last = findLast(first, upper, query);

                // Save values for next iteration
                lower = _intervals[first].Pointer;
                upper = _intervals[last].Pointer;

                while (first < last)
                    yield return _intervals[first++].Interval;
            }
        }

        public bool FindOverlap(IInterval<T> query, ref I overlap)
        {
            // No overlap if query is null, collection is empty, or query doesn't overlap collection
            if (query == null || IsEmpty || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(0, _firstLayerCount, query);

            // Check if index is in bound and if the interval overlaps the query
            var result = 0 <= i && i < _firstLayerCount && _intervals[i].Interval.Overlaps(query);

            if (result)
                overlap = _intervals[i].Interval;

            return result;
        }

        public bool FindOverlap(T query, ref I overlap)
        {
            return FindOverlap(new IntervalBase<T>(query), ref overlap);
        }

        private IEnumerable<I> findOverlapsRecursive(int layer, int lower, int upper, IInterval<T> query)
        {
            // Make sure first and last don't point at the same interval (theorem 2)
            if (lower >= upper)
                yield break;

            var first = lower;

            // The first interval doesn't overlap we need to search for it
            if (!_intervals[first].Interval.Overlaps(query))
            {
                // We know first doesn't overlap so we can increment it before searching
                first = findFirst(++first, upper, query);

                // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
                if (upper <= first || !_intervals[first].Interval.Overlaps(query))
                    yield break;
            }

            // We can use first as lower to speed up the search
            var last = findLast(first, upper, query);

            // Find intervals in sublist
            foreach (var interval in findOverlapsRecursive(layer + 1, _intervals[first].Pointer, _intervals[last].Pointer, query))
                yield return interval;

            while (first < last)
                yield return _intervals[first++].Interval;
        }

        public string Graphviz()
        {
            return String.Format("digraph LayeredContainmentList3 {{\n\trankdir=BT;\n\tnode [shape=record];\n\n{0}\n}}", graphviz());
        }

        private string graphviz()
        {
            var s = String.Empty;

            var layer = 0;
            int lower = 0, upper = _firstLayerCount;

            while (lower < upper)
            {
                var l = new ArrayList<string>();
                var p = String.Empty;

                for (var i = 0; i <= upper; i++)
                {
                    l.Add(String.Format("<n{0}> {0}: {1}", i, _intervals[i]));

                    p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, i, layer + 1, _intervals[i].Pointer);
                }

                s += String.Format("\tlayer{0} [fontname=consola, label=\"{1}\"];\n\t{2}\n", layer, String.Join("|", l.ToArray()), p);

                lower = _intervals[lower].Pointer;
                upper = _intervals[upper].Pointer;
                layer++;
            }

            s += String.Format("\tlayer{0} [fontname=consola, label=\"<n0> 0: *\"];", layer);

            return s;
        }
    }
}
