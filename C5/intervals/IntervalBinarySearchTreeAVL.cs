using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;

namespace C5.intervals
{
    // TODO: Document reference equality duplicates
    public class IntervalBinarySearchTreeAVL<T> : CollectionValueBase<IInterval<T>>, IIntervalCollection<T> where T : IComparable<T>
    {
        private Node _root;
        private int _count;
        private static readonly IEqualityComparer<IInterval<T>> Comparer = ComparerFactory<IInterval<T>>.CreateEqualityComparer(ReferenceEquals, IntervalExtensions.GetHashCode);

        #region AVL tree helper methods

        private static Node rotate(Node root, ref bool nodeWasAdded)
        {
            switch (root.Balance)
            {
                // Node is balanced after the node was added
                case 0:
                    nodeWasAdded = false;
                    break;

                // Node is unbalanced, so we rotate
                case -2:
                    switch (root.Left.Balance)
                    {
                        // Left Right Case
                        case +1:
                            root.Left = rotateLeft(root.Left);
                            root = rotateRight(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? 1 : 0);
                            root.Balance = 0;
                            break;

                        // Left Left Case
                        case -1:
                            root = rotateRight(root);
                            root.Balance = root.Right.Balance = 0;
                            break;
                    }
                    nodeWasAdded = false;
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
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? 1 : 0);
                            root.Balance = 0;
                            break;
                    }

                    nodeWasAdded = false;
                    break;
            }

