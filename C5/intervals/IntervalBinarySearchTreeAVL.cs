﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;

namespace C5.intervals
{
    // TODO: Document reference equality duplicates
    /// <summary>
    /// An implementation of the Interval Binary Search Tree as described by Hanson et. al in "The IBS-Tree: A Data Structure for Finding All Intervals That Overlap a Point" using an AVL tree balancing scheme.
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class IntervalBinarySearchTreeAvl<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private Node _root;
        private int _count;
        private static readonly IEqualityComparer<I> Comparer = ComparerFactory<I>.CreateEqualityComparer((x, y) => ReferenceEquals(x, y), x => x.GetHashCode());

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        // Check the invariants of the IBS tree.
        private void invariants()
        {
            // Check the balance invariant holds.
            Contract.Invariant(confirmBalance());

            // Check that the IBS tree invariants from the Hanson article holds.
            Contract.Invariant(Contract.ForAll(getNodeEnumerator(_root), checkIbsInvariants));
        }

        /// <summary>
        /// Check the invariants of the IBS tree.
        /// The invariants are from the article "The IBS-tree: A Data Structure for Finding All Intervals That Overlap a Point" by Hanson and Chaabouni.
        /// The invariants are located on page 4 of the article.
        /// </summary>
        /// <param name="v">The node to check (It only makes sense to check all the nodes of the tree, so call this enumerating the entire tree)</param>
        /// <returns>Returns true if all the invariants hold and false if one of them does not hold.</returns>
        [Pure]
        private bool checkIbsInvariants(Node v)
        {
            Node rightParent = null;
            Node leftParent = null;

            // Find j intervals and left and right parent
            var js = findJs(v, ref leftParent, ref rightParent);

            // Interval represented by Less
            var greaterInterval = rightParent != null ? new IntervalBase<T>(v.Key, rightParent.Key, IntervalType.Open) : null;
            // Interval represented by Equal
            var equalInterval = new IntervalBase<T>(v.Key);
            // Interval represented by Greater
            var lessInterval = leftParent != null ? new IntervalBase<T>(leftParent.Key, v.Key, IntervalType.Open) : null;


            // Check containment invariant
            if (v.Less != null && lessInterval != null)
                if (v.Less.Any(i => !i.Contains(lessInterval)))
                    return false;

            if (v.Greater != null && greaterInterval != null)
                if (v.Greater.Any(i => !i.Contains(greaterInterval)))
                    return false;

            if (v.Equal != null)
                if (v.Equal.Any(i => !i.Contains(equalInterval)))
                    return false;


            // Check maximum invariant
            foreach (var j in js)
            {
                // Check containment invariant for both Less and Greater
                if (v.Less != null && lessInterval != null)
                    // If there is an inteval in Less that contains J the invariant doesn't hold
                    if (v.Less.Any(i => i.Contains(j)))
                        return false;

                if (v.Greater != null && greaterInterval != null)
                    // If there is an inteval in Greater that contains J the invariant doesn't hold
                    if (v.Greater.Any(i => i.Contains(j)))
                        return false;

                if (v.Equal != null)
                    // If there is an inteval in Equal that contains J the invariant doesn't hold
                    if (v.Equal.Any(i => i.Contains(j)))
                        return false;
            }

            return true;
        }

        [Pure]
        private IEnumerable<IInterval<T>> findJs(Node v, ref Node leftParent, ref Node rightParent)
        {
            var set = new ArrayList<IInterval<T>>();
            var root = _root;

            while (root != null)
            {
                var compare = v.CompareTo(root);

                if (compare > 0)
                {
                    // Add a new j interval to the set
                    if (root.Right != null && v.CompareTo(root.Right) < 0)
                        set.Add(new IntervalBase<T>(root.Key, root.Right.Key, IntervalType.Open));

                    // Update left parent
                    leftParent = root;

                    root = root.Right;
                }
                else if (compare < 0)
                {
                    // Add a new j interval to the set
                    if (root.Left != null && v.CompareTo(root.Left) > 0)
                        set.Add(new IntervalBase<T>(root.Left.Key, root.Key, IntervalType.Open));

                    // Update right parent
                    rightParent = root;

                    root = root.Left;
                }
                else
                    // Stop the loop when we find the node
                    break;
            }

            return set;
        }

        /// <summary>
        /// Checks that the height of the tree is balanced.
        /// </summary>
        /// <returns>True if the tree is balanced, else false.</returns>
        [Pure]
        private bool confirmBalance()
        {
            var result = true;
            height(_root, ref result);
            return result;
        }

        /// <summary>
        /// Get the height of the tree.
        /// </summary>
        /// <param name="node">The node you wish to check the height on.</param>
        /// <param name="result">Reference to a bool that will be set to false if an in-balance is discovered.</param>
        /// <returns>Height of the tree.</returns>
        [Pure]
        private static int height(Node node, ref bool result)
        {
            if (node == null)
                return 0;
            var heightLeft = height(node.Left, ref result);
            var heightRight = height(node.Right, ref result);
            if (node.Balance != heightRight - heightLeft)
                result = false;
            return Math.Max(heightLeft, heightRight) + 1;
        }

        [Pure]
        private static IEnumerable<Node> getNodeEnumerator(Node root)
        {
            if (root == null)
                yield break;

            foreach (var node in getNodeEnumerator(root.Left))
                yield return node;

            yield return root;

            foreach (var node in getNodeEnumerator(root.Right))
                yield return node;
        }

        #endregion

        #region Inner Classes

        class Node : IComparable<Node>
        {
            #region Code Contracts

