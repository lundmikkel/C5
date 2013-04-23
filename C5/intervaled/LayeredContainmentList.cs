using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.intervaled
{
    class LayeredContainmentList<T> : CollectionValueBase<IInterval<T>>, IStaticIntervaled<T> where T : IComparable<T>
    {
        private readonly int _count;
        private readonly Node[][] _layers;
        private readonly int[] _counts;
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

            public override string ToString()
            {
                return Interval != null ? Interval.ToString() : "*";
            }
        }

        #endregion

        #region Constructor

        public LayeredContainmentList(IEnumerable<IInterval<T>> intervals)
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

                // Create the list that contains the containment lists
                _layers = new Node[layers.Count][];
                _counts = new int[layers.Count];
                // Create each containment list
                for (var i = 0; i < _counts.Length; i++)
                {
                    _layers[i] = layers[i].ToArray();
                    _counts[i] = layers[i].Count - 1; // Subtract one for the dummy node
                }

                // Save the span once
                _span = new IntervalBase<T>(_layers[0][0].Interval, _layers[0][_counts[0] - 1].Interval);
            }
        }

        private ArrayList<ArrayList<Node>> createLayers(IInterval<T>[] intervals)
        {
            // Use a stack to keep track of current containment
            IStack<IInterval<T>> stack = new ArrayList<IInterval<T>>();
            var layers = new ArrayList<ArrayList<Node>> { new ArrayList<Node>(intervals.Length) };

            foreach (var interval in intervals)
            {
                // Track containment
                while (!stack.IsEmpty)
                {
                    // If the interval is contained in the top of the stack, leave it...
                    if (stack.Last().Contains(interval))
                        break;

                    stack.Pop();
                }

                // Check if interval will be contained in the next layer
                while (!layers[stack.Count].IsEmpty && layers[stack.Count].Last.Interval.Contains(interval))
                    stack.Push(layers[stack.Count].Last.Interval);

                stack.Push(interval);

                while (layers.Count < stack.Count + 1)
                    layers.Add(new ArrayList<Node>());

                var list = stack.Count - 1;

                layers[list].Add(new Node(interval, layers[list + 1].Count));
            }

            var lastCount = 0;
            for (var i = layers.Count - 1; i >= 0; i--)
            {
                layers[i].Add(new Node(lastCount));
                lastCount = layers[i].Count - 1;
            }

            return layers;
        }

        /// <summary>
        /// Analyze the intervals to find out how many layers, we need and how big each is
        /// </summary>
        /// <param name="intervalArray">Sorted intervals that should be analyzed</param>
        private static int[] analyzeContainment(IInterval<T>[] intervalArray)
        {
            // Use a stack to keep track of current containment
            IStack<IInterval<T>> stack = new ArrayList<IInterval<T>>();
            var listCounts = new ArrayList<int>();

            for (int i = 0; i < intervalArray.Length; i++)
            {
                var interval = intervalArray[i];

                if (i > 21725)
                    i = i;

                // Track containment
                while (!stack.IsEmpty)
                {
                    var container = stack.Last();

                    // If the interval is contained in the top of the stack, leave it...
                    if (container.Contains(interval))
                        break;

                    stack.Pop();
                }
                stack.Push(interval);

                if (listCounts.Count < stack.Count)
                    listCounts.Add(0);

                // Increament the count for the containment list on the right layer
                listCounts[stack.Count - 1]++;
            }

            return listCounts.ToArray();
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
            if (Count == 0)
                throw new NoSuchItemException();

            return _layers.First().First().Interval;
        }

        #endregion

        public int CountOverlaps(IInterval<T> query)
        {
            // Break if we won't find any overlaps
            if (ReferenceEquals(query, null) || IsEmpty)
                return 0;

            return overlapCount(0, 0, _counts[0], query);
        }

        private int overlapCount(int layer, int lower, int upper, IInterval<T> query)
        {
            // Theorem 2
            if (lower >= upper)
                return 0;

            var first = lower;

            // The first interval doesn't overlap we need to search for it
            if (!_layers[layer][first].Interval.Overlaps(query))
            {
                // We know first doesn't overlap so we can increment it before searching
                first = searchHighInLows(layer, ++first, upper, query);

                // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
                if (first < lower || upper <= first || !_layers[layer][first].Interval.Overlaps(query))
                    return 0;
            }

            // We can use first as lower to speed up the search
            var last = searchLowInHighs(layer, first, upper, query);

            return last - first + overlapCount(layer + 1, _layers[layer][first].Pointer, _layers[layer][last].Pointer, query);
        }

        private int searchLowInHighs(int layer, int lower, int upper, IInterval<T> query)
        {
            int min = lower, max = upper - 1;

            while (min <= max)
            {
                var middle = (max + min) >> 1; // Shift one is the same as dividing by 2

                var interval = _layers[layer][middle].Interval;

                if (query.Overlaps(interval))
                {
                    // We know we have an overlap, but we need to check if it's the last one
                    // Only if the next interval to the overlapping interval does not overlap, we have found the right one
                    if (middle == upper - 1 || !query.Overlaps(_layers[layer][middle + 1].Interval))
                        // The right interval is found, return the index for the next interval
                        return middle + 1;

                    // The previous interval overlap as well, move right
                    min = middle + 1;
                }
                else
                {
                    // The interval does not overlap, find out whether query is lower or higher
                    if (query.CompareTo(interval) < 0)
                        // The query is lower than the interval, move left
                        max = middle - 1;
                    else
                        // The query is higher than the interval, move right
                        min = middle + 1;
                }
            }

            return max;
        }

        public int searchFirst(int layer, int lower, int upper, IInterval<T> query)
        {
            int min = lower, max = upper - 1;

            while (min <= max)
            {
                var mid = min + (max - min) / 2;

                var interval = _layers[layer][mid].Interval;

                // Check if 
                if (interval.Overlaps(query))
                {
                    // TODO: Check that the next exists
                    var previousInterval = _layers[layer][mid - 1].Interval;

                    if (!previousInterval.Overlaps(query))
                        return mid;

                    max = mid - 1;
                }

                else if (query.CompareTo(interval) < 0)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        public int searchNextAfterLast(int layer, int lower, int upper, IInterval<T> query)
        {
            int min = lower, max = upper - 1;

            while (min <= max)
            {
                var mid = min + (max - min) / 2;

                var interval = _layers[layer][mid].Interval;

                // Check if 
                if (interval.Overlaps(query))
                {
                    // TODO: Check that the next exists
                    var nextInterval = _layers[layer][mid + 1].Interval;

                    if (!nextInterval.Overlaps(query))
                        return mid + 1;

                    min = mid + 1;
                }

                else if (query.CompareTo(interval) < 0)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Will return the index of the first interval that overlaps the query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private int searchHighInLows(int layer, int lower, int upper, IInterval<T> query)
        {
            // Upper is excluded so we subtract by one
            int min = lower, max = upper - 1;

            while (min <= max)
            {
                var middle = (max + min) >> 1; // Shift one is the same as dividing by 2

                var interval = _layers[layer][middle].Interval;

                if (query.Overlaps(interval))
                {
                    // Only if the previous interval to the overlapping interval does not overlap, we have found the right one
                    // TODO: Why middle == lower? For lower = 1, upper = 2, we stop instantly...
                    if (middle == lower || !query.Overlaps(_layers[layer][middle - 1].Interval))
                        // The right interval is found
                        return middle;

                    // The previous interval overlap as well, move left
                    max = middle - 1;
                }
                else
                {
                    // The interval does not overlap, find out whether query is lower or higher
                    if (query.CompareTo(interval) < 0)
                        // The query is lower than the interval, move left
                        max = middle - 1;
                    else
                        // The query is higher than the interval, move right
                        min = middle + 1;
                }
            }

            // We return min so we know if the query was lower or higher than the list
            return min;
        }

        public override IEnumerator<IInterval<T>> GetEnumerator()
        {
            return getEnumerator(0, 0, _counts[0]);
        }

        private IEnumerator<IInterval<T>> getEnumerator(int level, int start, int end)
        {
            while (start < end)
            {
                var node = _layers[level][start];

                yield return node.Interval;

                // Check if we are at the last node
                if (node.Pointer < _layers[level][start + 1].Pointer)
                {
                    var child = getEnumerator(level + 1, node.Pointer, _layers[level][start + 1].Pointer);

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
            if (ReferenceEquals(query, null) || IsEmpty)
                return Enumerable.Empty<IInterval<T>>();

            return findOverlaps(0, 0, _counts[0], query);
        }

        private IEnumerable<IInterval<T>> findOverlaps(int layer, int lower, int upper, IInterval<T> query)
        {
            // TODO: Check bounds to be lower < upper? I don't think we need to

            var first = lower;

            // The first interval doesn't overlap we need to search for it
            if (!_layers[layer][first].Interval.Overlaps(query))
            {
                // We know first doesn't overlap so we can increment it before searching
                first = searchFirst(layer, ++first, upper, query);

                // If index is out of bound, or found interval doesn't overlap, then the list won't contain any overlaps
                if (first < lower || upper <= first || !_layers[layer][first].Interval.Overlaps(query))
                    yield break;
            }

            // We can use first as lower to speed up the search
            var last = searchLowInHighs(layer, first, upper, query);

            // Make sure first and last don't point at the same interval (theorem 2)
            // TODO: Should this be moved to beginning of method and thereby removing the extra variables?
            var firstPointer = _layers[layer][first].Pointer;
            var nextPointer = _layers[layer][last].Pointer;
            if (firstPointer != nextPointer)
                // Find intervals in sublist
                foreach (var interval in findOverlaps(layer + 1, firstPointer, nextPointer, query))
                    yield return interval;

            while (first < last)
                yield return _layers[layer][first++].Interval;
        }

        public bool OverlapExists(IInterval<T> query)
        {
            // No overlap if query is null, collection is empty, or query doesn't overlap collection
            if (query == null || IsEmpty || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = searchHighInLows(0, 0, _counts[0], query);

            // Check if index is in bound and if the interval overlaps the query
            return 0 <= i && i < _counts[0] && _layers[0][i].Interval.Overlaps(query);
        }

        public string Graphviz()
        {
            return String.Format("digraph LayeredContainmentList {{\n\tnode [shape=record];\n\n{0}\n}}", graphviz());
        }

        private string graphviz()
        {
            var s = String.Empty;

            for (var layer = 0; layer < _counts.Length; layer++)
            {
                var l = new ArrayList<string>();
                var p = String.Empty;
                for (var i = 0; i <= _counts[layer]; i++)
                {
                    l.Add(String.Format("<n{0}> {0}: {1}", i, _layers[layer][i]));

                    p += String.Format("layer{0}:n{1} -> layer{2}:n{3};\n\t", layer, i, layer + 1, _layers[layer][i].Pointer);
                }

                s += String.Format("\tlayer{0} [label=\"{1}\"];\n\t{2}\n", layer, String.Join("|", l.ToArray()), p);
            }

            s += String.Format("\tlayer{0} [label=\"<n0> 0: *\"];", _counts.Length);

            return s;
        }
    }
}
