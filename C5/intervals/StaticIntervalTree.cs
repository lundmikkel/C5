using System;
using System.Collections;
using System.Linq;
using SCG = System.Collections.Generic;

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
        private readonly Node _root;
        private readonly int _count;
        private IInterval<T> _span;

        #region Node nested classes

        class Node
        {
            internal T Key { get; private set; }
            // Left and right subtree
            internal Node Left { get; private set; }
            internal Node Right { get; private set; }
            // Left and right list of intersecting intervals for Key
            internal ListNode LeftList { get; private set; }
            internal ListNode RightList { get; private set; }

            internal Node(I[] intervals, ref IInterval<T> span)
            {
                Key = getKey(intervals);

                IList<I>
                    keyIntersections = new ArrayList<I>(),
                    lefts = new ArrayList<I>(),
                    rights = new ArrayList<I>();


                // Compute I_mid and construct two sorted lists, LeftList and RightList
                // Divide intervals according to intersection with key
                foreach (var I in intervals)
                {
                    if (I.High.CompareTo(Key) < 0)
                        lefts.Add(I);
                    else if (Key.CompareTo(I.Low) < 0)
                        rights.Add(I);
                    else
                        keyIntersections.Add(I);
                }

                // Sort intersecting intervals by Low and High
                var leftList = keyIntersections.OrderBy(I => I.Low).ThenBy(I => I.LowIncluded ? -1 : 1);
                var rightList = keyIntersections.OrderByDescending(I => I.High).ThenByDescending(I => I.HighIncluded ? 1 : -1);

                // Create left list
                ListNode previous = null;
                foreach (var interval in leftList)
                {
                    var node = new ListNode(interval);

                    if (previous != null)
                        previous.Next = node;
                    else
                        LeftList = node;

                    previous = node;
                }

                // Create right list
                previous = null;
                foreach (var interval in rightList)
                {
                    var node = new ListNode(interval);

                    if (previous != null)
                        previous.Next = node;
                    else
                        RightList = node;

                    previous = node;
                }


                // Set span
                var lowestInterval = LeftList.Interval;
                var lowCompare = lowestInterval.Low.CompareTo(span.Low);
                // If the left most interval in the node has a lower Low than the current _span, update _span
                if (lowCompare < 0 || (lowCompare == 0 && !span.LowIncluded && lowestInterval.LowIncluded))
                    span = new IntervalBase<T>(lowestInterval, span);

                // Set span
                var highestInterval = RightList.Interval;
                var highCompare = span.High.CompareTo(highestInterval.High);
                // If the right most interval in the node has a higher High than the current Span, update Span
                if (highCompare < 0 || (highCompare == 0 && !span.HighIncluded && highestInterval.HighIncluded))
                    span = new IntervalBase<T>(span, highestInterval);


                // Construct interval tree recursively for Left and Right subtrees
                if (!lefts.IsEmpty)
                    Left = new Node(lefts.ToArray(), ref span);

                if (!rights.IsEmpty)
                    Right = new Node(rights.ToArray(), ref span);
            }
        }

        class ListNode
        {
            public ListNode Next { get; internal set; }
            public I Interval { get; private set; }

            public ListNode(I interval)
            {
                Interval = interval;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a static interval tree from a collection of intervals.
        /// </summary>
        /// <param name="intervals">Interval collection</param>
        public StaticIntervalTree(SCG.IEnumerable<I> intervals)
        {
            var intervalArray = intervals as I[] ?? intervals.ToArray();

            if (intervalArray.Any())
            {
                _count = intervalArray.Count();

                IInterval<T> span = new IntervalBase<T>(intervalArray.First());

                _root = new Node(intervalArray, ref span);

                Span = span;
            }
        }

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

        #endregion

        #region IEnumerable

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

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private SCG.IEnumerator<I> getEnumerator(Node node)
        {
            // Just return if tree is empty
            if (node == null) yield break;

            // Recursively retrieve intervals in left subtree
            if (node.Left != null)
            {
                SCG.IEnumerator<I> child = getEnumerator(node.Left);

                while (child.MoveNext())
                {
                    yield return child.Current;
                }
            }

            // Go through all intervals in the node
            var n = node.LeftList;
            while (n != null)
            {
                yield return n.Interval;
                n = n.Next;
            }

            // Recursively retrieve intervals in right subtree
            if (node.Right != null)
            {
                SCG.IEnumerator<I> child = getEnumerator(node.Right);

                while (child.MoveNext())
                {
                    yield return child.Current;
                }
            }

        }

        #endregion

        #region Formatting

        /// <summary>
        /// Print the tree structure in Graphviz format
        /// </summary>
        /// <returns></returns>
        public string Graphviz()
        {
            return "digraph StaticIntervalTree {\n"
                + "\troot [fontname=consolas,shape=plaintext,label=\"Root\"];\n"
                + graphviz(_root, "root")
                + "}\n";
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
                String.Format("\tnode{0}left [fontname=consolas,shape=plaintext, label=\"{1}\"];\n", id, graphvizList(root.LeftList, false)) +
                String.Format("\tnode{0} -> node{0}left [style=dotted];\n", id) +
                String.Format("\tnode{0}right [fontname=consolas,shape=plaintext, label=\"{1}\"];\n", id, graphvizList(root.RightList, true)) +
                String.Format("\tnode{0} -> node{0}right [style=dotted];\n", id) +
                graphviz(root.Right, "node" + id);
        }

        private string graphvizList(ListNode root, bool revert)
        {
            var node = root;
            var s = new ArrayList<string>();

            while (node != null)
            {
                s.Add(node.Interval.ToString());
                node = node.Next;
            }

            if (revert)
                s.Reverse();

            return String.Join(", ", s.ToArray());
        }

        /// <inheritdoc/>
        public override SCG.IEnumerator<I> GetEnumerator()
        {
            return getEnumerator(_root);
        }


        #endregion

        #region ICollectionValue

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

            return _root.LeftList.Interval;
        }

        #endregion

        #region IIntervaled

        /// <inheritdoc/>
        public int MaximumOverlap
        {
            get { throw new NotSupportedException(); }
        }

        public bool AllowsReferenceDuplicates { get { return true; } }

        /// <inheritdoc/>
        public SCG.IEnumerable<I> FindOverlaps(T query)
        {
            if (ReferenceEquals(query, null))
                return Enumerable.Empty<I>();

            return findOverlap(_root, query);
        }

        private static SCG.IEnumerable<I> findOverlap(Node root, T query)
        {
            // Don't search empty leaves
            if (root == null) yield break;

            // If query matches root key, we just yield all intervals in root and stop our search
            var compare = query.CompareTo(root.Key);
            if (compare == 0)
            {
                // yield all elements in lists
                var currentListNode = root.LeftList;
                while (currentListNode != null
                    && !(currentListNode.Interval.Low.CompareTo(query) == 0 && !currentListNode.Interval.LowIncluded)
                    && !(currentListNode.Interval.High.CompareTo(query) == 0 && !currentListNode.Interval.HighIncluded))
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }
            }
            // If query comes before root key, we go through LeftList to find all intervals with a Low smaller than our query
            else if (compare < 0)
            {
                var currentListNode = root.LeftList;
                while (currentListNode != null
                    // Low is before query
                    && (currentListNode.Interval.Low.CompareTo(query) < 0
                    // Low is equal to query and Low is included
                    || (currentListNode.Interval.Low.CompareTo(query) == 0 && currentListNode.Interval.LowIncluded)))
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }

                // Recurse Left
                foreach (var interval in findOverlap(root.Left, query))
                    yield return interval;
            }
            else
            {
                var currentListNode = root.RightList;
                while (currentListNode != null
                    // High is after query
                    && (query.CompareTo(currentListNode.Interval.High) < 0
                    // High is equal to query and High is included
                    || (currentListNode.Interval.High.CompareTo(query) == 0 && currentListNode.Interval.HighIncluded)))
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }

                // Recurse Right
                foreach (var interval in findOverlap(root.Right, query))
                    yield return interval;
            }
        }

        /// <inheritdoc/>
        public SCG.IEnumerable<I> FindOverlaps(IInterval<T> query)
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

        /// <summary>
        /// Create an enumerable, enumerating all intersecting intervals on the path to the split node. Returns the split node in splitNode.
        /// </summary>
        private SCG.IEnumerable<I> findSplitNode(Node root, IInterval<T> query, Action<Node> splitNode)
        {
            if (root == null) yield break;

            splitNode(root);

            // Interval is lower than root, go left
            if (query.High.CompareTo(root.Key) < 0)
            {
                var currentListNode = root.LeftList;
                while (currentListNode != null && query.Overlaps(currentListNode.Interval))
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }

                // Recursively travese left subtree
                foreach (var interval in findSplitNode(root.Left, query, splitNode))
                    yield return interval;
            }
            // Interval is higher than root, go right
            else if (root.Key.CompareTo(query.Low) < 0)
            {
                var currentListNode = root.RightList;
                while (currentListNode != null && query.Overlaps(currentListNode.Interval))
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }

                // Recursively travese right subtree
                foreach (var interval in findSplitNode(root.Right, query, splitNode))
                    yield return interval;
            }
            // Otherwise add overlapping nodes in split node
            else
            {
                var node = root.LeftList;

                while (node != null)
                {
                    // TODO: A better way to go through them? What if query is [a:b] and splitnode is b, but all intervals are (b:c]?
                    if (query.Overlaps(node.Interval))
                        yield return node.Interval;

                    node = node.Next;
                }
            }
        }

        private SCG.IEnumerable<I> findLeft(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            //
            if (root.Key.CompareTo(query.Low) < 0)
            {
                // Add all intersecting intervals from right list
                var currentListNode = root.RightList;
                while (currentListNode != null && query.Overlaps(currentListNode.Interval))
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }

                // Recursively travese right subtree
                foreach (var interval in findLeft(root.Right, query))
                {
                    yield return interval;
                }
            }
            //
            else if (query.Low.CompareTo(root.Key) < 0)
            {
                // As our query interval contains the interval [root.Key:splitNode]
                // all intervals in root can be returned without any checks
                var currentListNode = root.RightList;
                while (currentListNode != null)
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }

                // Recursively add all intervals in right subtree as they must be
                // contained by [root.Key:splitNode]
                SCG.IEnumerator<I> child = getEnumerator(root.Right);
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
                var currentListNode = root.RightList;
                while (currentListNode != null && query.Overlaps(currentListNode.Interval))
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }

                // If we find the matching node, we can add everything in the left subtree
                SCG.IEnumerator<I> child = getEnumerator(root.Right);
                while (child.MoveNext())
                {
                    yield return child.Current;
                }
            }
        }

        private SCG.IEnumerable<I> findRight(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            //
            if (query.High.CompareTo(root.Key) < 0)
            {
                // Add all intersecting intervals from left list
                var currentListNode = root.LeftList;
                while (currentListNode != null && query.Overlaps(currentListNode.Interval))
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
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
                var currentListNode = root.RightList;
                while (currentListNode != null)
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }

                // Recursively add all intervals in right subtree as they must be
                // contained by [root.Key:splitNode]
                SCG.IEnumerator<I> child = getEnumerator(root.Left);
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
                var currentListNode = root.LeftList;
                while (currentListNode != null && query.Overlaps(currentListNode.Interval))
                {
                    yield return currentListNode.Interval;
                    currentListNode = currentListNode.Next;
                }

                // If we find the matching node, we can add everything in the left subtree
                SCG.IEnumerator<I> child = getEnumerator(root.Left);
                while (child.MoveNext())
                {
                    yield return child.Current;
                }
            }
        }

        /// <inheritdoc/>
        public IInterval<T> Span
        {
            get
            {
                if (_span == null)
                    throw new InvalidOperationException("An empty collection has no span");

                return _span;
            }

            private set { _span = value; }
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
        public void AddAll(SCG.IEnumerable<I> intervals)
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
    }
}