            [ContractInvariantMethod]
            private void invariant()
            {
                Contract.Invariant(!ReferenceEquals(Key, null));
                Contract.Invariant(IntervalsEndingInNode >= 0);
            }

            #endregion

            #region Fields

            public T Key { get; private set; }

            public Node Left { get; internal set; }
            public Node Right { get; internal set; }

            // The number of intervals with an endpoint in one of the interval sets in the node
            public int IntervalsEndingInNode;

            // Fields for Maximum Number of Overlaps
            public int DeltaAt { get; internal set; }
            public int DeltaAfter { get; internal set; }
            private int Sum { get; set; }
            public int Max { get; private set; }

            // Balance - between -2 and +2
            public sbyte Balance { get; internal set; }

            #endregion

            #region Properties

            public IntervalSet Less { get; set; }
            public IntervalSet Equal { get; set; }
            public IntervalSet Greater { get; set; }

            #endregion

            #region Constructors

            public Node(T key)
            {
                Key = key;
            }

            #endregion

            #region Public Methods

            public IEnumerable<I> GetIntervalsEndingInNode()
            {
                Contract.Ensures(Contract.Result<IEnumerable<I>>().All(i => i.HasEndpoint(Key)));

                var set = new IntervalSet();

                if (Less != null && !Less.IsEmpty)
                    foreach (var interval in Less.Where(interval => interval.HasEndpoint(Key) && set.Add(interval)))
                        yield return interval;

                if (Equal != null && !Equal.IsEmpty)
                    foreach (var interval in Equal.Where(interval => interval.HasEndpoint(Key) && set.Add(interval)))
                        yield return interval;

                if (Greater != null && !Greater.IsEmpty)
                    foreach (var interval in Greater.Where(interval => interval.HasEndpoint(Key) && set.Add(interval)))
                        yield return interval;
            }

            /// <summary>
            /// Update the maximum overlap value for the node.
            /// </summary>
            /// <returns>True if value changed.</returns>
            public bool UpdateMaximumOverlap()
            {
                Contract.Ensures(Contract.Result<bool>() == (Contract.OldValue(Max) != Max || Contract.OldValue(Sum) != Sum));

                // Cache values
                var oldMax = Max;
                var oldSum = Sum;

                // Set Max to Left's Max
                Max = Left != null ? Left.Max : 0;

                // Start building up the other possible Max sums
                var value = (Left != null ? Left.Sum : 0) + DeltaAt;
                // And check if they are higher the previously found max
                if (value > Max)
                    Max = value;

                // Add DeltaAfter and check for new max
                value += DeltaAfter;
                if (value > Max)
                    Max = value;

                // Save the sum value using the previous calculations
                Sum = value + (Right != null ? Right.Sum : 0);

                // Add Right's max and check for new max
                value += Right != null ? Right.Max : 0;
                if (value > Max)
                    Max = value;

                // Return true if either Max or Sum changed
                return oldMax != Max || oldSum != Sum;
            }

            public int CompareTo(Node other)
            {
                return Key.CompareTo(other.Key);
            }

            public override string ToString()
            {
                return Key.ToString();
            }

            public void SwapKeys(Node successor)
            {
                Contract.Requires(successor != null);

                var tmp = Key;
                Key = successor.Key;
                successor.Key = tmp;
            }

            #endregion

        }

        private sealed class IntervalSet : HashSet<I>
        {
            private IntervalSet(IEnumerable<I> intervals)
                : base(Comparer)
            {
                AddAll(intervals);
            }

            public IntervalSet()
                : base(Comparer)
            {
            }

            public override string ToString()
            {
                var s = new ArrayList<string>();

                foreach (var interval in this)
                    s.Add(interval.ToString());

                return s.IsEmpty ? String.Empty : String.Join(", ", s.ToArray());
            }

            public static IntervalSet operator -(IntervalSet s1, IntervalSet s2)
            {
                Contract.Requires(s1 != null);
                Contract.Requires(s2 != null);

                var res = new IntervalSet(s1);
                res.RemoveAll(s2);
                return res;
            }
        }

        #endregion

        #region AVL tree methods

