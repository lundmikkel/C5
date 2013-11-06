using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Text;
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;

namespace C5.intervals
{

    /// <summary>
    /// Key Tree class
    /// </summary>
    public class DynamicIntervalTree<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private Node _root;
        private int _count;

        private static readonly IComparer<IInterval<T>> Comparer = ComparerFactory<IInterval<T>>.CreateComparer((x, y) => y.CompareHigh(x));

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // Check the balance invariant holds.
            Contract.Invariant(confirmBalance(_root));

            // Check nodes are sorted
            Contract.Invariant(checkNodesAreSorted(_root));
            // Check spans
            Contract.Invariant(checkNodeSpans(_root));

            // Check that the MNO variables are correct for all nodes
            Contract.Invariant(checkMnoForEachNode(_root));

            // Check that the intervals are correctly placed
            Contract.Invariant(confirmIntervalPlacement(_root));
        }

        /// <summary>
        /// Checks that the height of the tree is balanced.
        /// </summary>
        /// <returns>True if the tree is balanced, else false.</returns>
        [Pure]
        private static bool confirmBalance(Node root)
        {
            var result = true;
            height(root, ref result);
            return result;
        }

        /// <summary>
        /// Get the height of the tree.
        /// </summary>
        /// <param name="node">The node you wish to check the height on.</param>
        /// <param name="result">Reference to a bool that will be set to false if an in-balance is discovered.</param>
        /// <returns>Height of the tree.</returns>
        [Pure]
        // TODO Can we use ref in pure methods?
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
        private static bool checkNodesAreSorted(Node root)
        {
            return nodes(root).IsSorted();
        }

        [Pure]
        private static bool checkNodeSpans(Node root)
        {
            foreach (var node in nodes(root))
                if (node.Span != null && node.LocalSpan != null && !node.Span.Contains(node.LocalSpan))
                    return false;

            return true;
        }

        [Pure]
        private bool checkMnoForEachNode(Node root)
        {
            var intervalsByEndpoint = getIntervalsByEndpoint();

            foreach (var keyValuePair in intervalsByEndpoint)
            {
                var key = keyValuePair.Key;
                var intervals = keyValuePair.Value;

                var deltaAt = 0;
                var deltaAfter = 0;

                foreach (var interval in intervals)
                {
                    if (interval.Low.CompareTo(key) == 0)
                    {
                        if (interval.LowIncluded)
                            deltaAt++;
                        else
                            deltaAfter++;
                    }

                    if (interval.High.CompareTo(key) == 0)
                    {
                        if (interval.HighIncluded)
                            deltaAfter--;
                        else
                            deltaAt--;
                    }
                }

                var node = findNode(root, key);

                // Check DeltaAt
                if (node.DeltaAt != deltaAt)
                    return false;

                // Check DeltaAfter
                if (node.DeltaAfter != deltaAfter)
                    return false;

                // Check Sum and Max
                if (!checkMno(node))
                    return false;
            }

            return true;
        }

        [Pure]
        private IEnumerable<KeyValuePair<T, ArrayList<I>>> getIntervalsByEndpoint()
        {
            var dictionary = new TreeDictionary<T, ArrayList<I>>();

            foreach (var interval in this)
            {
                // Make sure the sets exist
                if (!dictionary.Contains(interval.Low))
                    dictionary.Add(interval.Low, new ArrayList<I>());
                if (!dictionary.Contains(interval.High))
                    dictionary.Add(interval.High, new ArrayList<I>());

                // Add interval for low and high
                dictionary[interval.Low].Add(interval);

                if (!interval.IsPoint())
                    dictionary[interval.High].Add(interval);
            }

            return dictionary;
        }

        [Pure]
        private static bool checkMno(Node node)
        {
            Contract.Requires(node != null);

            // Check sum
            var sum = (node.Left != null ? node.Left.Sum : 0) + node.DeltaAt + node.DeltaAfter +
                      (node.Right != null ? node.Right.Sum : 0);

            if (node.Sum != sum)
                return false;

            // Check max
            var max = 0;

            sum = node.Left != null ? node.Left.Max : 0;
            if (sum > max)
                max = sum;

            sum = (node.Left != null ? node.Left.Sum : 0) + node.DeltaAt;
            if (sum > max)
                max = sum;

            sum = (node.Left != null ? node.Left.Sum : 0) + node.DeltaAt + node.DeltaAfter;
            if (sum > max)
                max = sum;

            sum = (node.Left != null ? node.Left.Sum : 0) + node.DeltaAt + node.DeltaAfter + (node.Right != null ? node.Right.Max : 0);
            if (sum > max)
                max = sum;

            if (node.Max != max)
                return false;

            return true;
        }

        /// <summary>
        /// Find the node containing the search key.
        /// </summary>
        /// <param name="node">The root node which subtree should be searched.</param>
        /// <param name="key">The key being searched.</param>
        /// <returns>The node containing the key if it exists, otherwise null.</returns>
        [Pure]
        private static Node findNode(Node node, T key)
        {
            Contract.Requires(node != null);
            Contract.Ensures(Contract.Result<Node>() != null);

            while (node != null)
            {
                var compare = key.CompareTo(node.Key);

                if (compare > 0)
                    node = node.Right;
                else if (compare < 0)
                    node = node.Left;
                else
                    break;
            }

            return node;
        }

        private static bool confirmIntervalPlacement(Node root)
        {
            foreach (var node in nodes(root))
            {
                if (node.IncludedList != null)
                    foreach (var interval in node.IncludedList)
                        if (!interval.Low.Equals(node.Key) || !interval.LowIncluded)
                            return false;

                if (node.ExcludedList != null)
                    foreach (var interval in node.ExcludedList)
                        if (!interval.Low.Equals(node.Key) || interval.LowIncluded)
                            return false;
            }

            return true;
        }

        #endregion

        #region Inner Classes

        // TODO: Implement
        private class IntervalList // : TreeBag<I>
        {
            /*
            public IntervalList()
                : base((IComparer<I>) DynamicIntervalTree<I, T>.Comparer)
            {
            }

            public I Highest
            {
                get { return this.FirstOrDefault(); }
            }

            public IEnumerable<I> FindOverlaps(IInterval<T> query)
            {
                return this.Where(i => i.Overlaps(query));
            }

            /*/
            private readonly IDictionary<I, ArrayList<I>> _dictionary;


            public IntervalList()
            {
                _dictionary = new SortedArrayDictionary<I, ArrayList<I>>((IComparer<I>) Comparer);
            }

            public bool Add(I interval)
            {
                if (!_dictionary.Contains(interval))
                    _dictionary.Add(interval, new ArrayList<I>());

                return _dictionary[interval].Add(interval);
            }

            public bool Remove(I interval)
            {
                if (!_dictionary.Contains(interval))
                    return false;

                var list = _dictionary[interval];

                var result = list.Remove(interval);

                if (list.IsEmpty)
                    _dictionary.Remove(interval);

                return result;
            }

            public bool IsEmpty
            {
                get { return _dictionary.IsEmpty; }
            }

            public I Choose()
            {
                if (IsEmpty)
                    throw new NoSuchItemException();

                return _dictionary.Keys.First();
            }

            public I Highest
            {
                get
                {
                    if (IsEmpty)
                        return default(I);

                    return _dictionary.First().Key;
                }
            }

            public IEnumerable<I> FindOverlaps(IInterval<T> query)
            {
                foreach (var keyValuePair in _dictionary)
                {
                    if (keyValuePair.Key.Overlaps(query))
                        foreach (var interval in keyValuePair.Value)
                        {
                            yield return interval;
                        }
                }
            }

            public IEnumerator<I> GetEnumerator()
            {
                foreach (var keyValuePair in _dictionary)
                {
                    foreach (var interval in keyValuePair.Value)
                    {
                        yield return interval;
                    }
                }
                //return _dictionary.SelectMany(keyValuePair => keyValuePair.Value).GetEnumerator();
            }


            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var list in this)
                    sb.Append(list);
                return sb.ToString();
            }
            //*/
        }

        private class Node : IComparable<Node>
        {
            private bool _delete = false;

            #region Code Contracts

            [ContractInvariantMethod]
            private void invariant()
            {
                Contract.Invariant(Dummy || Key != null);
                //Contract.Invariant(LocalSpan == null || (!IncludedList.IsEmpty || !ExcludedList.IsEmpty));

                /*
                // Span contains all intervals
                Contract.Invariant(LocalSpan == null || (IncludedList == null || Contract.ForAll(IncludedList, i => LocalSpan.Contains(i))));
                // Span has the lowest and highest endpoint of the collection
                Contract.Ensures(IsEmpty || Contract.ForAll(this, i => Contract.Result<IInterval<T>>().CompareLow(i) <= 0 && Contract.Result<IInterval<T>>().CompareHigh(i) >= 0));
                // There is an interval that has the same low as span
                Contract.Ensures(IsEmpty || Contract.Exists(this, i => Contract.Result<IInterval<T>>().CompareLow(i) == 0));
                // There is an interval that has the same high as span
                Contract.Ensures(IsEmpty || Contract.Exists(this, i => Contract.Result<IInterval<T>>().CompareHigh(i) == 0));
                */
            }

            #endregion

            #region Properties

            // The interval which Low is used as key in the binary search tree
            public T Key { get; private set; }

            // Children
            public Node Left { get; set; }
            public Node Right { get; set; }

            // The span of the intervals in the successor
            public IInterval<T> LocalSpan { get; private set; }
            // The span of the subtree rooted in this successor
            public IInterval<T> Span { get; set; }

            // List of intervals starting at the same low as Key
            public IntervalList IncludedList { get; private set; }
            public IntervalList ExcludedList { get; private set; }

            // Fields for Maximum Number of Overlaps
            internal int DeltaAt { get; private set; }
            internal int DeltaAfter { get; private set; }
            public int Sum { get; private set; }
            public int Max { get; private set; }

            // AVL Balance
            public int Balance { get; set; }

            // Used for printing
            public bool Dummy { get; private set; }

            #endregion

            #region Constructor

            public Node(T key, bool highIncluded)
            {
                Key = key;

                AddHigh(highIncluded);
                UpdateMaximumOverlap();
            }

            public Node(I interval)
            {
                Contract.Requires(interval != null);
                Contract.Ensures(Key != null);
                Contract.Ensures(LocalSpan != null);
                Contract.Ensures(Span != null);
                Contract.Ensures(Key.Equals(interval.Low));
                Contract.Ensures(LocalSpan.IntervalEquals(interval));
                Contract.Ensures(Span.IntervalEquals(interval));
                Contract.Ensures(interval.LowIncluded && IncludedList != null || ExcludedList != null);

                Key = interval.Low;

                // Insert the interval into a list
                AddLow(interval);

                UpdateSpan();
                UpdateMaximumOverlap();
            }

            public Node()
            {
                Dummy = true;
            }

            #endregion

            #region Methods

            public bool IsEmpty
            {
                get
                {
                    return _delete || IncludedList == null && ExcludedList == null && DeltaAt == 0 && DeltaAfter == 0;
                }

                set { _delete = value; }
            }

            public void UpdateSpan()
            {
                //var oldSpan = Span;

                // No children
                if (Left == null && Right == null)
                    Span = LocalSpan;
                // No right child with a span
                else if (Right == null || Right.Span == null)
                {
                    // Left node is an endpoint node
                    if (Left == null || Left.Span == null)
                        Span = LocalSpan;
                    // This node is an endpoint node
                    else if (LocalSpan == null)
                        Span = Left.Span;
                    // Neither is an endpoint node
                    else
                        Span = (Left.Span.CompareHigh(LocalSpan) >= 0)
                             ? Left.Span
                             : new IntervalBase<T>(Left.Span, LocalSpan);
                }
                // No left child with a span
                else if (Left == null || Left.Span == null)
                {
                    // This node is an endpoint node
                    if (LocalSpan == null)
                        Span = Right.Span;
                    // Neither is an endpoint node
                    else
                        Span = (Right.Span.CompareHigh(LocalSpan) > 0)
                             ? new IntervalBase<T>(LocalSpan, Right.Span)
                             : LocalSpan;
                }
                // Both children have a span
                else
                {
                    // Right span has higher high than left span
                    if (Right.Span.CompareHigh(Left.Span) > 0)
                        Span = (LocalSpan == null)
                             ? new IntervalBase<T>(Left.Span, Right.Span)
                             : new IntervalBase<T>(Left.Span, Right.Span.HighestHigh(LocalSpan));
                    else
                        Span = (LocalSpan == null)
                             ? Left.Span
                             : (Left.Span.CompareHigh(LocalSpan) >= 0 ? Left.Span : new IntervalBase<T>(Left.Span, LocalSpan));
                }

                //return (oldSpan == null ^ Span == null) || (oldSpan != null && oldSpan.IntervalEquals(Span));
            }

            /// <summary>
            /// Update the maximum overlap value for the successor.
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

            public bool AddLow(I interval)
            {
                Contract.Requires(interval != null);
                Contract.Ensures(LocalSpan != null);

                bool intervalWasAdded;

                // Make copy if no span exists, otherwise join with current span
                LocalSpan = LocalSpan == null ? new IntervalBase<T>(interval) : LocalSpan.JoinedSpan(interval);

                if (interval.LowIncluded)
                {
                    // Create the list if necessary
                    if (IncludedList == null)
                        IncludedList = new IntervalList();

                    intervalWasAdded = IncludedList.Add(interval);

                    // Update delta
                    if (intervalWasAdded)
                        DeltaAt++;
                }
                else
                {
                    // Create the list if necessary
                    if (ExcludedList == null)
                        ExcludedList = new IntervalList();

                    intervalWasAdded = ExcludedList.Add(interval);

                    // Update delta
                    if (intervalWasAdded)
                        DeltaAfter++;
                }

                // Should also return true, as elements added to a bag will always be added
                return intervalWasAdded;
            }

            public void AddHigh(bool highIncluded)
            {
                if (highIncluded)
                    DeltaAfter--;
                else
                    DeltaAt--;
            }

            /// <summary>
            /// Deletes the specified Key from this root. 
            /// If the Key tree is used with unique intervals, this method removes the Key specified as an argument.
            /// If multiple identical intervals (starting at the same time and also ending at the same time) are allowed, this function will delete one of them. 
            /// In this case, it is easy enough to either specify the (Key, query) pair to be deleted or enforce uniqueness by changing the Add procedure.
            /// </summary>
            public bool RemoveLow(I interval)
            {
                Contract.Requires(interval != null);

                var intervalWasRemoved = false;

                if (interval.LowIncluded)
                {
                    // Create the list if necessary
                    if (IncludedList != null)
                    {
                        intervalWasRemoved = IncludedList.Remove(interval);

                        if (intervalWasRemoved)
                            DeltaAt--;

                        // RemoveLow list if empty
                        if (IncludedList.IsEmpty)
                            IncludedList = null;
                    }
                }
                else
                {
                    if (ExcludedList != null)
                    {
                        intervalWasRemoved = ExcludedList.Remove(interval);

                        if (intervalWasRemoved)
                            DeltaAfter--;

                        // RemoveLow list if empty
                        if (ExcludedList.IsEmpty)
                            ExcludedList = null;
                    }
                }

                // Update span if we removed the interval
                if (intervalWasRemoved)
                {
                    if (IncludedList == null && ExcludedList == null)
                        LocalSpan = null;
                    else if (IncludedList == null)
                    {
                        Contract.Assume(ExcludedList != null && !ExcludedList.IsEmpty);

                        LocalSpan = new IntervalBase<T>(ExcludedList.Highest);
                    }
                    else if (ExcludedList == null)
                    {
                        Contract.Assume(IncludedList != null && !IncludedList.IsEmpty);

                        LocalSpan = new IntervalBase<T>(IncludedList.Highest);
                    }
                    else
                    {
                        LocalSpan = IncludedList.Highest.JoinedSpan(ExcludedList.Highest);
                    }
                }

                return intervalWasRemoved;
            }

            public void RemoveHigh(bool highIncluded)
            {
                if (highIncluded)
                    DeltaAfter++;
                else
                    DeltaAt++;
            }

            public void Swap(Node successor)
            {
                Contract.Requires(successor != null);
                Contract.Ensures(Key != null);

                // TODO: Remember MNO values

                // Swap key
                var tmpKeyInterval = Key;
                Key = successor.Key;
                successor.Key = tmpKeyInterval;

                // Copy successor data to node
                LocalSpan = successor.LocalSpan;
                IncludedList = successor.IncludedList;
                ExcludedList = successor.ExcludedList;
                DeltaAfter = successor.DeltaAfter;
                DeltaAt = successor.DeltaAt;

                successor.IsEmpty = true;
            }

            public int CompareTo(Node other)
            {
                return Key.CompareTo(other.Key);
            }

            public override string ToString()
            {
                return Key.ToString();
            }

            #endregion
        }

        #endregion

        #region AVL tree methods

        /// <summary>
        /// Rotates lefts this instance
        /// </summary>
        private static Node rotateLeft(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Right != null);

            var node = root.Right;

            root.Right = node.Left;
            root.UpdateSpan();
            root.UpdateMaximumOverlap();

            node.Left = root;
            node.UpdateSpan();
            node.UpdateMaximumOverlap();

            return node;
        }

        /// <summary>
        /// Rotates right this instance.
        /// </summary>
        private static Node rotateRight(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Left != null);

            var node = root.Left;

            root.Left = node.Right;
            root.UpdateSpan();
            root.UpdateMaximumOverlap();

            node.Right = root;
            node.UpdateSpan();
            node.UpdateMaximumOverlap();

            return node;
        }

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
                // Node is balanced after the root was added
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
                // HighestHigh will not change for parent, so we can stop here
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
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }
                    break;
            }

            return root;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// </summary>
        public DynamicIntervalTree()
        {
        }

        /// <summary>
        /// </summary>
        public DynamicIntervalTree(IEnumerable<I> intervals)
        {
            Contract.Requires(intervals != null);

            foreach (var interval in intervals)
                Add(interval);
        }

        #endregion

        #region Collection Value

        public override bool IsEmpty
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (_root == null));
                return _root == null;
            }
        }

        public override int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() == _count);
                return _count;
            }
        }

        public override Speed CountSpeed { get { return Speed.Constant; } }

        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            var root = _root;

            while (true)
            {
                // Choose an interval from one of the lists
                if (root.IncludedList != null)
                    return root.IncludedList.Choose();
                if (root.ExcludedList != null)
                    return root.ExcludedList.Choose();

                // The successor might represent a high endpoint for MNO, so search left for a low endpoint
                // The left most successor will always contain at least one interval!
                // TODO: Add comment above as invariant
                root = root.Left;
            }
        }

        #endregion

        #region Enumerable

        public override IEnumerator<I> GetEnumerator()
        {
            foreach (var node in nodes(_root))
            {
                if (node.IncludedList != null)
                    foreach (var interval in node.IncludedList)
                        yield return interval;

                if (node.ExcludedList != null)
                    foreach (var interval in node.ExcludedList)
                        yield return interval;
            }
        }

        private static IEnumerable<Node> nodes(Node root)
        {
            if (root == null)
                yield break;

            foreach (var node in nodes(root.Left))
                yield return node;

            yield return root;

            foreach (var node in nodes(root.Right))
                yield return node;
        }

        #endregion

        #region Interval Collection

        #region Properties

        /// <inheritdoc/>
        public IInterval<T> Span
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("An empty collection has no span");

                // Return copy so users can't change object
                return new IntervalBase<T>(_root.Span);
            }
        }

        /// <inheritdoc/>
        public int MaximumOverlap
        {
            get
            {
                return IsEmpty ? 0 : _root.Max;
            }
        }

        #endregion

        #region Find Overlaps

        /// <summary>
        /// Searches for all intervals overlapping the one specified.
        /// If multiple intervals starting at the same time/query are found to overlap the specified Key, they are returned in decreasing order of their End values.
        /// </summary>
        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            if (IsEmpty || query == null)
                return Enumerable.Empty<I>();

            return findOverlaps(query, _root);
        }


        /// <summary>
        /// Gets all intervals in this root that are overlapping the argument Key. 
        /// If multiple intervals starting at the same time/query are found to overlap, they are returned in decreasing order of their End values.
        /// </summary>
        private static IEnumerable<I> findOverlaps(IInterval<T> query, Node root)
        {
            Contract.Requires(query != null);
            Contract.Requires(root != null);

            // Query is to the left of root's Low
            if (root.LocalSpan == null && query.High.CompareTo(root.Key) <= 0 || root.LocalSpan != null && query.CompareHighLow(root.LocalSpan) < 0)
            {
                if (root.Left != null)
                    foreach (var value in findOverlaps(query, root.Left))
                        yield return value;
            }
            // None of the intervals in tree overlap query
            else if (root.Span != null && root.Span.CompareHighLow(query) >= 0)
            {
                // Search left
                if (root.Left != null)
                    foreach (var interval in findOverlaps(query, root.Left))
                        yield return interval;

                if (root.LocalSpan != null)
                {
                    // Find overlaps in lists
                    if (root.IncludedList != null)
                        foreach (var interval in root.IncludedList.FindOverlaps(query))
                            yield return interval;
                    if (root.ExcludedList != null)
                        foreach (var interval in root.ExcludedList.FindOverlaps(query))
                            yield return interval;
                }

                // Search right
                if (root.Right != null)
                    foreach (var interval in findOverlaps(query, root.Right))
                        yield return interval;
            }
        }

        public IEnumerable<I> FindOverlaps(T query)
        {
            if (query == null)
                return Enumerable.Empty<I>();

            return FindOverlaps(new IntervalBase<T>(query));
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

        /// <summary>
        /// Adds the specified Key.
        /// If there is more than one Key starting at the same time/query, the intervalnode.Key stores the low time and the maximum end time of all intervals starting at the same query.
        /// All end values (except the maximum end time/query which is stored in the Key root itself) are stored in the intervalList list in decreasing order.
        /// Note: this is okay for problems where intervals starting at the same time /query is not a frequent occurrence, however you can use other query structure for better performance depending on your problem needs
        /// </summary>
        public bool Add(I interval)
        {
            var nodeWasAdded = false;
            var intervalWasAdded = false;

            _root = addLow(interval, _root, ref nodeWasAdded, ref intervalWasAdded);

            if (intervalWasAdded)
            {
                _root = addHigh(interval, _root, ref nodeWasAdded);

                _count++;
            }

            return intervalWasAdded;
        }

        public void AddAll(IEnumerable<I> intervals)
        {
            foreach (var interval in intervals)
                Add(interval);
        }

        private static Node addLow(I interval, Node root, ref bool nodeWasAdded, ref bool intervalWasAdded)
        {
            Contract.Requires(interval != null);

            if (root == null)
            {
                nodeWasAdded = true;
                intervalWasAdded = true;

                return new Node(interval);
            }

            var compare = interval.Low.CompareTo(root.Key);

            if (compare > 0)
            {
                root.Right = addLow(interval, root.Right, ref nodeWasAdded, ref intervalWasAdded);

                Contract.Assert(root.Right != null);

                if (nodeWasAdded)
                    root.Balance++;
            }
            else if (compare < 0)
            {
                root.Left = addLow(interval, root.Left, ref nodeWasAdded, ref intervalWasAdded);

                Contract.Assert(root.Left != null);

                if (nodeWasAdded)
                    root.Balance--;
            }
            else
                // if there are more than one query with the same low endpoint, the
                // Node.Key stores the low and the maximum high of all
                // intervals starting at the low. All end values (except the maximum end
                // time/query which is stored in the Key root itself) are stored in the
                // intervalList list in decreasing order.
                // note: this is ok for problems where intervals starting at the same time
                //       /query is not a frequent occurrence, however you can use other query
                //       structure for better performance depending on your problem needs

                intervalWasAdded = root.AddLow(interval);


            if (intervalWasAdded)
            {
                root.UpdateSpan();
                root.UpdateMaximumOverlap();
            }

            // Tree might be unbalanced after root was added, so we rotate
            if (nodeWasAdded)
                root = rotateForAdd(root, ref nodeWasAdded);

            return root;
        }

        private static Node addHigh(I interval, Node root, ref bool nodeWasAdded)
        {
            Contract.Requires(interval != null);

            if (root == null)
            {
                nodeWasAdded = true;
                return new Node(interval.High, interval.HighIncluded);
            }

            var compare = interval.High.CompareTo(root.Key);

            if (compare > 0)
            {
                root.Right = addHigh(interval, root.Right, ref nodeWasAdded);

                Contract.Assert(root.Right != null);

                if (nodeWasAdded)
                    root.Balance++;
            }
            else if (compare < 0)
            {
                root.Left = addHigh(interval, root.Left, ref nodeWasAdded);

                Contract.Assert(root.Left != null);

                if (nodeWasAdded)
                    root.Balance--;
            }
            else
                root.AddHigh(interval.HighIncluded);

            // Span won't change, as we don't change local span anywhere
            // root.UpdateSpan();
            root.UpdateMaximumOverlap();

            // Tree might be unbalanced after root was added, so we rotate
            if (nodeWasAdded)
                root = rotateForAdd(root, ref nodeWasAdded);

            return root;
        }

        #endregion

        #region Remove

        /// <summary>
        /// Deletes the specified Key.
        /// If the Key tree is used with unique intervals, this method removes the Key specified as an argument.
        /// If multiple identical intervals (starting at the same time and also ending at the same time) are allowed, this function will delete one of them( see procedure RemoveLow for details)
        /// In this case, it is easy enough to either specify the (Key, query) pair to be deleted or enforce uniqueness by changing the Add procedure.
        /// </summary>
        public bool Remove(I interval)
        {
            if (IsEmpty || !interval.Overlaps(Span))
                return false;

            var nodeWasDeleted = false;
            var intervalWasRemoved = false;

            _root = removeLow(interval, _root, ref nodeWasDeleted, ref intervalWasRemoved);

            if (intervalWasRemoved)
            {
                _root = removeHigh(interval, _root, ref nodeWasDeleted);

                _count--;
            }

            return intervalWasRemoved;
        }

        private static Node removeLow(I interval, Node root, ref bool nodeWasDeleted, ref bool intervalWasRemoved)
        {
            Contract.Requires(interval != null);
            Contract.Requires(root != null);

            var compare = interval.Low.CompareTo(root.Key);

            // Search right
            if (compare > 0)
            {
                // If there isn't a right subtree, the collection does not contain the interval
                if (root.Right == null)
                    return root;

                root.Right = removeLow(interval, root.Right, ref nodeWasDeleted, ref intervalWasRemoved);

                // Adjust balance if necessary
                if (nodeWasDeleted)
                    root.Balance--;
            }
            // Search left
            else if (compare < 0)
            {
                // If there isn't a right subtree, the collection does not contain the interval
                if (root.Left == null)
                    return root;

                root.Left = removeLow(interval, root.Left, ref nodeWasDeleted, ref intervalWasRemoved);

                // Adjust balance if necessary
                if (nodeWasDeleted)
                    root.Balance++;
            }
            else
            {
                // RemoveLow interval from node
                intervalWasRemoved = root.RemoveLow(interval);

                // If the interval was removed, we might be able to removeLow the node as well
                if (intervalWasRemoved && root.IsEmpty)
                {
                    // The root has two children
                    if (root.Left != null && root.Right != null)
                    {
                        // Swap the root and successor
                        root.Swap(findSuccessor(root.Right));

                        // RemoveLow the successor node from the right subtree
                        nodeWasDeleted = false;
                        root.Right = removeLow(interval, root.Right, ref nodeWasDeleted, ref intervalWasRemoved);

                        // Adjust balance if necessary
                        if (nodeWasDeleted)
                            root.Balance--;
                    }
                    // At least one child is null
                    else
                    {
                        nodeWasDeleted = true;

                        // We simply removeLow the node by returning a reference to its child
                        return root.Left ?? root.Right;
                    }
                }
            }

            // If the interval was removed the span might have changed
            if (intervalWasRemoved)
            {
                root.UpdateSpan();
                root.UpdateMaximumOverlap();
            }

            // Rotate if necessary
            if (nodeWasDeleted)
                root = rotateForRemove(root, ref nodeWasDeleted);

            return root;
        }
        private static Node removeHigh(I interval, Node root, ref bool nodeWasDeleted)
        {
            Contract.Requires(interval != null);
            Contract.Requires(root != null);

            var compare = interval.High.CompareTo(root.Key);

            // Search right
            if (compare > 0)
            {
                root.Right = removeHigh(interval, root.Right, ref nodeWasDeleted);

                // Adjust balance if necessary
                if (nodeWasDeleted)
                    root.Balance--;
            }
            // Search left
            else if (compare < 0)
            {
                root.Left = removeHigh(interval, root.Left, ref nodeWasDeleted);

                // Adjust balance if necessary
                if (nodeWasDeleted)
                    root.Balance++;
            }
            else
            {
                // RemoveLow interval from node
                root.RemoveHigh(interval.HighIncluded);

                // If the interval was removed, we might be able to removeLow the node as well
                if (root.IsEmpty)
                {
                    // The root has two children
                    if (root.Left != null && root.Right != null)
                    {
                        // Swap the root and successor
                        root.Swap(findSuccessor(root.Right));

                        // RemoveLow the successor node from the right subtree
                        nodeWasDeleted = false;
                        root.Right = removeHigh(interval, root.Right, ref nodeWasDeleted);

                        // Adjust balance if necessary
                        if (nodeWasDeleted)
                            root.Balance--;
                    }
                    // At least one child is null
                    else
                    {
                        nodeWasDeleted = true;

                        // We simply removeLow the node by returning a reference to its child
                        return root.Left ?? root.Right;
                    }
                }
            }

            root.UpdateMaximumOverlap();

            // Rotate if necessary
            if (nodeWasDeleted)
                root = rotateForRemove(root, ref nodeWasDeleted);

            return root;
        }

        /// <summary>
        /// Find the successor node.
        /// </summary>
        /// <returns>The successor node.</returns>
        private static Node findSuccessor(Node node)
        {
            Contract.Requires(node != null);
            Contract.Ensures(Contract.Result<Node>() != null);
            Contract.Ensures(Contract.Result<Node>() == nodes(Contract.OldValue(node)).First());

            while (node.Left != null)
                node = node.Left;

            return node;
        }

        #endregion

        #region Clear

        /// <inheritdoc/>
        public void Clear()
        {
            // Return if tree is empty
            if (IsEmpty)
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
            Contract.Ensures(_root == null);
            Contract.Ensures(_count == 0);

            _root = null;
            _count = 0;
        }

        #endregion

        #endregion

        #endregion

        #region QuickGraph

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

                foreach (var node in nodes(_root))
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
                        cell.Cells.Add(new GraphvizRecordCell
                            {
                                Text = e.Vertex.Key.ToString()
                            });

                        // Add Span in middle cell
                        cell.Cells.Add(new GraphvizRecordCell
                            {
                                Text = String.Format("LS: {0} - S: {1}", e.Vertex.LocalSpan, e.Vertex.Span)
                            });

                        // Add IncludedList in bottom cell
                        cell.Cells.Add(new GraphvizRecordCell
                        {
                            Text = String.Format("Inc: {0} - Ex: {1}", e.Vertex.IncludedList == null ? "Ø" : e.Vertex.IncludedList.ToString(), e.Vertex.ExcludedList == null ? "Ø" : e.Vertex.ExcludedList.ToString())
                        });

                        //*
                        cell.Cells.Add(new GraphvizRecordCell
                        {
                            Text = String.Format("dAt: {0}, dAfter: {1}, Sum: {2}, Max: {3}", e.Vertex.DeltaAt, e.Vertex.DeltaAfter, e.Vertex.Sum, e.Vertex.Max)
                        });
                        //*/

                        // Add cell to record
                        e.VertexFormatter.Record.Cells.Add(cell);
                    }
                };
                gw.FormatEdge += delegate(object sender, FormatEdgeEventArgs<Node, Edge<Node>> e)
                {
                    e.EdgeFormatter.Label = new GraphvizEdgeLabel
                    {
                        Value = !e.Edge.Target.Dummy
                            ? ((e.Edge.Target.Balance > 0 ? "+" : "") + e.Edge.Target.Balance)
                            : ""
                    };
                };


                return gw.Generate();
            }
        }

        #endregion
    }
}
