using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

#if DEBUG
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
#endif

namespace C5.Intervals
{
    // TODO: Implement CountOverlaps with count variable in each node

    /// <summary>
    /// An doubly-linked binary search tree for non-overlapping intervals.
    /// </summary>
    /// <typeparam name="I">The interval type.</typeparam>
    /// <typeparam name="T">The interval endpoint type.</typeparam>
    public class DoublyLinkedFiniteIntervalTree<I, T> : FiniteIntervalCollectionBase<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private Node _root;
        private readonly Node _first, _last;
        private readonly IDictionary<I, Node> _nodeDictionary;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // Check the balance invariant holds.
            Contract.Invariant(contractHelperConfirmBalance(_root));

            // First and last point to each other if the collection is empty
            Contract.Invariant(!IsEmpty || _first.Next == _last && _last.Previous == _first);
            // First and last are empty but the next and previous pointers respectively
            Contract.Invariant(_first.Key == null && _first.Previous == null && _first.Right == null && _first.Left == null && _first.Balance == 0);
            Contract.Invariant(_last.Key == null && _last.Next == null && _last.Right == null && _last.Left == null && _last.Balance == 0);
            Contract.Invariant(_first != _last);

            // Check enumerator is sorted
            Contract.Invariant(this.IsSorted<I, T>());

            // Check that doubly linked lists are sorted in both direction
            Contract.Invariant(enumerateFrom(_first.Next).IsSorted<I, T>());
            Contract.Invariant(enumerateBackwardsFrom(_last.Previous).Reverse().IsSorted<I, T>());

            // Check in-order traversal is sorted
            Contract.Invariant(contractHelperInOrderNodes(_root).IsSorted());