        private static Node rotateForAdd(Node root, ref bool updateBalance)
        {
            Contract.Requires(root != null);

            Contract.Requires(root.Balance != -2 || root.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != -1 || root.Left.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != +1 || root.Left.Right != null);

            Contract.Requires(root.Balance != +2 || root.Right != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != -1 || root.Right.Left != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != +1 || root.Right.Right != null);

            switch (root.Balance)
            {
                // Node is balanced after the node was added
                case 0:
                    updateBalance = false;
                    break;

                // Node is unbalanced, so we rotate
                case -2:
                    switch (root.Left.Balance)
                    {
                        // Left Left Case
                        case -1:
                            root = rotateRight(root);
                            root.Balance = root.Right.Balance = 0;
                            break;

                        // Left Right Case
                        case +1:
                            root.Left = rotateLeft(root.Left);
                            root = rotateRight(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }
                    updateBalance = false;
                    break;

                // Node is unbalanced, so we rotate
                case +2:
                    switch (root.Right.Balance)
                    {
                        // Right Right Case
                        case +1:
                            root = rotateLeft(root);
                            root.Balance = root.Left.Balance = 0;
                            break;

                        // Right Left Case
                        case -1:
                            root.Right = rotateRight(root.Right);
                            root = rotateLeft(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }

                    updateBalance = false;
                    break;
            }

            return root;
        }

        private static Node rotateForRemove(Node root, ref bool updateBalance)
        {
            Contract.Requires(root != null);

            Contract.Requires(root.Balance != -2 || root.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != -1 || root.Left.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != +1 || root.Left.Right != null);

            Contract.Requires(root.Balance != +2 || root.Right != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != -1 || root.Right.Left != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != +1 || root.Right.Right != null);

            switch (root.Balance)
            {
                // High will not change for parent, so we can stop here
                case -1:
                case +1:
                    updateBalance = false;
                    break;

                // Node is unbalanced, so we rotate
                case -2:
                    switch (root.Left.Balance)
                    {
                        // Left Left Case
                        case -1:
                            root = rotateRight(root);
                            root.Balance = root.Right.Balance = 0;
                            break;

                        case 0:
                            root = rotateRight(root);
                            root.Right.Balance = -1;
                            root.Balance = +1;
                            updateBalance = false;
                            break;

                        // Left Right Case
                        case +1:
                            root.Left = rotateLeft(root.Left);
                            root = rotateRight(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) ((root.Balance == +1) ? -1 : 0);
                            root.Right.Balance = (sbyte) ((root.Balance == -1) ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }
                    break;

                // Node is unbalanced, so we rotate
                case +2:
                    switch (root.Right.Balance)
                    {
                        // Right Right Case
                        case +1:
                            root = rotateLeft(root);
                            root.Balance = root.Left.Balance = 0;
                            break;

                        case 0:
                            root = rotateLeft(root);
                            root.Left.Balance = 1;
                            root.Balance = -1;
                            updateBalance = false;
                            break;

                        // Right Left Case
                        case -1:
                            root.Right = rotateRight(root.Right);
                            root = rotateLeft(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) ((root.Balance == +1) ? -1 : 0);
                            root.Right.Balance = (sbyte) ((root.Balance == -1) ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }
                    break;
            }

            return root;
        }

        private static Node rotateRight(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Left != null);

            // Rotate
            var node = root.Left;
            root.Left = node.Right;
            node.Right = root;


            // 1
            if (root.Less != null && !root.Less.IsEmpty)
            {
                if (node.Less == null)
                    node.Less = new IntervalSet();

                node.Less.AddAll(root.Less);

                if (node.Equal == null)
                    node.Equal = new IntervalSet();

                node.Equal.AddAll(root.Less);
            }

            // 2
            if (node.Greater != null && !node.Greater.IsEmpty)
            {
                var between = root.Greater != null ? node.Greater - root.Greater : node.Greater;
                if (root.Less == null)
                    root.Less = new IntervalSet();

                root.Less.AddAll(between);

                node.Greater.RemoveAll(between);
            }

            // 3
            if (node.Greater != null && !node.Greater.IsEmpty)
            {
                if (root.Equal != null && !root.Equal.IsEmpty)
                    root.Equal.RemoveAll(node.Greater);
                if (root.Greater != null && !root.Greater.IsEmpty)
                    root.Greater.RemoveAll(node.Greater);
            }


            // Update MNO
            root.UpdateMaximumOverlap();
            node.UpdateMaximumOverlap();

            return node;
        }

        private static Node rotateLeft(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Right != null);

            // Rotate
            var node = root.Right;
            root.Right = node.Left;
            node.Left = root;

            // TODO: Look into if these operations can be optimised
            // 1
            if (root.Greater != null && !root.Greater.IsEmpty)
            {
                if (node.Greater == null)
                    node.Greater = new IntervalSet();

                node.Greater.AddAll(root.Greater);

                if (node.Equal == null)
                    node.Equal = new IntervalSet();

                node.Equal.AddAll(root.Greater);
            }

            // 2
            if (node.Less != null && !node.Less.IsEmpty)
            {
                var between = root.Less != null ? node.Less - root.Less : node.Less;
                if (root.Greater == null)
                    root.Greater = new IntervalSet();

                root.Greater.AddAll(between);

                node.Less.RemoveAll(between);
            }

            // 3
            if (node.Less != null && node.Less.IsEmpty)
            {
                if (root.Equal != null && !root.Equal.IsEmpty)
                    root.Equal.RemoveAll(node.Less);
                if (root.Less != null && !root.Less.IsEmpty)
                    root.Less.RemoveAll(node.Less);
            }


            // Update MNO
            root.UpdateMaximumOverlap();
            node.UpdateMaximumOverlap();

            return node;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create an Interval Binary Search Tree with a collection of intervals.
        /// </summary>
        /// <param name="intervals">The collection of intervals.</param>
        public IntervalBinarySearchTreeAvl(IEnumerable<I> intervals)
        {
            Contract.Requires(intervals != null);

            // TODO: Pre-generate balanced tree based on endpoints and insert intervals afterwards

            foreach (var interval in intervals)
                Add(interval);
        }

        /// <summary>
        /// Create empty Interval Binary Search Tree.
        /// </summary>
        public IntervalBinarySearchTreeAvl()
        {
        }

        #endregion

        #region Collection Value

        /// <inheritdoc/>
        public override bool IsEmpty
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (_root == null));
                return _root == null;
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
            if (_root == null)
                throw new NoSuchItemException();

            // At least one of Less, Equal, or Greater will contain at least one interval
            if (!_root.Less.IsEmpty)
                return _root.Less.Choose();

            if (!_root.Equal.IsEmpty)
                return _root.Equal.Choose();

            return _root.Greater.Choose();
        }

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator()
        {
            var set = new IntervalSet();

            var enumerator = getEnumerator(_root);
            while (enumerator.MoveNext())
                if (set.Add(enumerator.Current))
                    yield return enumerator.Current;
        }

        private static IEnumerator<I> getEnumerator(Node root)
        {
            // Just return if tree is empty
            if (root == null) yield break;

            // Recursively retrieve intervals in left subtree
            if (root.Left != null)
            {
                var child = getEnumerator(root.Left);

                while (child.MoveNext())
                    yield return child.Current;
            }

            // Go through all intervals in the node
            if (root.Less != null && !root.Less.IsEmpty)
                foreach (var interval in root.Less)
                    yield return interval;
            if (root.Equal != null && !root.Equal.IsEmpty)
                foreach (var interval in root.Equal)
                    yield return interval;
            if (root.Greater != null && !root.Greater.IsEmpty)
                foreach (var interval in root.Greater)
                    yield return interval;

            // Recursively retrieve intervals in right subtree
            if (root.Right != null)
            {
                var child = getEnumerator(root.Right);

                while (child.MoveNext())
                    yield return child.Current;
            }
        }

        private static IEnumerable<Node> nodeEnumerator(Node root)
        {
            if (root == null)
                yield break;

            foreach (var node in nodeEnumerator(root.Left))
                yield return node;

            yield return root;

            foreach (var node in nodeEnumerator(root.Right))
                yield return node;
        }

        #endregion

        #region Events

        /// <inheritdoc/>
        public override EventTypeEnum ListenableEvents { get { return EventTypeEnum.Basic; } }
        //public EventTypeEnum ActiveEvents { get; private set; }
        //public event CollectionChangedHandler<T> CollectionChanged;
        //public event CollectionClearedHandler<T> CollectionCleared;
        //public event ItemsAddedHandler<T> ItemsAdded;
        //public event ItemInsertedHandler<T> ItemInserted;
        //public event ItemsRemovedHandler<T> ItemsRemoved;
        //public event ItemRemovedAtHandler<T> ItemRemovedAt;

        #endregion

        #region Interval Collection

        #region Properties

        #region Span

        /// <inheritdoc/>
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
            Contract.Requires(root != null);

            if (root.Less != null && !root.Less.IsEmpty)
                return root.Less.Choose();

            if (root.Left != null)
                return getLowest(root.Left);

            return root.Equal != null && !root.Equal.IsEmpty ? root.Equal.Choose() : root.Greater.Choose();
        }