            return root;
        }

        private static Node rotateRight(Node root)
        {
            // Rotate
            var node = root.Left;
            root.Left = node.Right;
            node.Right = root;


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


            // Update MNO
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

            // TODO: Look into if these operations can be optimised
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


            // Update MNO
            root.UpdateMaximumOverlap();
            node.UpdateMaximumOverlap();

            return node;
        }

        #endregion

        #region Node nested classes

        class Node
        {
            private readonly T _key;

            public T Key { get { return _key; } }

            private IntervalSet _less;
            private IntervalSet _equal;
            private IntervalSet _greater;

            public IntervalSet Less
            {
                get { return _less ?? (_less = new IntervalSet()); }
            }
            public IntervalSet Equal
            {
                get { return _equal ?? (_equal = new IntervalSet()); }
            }
            public IntervalSet Greater
            {
                get { return _greater ?? (_greater = new IntervalSet()); }
            }

            public Node Left { get; internal set; }
            public Node Right { get; internal set; }

            // The number of intervals with an endpoint in one of the interval sets in the node
            public int intervalsEndingInNode;

            // Fields for Maximum Number of Overlaps
            public int DeltaAt { get; internal set; }
            public int DeltaAfter { get; internal set; }
            private int Sum { get; set; }
            public int Max { get; private set; }

            // Balance - between -2 and +2
            public sbyte Balance { get; internal set; }

            public Node(T key)
            {
                _key = key;
            }

            public void UpdateMaximumOverlap()
            {
                // TODO: Find a clever way only to update what needs to be updated!

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
            }

            public override string ToString()
            {
                return _key.ToString();
            }
        }

        private sealed class IntervalSet : HashSet<IInterval<T>>
        {
            private IntervalSet(IEnumerable<IInterval<T>> intervals)
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

                return s.IsEmpty() ? String.Empty : String.Join(", ", s.ToArray());
            }

            public static IntervalSet operator -(IntervalSet s1, IntervalSet s2)
            {
                if (s1 == null || s2 == null)
                    throw new ArgumentNullException("Set-Set");

                var res = new IntervalSet(s1);
                res.RemoveAll(s2);
                return res;
            }
        }

        #endregion

        #region Constructors

        public IntervalBinarySearchTreeAVL(IEnumerable<IInterval<T>> intervals)
        {
            // TODO: Pre-generate balanced tree based on endpoints and insert intervals afterwards

            foreach (var interval in intervals)
                Add(interval);
        }

        /// <summary>
        /// Create empty Interval Binary Search Tree
        /// </summary>
        public IntervalBinarySearchTreeAVL()
        {
        }

        #endregion

        #region Events

        public override EventTypeEnum ListenableEvents { get { return EventTypeEnum.Basic; } }
        //public EventTypeEnum ActiveEvents { get; private set; }
        //public event CollectionChangedHandler<T> CollectionChanged;
        //public event CollectionClearedHandler<T> CollectionCleared;
        //public event ItemsAddedHandler<T> ItemsAdded;
        //public event ItemInsertedHandler<T> ItemInserted;
        //public event ItemsRemovedHandler<T> ItemsRemoved;
        //public event ItemRemovedAtHandler<T> ItemRemovedAt;

        #endregion

        #region Add

        public bool Add(IInterval<T> interval)
        {
            // Used tree rotations
            var nodeWasAdded = false;
            // Used to check if interval was actually added
            var intervalWasAdded = false;

            _root = addLow(_root, null, interval, ref nodeWasAdded, ref intervalWasAdded);

            // Only try to add High if it is different from Low
            if (intervalWasAdded && interval.CompareEndpointsValues() < 0)
            {
                nodeWasAdded = false;
                _root = addHigh(_root, null, interval, ref nodeWasAdded, ref intervalWasAdded);
            }

            // Increase counter and raise event if interval was added
            if (intervalWasAdded)
            {
                _count++;
                raiseForAdd(interval);
            }

            // TODO: Add event for change in MNO

            return intervalWasAdded;
        }

        // TODO: Make iterative?
        private static Node addLow(Node root, Node right, IInterval<T> interval, ref bool nodeWasAdded, ref bool intervalWasAdded)
        {
            // No node existed for the low endpoint
            if (root == null)
            {
                root = new Node(interval.Low);
                nodeWasAdded = true;
                intervalWasAdded = true;
            }

            var compare = interval.Low.CompareTo(root.Key);

            if (compare > 0)
            {
                root.Right = addLow(root.Right, right, interval, ref nodeWasAdded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (nodeWasAdded)
                    root.Balance++;
            }
            else if (compare < 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    intervalWasAdded = root.Greater.Add(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.High) < 0)
                    intervalWasAdded = root.Equal.Add(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Left = addLow(root.Left, root, interval, ref nodeWasAdded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (nodeWasAdded)
                    root.Balance--;
            }
            else
            {
                // If everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    intervalWasAdded = root.Greater.Add(interval);

                if (interval.LowIncluded)
                    intervalWasAdded = root.Equal.Add(interval);

                if (intervalWasAdded)
                {
                    root.intervalsEndingInNode++;

                    // If interval was added, we need to update delta
                    if (interval.LowIncluded)
                        root.DeltaAt++;
                    else
                        root.DeltaAfter++;
                }
            }

            // Tree might be unbalanced after node was added, so we rotate
            if (nodeWasAdded && compare != 0)
                root = rotate(root, ref nodeWasAdded);

            // Update MNO
            if (intervalWasAdded)
                root.UpdateMaximumOverlap();

            return root;
        }

        private static Node addHigh(Node root, Node left, IInterval<T> interval, ref bool nodeWasAdded, ref bool intervalWasAdded)
        {
            // No node existed for the high endpoint
            if (root == null)
            {
                root = new Node(interval.High);
                nodeWasAdded = true;
                intervalWasAdded = true;
            }

            var compare = interval.High.CompareTo(root.Key);

            if (compare < 0)
            {
                root.Left = addHigh(root.Left, left, interval, ref nodeWasAdded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (nodeWasAdded)
                    root.Balance--;
            }
            else if (compare > 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    intervalWasAdded = root.Less.Add(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.Low) > 0)
                    intervalWasAdded = root.Equal.Add(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                root.Right = addHigh(root.Right, root, interval, ref nodeWasAdded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (nodeWasAdded)
                    root.Balance++;
            }
            else
            {
                // If everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    intervalWasAdded = root.Less.Add(interval);

                if (interval.HighIncluded)
                    intervalWasAdded = root.Equal.Add(interval);

                // If interval was added, we need to update MNO
                if (intervalWasAdded)
                {
                    root.intervalsEndingInNode++;

                    if (!interval.HighIncluded)
                        root.DeltaAt--;
                    else
                        root.DeltaAfter--;
                }
            }

            // Tree might be unbalanced after node was added, so we rotate
            if (nodeWasAdded && compare != 0)
                root = rotate(root, ref nodeWasAdded);

            // Update MNO
            if (intervalWasAdded)
                root.UpdateMaximumOverlap();

            return root;
        }

        #endregion

        #region Remove

        public bool Remove(IInterval<T> interval)
        {
            // Used tree rotations
            var removeNode = false;
            // Used to check if interval was actually added
            var intervalWasRemoved = false;

            removeLow(_root, null, interval, ref removeNode, ref intervalWasRemoved);

            // Remove node and rebalance tree, if node containing low endpoint is now empty
            // (it doesn't contain intervals with endpoint values equal to the node value)
            if (removeNode)
                removeNodeWithKey(interval.Low);

            // Only try to add High if it is different from Low
            if (intervalWasRemoved && interval.CompareEndpointsValues() < 0)
            {
                // Reset value
                removeNode = false;

                removeHigh(_root, null, interval, ref removeNode, ref intervalWasRemoved);

                // Remove node and rebalance tree, if node containing high endpoint is now empty
                // (it doesn't contain intervals with endpoint values equal to the node value)
                if (removeNode)
                    removeNodeWithKey(interval.High);
            }

            // Increase counter and raise event if interval was added
            if (intervalWasRemoved)
            {
                _count--;
                raiseForRemove(interval);
            }

            // TODO: Add event for change in MNO

            return intervalWasRemoved;
        }
        
        private static void removeLow(Node root, Node right, IInterval<T> interval, ref bool removeNode, ref bool intervalWasRemoved)
        {
            // No node existed for the low endpoint
            if (root == null)
                return;

            var compare = interval.Low.CompareTo(root.Key);

            if (compare > 0)
                removeLow(root.Right, right, interval, ref removeNode, ref intervalWasRemoved);
            else if (compare < 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    intervalWasRemoved = root.Greater.Remove(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.High) < 0)
                    intervalWasRemoved = root.Equal.Remove(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                removeLow(root.Left, root, interval, ref removeNode, ref intervalWasRemoved);
            }
            else
            {
                // If everything in the right subtree of root will lie within the interval
                if (right != null && right.Key.CompareTo(interval.High) <= 0)
                    intervalWasRemoved = root.Greater.Remove(interval);

                if (interval.LowIncluded)
                    intervalWasRemoved = root.Equal.Remove(interval);

                // If interval was added, we need to update MNO
                if (intervalWasRemoved)
                {
                    if (--root.intervalsEndingInNode == 0)
                        removeNode = true;

                    // Update delta
                    if (interval.LowIncluded)
                        root.DeltaAt--;
                    else
                        root.DeltaAfter--;
                }
            }

            // Update MNO
            if (intervalWasRemoved)
                root.UpdateMaximumOverlap();
        }

        private static void removeHigh(Node root, Node left, IInterval<T> interval, ref bool removeNode, ref bool intervalWasRemoved)
        {
            // No node existed for the high endpoint
            if (root == null)
                return;

            var compare = interval.High.CompareTo(root.Key);

            if (compare < 0)
                removeHigh(root.Left, left, interval, ref removeNode, ref intervalWasRemoved);
            else if (compare > 0)
            {
                // Everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    intervalWasRemoved = root.Less.Remove(interval);

                // root key is between interval.low and interval.high
                if (root.Key.CompareTo(interval.Low) > 0)
                    intervalWasRemoved = root.Equal.Remove(interval);

                // TODO: Figure this one out: if (interval.low != -inf.)
                removeHigh(root.Right, root, interval, ref removeNode, ref intervalWasRemoved);
            }
            else
            {
                // If everything in the right subtree of root will lie within the interval
                if (left != null && left.Key.CompareTo(interval.Low) >= 0)
                    intervalWasRemoved = root.Less.Remove(interval);

                if (interval.HighIncluded)
                    intervalWasRemoved = root.Equal.Remove(interval);

                // If interval was removed, we need to update MNO
                if (intervalWasRemoved)
                {
                    if (--root.intervalsEndingInNode == 0)
                        removeNode = true;

                    if (!interval.HighIncluded)
                        root.DeltaAt++;
                    else
                        root.DeltaAfter++;
                }
            }

            // Update MNO
            if (intervalWasRemoved)
                root.UpdateMaximumOverlap();
        }

        private void removeNodeWithKey(T key)
        {
            // TODO: Implement
            var node = findNode(_root, key);
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
        private Node findMinNode(Node node)
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

        #region ICollectionValue

        public override bool IsEmpty { get { return _root == null; } }

        public override int Count { get { return _count; } }

        public override Speed CountSpeed { get { return Speed.Constant; } }

        public override IInterval<T> Choose()
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


        #region IEnumerable

        public override IEnumerator<IInterval<T>> GetEnumerator()
        {
            // TODO: Make enumerator lazy, by adding each interval and yield it if it wasn't already yielded

            var set = new IntervalSet();

            var enumerator = getEnumerator(_root);
            while (enumerator.MoveNext())
                set.Add(enumerator.Current);

            return set.GetEnumerator();
        }

        #endregion

        #region GraphViz

        private IEnumerable<Node> nodeEnumerator(Node root)
        {
            if (root == null)
                yield break;

            foreach (var node in nodeEnumerator(root.Left))
                yield return node;

            yield return root;

            foreach (var node in nodeEnumerator(root.Right))
                yield return node;
        }

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
                bottom.Cells.Add(new GraphvizRecordCell { Text = e.Vertex.Less.ToString() });
                bottom.Cells.Add(new GraphvizRecordCell { Text = e.Vertex.Equal.ToString() });
                bottom.Cells.Add(new GraphvizRecordCell { Text = e.Vertex.Greater.ToString() });
                cell.Cells.Add(bottom);
                // Add cell to record
                e.VertexFormatter.Record.Cells.Add(cell);
            };
            gw.FormatEdge += delegate(object sender, FormatEdgeEventArgs<Node, Edge<Node>> e)
            {
                e.EdgeFormatter.Label = new GraphvizEdgeLabel
                {
                    Value = (e.Edge.Target.Balance > 0 ? "+" : "") + e.Edge.Target.Balance
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

        private int nodeCounter;
        private int nullCounter;

        private string graphviz(Node root, string parent, string direction)
        {
            int id;
            if (root == null)
            {
                id = nullCounter++;
                return String.Format("\tleaf{0} [shape=point];\n", id) +
                    String.Format("\t{0}:{1} -> leaf{2};\n", parent, direction, id);
            }

            id = nodeCounter++;
            var rootString = direction == null ? "" : String.Format("\t{0} -> struct{1}:n [color={2}];\n", parent, id);

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

        public IEnumerable<IInterval<T>> FindOverlaps(T query)
        {
            // TODO: Add checks for span overlap, null query, etc.
            return findOverlap(_root, query);
        }

        private static IEnumerable<IInterval<T>> findOverlap(Node root, T query)
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
                    foreach (var interval in root.Less)
                        yield return interval;

                    // Move left
                    root = root.Left;
                }
                // Query is to the right of the current node
                else if (0 < compareTo)
                {
                    // Return all intervals in Greater
                    foreach (var interval in root.Greater)
                        yield return interval;

                    // Move right
                    root = root.Right;
                }
                // Node with query value found
                else
                {
                    // Return all intervals in Equal
                    foreach (var interval in root.Equal)
                        yield return interval;

                    // Stop as the search is done
                    yield break;
                }
            }
        }

        public IEnumerable<IInterval<T>> FindOverlaps(IInterval<T> query)
        {
            if (ReferenceEquals(query, null))
                yield break;

            // Break if collection is empty or the query is outside the collections span
            if (IsEmpty || !Span.Overlaps(query))
                yield break;

            var set = new IntervalSet();

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

        public IInterval<T> FindAnyOverlap(IInterval<T> query)
        {
            if (query == null)
                return null;

            var enumerator = FindOverlaps(query).GetEnumerator();

            return enumerator.MoveNext() ? enumerator.Current : null;
        }

        public IInterval<T> FindAnyOverlap(T query)
        {
            if (ReferenceEquals(query, null))
                return null;

            var enumerator = FindOverlaps(query).GetEnumerator();

            return enumerator.MoveNext() ? enumerator.Current : null;
        }

        /// <summary>
        /// Create an enumerable, enumerating all intersecting intervals on the path to the split node. Returns the split node in splitNode.
        /// </summary>
        private IEnumerable<IInterval<T>> findSplitNode(Node root, IInterval<T> query, Action<Node> splitNode)
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
                foreach (var interval in root.Less.Where(interval => query.Overlaps(interval)))
                    yield return interval;
                foreach (var interval in root.Equal.Where(interval => query.Overlaps(interval)))
                    yield return interval;
                foreach (var interval in root.Greater.Where(interval => query.Overlaps(interval)))
                    yield return interval;
            }
        }

        private IEnumerable<IInterval<T>> findLeft(Node root, IInterval<T> query)
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
                foreach (var interval in root.Less)
                    yield return interval;
                foreach (var interval in root.Equal)
                    yield return interval;
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
                foreach (var interval in root.Greater)
                    yield return interval;

                if (query.LowIncluded)
                    foreach (var interval in root.Equal)
                        yield return interval;

                // If we find the matching node, we can add everything in the left subtree
                IEnumerator<IInterval<T>> child = getEnumerator(root.Right);
                while (child.MoveNext())
                    yield return child.Current;
            }
        }

        private IEnumerable<IInterval<T>> findRight(Node root, IInterval<T> query)
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
                foreach (var interval in root.Less)
                    yield return interval;
                foreach (var interval in root.Equal)
                    yield return interval;
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


        private IEnumerator<IInterval<T>> getEnumerator(Node root)
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
            foreach (var interval in root.Less)
                yield return interval;
            foreach (var interval in root.Equal)
                yield return interval;
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

        public bool OverlapExists(IInterval<T> query)
        {
            if (query == null)
                return false;

            return FindOverlaps(query).GetEnumerator().MoveNext();
        }

        public int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }

        public int MaximumOverlap
        {
            get { return _root != null ? _root.Max : 0; }
        }

        #endregion
    }
}
