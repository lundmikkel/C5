using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.intervals
{
    /// <summary>
    /// Extension methods used to find the median endpoint
    /// </summary>
    static class StaticIntervalTreeExtensions
    {
        /// <summary>
        /// Randomly shuffles a list in linear time
        /// </summary>
        /// <param name="list">The list</param>
        /// <typeparam name="T">The element type</typeparam>
        public static void Shuffle<T>(this IList<T> list)
        {
            var random = new Random();
            var n = list.Count;
            while (--n > 0)
                list.Swap(random.Next(n + 1), n);
        }

        /// <summary>
        /// Swaps two elements in a list
        /// </summary>
        /// <param name="list">The list</param>
        /// <param name="i">The index of the first element</param>
        /// <param name="j">The index of the second element</param>
        /// <typeparam name="T">The element type</typeparam>
        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    /// <summary>
    /// An implementation of a classic static interval tree as described by Berg et. al in "Computational Geometry - Algorithms and Applications"
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class StaticIntervalTree<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly Node _root;
        private readonly int _count;
        private IInterval<T> _span;
        private int _maximumOverlap = -1;

        #endregion

        #region Inner classes

        class Node
        {
            #region Fields

            internal T Key { get; private set; }
            // Left and right subtree
            internal Node Left { get; private set; }
            internal Node Right { get; private set; }
            // Left and right list of intersecting intervals for Key
            internal I[] LeftList { get; private set; }
            internal I[] RightList { get; private set; }

            private static IComparer<I> LeftComparer = IntervalExtensions.CreateComparer<I, T>();
            private static IComparer<I> RightComparer = IntervalExtensions.CreateReversedComparer<I, T>();

            #endregion

            #region Constructor

            internal Node(I[] intervals, ref IInterval<T> span)
            {
                Key = getKey(intervals);

                IList<I>
                    keyIntersections = new ArrayList<I>(),
                    lefts = null,
                    rights = null;

                // Compute I_mid and construct two sorted lists, LeftList and RightList
                // Divide intervals according to intersection with key
                foreach (var interval in intervals)
                {
                    if (interval.High.CompareTo(Key) < 0)
                    {
                        if (lefts == null)
                            lefts = new ArrayList<I>();

                        lefts.Add(interval);
                    }
                    else if (Key.CompareTo(interval.Low) < 0)
                    {
                        if (rights == null)
                            rights = new ArrayList<I>();

                        rights.Add(interval);
                    }
                    else
                        keyIntersections.Add(interval);
                }

                var count = keyIntersections.Count;
                // Sort intersecting intervals by Low and High
                LeftList = new I[count];
                RightList = new I[count];

                for (var i = 0; i < count; i++)
                    LeftList[i] = RightList[i] = keyIntersections[i];

                // Sort lists
                Sorting.IntroSort(LeftList, 0, count, LeftComparer);
                Sorting.IntroSort(RightList, 0, count, RightComparer);

                // Set span
                var lowestInterval = LeftList[0];
                var lowCompare = lowestInterval.Low.CompareTo(span.Low);
                // If the left most interval in the node has a lower Low than the current _span, update _span
                if (lowCompare < 0 || (lowCompare == 0 && !span.LowIncluded && lowestInterval.LowIncluded))
                    span = new IntervalBase<T>(lowestInterval, span);

                // Set span
                var highestInterval = RightList[0];
                var highCompare = span.High.CompareTo(highestInterval.High);
                // If the right most interval in the node has a higher High than the current Span, update Span
                if (highCompare < 0 || (highCompare == 0 && !span.HighIncluded && highestInterval.HighIncluded))
                    span = new IntervalBase<T>(span, highestInterval);


                // Construct interval tree recursively for Left and Right subtrees
                if (lefts != null)
                    Left = new Node(lefts.ToArray(), ref span);

                if (rights != null)
                    Right = new Node(rights.ToArray(), ref span);
            }

            #endregion
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a static interval tree from a collection of intervals.
        /// </summary>
        /// <param name="intervals">Interval collection</param>
        public StaticIntervalTree(IEnumerable<I> intervals)
        {
            var intervalArray = intervals as I[] ?? intervals.ToArray();

            if (!intervalArray.Any())
                return;

            _count = intervalArray.Count();

            _span = new IntervalBase<T>(intervalArray.First());
            _root = new Node(intervalArray, ref _span);
        }

        #endregion

        #region Median

        private static T getKey(I[] list)
        {
            IList<T> endpoints = new ArrayList<T>();

            foreach (var interval in list)
            {
                // Add both endpoints
                endpoints.Add(interval.Low);
                endpoints.Add(interval.High);
            }

            return getK(endpoints, list.Count() - 1);
        }

        private static T getK(IList<T> list, int k)
        {
            list.Shuffle();

            int low = 0, high = list.Count - 1;

            while (high > low)
            {
                var j = partition(list, low, high);

                if (j > k)
                    high = j - 1;
                else if (j < k)
                    low = j + 1;
                else
                    return list[k];
            }

            return list[k];
        }

        private static int partition(IList<T> list, int low, int high)
        {
            int i = low, j = high + 1;
            var v = list[low];

            while (true)
            {
                while (list[++i].CompareTo(v) < 0)
                    if (i == high)
                        break;
                while (v.CompareTo(list[--j]) < 0)
                    if (j == low)
                        break;
                if (i >= j)
                    break;

                list.Swap(i, j);
            }

            list.Swap(low, j);

            return j;
        }

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator()
        {
            return getEnumerator(_root);
        }

        private IEnumerator<I> getEnumerator(Node node)
        {
            // Just return if tree is empty
            if (node == null)
                yield break;

            // Recursively retrieve intervals in left subtree
            if (node.Left != null)
            {
                IEnumerator<I> child = getEnumerator(node.Left);

                while (child.MoveNext())
                    yield return child.Current;
            }

            // Go through all intervals in the node
            foreach (var interval in node.LeftList)
                yield return interval;

            // Recursively retrieve intervals in right subtree
            if (node.Right != null)
            {
                var child = getEnumerator(node.Right);

                while (child.MoveNext())
                    yield return child.Current;
            }
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty { get { return _root == null; } }
        /// <inheritdoc/>
        public override int Count { get { return _count; } }
        /// <inheritdoc/>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (_root == null)
                throw new NoSuchItemException();

            return _root.LeftList.First();
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
                if (_span == null)
                    throw new InvalidOperationException("An empty collection has no span");

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
                if (_maximumOverlap < 0)
                    _maximumOverlap = findMaximumOverlap();

                return _maximumOverlap;
            }
        }

        private int findMaximumOverlap()
        {
            // Check MNO is correct
            var max = 0;

            // Create queue sorted on high intervals
            var highComparer = ComparerFactory<IInterval<T>>.CreateComparer(IntervalExtensions.CompareHigh);
            var queue = new IntervalHeap<IInterval<T>>(highComparer);

            // Loop through intervals in sorted order
            foreach (var interval in ToArray())
            {
                // Remove all intervals from the queue not overlapping the current interval
                while (!queue.IsEmpty && interval.CompareLowHigh(queue.FindMin()) > 0)
                    queue.DeleteMin();

                queue.Add(interval);

                if (queue.Count > max)
                    max = queue.Count;
            }

            return max;
        }

        #endregion

        /// <inheritdoc/>
        public bool AllowsReferenceDuplicates { get { return true; } }

        #endregion

        #region Count Overlaps

        /// <inheritdoc/>
        public int CountOverlaps(T query)
        {
            return FindOverlaps(query).Count();
        }

        /// <inheritdoc/>
        public int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            return findOverlaps(_root, query);
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (ReferenceEquals(query, null))
                yield break;

            // Break if collection is empty or the query is outside the collections span
            if (IsEmpty || !Span.Overlaps(query))
                yield break;

            var splitNode = _root;
            // Use a lambda instead of out, as out or ref isn't allowed for itorators
            foreach (var interval in findSplitNode(_root, query, n => { splitNode = n; }))
                yield return interval;

            // Find all intersecting intervals in left subtree
            foreach (var interval in findLeft(splitNode.Left, query))
                yield return interval;

            // Find all intersecting intervals in right subtree
            foreach (var interval in findRight(splitNode.Right, query))
                yield return interval;
        }

        private static IEnumerable<I> findOverlaps(Node root, T query)
        {
            // Don't search empty leaves
            if (root == null)
                yield break;

            // If query matches root key, we just yield all intervals in root and stop our search
            var compare = query.CompareTo(root.Key);

            if (compare > 0)
            {
                foreach (var interval in root.RightList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // Recurse Right
                foreach (var interval in findOverlaps(root.Right, query))
                    yield return interval;
            }
            // If query comes before root key, we go through LeftList to find all intervals with a Low smaller than our query
            else if (compare < 0)
            {
                foreach (var interval in root.LeftList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // Recurse Left
                foreach (var interval in findOverlaps(root.Left, query))
                    yield return interval;
            }
            else
            {
                // yield all elements in lists
                foreach (var interval in root.LeftList)
                    if (interval.Overlaps(query))
                        yield return interval;
            }
        }

        /// <summary>
        /// Create an enumerable, enumerating all intersecting intervals on the path to the split node. Returns the split node in splitNode.
        /// </summary>
        private IEnumerable<I> findSplitNode(Node root, IInterval<T> query, Action<Node> splitNode)
        {
            if (root == null) yield break;

            splitNode(root);

            // Interval is lower than root, go left
            if (query.High.CompareTo(root.Key) < 0)
            {
                // yield all elements in lists
                foreach (var interval in root.LeftList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // Recursively travese left subtree
                foreach (var interval in findSplitNode(root.Left, query, splitNode))
                    yield return interval;
            }
            // Interval is higher than root, go right
            else if (root.Key.CompareTo(query.Low) < 0)
            {
                // yield all elements in lists
                foreach (var interval in root.RightList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // Recursively travese right subtree
                foreach (var interval in findSplitNode(root.Right, query, splitNode))
                    yield return interval;
            }
            // Otherwise add overlapping nodes in split node
            else
            {
                // yield all elements in lists
                foreach (var interval in root.LeftList)
                    if (interval.Overlaps(query))
                        yield return interval;
            }
        }

        private IEnumerable<I> findLeft(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            //
            if (root.Key.CompareTo(query.Low) < 0)
            {
                // Add all intersecting intervals from right list
                // yield all elements in lists
                foreach (var interval in root.RightList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // Recursively travese right subtree
                foreach (var interval in findLeft(root.Right, query))
                    yield return interval;
            }
            //
            else if (query.Low.CompareTo(root.Key) < 0)
            {
                // As our query interval contains the interval [root.Key:splitNode]
                // all intervals in root can be returned without any checks

                // yield all elements in lists
                foreach (var interval in root.RightList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // Recursively add all intervals in right subtree as they must be
                // contained by [root.Key:splitNode]
                IEnumerator<I> child = getEnumerator(root.Right);
                while (child.MoveNext())
                {
                    yield return child.Current;
                }

                // Recursively travese left subtree
                foreach (var interval in findLeft(root.Left, query))
                {
                    yield return interval;
                }
            }
            else
            {
                // Add all intersecting intervals from right list
                // yield all elements in lists
                foreach (var interval in root.RightList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // If we find the matching node, we can add everything in the left subtree
                IEnumerator<I> child = getEnumerator(root.Right);
                while (child.MoveNext())
                {
                    yield return child.Current;
                }
            }
        }

        private IEnumerable<I> findRight(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            //
            if (query.High.CompareTo(root.Key) < 0)
            {
                // Add all intersecting intervals from left list
                foreach (var interval in root.LeftList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // Otherwise Recursively travese left subtree
                foreach (var interval in findRight(root.Left, query))
                {
                    yield return interval;
                }
            }
            //
            else if (root.Key.CompareTo(query.High) < 0)
            {
                // As our query interval contains the interval [root.Key:splitNode]
                // all intervals in root can be returned without any checks
                foreach (var interval in root.RightList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // Recursively add all intervals in right subtree as they must be
                // contained by [root.Key:splitNode]
                IEnumerator<I> child = getEnumerator(root.Left);
                while (child.MoveNext())
                {
                    yield return child.Current;
                }

                // Recursively travese left subtree
                foreach (var interval in findRight(root.Right, query))
                {
                    yield return interval;
                }
            }
            else
            {
                // Add all intersecting intervals from left list
                foreach (var interval in root.LeftList)
                {
                    if (!interval.Overlaps(query))
                        break;

                    yield return interval;
                }

                // If we find the matching node, we can add everything in the left subtree
                IEnumerator<I> child = getEnumerator(root.Left);
                while (child.MoveNext())
                {
                    yield return child.Current;
                }
            }
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public bool FindOverlap(T query, ref I overlap)
        {
            bool result;

            using (var enumerator = FindOverlaps(query).GetEnumerator())
            {
                result = enumerator.MoveNext();

                if (result)
                    overlap = enumerator.Current;
            }

            return result;
        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, ref I overlap)
        {
            bool result;

            using (var enumerator = FindOverlaps(query).GetEnumerator())
            {
                result = enumerator.MoveNext();

                if (result)
                    overlap = enumerator.Current;
            }

            return result;
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

        #region Formatting

        /// <summary>
        /// Print the tree structure in Graphviz format
        /// </summary>
        /// <returns></returns>
        public string Graphviz
        {
            get
            {
                return "digraph StaticIntervalTree {\n"
                    + "\troot [fontname=consolas,shape=plaintext,label=\"Root\"];\n"
                    + graphviz(_root, "root")
                    + "}\n";
            }
        }

        private int _nodeCounter;
        private int _nullCounter;

        private string graphviz(Node root, string parent)
        {
            // Leaf
            int id;
            if (root == null)
            {
                id = _nullCounter++;
                return
                    String.Format("\tleaf{0} [shape=point];\n", id) +
                    String.Format("\t{0} -> leaf{1};\n", parent, id);
            }

            id = _nodeCounter++;
            return
                String.Format("\tnode{0} [fontname=consolas,label=\"{1}\"];\n", id, root.Key) +
                String.Format("\t{0} -> node{1};\n", parent, id) +
                graphviz(root.Left, "node" + id) +
                String.Format("\tnode{0}left [fontname=consolas,shape=plaintext, label=\"{1}\"];\n", id, root.LeftList) +
                String.Format("\tnode{0} -> node{0}left [style=dotted];\n", id) +
                String.Format("\tnode{0}right [fontname=consolas,shape=plaintext, label=\"{1}\"];\n", id, root.RightList.Reverse()) +
                String.Format("\tnode{0} -> node{0}right [style=dotted];\n", id) +
                graphviz(root.Right, "node" + id);
        }

        #endregion
    }
}