        private static IInterval<T> getHighest(Node root)
        {
            Contract.Requires(root != null);

            if (root.Greater != null && !root.Greater.IsEmpty)
                return root.Greater.Choose();

            if (root.Right != null)
                return getHighest(root.Right);

            return root.Equal != null && !root.Equal.IsEmpty ? root.Equal.Choose() : root.Less.Choose();
        }

        #endregion

        #region MNO

        /// <inheritdoc/>
        public int MaximumOverlap
        {
            get { return _root != null ? _root.Max : 0; }
        }

        /// <summary>
        /// Recursively search for the split node, while updating the maximum overlap on the way
        /// back if necessary.
        /// </summary>
        /// <param name="root">The root for the tree to search.</param>
        /// <param name="interval">The interval whose endpoints we search for.</param>
        /// <returns>True if we need to update the maximum overlap for the parent node.</returns>
        private static bool updateMaximumOverlap(Node root, IInterval<T> interval)
        {
            Contract.Requires(root != null);
            Contract.Requires(interval != null);

            // Search left for split node and update MNO if necessary
            if (interval.High.CompareTo(root.Key) < 0)
                return updateMaximumOverlap(root.Left, interval) && root.UpdateMaximumOverlap();

            // Search right for split node and update MNO if necessary
            if (root.Key.CompareTo(interval.Low) < 0)
                return updateMaximumOverlap(root.Right, interval) && root.UpdateMaximumOverlap();

            // Return true if MNO has changed for either endpoint
            return updateLowMaximumOverlap(root, interval.Low) || updateHighMaximumOverlap(root, interval.High);
        }

        private static bool updateLowMaximumOverlap(Node root, T low)
        {
            Contract.Requires(root != null);

            var compare = low.CompareTo(root.Key);

            // Search left for low and update MNO if necessary
            if (compare < 0)
                return updateLowMaximumOverlap(root.Left, low) && root.UpdateMaximumOverlap();

            // Search right for low and update MNO if necessary
            if (compare > 0)
                return updateLowMaximumOverlap(root.Right, low) && root.UpdateMaximumOverlap();

            // Update MNO when low is found
            return root.UpdateMaximumOverlap();
        }

        private static bool updateHighMaximumOverlap(Node root, T high)
        {
            Contract.Requires(root != null);

            var compare = high.CompareTo(root.Key);

            // Search left for high and update MNO if necessary
            if (compare < 0)
                return updateHighMaximumOverlap(root.Left, high) && root.UpdateMaximumOverlap();

            // Search right for high and update MNO if necessary
            if (compare > 0)
                return updateHighMaximumOverlap(root.Right, high) && root.UpdateMaximumOverlap();

            // Update MNO when high is found
            return root.UpdateMaximumOverlap();
        }

        #endregion

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(T query)
        {
            // TODO: Figure out why we need to check for duplicates
            var set = new IntervalSet();
            return findOverlaps(_root, query).Where(set.Add);
        }

        /// <inheritdoc/>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (query == null)
                yield break;

            // Break if collection is empty or the query is outside the collections span
            if (IsEmpty || !Span.Overlaps(query))
                yield break;

            var set = new IntervalSet();

