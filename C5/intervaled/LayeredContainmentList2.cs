using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.intervaled
{
    public class LayeredContainmentList2<T> : CollectionValueBase<IInterval<T>>, IStaticIntervaled<T> where T : IComparable<T>
    {
        private readonly int _count;
        private readonly int _firstLayerCount;

        private readonly IInterval<T>[][] _intervalLayers;
        private readonly int[][] _pointerLayers;

        private readonly IInterval<T> _span;

        #region Node nested classes

        struct Node
        {
            internal IInterval<T> Interval { get; private set; }
            internal int Pointer { get; private set; }

            internal Node(IInterval<T> interval, int pointer)
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
        }

        #endregion

        #region Constructor

        public LayeredContainmentList2(IEnumerable<IInterval<T>> intervals)
        {
            // Make intervals to array to allow fast sorting and counting
            var intervalArray = intervals as IInterval<T>[] ?? intervals.ToArray();

            // Only do the work if we have something to work with
            if (!intervalArray.IsEmpty())
            {
                // Count intervals so we can use it later on
                _count = intervalArray.Length;

                // Sort intervals
                var comparer = ComparerFactory<IInterval<T>>.CreateComparer(IntervalExtensions.CompareTo);
                Sorting.IntroSort(intervalArray, 0, _count, comparer);

                // Put intervals in the arrays
                var layers = createLayers(intervalArray);

                // Create the list that contains the containment layers
                _intervalLayers = new IInterval<T>[layers.Count][];
                _pointerLayers = new int[layers.Count][];
                _firstLayerCount = layers[0].Count - 1;

                // Create each containment layer
                for (var i = 0; i < layers.Count - 1; i++)
                {
                    var count = layers[i].Count; // Subtract one for the dummy node
                    _intervalLayers[i] = new IInterval<T>[count];
                    _pointerLayers[i] = new int[count];

                    var j = 0;
                    foreach (var node in layers[i])
                    {
                        _intervalLayers[i][j] = node.Interval;
                        _pointerLayers[i][j] = node.Pointer;
                        j++;
                    }
                }

                // Save the span once
                _span = new IntervalBase<T>(_intervalLayers[0][0], _intervalLayers[0][_firstLayerCount - 1]);
            }
        }

        private ArrayList<ArrayList<Node>> createLayers(IInterval<T>[] intervals)
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
                    if (layers[layer - 1].Last.Interval.Contains(interval))
                        break;

                    layer--;
                }

                // Check if interval will be contained in the next layer
                while (!layers[layer].IsEmpty && layers[layer].Last.Interval.Contains(interval))
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

        public override IInterval<T> Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _intervalLayers.First().First();
        }

        #endregion

        public int CountOverlaps(IInterval<T> query)
        {
            // Break if we won't find any overlaps
            if (query == null || IsEmpty)
                return 0;

            return countOverlaps(0, 0, _firstLayerCount, query);
        }

        private int countOverlaps(int layer, int lower, int upper, IInterval<T> query)
        {
            var count = 0;

            while (lower < upper)
            {
                var first = lower;

                // The first interval doesn't overlap we need to search for it
                if (!_intervalLayers[layer][first].Overlaps(query))
                {
                    // We know first doesn't overlap so we can increment it before searching
                    first = findFirst(layer, ++first, upper, query);

                    // If index is out of bound, or found interval doesn't overlap, then the layer won't contain any overlaps
                    if (upper <= first || !_intervalLayers[layer][first].Overlaps(query))
                        return count;
                }

                // We can use first as lower to speed up the search
                var last = findLast(layer, first, upper, query);

                lower = _pointerLayers[layer][first];
                upper = _pointerLayers[layer][last];
                layer++;

                count += last - first;
            }

            return count;
        }

        /// <summary>
        /// Will return the index of the first interval that overlaps the query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private int findFirst(int layer, int lower, int upper, IInterval<T> query)
        {
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

        public override IEnumerator<IInterval<T>> GetEnumerator()
        {
            if (IsEmpty)
                return (new IInterval<T>[] { }).Cast<IInterval<T>>().GetEnumerator();

            return getEnumerator(0, 0, _firstLayerCount);
        }

        private IEnumerator<IInterval<T>> getEnumerator(int level, int start, int end)
        {
            while (start < end)
            {
                var node = _intervalLayers[level][start];

                yield return node;

                // Check if we are at the last node
                if (_pointerLayers[level][start] < _pointerLayers[level][start + 1])
                {
                    var child = getEnumerator(level + 1, _pointerLayers[level][start], _pointerLayers[level][start + 1]);

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

        public IEnumerable<IInterval<T>> FindOverlaps(T query)
        {
            // Break if we won't find any overlaps
            if (ReferenceEquals(query, null) || IsEmpty)
                return Enumerable.Empty<IInterval<T>>();

            return FindOverlaps(new IntervalBase<T>(query));
        }

        public IEnumerable<IInterval<T>> FindOverlaps(IInterval<T> query)
        {
            // Break if we won't find any overlaps
            if (query == null || IsEmpty)
                yield break;

            int layer = 0, lower = 0, upper = _firstLayerCount;

            // Make sure first and last don't point at the same interval (theorem 2)
            while (lower < upper)
            {
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

                // We can use first as lower to speed up the search
                var last = findLast(layer, first, upper, query);

                // Save values for next iteration
                lower = _pointerLayers[layer][first]; // 0
                upper = _pointerLayers[layer][last]; // _counts[layer]
                layer++;

                while (first < last)
                    yield return currentLayer[first++];
            }
        }

        public bool OverlapExists(IInterval<T> query)
        {
            // No overlap if query is null, collection is empty, or query doesn't overlap collection
            if (query == null || IsEmpty || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(0, 0, _firstLayerCount, query);

            // Check if index is in bound and if the interval overlaps the query
            return 0 <= i && i < _firstLayerCount && _intervalLayers[0][i].Overlaps(query);
        }

        public string Graphviz()
        {
            return String.Format("digraph LayeredContainmentList2 {{\n\trankdir=BT;\n\tnode [shape=record];\n\n{0}\n}}", graphviz());
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
                    // TODO: Would it be worth replacing with a custom formatter for the null values?
                    // http://stackoverflow.com/questions/7689040/can-i-format-null-values-in-string-format
                    var interval = _intervalLayers[layer][i] != null ? _intervalLayers[layer][i].ToString() : "*";
                    l.Add(String.Format("<n{0}> {0}: {1}", i, interval));

                    p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, i, layer + 1, _pointerLayers[layer][i]);
                }

                s += String.Format("\tlayer{0} [fontname=consola, label=\"{1}\"];\n\t{2}\n", layer, String.Join("|", l.ToArray()), p);


                lower = _pointerLayers[layer][lower];
                upper = _pointerLayers[layer][upper];
                layer++;
            }

            s += String.Format("\tlayer{0} [fontname=consola, label=\"<n0> 0: *\"];", layer);

            return s;
        }
    }
}