            // Check node dictionary isn't bigger than tree
            Contract.Invariant(Count == _nodeDictionary.Count);
            // Check that node dictionary is up to date
            Contract.Invariant(contractHelperDictionaryIsUpdated());
        }

        /// <summary>
        /// Checks that the contractHelperHeight of the tree is balanced.
        /// </summary>
        /// <returns>True if the tree is balanced, else false.</returns>
        [Pure]
        private static bool contractHelperConfirmBalance(Node root)
        {
            var result = true;
            contractHelperHeight(root, ref result);
            return result;
        }

        /// <summary>
        /// Get the contractHelperHeight of the tree.
        /// </summary>
        /// <param name="node">The node you wish to check the contractHelperHeight on.</param>
        /// <param name="result">Reference to a bool that will be set to false if an in-balance is discovered.</param>
        /// <returns>Height of the tree.</returns>
        [Pure]
        private static int contractHelperHeight(Node node, ref bool result)
        {
            if (node == null)
                return 0;

            var heightLeft = contractHelperHeight(node.Left, ref result);
            var heightRight = contractHelperHeight(node.Right, ref result);

            if (node.Balance != heightRight - heightLeft)
                result = false;

            return Math.Max(heightLeft, heightRight) + 1;
        }

        [Pure]
        private static IEnumerable<Node> contractHelperInOrderNodes(Node root)
        {
            if (root == null)
                yield break;

            foreach (var node in contractHelperInOrderNodes(root.Left))
                yield return node;

            yield return root;

            foreach (var node in contractHelperInOrderNodes(root.Right))
                yield return node;
        }

        [Pure]
        private bool contractHelperDictionaryIsUpdated()
        {
            var node = _first.Next;

            while (node != _last)
            {
                if (!ReferenceEquals(_nodeDictionary[node.Key], node))
                    return false;

                node = node.Next;
            }

            return true;
        }

        #endregion

        #region Inner Classes

        private sealed class Node : IComparable<Node>
        {
            #region Fields

            public I Key;

            public Node Left, Right;
            public Node Previous, Next;

            public int Count, Balance;

#if DEBUG
            public readonly bool Dummy;
#endif

            #endregion

            #region Code Contracts

            [ContractInvariantMethod]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
            private void invariant()
            {
                Contract.Invariant(Key == null || Count == count(Left) + 1 + count(Right));
                Contract.Invariant(Key == null || Count == subtree(this).Count());
            }

            private IEnumerable<Node> subtree(Node root)
            {
                if (root.Left != null)
                    foreach (var node in subtree(root.Left))
                        yield return node;

                yield return root;

                if (root.Right != null)
                    foreach (var node in subtree(root.Right))
                        yield return node;
            }

            #endregion

            #region Constructor

            public Node(I key, Node previous)
            {
                Contract.Requires(key != null);
                Contract.Requires(previous != null);
                Contract.Requires(previous.Next != null);
                Contract.Ensures(key != null);
                Contract.Ensures(Next != null && Previous != null);

                Key = key;
                Count = 1;
                insertAfter(previous);
            }

            public Node()
            {
#if DEBUG
                Dummy = true;
#endif
            }

            #endregion

            private void insertAfter(Node previous)
            {
                Contract.Requires(previous != null);
                Contract.Requires(previous.Next != null);
                Contract.Ensures(Contract.OldValue(previous.Next) == previous.Next.Next);
                Contract.Ensures(previous.Next == this);
                Contract.Ensures(this.Previous == previous);
                Contract.Ensures(Contract.OldValue(previous.Next).Previous == this);
                Contract.Ensures(this.Next == Contract.OldValue(previous.Next));

                var next = previous.Next;

                previous.Next = this;
                Previous = previous;

                Next = next;
                next.Previous = this;
            }

            public void Remove()
            {

                Previous.Next = Next;
                Next.Previous = Previous;
            }

            public int CompareTo(Node other)
            {
                return Key.CompareTo(other.Key);
            }

            public void Swap(Node successor)
            {
                Contract.Requires(successor != null);

                var tmp = Key;
                Key = successor.Key;
                successor.Key = tmp;
            }

            public void UpdateCount()
            {
                Count = count(Left) + 1 + count(Right);
            }

            public override string ToString()
            {
                return Key == null ? "" : Key.ToIntervalString();
            }
        }

        #endregion

        #region AVL Tree Methods

        private static Node rotateForAdd(Node root, ref bool rotationNeeded)
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
                    rotationNeeded = false;
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
                            root.Left.Balance = (sbyte)(root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte)(root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }
                    rotationNeeded = false;
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
                            root.Left.Balance = (sbyte)(root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte)(root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }

                    rotationNeeded = false;
                    break;
            }

            return root;
        }

        private static Node rotateForRemove(Node root, ref bool rotationNeeded)
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
                    rotationNeeded = false;
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
                            rotationNeeded = false;
                            break;

                        // Left Right Case
                        case +1:
                            root.Left = rotateLeft(root.Left);
                            root = rotateRight(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte)((root.Balance == +1) ? -1 : 0);
                            root.Right.Balance = (sbyte)((root.Balance == -1) ? +1 : 0);
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
                            rotationNeeded = false;
                            break;

                        // Right Left Case
                        case -1:
                            root.Right = rotateRight(root.Right);
                            root = rotateLeft(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte)(root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte)(root.Balance == -1 ? +1 : 0);
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

            root.UpdateCount();
            node.UpdateCount();

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

            root.UpdateCount();
            node.UpdateCount();

            return node;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create an empty Doubly-linked Finite Interval Tree.
        /// </summary>
        public DoublyLinkedFiniteIntervalTree()
        {
            Contract.Ensures(_first != null);
            Contract.Ensures(_last != null);
            Contract.Ensures(_first.Next == _last);
            Contract.Ensures(_last.Previous == _first);

            _first = new Node();
            _last = new Node();

            _first.Next = _last;
            _last.Previous = _first;

            _nodeDictionary = new HashDictionary<I, Node>(IntervalExtensions.CreateReferenceEqualityComparer<I, T>());
        }

        /// <summary>
        /// Create an Doubly-linked Finite Interval Tree from a collection of intervals.
        /// This has the same effect as creating the collection and then calling <see cref="IIntervalCollection{I,T}.AddAll"/>.
        /// </summary>
        public DoublyLinkedFiniteIntervalTree(IEnumerable<I> intervals)
            : this()
        {
            Contract.Requires(intervals != null);

            foreach (var interval in intervals)
                Add(interval);
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
        public override int Count { get { return count(_root); } }

        [Pure]
        private static int count(Node node)
        {
            return node == null ? 0 : node.Count;
        }

        /// <inheritdoc/>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _root.Key;
        }

        #endregion

        #region Interval Collection

        #region Properties

        #region Data Structure Properties

        /// <inheritdoc/>
        public override bool IsReadOnly { get { return false; } }

        /// <inheritdoc/>
        public override Speed IndexingSpeed { get { return Speed.Log; } }

        #endregion

        #region Collection Properties

        /// <inheritdoc/>
        public override I LowestInterval { get { return _first.Next.Key; } }

        /// <inheritdoc/>
        public override I HighestInterval { get { return _last.Previous.Key; } }

        #endregion

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public override IEnumerator<I> GetEnumerator() { return Sorted().GetEnumerator(); }

        /// <inheritdoc/>
        public override IEnumerable<I> Sorted() { return enumerateFrom(_first.Next); }

        /// <inheritdoc/>
        public override IEnumerable<I> SortedBackwards() { return enumerateBackwardsFrom(_last.Previous); }

        #region Enumerate from Point

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateFrom(T point, bool includeOverlaps = true)
        {
            bool overlaps;
            var node = findNode(point, out overlaps);
            return node == null ? Enumerable.Empty<I>() : enumerateFrom(!includeOverlaps && overlaps ? node.Next : node);
        }

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateBackwardsFrom(T point, bool includeOverlaps = true)
        {
            bool overlaps;
            var node = findNode(point, out overlaps);
            return node == null ? Enumerable.Empty<I>() : enumerateBackwardsFrom(includeOverlaps && overlaps ? node : node.Previous);
        }

        #endregion

        #region Enumerate from Interval

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateFrom(I interval, bool includeInterval = true)
        {
            bool intervalFound;
            var node = findContainingNode(interval, out intervalFound);
            return intervalFound ? enumerateFrom(includeInterval ? node : node.Next) : Enumerable.Empty<I>();
        }

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateBackwardsFrom(I interval, bool includeInterval = true)
        {
            bool intervalFound;
            var node = findContainingNode(interval, out intervalFound);
            return intervalFound ? enumerateBackwardsFrom(includeInterval ? node : node.Previous) : Enumerable.Empty<I>();
        }

        #endregion

        #region Enumerate from Index

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateFromIndex(int index)
        {
            return enumerateFrom(indexer(_root, index));
        }

        /// <inheritdoc/>
        public override IEnumerable<I> EnumerateBackwardsFromIndex(int index)
        {
            return enumerateBackwardsFrom(indexer(_root, index));
        }

        #endregion

        #region Helpers

        [Pure]
        private IEnumerable<I> enumerateFrom(Node node)
        {
            Contract.Requires(node != null);
            Contract.Requires(node != _first);

            // Iterate until the _last node
            while (node != _last)
            {
                yield return node.Key;
                node = node.Next;
            }
        }

        [Pure]
        private IEnumerable<I> enumerateBackwardsFrom(Node node)
        {
            Contract.Requires(node != null);
            Contract.Requires(node != _last);

            // Iterate until the _first node
            while (node != _first)
            {
                yield return node.Key;
                node = node.Previous;
            }
        }

        [Pure]
        private Node findContainingNode(IInterval<T> interval, out bool intervalFound)
        {
            Contract.Requires(interval != null);
            Contract.Ensures(!Contract.ValueAtReturn(out intervalFound) || ReferenceEquals(Contract.Result<Node>(), _nodeDictionary[interval as I]));

            intervalFound = false;
            var node = _root;

            while (node != null)
            {
                var compare = interval.CompareLow(node.Key);
                if (compare < 0)
                    node = node.Left;
                else if (compare > 0)
                    node = node.Right;
                else
                {
                    intervalFound = ReferenceEquals(interval, node.Key);
                    return node;
                }
            }

            return null;
        }

        [Pure]
        private Node findContainingNode(I interval, out bool intervalFound)
        {
            Contract.Requires(interval != null);
            Contract.Ensures(Contract.ValueAtReturn(out intervalFound) == (Contract.Result<Node>() != null));

            Node node;
            intervalFound = _nodeDictionary.Find(ref interval, out node);
            return node;
        }

        #endregion

        #endregion

        #region Find Equals

        public override IEnumerable<I> FindEquals(IInterval<T> query)
        {
            bool intervalFound;
            var node = findContainingNode(query, out intervalFound);
            if (intervalFound || node != null)
                yield return node.Key;
        }

        #endregion

        #region Find Overlaps

        /// <inheritdoc/>
        public override IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (IsEmpty)
                yield break;

            var node = _root;

            // Search for the node containing the first overlap
            while (true)
            {
                var compare = query.CompareLowHigh(node.Key);

                if (compare < 0)
                {
                    if (node.Left == null)
                        break;

                    // Move left
                    node = node.Left;
                }
                else if (compare > 0)
                {
                    if (node.Right == null)
                    {
                        // If there is no right node, use the next in sorting order
                        node = node.Next;

                        // Make sure the low is still lower or equal to the query's high
                        if (node == _last || query.CompareLowHigh(node.Key) > 0)
                            yield break;

                        break;
                    }

                    // Move right
                    node = node.Right;
                }
                else
                    break;
            }

            // Iterate overlaps
            foreach (var interval in enumerateFrom(node).TakeWhile(x => x.CompareLowHigh(query) <= 0))
                yield return interval;
        }

        #endregion

        #region Find Overlap

        /// <inheritdoc/>
        public override bool FindOverlap(T query, out I overlap)
        {
            bool overlaps;
            var node = findNode(query, out overlaps);
            overlap = overlaps ? node.Key : null;
            return overlaps;
        }

        /// <summary>
        /// Returns the node containing an interval overlapping <paramref name="query"/>.
        /// If no interval overlaps, the node with the first interval after
        /// <paramref name="query"/> is returned.
        /// </summary>
        /// <param name="query">The query point.</param>
        /// <param name="overlaps">True if the node's interval overlaps <paramref name="query"/>.</param>
        /// <returns>The node.</returns>
        private Node findNode(T query, out bool overlaps)
        {
            overlaps = false;

            // Stop immediately if empty
            if (IsEmpty)
                return null;

            var node = _root;
            while (true)
            {
                var compare = query.CompareTo(node.Key.Low);

                if (compare < 0)
                {
                    // Continue search left if possible
                    if (node.Left != null)
                        node = node.Left;
                    else
                    {
                        // Move to the previous node as it might contain an overlapping interval
                        var previous = node.Previous;

                        // Check if previous interval overlaps
                        if (previous != _first && previous.Key.Overlaps(query))
                        {
                            overlaps = true;
                            return previous;
                        }

                        // Stop as no overlap exists
                        return node;
                    }
                }
                else if (compare > 0)
                {
                    // Continue search right if possible
                    if (node.Right != null)
                        node = node.Right;
                    else
                    {
                        // Check if interval overlaps
                        if (node.Key.Overlaps(query))
                        {
                            overlaps = true;
                            return node;
                        }

                        // Stop as no overlap exists
                        return node.Next;
                    }
                }
                else
                {
                    // Check if low is included, thereby overlapping the query
                    if (node.Key.LowIncluded)
                    {
                        overlaps = true;
                        return node;
                    }

                    // Check if previous node contains overlap
                    var previous = node.Previous;

                    // Check if the previous interval overlaps
                    if (previous != _first && previous.Key.High.CompareTo(query) == 0 && previous.Key.HighIncluded)
                    {
                        overlaps = true;
                        return previous;
                    }

                    // Stop as no overlap exists
                    return node;
                }
            }
        }

        /// <inheritdoc/>
        public override bool FindOverlap(IInterval<T> query, out I overlap)
        {
            overlap = null;

            var node = _root;

            while (node != null)
            {
                if (query.CompareHighLow(node.Key) < 0)
                    node = node.Left;
                else if (query.CompareLowHigh(node.Key) > 0)
                    node = node.Right;
                else
                {
                    overlap = node.Key;
                    return true;
                }
            }

            return false;
        }


        #region Neighbourhood

        /// <inheritdoc/>
        public override Neighbourhood<I, T> GetNeighbourhood(T query)
        {
            if (IsEmpty)
                return new Neighbourhood<I, T>();

            bool overlaps;
            var node = findNode(query, out overlaps);

            var previous = node != _first ? node.Previous.Key : null;
            var overlap = overlaps ? node.Key : null;
            var next = overlaps ? node.Next.Key : node.Key;

            return new Neighbourhood<I, T>(previous, overlap, next);
        }

        /// <inheritdoc/>
        public override Neighbourhood<I, T> GetNeighbourhood(I query)
        {
            bool intervalFound;
            var node = findContainingNode(query, out intervalFound);

            if (!intervalFound)
                return new Neighbourhood<I, T>();

            var previous = node != _first ? node.Previous.Key : null;
            var overlap = query;
            var next = node != _last ? node.Next.Key : null;

            return new Neighbourhood<I, T>(previous, overlap, next);
        }

        #endregion

        #endregion

        #region Extensible

        #region Add

        /// <inheritdoc/>
        public override sealed bool Add(I interval)
        {
            var intervalWasAdded = false;
            var rotationNeeded = false;

            _root = add(interval, _root, _first, ref rotationNeeded, ref intervalWasAdded);

            if (intervalWasAdded)
                raiseForAdd(interval);

            return intervalWasAdded;
        }

        private Node add(I interval, Node root, Node previous, ref bool rotationNeeded, ref bool intervalWasAdded)
        {
            if (root == null)
            {
                // Return if previous or next overlaps interval
                if (previous != _first && previous.Key.CompareHighLow(interval) >= 0 ||
                    previous.Next != _last && interval.CompareHighLow(previous.Next.Key) >= 0)
                    return null;

                rotationNeeded = true;
                intervalWasAdded = true;
                var node = new Node(interval, previous);
                _nodeDictionary.Add(interval, node);
                return node;
            }

            var compare = interval.CompareLow(root.Key);

            if (compare < 0)
            {
                root.Left = add(interval, root.Left, root.Previous, ref rotationNeeded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance--;
            }
            else if (compare > 0)
            {
                root.Right = add(interval, root.Right, root, ref rotationNeeded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance++;
            }
            else
            {
                // Interval was not added
                return root;
            }

            if (intervalWasAdded)
                ++root.Count;

            // Tree might be unbalanced after node was added, so we rotate
            if (rotationNeeded)
                root = rotateForAdd(root, ref rotationNeeded);

            return root;
        }

        #region Force Add

        // TODO: Does this belong in C5.Intervals?
        // public bool ForceAdd(I interval, Func<I, I, bool> action, bool continueWhenNoConflict = false, bool forcePosition = false)
        // {
        //     return ForceAdd(interval, action, () => continueWhenNoConflict, forcePosition);
        // }
        // /// <summary>
        // /// Forcingly adds an interval, even if it has overlaps in the collection. The 
        // /// function must resolve any overlapping conflict for a pair of overlapping 
        // /// intervals by moving either interval. For each pair of overlapping interveals
        // /// <paramref name="action"/> will be called to resolve the conflicts that arose 
        // /// from the insertion.
        // /// </summary>
        // /// <param name="interval">The interval.</param>
        // /// <param name="action">The action, that will resolve overlap conflict.</param>
        // // TODO: Document continueWhenNoConflict and forcePosition
        // /// <param name="continueWhenNoConflict"></param>
        // /// <param name="forcePosition"></param>
        // /// <returns>True, if the action was called to resolve a conflict.</returns>
        // public bool ForceAdd(I interval, Func<I, I, bool> action, Func<bool> continueWhenNoConflict, bool forcePosition = false)
        // {
        //     var rotationNeeded = false;
        //     Node node;
        //     _root = forceAdd(interval, _root, _first, ref rotationNeeded, out node);
        // 
        //     // If interval has the same low value as node then we need to swap
        //     if (node.Next.Key != null)
        //         if (node.Key.LowEquals(node.Next.Key))
        //             node.Next.Swap(node);
        // 
        //     if (forcePosition && node.Previous != _first && node.Previous.Key.CompareHighLow(node.Key) <= 0)
        //     {
        //         node.Previous.Swap(node);
        //         node = node.Previous;
        //     }
        // 
        //     var result = false;
        // 
        //     while (node.Next != _last && node.Key.CompareHighLow(node.Next.Key) >= 0 || continueWhenNoConflict())
        //     {
        //         result = true;
        //         if (action(node.Key, node.Next.Key))
        //         {
        //             Remove(node.Next.Key);
        //             continue;
        //         }
        // 
        //         if (node.Key.CompareHighLow(node.Next.Key) >= 0)
        //             throw new InvalidOperationException("The interval has to be schedule after the previous one. The invariant is now broken.");
        // 
        //         // Move to next node
        //         node = node.Next;
        //     }
        // 
        //     return result;
        // }
        // 
        // private Node forceAdd(I interval, Node root, Node previous, ref bool rotationNeeded, out Node startNode)
        // {
        //     if (root == null)
        //     {
        //         rotationNeeded = true;
        //         var node = new Node(interval, previous);
        // 
        //         startNode = previous != _first && previous.Key.CompareHighLow(interval) >= 0 ? previous : node;
        // 
        //         return node;
        //     }
        // 
        //     var compare = interval.CompareLow(root.Key);
        // 
        //     if (compare < 0)
        //     {
        //         root.Left = forceAdd(interval, root.Left, root.Previous, ref rotationNeeded, out startNode);
        // 
        //         // Adjust node balance, if node was added
        //         if (rotationNeeded)
        //             root.Balance--;
        //     }
        //     else
        //     {
        //         root.Right = forceAdd(interval, root.Right, root, ref rotationNeeded, out startNode);
        // 
        //         // Adjust node balance, if node was added
        //         if (rotationNeeded)
        //             root.Balance++;
        //     }
        // 
        //     // Tree might be unbalanced after node was added, so we rotate
        //     if (rotationNeeded)
        //         root = rotateForAdd(root, ref rotationNeeded);
        // 
        //     return root;
        // }

        #endregion

        #endregion

        #region Remove

        /// <inheritdoc/>
        public override bool Remove(I interval)
        {
            var intervalWasRemoved = false;
            var rotationNeeded = false;
            _root = remove(interval, _root, ref intervalWasRemoved, ref rotationNeeded);

            if (intervalWasRemoved)
                raiseForRemove(interval);

            return intervalWasRemoved;
        }

        private Node remove(I interval, Node root, ref bool intervalWasRemoved, ref bool rotationNeeded)
        {
            if (root == null)
                return null;

            var compare = interval.CompareTo(root.Key);

            if (compare < 0)
            {
                root.Left = remove(interval, root.Left, ref intervalWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance++;
            }
            else if (compare > 0)
            {
                root.Right = remove(interval, root.Right, ref intervalWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance--;
            }
            else if (!ReferenceEquals(root.Key, interval))
            {
                return root;
            }
            else if (root.Left != null && root.Right != null)
            {
                var successor = root.Next;

                // Update dictionary
                _nodeDictionary.Remove(interval);
                _nodeDictionary[successor.Key] = root;

                // Swap root and successor nodes
                root.Swap(successor);


                // Remove the successor node
                root.Right = remove(successor.Key, root.Right, ref intervalWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance--;
            }
            else
            {
                rotationNeeded = true;
                intervalWasRemoved = true;
                root.Remove();

                // Update dictionary
                _nodeDictionary.Remove(interval);

                // Return Left if not null, otherwise Right - one must be null
                return root.Left ?? root.Right;
            }

            if (intervalWasRemoved)
                --root.Count;

            if (rotationNeeded)
                root = rotateForRemove(root, ref rotationNeeded);

            return root;
        }

        #endregion

        #region Clear

        /// <inheritdoc/>
        protected override void clear()
        {
            Contract.Ensures(_root == null);
            Contract.Ensures(_first.Next == _last);
            Contract.Ensures(_last.Previous == _first);

            _root = null;

            _first.Next = _last;
            _last.Previous = _first;
        }

        #endregion

        #endregion

        #region Indexed Access

        public override int IndexOf(I interval)
        {
            bool intervalFound;
            var index = indexOf(interval, _root, out intervalFound);
            return intervalFound ? index : ~index;
        }

        private static int indexOf(IInterval<T> interval, Node root, out bool intervalFound)
        {
            intervalFound = false;
            var index = 0;

            while (root != null)
            {
                var compareLow = interval.CompareLow(root.Key);

                if (compareLow < 0)
                    root = root.Left;
                else if (compareLow > 0)
                {
                    index += 1 + count(root.Left);
                    root = root.Right;
                }
                else
                {
                    intervalFound = ReferenceEquals(interval, root.Key);
                    index += count(root.Left) + (interval.CompareHigh(root.Key) <= 0 ? 0 : 1);
                    break;
                }
            }

            return index;
        }

        public override I this[int i] { get { return indexer(_root, i).Key; } }

        private static Node indexer(Node node, int index)
        {
            while (true)
            {
                var leftCount = count(node.Left);

                if (index < leftCount)
                    node = node.Left;
                else if (index > leftCount)
                {
                    node = node.Right;
                    index -= leftCount + 1;
                }
                else
                    return node;
            }
        }

        #endregion

        #endregion

        #region QuickGraph

#if DEBUG
        /// <summary>
        /// Get a string representation of the tree in GraphViz dot format using QuickGraph.
        /// </summary>
        /// <returns>GraphViz string.</returns>
        public string QuickGraph
        {
            get
            {
                var graph = new AdjacencyGraph<Node, Edge<Node>>();
                if (_root != null)
                {
                    var node = new Node();
                    graph.AddVertex(node);
                    graph.AddEdge(new Edge<Node>(node, _root));
                }

                foreach (var node in contractHelperInOrderNodes(_root))
                {
                    graph.AddVertex(node);
                    if (node.Left != null)
                    {
                        graph.AddVertex(node.Left);
                        graph.AddEdge(new Edge<Node>(node, node.Left));
                    }
                    else
                    {
                        var dummy = new Node();
                        graph.AddVertex(dummy);
                        graph.AddEdge(new Edge<Node>(node, dummy));
                    }

                    if (node.Right != null)
                    {
                        graph.AddVertex(node.Right);
                        graph.AddEdge(new Edge<Node>(node, node.Right));
                    }
                    else
                    {
                        var dummy = new Node();
                        graph.AddVertex(dummy);
                        graph.AddEdge(new Edge<Node>(node, dummy));
                    }

                    /* 
                    if (node.Previous != null)
                    {
                        graph.AddVertex(node.Previous);
                        graph.AddEdge(new Edge<Node>(node, node.Previous));
                    }
                    else
                    {
                        var dummy = new Node();
                        graph.AddVertex(dummy);
                        graph.AddEdge(new Edge<Node>(node, dummy));
                    }

                    if (node.Next != null)
                    {
                        graph.AddVertex(node.Next);
                        graph.AddEdge(new Edge<Node>(node, node.Next));
                    }
                    else
                    {
                        var dummy = new Node();
                        graph.AddVertex(dummy);
                        graph.AddEdge(new Edge<Node>(node, dummy));
                    }
                    */
                }

                var gw = new GraphvizAlgorithm<Node, Edge<Node>>(graph);
                gw.FormatVertex += delegate(object sender, FormatVertexEventArgs<Node> e)
                {
                    if (e.Vertex.Dummy)
                    {
                        e.VertexFormatter.Shape = GraphvizVertexShape.Point;
                    }
                    else
                    {
                        e.VertexFormatter.Shape = GraphvizVertexShape.Record;
                        e.VertexFormatter.Style = GraphvizVertexStyle.Rounded;
                        e.VertexFormatter.Font = new GraphvizFont("consola", 12);
                        // Generate main cell
                        var cell = new GraphvizRecordCell();
                        // Add Key in top cell
                        cell.Cells.Add(new GraphvizRecordCell { Text = e.Vertex.Key.ToString() });


                        // Add cell to record
                        e.VertexFormatter.Record.Cells.Add(cell);
                    }
                };
                return gw.Generate();
            }
        }

#endif

        #endregion
    }
}
