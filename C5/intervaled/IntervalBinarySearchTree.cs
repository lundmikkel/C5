using System;
using SCG = System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C5.intervaled
{
    public class IntervalBinarySearchTree<T> : CollectionValueBase<IInterval<T>>, IDynamicIntervaled<T> where T : IComparable<T>
    {
        private const bool RED = true;
        private const bool BLACK = false;

        private Node _root;
        private int _count;

        #region IEqualityComparer

        public class IntervalReferenceEqualityComparer : SCG.IEqualityComparer<IInterval<T>>
        {
            public bool Equals(IInterval<T> i, IInterval<T> j)
            {
                return ReferenceEquals(i, j);
            }

            public int GetHashCode(IInterval<T> i)
            {
                return i.GetHashCode();
            }
        }

        #endregion

        #region Red-black tree helper methods

        private static bool isRed(Node node)
        {
            // Null nodes are by convention black
            return node != null && node.Color == RED;
        }

        private static Node rotate(Node root)
        {
            if (isRed(root.Right) && !isRed(root.Left))
                root = rotateLeft(root);
            if (isRed(root.Left) && isRed(root.Left.Left))
                root = rotateRight(root);
            if (isRed(root.Left) && isRed(root.Right))
            {
                root.Color = RED;
                root.Left.Color = root.Right.Color = BLACK;
            }

            return root;
        }

        private static Node rotateRight(Node root)
        {
            // Rotate
            var node = root.Left;
            root.Left = node.Right;
            node.Right = root;
            node.Color = root.Color;
            root.Color = RED;


            // 1
            node.Less.AddAll(root.Less);
            node.Equal.AddAll(root.Less);

            // 2
            var between = node.Greater - root.Greater;
            root.Less.AddAll(between);
            node.Greater.RemoveAll(between);

            // 3
            root.Equal.RemoveAll(node.Greater);
            root.Greater.RemoveAll(node.Greater);


            // Update PMO
            root.UpdateMaximumOverlap();
            node.UpdateMaximumOverlap();

            return node;
        }

        private static Node rotateLeft(Node root)
        {
            // Rotate
            var node = root.Right;
            root.Right = node.Left;
            node.Left = root;
            node.Color = root.Color;
            root.Color = RED;


            // 1
            node.Greater.AddAll(root.Greater);
            node.Equal.AddAll(root.Greater);

            // 2
            var between = node.Less - root.Less;
            root.Greater.AddAll(between);
            node.Less.RemoveAll(between);

            // 3
            root.Equal.RemoveAll(node.Less);
            root.Less.RemoveAll(node.Less);


            // Update PMO
            root.UpdateMaximumOverlap();
            node.UpdateMaximumOverlap();

            return node;
        }

        #endregion

        #region Node nested classes

        class Node
        {
            public T Key { get; private set; }

            private NodeSet _less;
            private NodeSet _equal;
            private NodeSet _greater;

            public NodeSet Less
            {
                get { return _less ?? (_less = new NodeSet()); }
            }
            public NodeSet Equal
            {
                get { return _equal ?? (_equal = new NodeSet()); }
            }
            public NodeSet Greater
            {
                get { return _greater ?? (_greater = new NodeSet()); }
            }

            public Node Left { get; internal set; }
            public Node Right { get; internal set; }

            public int Delta { get; internal set; }
            public int DeltaAfter { get; internal set; }
            private int Sum { get; set; }
            public int Max { get; private set; }

            public bool Color { get; set; }

            public Node(T key)
            {
                Key = key;
                Color = RED;
            }

            public void UpdateMaximumOverlap()
            {
                Sum = (Left != null ? Left.Sum : 0) + Delta + DeltaAfter + (Right != null ? Right.Sum : 0);

                Max = (new[]
                    {
                        (Left != null ? Left.Max : 0),
                        (Left != null ? Left.Sum : 0) + Delta,
                        (Left != null ? Left.Sum : 0) + Delta + DeltaAfter,
                        (Left != null ? Left.Sum : 0) + Delta + DeltaAfter + (Right != null ? Right.Max : 0)
                    }).Max();
            }

            public override string ToString()
            {
                return String.Format("{0}-{1}", Color ? "R" : "B", Key);
            }
        }

        public sealed class NodeSet : HashSet<IInterval<T>>
        {
            private static SCG.IEqualityComparer<IInterval<T>> _comparer = new IntervalReferenceEqualityComparer();

            public NodeSet(SCG.IEnumerable<IInterval<T>> intervals)
                : base(_comparer)
            {
                AddAll(intervals);
            }

            public NodeSet()
                : base(new IntervalReferenceEqualityComparer())
            {
            }

            public static NodeSet operator -(NodeSet s1, NodeSet s2)
            {
                if (s1 == null || s2 == null)
                    throw new ArgumentNullException("Set-Set");

                var res = new NodeSet(s1);
                res.RemoveAll(s2);
                return res;
            }

            public static NodeSet operator +(NodeSet s1, NodeSet s2)
            {
                if (s1 == null || s2 == null)
                    throw new ArgumentNullException("Set+Set");

                var res = new NodeSet(s1);
                res.AddAll(s2);
                return res;
            }
        }

        #endregion

        #region Constructors

        public IntervalBinarySearchTree(SCG.IEnumerable<IInterval<T>> intervals)
        {
            foreach (var interval in intervals)
                Add(interval);
        }

        public IntervalBinarySearchTree()
        {
        }

        #endregion

        #region ICollection, IExtensible

        private Node addLow(Node root, Node right, IInterval<T> interval)
        {
            if (root == null)
                root = new Node(interval.Low);

            var compareTo = root.Key.CompareTo(interval.Low);

            if (compareTo < 0)
            {
                root.Right = addLow(root.Right, right, interval);
            }
            else if (compareTo == 0)
            {
                // If everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    root.Greater.Add(interval);

                if (interval.LowIncluded)
                    root.Equal.Add(interval);

                // Update delta
                if (interval.LowIncluded)
                    root.Delta++;
                else
                    root.DeltaAfter++;
            }
            else if (compareTo > 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    root.Greater.Add(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.High) < 0)
                    root.Equal.Add(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Left = addLow(root.Left, root, interval);
            }

            // Red Black tree rotations
            root = rotate(root);

            // Update PMO
            root.UpdateMaximumOverlap();

            return root;
        }

        private Node addHigh(Node root, Node left, IInterval<T> interval)
        {
            if (root == null)
                root = new Node(interval.High);

            var compareTo = root.Key.CompareTo(interval.High);

            if (compareTo > 0)
            {
                root.Left = addHigh(root.Left, left, interval);
            }
            else if (compareTo == 0)
            {
                // If everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    root.Less.Add(interval);

                if (interval.HighIncluded)
                    root.Equal.Add(interval);

                if (!interval.HighIncluded)
                    root.Delta--;
                else
                    root.DeltaAfter--;
            }
            else if (compareTo < 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    root.Less.Add(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.Low) > 0)
                    root.Equal.Add(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Right = addHigh(root.Right, root, interval);
            }

            // Red Black tree rotations
            root = rotate(root);

            // TODO: Figure out if this is still the correct place to put update
            // Update PMO
            root.UpdateMaximumOverlap();

            return root;
        }

        public void Add(IInterval<T> interval)
        {
            // TODO: Add event!

            _root = addLow(_root, null, interval);
            _root = addHigh(_root, null, interval);

            _root.Color = BLACK;
        }

        public void Clear()
        {
            _root = null;
            _count = 0;
            // TODO: Add what ever is missing
        }

        public bool Contains(IInterval<T> item)
        {
            throw new NotImplementedException();
        }


        public override bool IsEmpty { get { return _root == null; } }

        public bool Remove(IInterval<T> item)
        {
            throw new NotImplementedException();
        }

        // TODO: Implement
        public override int Count
        {
            get { return _count; }
        }

        public bool IsReadOnly { get { return false; } }

        public override Speed CountSpeed { get { return Speed.Constant; } }
        public bool AllowsDuplicates { get { return true; } }

        #endregion


        #region IEnumerable

        public override SCG.IEnumerator<IInterval<T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollectionValue

        public override IInterval<T> Choose()
        {
            if (_root == null)
                throw new NoSuchItemException();

            return (_root.Less + _root.Equal + _root.Greater).Choose();
        }

        #endregion

        #region IIntervaled

        public IInterval<T> Span
        {
            get
            {
                if (_root == null)
                    throw new InvalidOperationException("An empty collection has no span");

                return new IntervalBase<T>(getLowest(_root), getHighest(_root));
            }
        }

        private static IInterval<T> getLowest(Node root)
        {
            // TODO: Assert root != null

            if (!root.Less.IsEmpty)
                return root.Less.Choose();

            if (root.Left != null)
                return getLowest(root.Left);

            return !root.Equal.IsEmpty ? root.Equal.Choose() : root.Greater.Choose();
        }

        private static IInterval<T> getHighest(Node root)
        {
            // TODO: Assert root != null

            if (!root.Greater.IsEmpty)
                return root.Greater.Choose();

            if (root.Right != null)
                return getHighest(root.Right);

            return !root.Equal.IsEmpty ? root.Equal.Choose() : root.Less.Choose();
        }

        public SCG.IEnumerable<IInterval<T>> FindOverlaps(T query)
        {
            if (ReferenceEquals(query, null))
                return Enumerable.Empty<IInterval<T>>();

            var set = new NodeSet();

            foreach (var interval in findOverlap(_root, query))
                set.Add(interval);

            return set;
        }

        private SCG.IEnumerable<IInterval<T>> findOverlap(Node root, T query)
        {
            if (root == null)
                yield break;

            var compareTo = query.CompareTo(root.Key);
            if (compareTo < 0)
            {
                foreach (var interval in root.Less)
                    yield return interval;

                foreach (var interval in findOverlap(root.Left, query))
                    yield return interval;
            }
            else if (compareTo > 0)
            {
                foreach (var interval in root.Greater)
                    yield return interval;

                foreach (var interval in findOverlap(root.Right, query))
                    yield return interval;
            }
            else
            {
                foreach (var interval in root.Equal)
                    yield return interval;
            }
        }

        public SCG.IEnumerable<IInterval<T>> FindOverlaps(IInterval<T> query)
        {
            if (ReferenceEquals(query, null))
                yield break;

            // Break if collection is empty or the query is outside the collections span
            if (IsEmpty || !Span.Overlaps(query))
                yield break;

            var set = new NodeSet();

            var splitNode = _root;
            // Use a lambda instead of out, as out or ref isn't allowed for itorators
            set.AddAll(findSplitNode(_root, query, n => { splitNode = n; }));

            // Find all intersecting intervals in left subtree
            if (query.Low.CompareTo(splitNode.Key) < 0)
                set.AddAll(findLeft(splitNode.Left, query));

            // Find all intersecting intervals in right subtree
            if (splitNode.Key.CompareTo(query.High) < 0)
                set.AddAll(findRight(splitNode.Right, query));

            foreach (var interval in set)
                yield return interval;
        }

        /// <summary>
        /// Create an enumerable, enumerating all intersecting intervals on the path to the split node. Returns the split node in splitNode.
        /// </summary>
        private SCG.IEnumerable<IInterval<T>> findSplitNode(Node root, IInterval<T> query, Action<Node> splitNode)
        {
            if (root == null) yield break;

            splitNode(root);

            // Interval is lower than root, go left
            if (query.High.CompareTo(root.Key) < 0)
            {
                foreach (var interval in root.Less)
                    yield return interval;

                // Recursively travese left subtree
                foreach (var interval in findSplitNode(root.Left, query, splitNode))
                    yield return interval;
            }
            // Interval is higher than root, go right
            else if (root.Key.CompareTo(query.Low) < 0)
            {
                foreach (var interval in root.Greater)
                    yield return interval;

                // Recursively travese right subtree
                foreach (var interval in findSplitNode(root.Right, query, splitNode))
                    yield return interval;
            }
            // Otherwise add overlapping nodes in split node
            else
            {
                foreach (var interval in root.Less + root.Equal + root.Greater)
                    if (query.Overlaps(interval))
                        yield return interval;
            }
        }

        private SCG.IEnumerable<IInterval<T>> findLeft(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            var compareTo = query.Low.CompareTo(root.Key);

            //
            if (compareTo > 0)
            {
                foreach (var interval in root.Greater)
                    yield return interval;

                // Recursively travese right subtree
                foreach (var interval in findLeft(root.Right, query))
                    yield return interval;
            }
            //
            else if (compareTo < 0)
            {
                foreach (var interval in root.Less + root.Equal + root.Greater)
                    yield return interval;

                // Recursively add all intervals in right subtree as they must be
                // contained by [root.Key:splitNode]
                var child = getEnumerator(root.Right);
                while (child.MoveNext())
                    yield return child.Current;

                // Recursively travese left subtree
                foreach (var interval in findLeft(root.Left, query))
                    yield return interval;
            }
            else
            {
                // Add all intersecting intervals from right list
                foreach (var interval in root.Greater)
                    yield return interval;

                if (query.LowIncluded)
                    foreach (var interval in root.Equal)
                        yield return interval;

                // If we find the matching node, we can add everything in the left subtree
                SCG.IEnumerator<IInterval<T>> child = getEnumerator(root.Right);
                while (child.MoveNext())
                    yield return child.Current;
            }
        }

        private SCG.IEnumerable<IInterval<T>> findRight(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            var compareTo = query.High.CompareTo(root.Key);

            //
            if (compareTo < 0)
            {
                // Add all intersecting intervals from left list
                foreach (var interval in root.Less)
                    yield return interval;

                // Otherwise Recursively travese left subtree
                foreach (var interval in findRight(root.Left, query))
                    yield return interval;
            }
            //
            else if (compareTo > 0)
            {
                // As our query interval contains the interval [root.Key:splitNode]
                // all intervals in root can be returned without any checks
                foreach (var interval in root.Less + root.Equal + root.Greater)
                    yield return interval;

                // Recursively add all intervals in right subtree as they must be
                // contained by [root.Key:splitNode]
                var child = getEnumerator(root.Left);
                while (child.MoveNext())
                    yield return child.Current;

                // Recursively travese left subtree
                foreach (var interval in findRight(root.Right, query))
                    yield return interval;
            }
            else
            {
                // Add all intersecting intervals from left list
                foreach (var interval in root.Less)
                    yield return interval;

                if (query.HighIncluded)
                    foreach (var interval in root.Equal)
                        yield return interval;

                // If we find the matching node, we can add everything in the left subtree
                var child = getEnumerator(root.Left);
                while (child.MoveNext())
                    yield return child.Current;
            }
        }


        private SCG.IEnumerator<IInterval<T>> getEnumerator(Node node)
        {
            // Just return if tree is empty
            if (node == null) yield break;

            // Recursively retrieve intervals in left subtree
            if (node.Left != null)
            {
                var child = getEnumerator(node.Left);

                while (child.MoveNext())
                    yield return child.Current;
            }

            // Go through all intervals in the node
            foreach (var interval in node.Less + node.Equal + node.Greater)
                yield return interval;

            // Recursively retrieve intervals in right subtree
            if (node.Right != null)
            {
                var child = getEnumerator(node.Right);

                while (child.MoveNext())
                    yield return child.Current;
            }

        }

        public bool OverlapExists(IInterval<T> query)
        {
            if (query == null)
                return false;

            return FindOverlaps(query).GetEnumerator().MoveNext();
        }

        public int MaximumOverlap
        {
            get { return _root != null ? _root.Max : 0; }
        }

        #endregion

    }
}