            var splitNode = _root;
            // Use a lambda instead of out, as out or ref isn't allowed for itorators
            foreach (var interval in findSplitNode(_root, query, n => { splitNode = n; }).Where(set.Add))
                yield return interval;

            // Find all intersecting intervals in left subtree
            if (query.Low.CompareTo(splitNode.Key) < 0)
                foreach (var interval in findLeft(splitNode.Left, query).Where(set.Add))
                    yield return interval;


            // Find all intersecting intervals in right subtree
            if (splitNode.Key.CompareTo(query.High) < 0)
                foreach (var interval in findRight(splitNode.Right, query).Where(set.Add))
                    yield return interval;
        }

        private static IEnumerable<I> findOverlaps(Node root, T query)
        {
            // Search the tree until we reach the bottom of the tree    
            while (root != null)
            {
                // Store compare value as we need it twice
                var compareTo = query.CompareTo(root.Key);

                // Query is to the left of the current node
                if (compareTo < 0)
                {
                    // Return all intervals in Less
                    if (root.Less != null && !root.Less.IsEmpty)
                        foreach (var interval in root.Less)
                            yield return interval;

                    // Move left
                    root = root.Left;
                }
                // Query is to the right of the current node
                else if (0 < compareTo)
                {
                    // Return all intervals in Greater
                    if (root.Greater != null && !root.Greater.IsEmpty)
                        foreach (var interval in root.Greater)
                            yield return interval;

                    // Move right
                    root = root.Right;
                }
                // Node with query value found
                else
                {
                    // Return all intervals in Equal
                    if (root.Equal != null && !root.Equal.IsEmpty)
                        foreach (var interval in root.Equal)
                            yield return interval;

                    // Stop as the search is done
                    yield break;
                }
            }
        }

        /// <summary>
        /// Create an enumerable, enumerating all intersecting intervals on the path to the split node. Returns the split node in splitNode.
        /// </summary>
        private static IEnumerable<I> findSplitNode(Node root, IInterval<T> query, Action<Node> setSplitNode)
        {
            while (root != null)
            {
                // Update split node
            setSplitNode(root);

            // Interval is lower than root, go left
            if (query.High.CompareTo(root.Key) < 0)
            {
                if (root.Less != null && !root.Less.IsEmpty)
                    foreach (var interval in root.Less)
                        yield return interval;

                    // Update root to left node
                    root = root.Left;
            }
            // Interval is higher than root, go right
            else if (root.Key.CompareTo(query.Low) < 0)
            {
                if (root.Greater != null && !root.Greater.IsEmpty)
                    foreach (var interval in root.Greater)
                        yield return interval;

                    // Update root to right node
                    root = root.Right;
            }
            // Otherwise add overlapping nodes in split node
            else
            {
                if (root.Less != null && !root.Less.IsEmpty)
                    foreach (var interval in root.Less.Where(i => query.Overlaps(i)))
                        yield return interval;
                if (root.Equal != null && !root.Equal.IsEmpty)
                    foreach (var interval in root.Equal.Where(i => query.Overlaps(i)))
                        yield return interval;
                if (root.Greater != null && !root.Greater.IsEmpty)
                    foreach (var interval in root.Greater.Where(i => query.Overlaps(i)))
                        yield return interval;

                    yield break;
                }
            }
        }

        private static IEnumerable<I> findLeft(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            var compareTo = query.Low.CompareTo(root.Key);

            //
            if (compareTo > 0)
            {
                if (root.Greater != null && !root.Greater.IsEmpty)
                    foreach (var interval in root.Greater)
                        yield return interval;

                // Recursively travese right subtree
                foreach (var interval in findLeft(root.Right, query))
                    yield return interval;
            }
            //
            else if (compareTo < 0)
            {
                if (root.Less != null && !root.Less.IsEmpty)
                    foreach (var interval in root.Less)
                        yield return interval;
                if (root.Equal != null && !root.Equal.IsEmpty)
                    foreach (var interval in root.Equal)
                        yield return interval;
                if (root.Greater != null && !root.Greater.IsEmpty)
                    foreach (var interval in root.Greater)
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
                if (root.Greater != null && !root.Greater.IsEmpty)
                    foreach (var interval in root.Greater)
                        yield return interval;

                if (query.LowIncluded && root.Equal != null && !root.Equal.IsEmpty)
                    foreach (var interval in root.Equal)
                        yield return interval;

                // If we find the matching node, we can add everything in the left subtree
                var child = getEnumerator(root.Right);
                while (child.MoveNext())
                    yield return child.Current;
            }
        }

        private static IEnumerable<I> findRight(Node root, IInterval<T> query)
        {
            // If root is null we have reached the end
            if (root == null) yield break;

            var compareTo = query.High.CompareTo(root.Key);

            //
            if (compareTo < 0)
            {
                // Add all intersecting intervals from left list
                if (root.Less != null && !root.Less.IsEmpty)
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
                if (root.Less != null && !root.Less.IsEmpty)
                    foreach (var interval in root.Less)
                        yield return interval;
                if (root.Equal != null && !root.Equal.IsEmpty)
                    foreach (var interval in root.Equal)
                        yield return interval;
                if (root.Greater != null && !root.Greater.IsEmpty)
                    foreach (var interval in root.Greater)
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
                if (root.Less != null && !root.Less.IsEmpty)
                    foreach (var interval in root.Less)
                        yield return interval;

                if (query.HighIncluded && root.Equal != null && !root.Equal.IsEmpty)
                    foreach (var interval in root.Equal)
                        yield return interval;

                // If we find the matching node, we can add everything in the left subtree
                var child = getEnumerator(root.Left);
                while (child.MoveNext())
                    yield return child.Current;
            }
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public bool FindOverlap(T query, ref I overlap)
        {
            var enumerator = FindOverlaps(query).GetEnumerator();
            var result = enumerator.MoveNext();

            if (result)
                overlap = enumerator.Current;

            return result;
        }

        /// <inheritdoc/>
        public bool FindOverlap(IInterval<T> query, ref I overlap)
        {
            var enumerator = FindOverlaps(query).GetEnumerator();
            var result = enumerator.MoveNext();

            if (result)
                overlap = enumerator.Current;

            return result;
        }

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

        #region Extensible

        /// <inheritdoc/>
        public bool IsReadOnly { get { return false; } }

        #region Add

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            // References to endpoint nodes needed when maintaining Interval
            Node lowNode = null, highNode = null;

            // Used to check if interval was actually added
            var intervalWasAdded = false;

            // Insert low endpoint
            var nodeWasAdded = false;
            _root = addLow(_root, null, interval, ref nodeWasAdded, ref intervalWasAdded, ref lowNode);

            // Insert high endpoint
            nodeWasAdded = false;
            _root = addHigh(_root, null, interval, ref nodeWasAdded, ref intervalWasAdded, ref highNode);

            // Increase counters and raise event if interval was added
            if (intervalWasAdded)
            {
                // Update MNO delta for low
                if (interval.LowIncluded)
                    lowNode.DeltaAt++;
                else
                    lowNode.DeltaAfter++;

                // Update MNO delta for high
                if (!interval.HighIncluded)
                    highNode.DeltaAt--;
                else
                    highNode.DeltaAfter--;

                // Update MNO
                updateMaximumOverlap(_root, interval);

                lowNode.IntervalsEndingInNode++;
                highNode.IntervalsEndingInNode++;

                _count++;
                raiseForAdd(interval);
            }

            // TODO: Add event for change in MNO

            return intervalWasAdded;
        }

        /// <inheritdoc/>
        public void AddAll(IEnumerable<I> intervals)
        {
            // TODO: Any fancy things missing here?
            foreach (var interval in intervals)
                Add(interval);
        }

        // TODO: Make iterative?
        private static Node addLow(Node root, Node right, I interval, ref bool nodeWasAdded, ref bool intervalWasAdded, ref Node lowNode)
        {
            Contract.Requires(!ReferenceEquals(interval, null));

            // No node existed for the low endpoint
            if (root == null)
            {
                root = new Node(interval.Low);
                nodeWasAdded = true;
                intervalWasAdded = true;
            }

            Contract.Assert(root != null);

            var compare = interval.Low.CompareTo(root.Key);

            if (compare > 0)
            {
                root.Right = addLow(root.Right, right, interval, ref nodeWasAdded, ref intervalWasAdded, ref lowNode);

                // Adjust node balance, if node was added
                if (nodeWasAdded)
                    root.Balance++;
            }
            else if (compare < 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                {
                    if (root.Greater == null)
                        root.Greater = new IntervalSet();
                    intervalWasAdded |= root.Greater.Add(interval);
                }

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.High) < 0)
                {
                    if (root.Equal == null)
                        root.Equal = new IntervalSet();

                    intervalWasAdded |= root.Equal.Add(interval);
                }

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Left = addLow(root.Left, root, interval, ref nodeWasAdded, ref intervalWasAdded, ref lowNode);

                // Adjust node balance, if node was added
                if (nodeWasAdded)
                    root.Balance--;
            }
            else
            {
                // If everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                {
                    if (root.Greater == null)
                        root.Greater = new IntervalSet();

                    intervalWasAdded |= root.Greater.Add(interval);
                }

                if (interval.LowIncluded)
                {
                    if (root.Equal == null)
                        root.Equal = new IntervalSet();

                    intervalWasAdded |= root.Equal.Add(interval);
                }

                // Save reference to endpoint node
                lowNode = root;
            }

            // Tree might be unbalanced after node was added, so we rotate
            if (nodeWasAdded && compare != 0)
                root = rotateForAdd(root, ref nodeWasAdded);

            return root;
        }

        private static Node addHigh(Node root, Node left, I interval, ref bool nodeWasAdded, ref bool intervalWasAdded, ref Node highNode)
        {
            Contract.Requires(!ReferenceEquals(interval, null));

            // No node existed for the high endpoint
            if (root == null)
            {
                root = new Node(interval.High);
                nodeWasAdded = true;
                intervalWasAdded = true;
            }

            Contract.Assert(root != null);

            var compare = interval.High.CompareTo(root.Key);

            if (compare < 0)
            {
                root.Left = addHigh(root.Left, left, interval, ref nodeWasAdded, ref intervalWasAdded, ref highNode);

                // Adjust node balance, if node was added
                if (nodeWasAdded)
                    root.Balance--;
            }
            else if (compare > 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                {
                    if (root.Less == null)
                        root.Less = new IntervalSet();

                    intervalWasAdded |= root.Less.Add(interval);
                }

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.Low) > 0)
                {
                    if (root.Equal == null)
                        root.Equal = new IntervalSet();

                    intervalWasAdded |= root.Equal.Add(interval);
                }

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Right = addHigh(root.Right, root, interval, ref nodeWasAdded, ref intervalWasAdded, ref highNode);

                // Adjust node balance, if node was added
                if (nodeWasAdded)
                    root.Balance++;
            }
            else
            {
                // If everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                {
                    if (root.Less == null)
                        root.Less = new IntervalSet();

                    intervalWasAdded |= root.Less.Add(interval);
                }

                if (interval.HighIncluded)
                {
                    if (root.Equal == null)
                        root.Equal = new IntervalSet();

                    intervalWasAdded |= root.Equal.Add(interval);
                }

                // Save reference to endpoint node
                highNode = root;
            }

            // Tree might be unbalanced after node was added, so we rotate
            if (nodeWasAdded && compare != 0)
                root = rotateForAdd(root, ref nodeWasAdded);

            return root;
        }

        #endregion

        #region Remove

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            // References to endpoint nodes needed when maintaining Interval
            Node lowNode = null, highNode = null;

            // Used to check if interval was actually added
            var intervalWasRemoved = false;

            // Remove low endpoint
            removeLow(_root, null, interval, ref intervalWasRemoved, ref lowNode);

            // Remove high endpoint
            removeHigh(_root, null, interval, ref intervalWasRemoved, ref highNode);

            // Increase counters and raise event if interval was added
            if (intervalWasRemoved)
            {
                // Update MNO delta for low
                if (interval.LowIncluded)
                    lowNode.DeltaAt--;
                else
                    lowNode.DeltaAfter--;
                // Update MNO delta for high
                if (!interval.HighIncluded)
                    highNode.DeltaAt++;
                else
                    highNode.DeltaAfter++;

                // Update MNO
                updateMaximumOverlap(_root, interval);

                // Check for unnecessary endpoint nodes, if interval was actually removed
                if (--lowNode.IntervalsEndingInNode == 0)
                    removeNodeWithKey(interval.Low);
                if (--highNode.IntervalsEndingInNode == 0)
                    removeNodeWithKey(interval.High);

                _count--;
                raiseForRemove(interval);
            }

            // TODO: Add event for change in MNO

            return intervalWasRemoved;
        }

        private static void removeLow(Node root, Node right, I interval, ref bool intervalWasRemoved, ref Node lowNode)
        {
            Contract.Requires(!ReferenceEquals(interval, null));

            // No node existed for the low endpoint
            if (root == null)
                return;

            var compare = interval.Low.CompareTo(root.Key);

            if (compare > 0)
                removeLow(root.Right, right, interval, ref intervalWasRemoved, ref lowNode);
            else if (compare < 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    intervalWasRemoved |= root.Greater.Remove(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.High) < 0)
                    intervalWasRemoved |= root.Equal.Remove(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                removeLow(root.Left, root, interval, ref intervalWasRemoved, ref lowNode);
            }
            else
            {
                // If everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    intervalWasRemoved |= root.Greater.Remove(interval);

                if (interval.LowIncluded)
                    intervalWasRemoved |= root.Equal.Remove(interval);

                // Save reference to endpoint node
                lowNode = root;
            }
        }

        private static void removeHigh(Node root, Node left, I interval, ref bool intervalWasRemoved, ref Node highNode)
        {
            // No node existed for the high endpoint
            if (root == null)
                return;

            var compare = interval.High.CompareTo(root.Key);

            if (compare < 0)
                removeHigh(root.Left, left, interval, ref intervalWasRemoved, ref highNode);
            else if (compare > 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    intervalWasRemoved |= root.Less.Remove(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.Low) > 0)
                    intervalWasRemoved |= root.Equal.Remove(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                removeHigh(root.Right, root, interval, ref intervalWasRemoved, ref highNode);
            }
            else
            {
                // If everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    intervalWasRemoved |= root.Less.Remove(interval);

                if (interval.HighIncluded)
                    intervalWasRemoved |= root.Equal.Remove(interval);

                // Save reference to endpoint node
                highNode = root;
            }

            // Update MNO
            if (intervalWasRemoved)
                root.UpdateMaximumOverlap();
        }

        private void removeNodeWithKey(T key)
        {
            // TODO: Implement
            var node = findNode(_root, key);

            // We only remove nodes that exist
            Contract.Assert(node != null);

            if (node.Left == null && node.Right == null)
            { }
        }

        private static Node removeNodeWithKey(Node root, T key, ref bool updateBalanace)
        {
            if (root == null)
                return null;

            var compare = root.Key.CompareTo(key);

            // Remove node from right subtree
            if (compare > 0)
            {
                root.Right = removeNodeWithKey(root.Right, key, ref updateBalanace);

                if (updateBalanace)
                    root.Balance--;
            }
            // Remove node from left subtree
            else if (compare < 0)
            {
                root.Left = removeNodeWithKey(root.Left, key, ref updateBalanace);

                if (updateBalanace)
                    root.Balance++;
            }
            // Node found
            else
            {
                updateBalanace = true;

                // Replace node with successor
                if (root.Left != null && root.Right != null)
                {
                    // TODO: maintain IBS invariant

                    var successor = findMinNode(root.Right);

                    var intervalsNeedingReinsertion = successor.GetIntervalsEndingInNode();

                    // Swap keys, so we can search for
                    root.SwapKeys(successor);

                    updateBalanace = false;

                    root.Right = removeNodeWithKey(root.Right, successor.Key, ref updateBalanace);

                    if (updateBalanace)
                        root.Balance--;
                }
                // Replace node with right child
                else if (root.Left == null)
                    // If no children root.Right is null too, so we just return null
                    return root.Right;
                // Replace node with left child
                else
                    return root.Left;
            }

            if (updateBalanace)
                root = rotateForRemove(root, ref updateBalanace);

            return root;
        }

        /// <summary>
        /// Find the node containing the search key.
        /// </summary>
        /// <param name="node">The root node which subtree should be searched.</param>
        /// <param name="searchKey">The key being searched.</param>
        /// <returns>The node containing the key if it exists, otherwise null.</returns>
        private static Node findNode(Node node, T searchKey)
        {
            while (node != null)
            {
                var compare = node.Key.CompareTo(searchKey);

                if (compare > 0)
                    node = node.Right;
                else if (compare < 0)
                    node = node.Left;
                else
                    break;
            }

            return node;
        }

        /// <summary>
        /// Find the current least node in the interval tree.
        /// </summary>
        /// <returns>The least node. Null if the tree is empty.</returns>
        private static Node findMinNode(Node node)
        {
            while (node != null)
                node = node.Left;

            return node;
        }

        #endregion

        #region Clear

        /// <summary>
        /// Remove all intervals from this collection.
        /// </summary>  
        public void Clear()
        {
            Contract.Ensures(_root == null);

            // Return if tree is empty
            if (_root == null)
                return;

            // Save old count and reset all values
            var oldCount = _count;
            clear();

            // Raise events
            if ((ActiveEvents & EventTypeEnum.Cleared) != 0)
                raiseCollectionCleared(true, oldCount);
            if ((ActiveEvents & EventTypeEnum.Changed) != 0)
                raiseCollectionChanged();
        }

        private void clear()
        {
            _root = null;
            _count = 0;
        }

        #endregion

        #endregion

        #endregion

        #region GraphViz

        /// <summary>
        /// Get a string representation of the tree in GraphViz dot format using QuickGraph.
        /// </summary>
        /// <returns>GraphViz string.</returns>
        public string QuickGraph()
        {
            var graph = new AdjacencyGraph<Node, Edge<Node>>();

            if (_root != null)
            {
                var node = new Node(default(T));
                graph.AddVertex(node);
                graph.AddEdge(new Edge<Node>(node, _root));
            }

            foreach (var node in nodeEnumerator(_root))
            {
                graph.AddVertex(node);

                if (node.Left != null)
                {
                    graph.AddVertex(node.Left);
                    graph.AddEdge(new Edge<Node>(node, node.Left));
                }

                if (node.Right != null)
                {
                    graph.AddVertex(node.Right);
                    graph.AddEdge(new Edge<Node>(node, node.Right));
                }
            }

            var gw = new GraphvizAlgorithm<Node, Edge<Node>>(graph);

            gw.FormatVertex += delegate(object sender, FormatVertexEventArgs<Node> e)
            {
                e.VertexFormatter.Shape = GraphvizVertexShape.Record;
                e.VertexFormatter.Style = GraphvizVertexStyle.Rounded;
                e.VertexFormatter.Font = new GraphvizFont("consola", 12);

                // Generate main cell
                var cell = new GraphvizRecordCell();
                // Add Key in top cell
                cell.Cells.Add(new GraphvizRecordCell { Text = e.Vertex.Key.ToString() });
                // Add Less, Equal and Greater set in bottom cell
                var bottom = new GraphvizRecordCell();
                if (e.Vertex.Less != null)
                    bottom.Cells.Add(new GraphvizRecordCell { Text = e.Vertex.Less.ToString() });
                if (e.Vertex.Equal != null)
                    bottom.Cells.Add(new GraphvizRecordCell { Text = e.Vertex.Equal.ToString() });
                if (e.Vertex.Greater != null)
                    bottom.Cells.Add(new GraphvizRecordCell { Text = e.Vertex.Greater.ToString() });
                cell.Cells.Add(bottom);
                // Add cell to record
                e.VertexFormatter.Record.Cells.Add(cell);
            };
            gw.FormatEdge += delegate(object sender, FormatEdgeEventArgs<Node, Edge<Node>> e)
            {
                e.EdgeFormatter.Label = new GraphvizEdgeLabel
                {
                    Value = (e.Edge.Target.Balance > 0 ? "+" : "") + e.Edge.Target.Balance + " / " + e.Edge.Target.IntervalsEndingInNode
                };
            };


            var graphviz = gw.Generate();

            return graphviz.Replace("GraphvizColor", "color");
        }

        /// <summary>
        /// Print the tree structure in Graphviz format
        /// </summary>
        /// <returns></returns>
        public string Graphviz()
        {
            return "digraph IntervalBinarySearchTree {\n"
                + "\tnode [shape=record, style=rounded];\n"
                + graphviz(_root, "root", null)
                + "}\n";
        }

        private int _nodeCounter;
        private int _nullCounter;

        private string graphviz(Node root, string parent, string direction)
        {
            int id;
            if (root == null)
            {
                id = _nullCounter++;
                return String.Format("\tleaf{0} [shape=point];\n", id) +
                    String.Format("\t{0}:{1} -> leaf{2};\n", parent, direction, id);
            }

            id = _nodeCounter++;
            var rootString = direction == null ? "" : String.Format("\t{0} -> struct{1}:n;\n", parent, id);

            return
                // Creates the structid: structid [label="<key> keyValue|{lessSet|equalSet|greaterSet}|{<idleft> leftChild|<idright> rightChild}"];
                String.Format("\tstruct{0} [fontname=consola, label=\"{{<key> {1}|{{{2}|{3}|{4}}}}}\"];\n", id, root.Key, root.Less, root.Equal, root.Greater)

                // Links the parents leftChild to nodeid: parent:left -> structid:key;
                + rootString

                // Calls graphviz() recursively on leftChild
                + graphviz(root.Left, "struct" + id, "left")

                // Calls graphviz() recursively on rightChild
                + graphviz(root.Right, "struct" + id, "right");
        }

        #endregion

    }
}
