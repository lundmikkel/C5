///////////////////////////////////////////////////////////////////////
//// File Name               : AvlTree.cs
////      Created            : 10 8 2012   22:30
////      Author             : Costin S
////
/////////////////////////////////////////////////////////////////////

////---------------------------------------
//// TREE_WITH_PARENT_POINTERS:
//// Defines whether or not each node in the tree maintains a reference to its parent node.
//// Only parent pointers traversal is implemented in the code below.
//// To disable uncomment the following line

using System;
using System.Collections.Generic;
using System.Globalization;

#define TREE_WITH_PARENT_POINTERS

////---------------------------------------
//// TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS: 
//// Defines whether the tree exposes and implements concatenate and split operations.
//// 
//// When concat and split operations are defined, the code defines stored both the Balance and the Height in each node which is obviously not necessary. 
//// To reduce space, you can do one of two things:
////      1. The simplest change is to store both Balance and Height in one integer. Balance field needs only 2 bits which lefts 30 bits for the Height field. A tree with a HEIGHT > 2^30 (2 to the power of 30) is very unlikely you will ever build.
////      2. Simple enough to modify and remove Balance field. Concat and Split were added as an afterthought after the implementation was already done using a Balance field.

#define TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS

namespace C5.intervals
{
    /// <summary>
    /// Dictionary class.
    /// </summary>
    /// <typeparam name="T">The type of the data stored in the nodes</typeparam>
    public class AvlTree<T>
    {
        #region Fields

        private Node<T> _root;
        private readonly IComparer<T> _comparer;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AvlTree{T}"/> class.
        /// </summary>
        public AvlTree()
        {
            _comparer = getComparer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvlTree{T}"/> class.
        /// </summary>
        /// <param name="elems">The elements to be added to the tree.</param>
        /// <param name="comparer"></param>
        public AvlTree(IEnumerable<T> elems, IComparer<T> comparer)
        {
            _comparer = comparer;

            if (elems != null)
                foreach (var elem in elems)
                    Add(elem);
        }

        #endregion

        #region Delegates

        /// <summary>
        /// the visitor delegate
        /// </summary>
        /// <typeparam name="TNode">The type of the node.</typeparam>
        /// <param name="node">The node.</param>
        /// <param name="level">The level.</param>
        private delegate void VisitNodeHandler<TNode>(TNode node, int level);

        #endregion

        #region Enums

        /// <summary></summary>
        public enum SplitOperationMode
        {
            /// <summary></summary>
            IncludeSplitValueToLeftSubtree,
            /// <summary></summary>
            IncludeSplitValueToRightSubtree,
            /// <summary></summary>
            DoNotIncludeSplitValue
        }

        #endregion

        #region Properties

#if TREE_WITH_PARENT_POINTERS

        /// <summary>
        /// Gets the collection of values in ascending order. 
        /// Complexity: O(N)
        /// </summary>
        public IEnumerable<T> ValuesCollection
        {
            get
            {
                if (_root == null)
                    yield break;

                var p = findMin(_root);
                while (p != null)
                {
                    yield return p.Data;
                    p = successor(p);
                }
            }
        }

        /// <summary>
        /// Gets the collection of values in reverse/descending order. 
        /// Complexity: O(N)
        /// </summary>
        public IEnumerable<T> ValuesCollectionDescending
        {
            get
            {
                if (_root == null)
                    yield break;

                var p = findMax(_root);
                while (p != null)
                {
                    yield return p.Data;
                    p = predecesor(p);
                }
            }
        }

#endif

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the specified value argument. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="data">The data</param>
        public bool Add(T data)
        {
            var wasAdded = false;
            var wasSuccessful = false;

            _root = add(_root, data, ref wasAdded, ref wasSuccessful);

            return wasSuccessful;
        }

        /// <summary>
        /// Deletes the specified value argument. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="data">The data.</param>
        public bool Delete(T data)
        {
            var wasSuccessful = false;
            var wasDeleted = false;

            if (_root != null)
                _root = delete(_root, data, ref wasDeleted, ref wasSuccessful);

            return wasSuccessful;
        }

        /// <summary>
        /// Gets the min value stored in the tree. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="value">The location which upon return will store the min value in the tree.</param>
        /// <returns>a boolean indicating success or failure</returns>
        public bool GetMin(out T value)
        {
            if (_root != null)
            {
                var min = findMin(_root);
                if (min != null)
                {
                    value = min.Data;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Gets the max value stored in the tree. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="value">The location which upon return will store the max value in the tree.</param>
        /// <returns>a boolean indicating success or failure</returns>
        public bool GetMax(out T value)
        {
            if (_root != null)
            {
                var max = findMax(_root);
                if (max != null)
                {
                    value = max.Data;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Determines whether the tree contains the specified argument value. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="arg">The data to test against.</param>
        /// <returns>
        ///   <c>true</c> if tree contains the specified data; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T arg)
        {
            return search(_root, arg) != null;
        }

        /// <summary>
        /// Deletes the min. value in the tree. 
        /// Complexity: O(log(N))
        /// </summary>
        public bool DeleteMin()
        {
            if (_root != null)
            {
                bool wasDeleted = false, wasSuccessful = false;
                _root = deleteMin(_root, ref wasDeleted, ref wasSuccessful);

                return wasSuccessful;
            }

            return false;
        }

        /// <summary>
        /// Deletes the max. value in the tree. 
        /// Complexity: O(log(N))
        /// </summary>
        public bool DeleteMax()
        {
            if (_root != null)
            {
                bool wasDeleted = false, wasSuccessful = false;
                _root = deleteMax(_root, ref wasDeleted, ref wasSuccessful);

                return wasSuccessful;
            }

            return false;
        }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS

        /// <summary>
        /// Concatenates the elements of the two trees. 
        /// Precondition: ALL elements of the 'other' argument AVL tree must be LARGER than all elements contained in this instance.
        /// Complexity: O(log(N1) + log(N2)). See Remarks section below.
        /// </summary>
        /// <remarks>
        /// Complexity: 
        ///     Assuming height(node1) > height(node2), our procedure runs in height(node1) + height(node2) i.e. O(log(n1)) + O(log(n2)) due to the two calls to findMin/deleteMin (or findMax, deleteMax respectively). 
        ///     Runs in O(height(node1)) if height(node1) == height(node2).
        /// Improvements:
        ///     Performing find/delete in one operation gives O(height(node1)) speed.
        ///     Furthermore, if storing min value for each subtree, one obtains the theoretical O(height(node1) - height(node2)). 
        /// </remarks>
        public AvlTree<T> Concat(AvlTree<T> other)
        {
            if (other == null)
                return this;

            var root = concat(_root, other._root);
            return root != null ? new AvlTree<T> { _root = root } : null;
        }

        /// <summary>
        /// Splits this AVL Tree instance into 2 AVL subtrees using the specified value as the cut/split point.
        /// The value to split by must exist in the tree.
        /// This function is destructive (i.e. the current AVL tree instance is not a valid anymore upon return of this function)
        /// </summary>
        /// <param name="value">The value to use when splitting this instance.</param>
        /// <param name="mode">The mode specifying what to do with the value used for splitting. Options are not to include this value in either of the two resulting trees, to include it in the left or to include it in the right tree respectively</param>
        /// <param name="splitLeftTree">[out] The left avl tree. Upon return, all values of this subtree are less than the value argument.</param>
        /// <param name="splitRightTree">[out] The right avl tree. Upon return, all values of this subtree are greater than the value argument.</param>
        /// <returns>a boolean indicating success or failure</returns>
        public bool Split(T value, SplitOperationMode mode, out AvlTree<T> splitLeftTree, out AvlTree<T> splitRightTree)
        {
            splitLeftTree = null;
            splitRightTree = null;

            Node<T> splitLeftRoot = null, splitRightRoot = null;
            var wasFound = false;

            split(_root, value, ref splitLeftRoot, ref splitRightRoot, mode, ref wasFound);
            if (wasFound)
            {
                splitLeftTree = new AvlTree<T> { _root = splitLeftRoot };
                splitRightTree = new AvlTree<T> { _root = splitRightRoot };
            }

            return wasFound;
        }

#endif

        /// <summary>
        /// Returns the height of the tree. 
        /// Complexity: O(log N).
        /// </summary>
        /// <returns>the avl tree height</returns>
        public int GetHeightLogN()
        {
            return getHeightLogN(_root);
        }

        /// <summary>
        /// Clears this instance.
        /// Complexity: O(1).
        /// </summary>
        public void Clear()
        {
            _root = null;
        }

        internal int GetCount()
        {
            var count = 0;
            visit((node, level) => { count++; });
            return count;
        }

        #endregion

        #region Private Methods

        private static IComparer<T> getComparer()
        {
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)) || typeof(IComparable).IsAssignableFrom(typeof(T)))
                return Comparer<T>.Default;

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The type {0} cannot be compared. It must implement IComparable<T> or IComparable interface", typeof(T).FullName));
        }

        /// <summary>
        /// Gets the height of the tree in log(n) time.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The height of the tree. Runs in O(log(n)) where n is the number of nodes in the tree </returns>
        private static int getHeightLogN(Node<T> node)
        {
            if (node == null)
                return 0;

            var leftHeight = getHeightLogN(node.Left);
            if (node.Balance == 1)
                leftHeight++;

            return 1 + leftHeight;
        }

        /// <summary>
        /// Adds the specified data to the tree identified by the specified argument.
        /// </summary>
        /// <param name="elem">The elem.</param>
        /// <param name="data">The data.</param>
        /// <param name="wasAdded"></param>
        /// <param name="wasSuccessful"></param>
        /// <returns></returns>
        private Node<T> add(Node<T> elem, T data, ref bool wasAdded, ref bool wasSuccessful)
        {
            if (elem == null)
            {
                elem = new Node<T> {Data = data, Left = null, Right = null, Balance = 0, Height = 1};

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
#endif

                wasAdded = true;
                wasSuccessful = true;
            }
            else
            {
                var resultCompare = _comparer.Compare(data, elem.Data);

                if (resultCompare < 0)
                {
                    var newLeft = add(elem.Left, data, ref wasAdded, ref wasSuccessful);
                    if (elem.Left != newLeft)
                    {
                        elem.Left = newLeft;
#if TREE_WITH_PARENT_POINTERS
                        newLeft.Parent = elem;
#endif
                    }

                    if (wasAdded)
                    {
                        --elem.Balance;

                        if (elem.Balance == 0)
                            wasAdded = false;
                        else if (elem.Balance == -2)
                        {
                            var leftBalance = newLeft.Balance;
                            if (leftBalance == 1)
                            {
                                var elemLeftRightBalance = newLeft.Right.Balance;

                                elem.Left = rotateLeft(newLeft);
                                elem = rotateRight(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemLeftRightBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemLeftRightBalance == -1 ? 1 : 0;
                            }
                            else if (leftBalance == -1)
                            {
                                elem = rotateRight(elem);
                                elem.Balance = 0;
                                elem.Right.Balance = 0;
                            }

                            wasAdded = false;
                        }
                    }
                }
                else if (resultCompare > 0)
                {
                    var newRight = add(elem.Right, data, ref wasAdded, ref wasSuccessful);
                    if (elem.Right != newRight)
                    {
                        elem.Right = newRight;
#if TREE_WITH_PARENT_POINTERS
                        newRight.Parent = elem;
#endif
                    }

                    if (wasAdded)
                    {
                        ++elem.Balance;
                        if (elem.Balance == 0)
                            wasAdded = false;
                        else if (elem.Balance == 2)
                        {
                            var rightBalance = newRight.Balance;
                            if (rightBalance == -1)
                            {
                                var elemRightLeftBalance = newRight.Left.Balance;

                                elem.Right = rotateRight(newRight);
                                elem = rotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemRightLeftBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemRightLeftBalance == -1 ? 1 : 0;
                            }
                            else if (rightBalance == 1)
                            {
                                elem = rotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = 0;
                            }

                            wasAdded = false;
                        }
                    }
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                elem.Height = 1 + Math.Max(
                                        elem.Left != null ? elem.Left.Height : 0,
                                        elem.Right != null ? elem.Right.Height : 0);
#endif
            }

            return elem;
        }

        /// <summary>
        /// Deletes the specified data. value from the tree.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="arg">The data.</param>
        /// <param name="wasDeleted"></param>
        /// <param name="wasSuccessful"></param>
        /// <returns></returns>
        private Node<T> delete(Node<T> node, T arg, ref bool wasDeleted, ref bool wasSuccessful)
        {
            var compare = _comparer.Compare(arg, node.Data);
            Node<T> newChild;

            if (compare < 0)
            {
                if (node.Left != null)
                {
                    newChild = delete(node.Left, arg, ref wasDeleted, ref wasSuccessful);
                    if (node.Left != newChild)
                        node.Left = newChild;

                    if (wasDeleted)
                        node.Balance++;
                }
            }
            else if (compare == 0)
            {
                wasDeleted = true;
                if (node.Left != null && node.Right != null)
                {
                    var min = findMin(node.Right);
                    var data = node.Data;
                    node.Data = min.Data;
                    min.Data = data;

                    wasDeleted = false;

                    newChild = delete(node.Right, data, ref wasDeleted, ref wasSuccessful);
                    
                    if (node.Right != newChild)
                        node.Right = newChild;

                    if (wasDeleted)
                        node.Balance--;
                }
                else if (node.Left == null)
                {
                    wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                    if (node.Right != null)
                        node.Right.Parent = node.Parent;
#endif
                    return node.Right;
                }
                else
                {
                    wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                    if (node.Left != null)
                        node.Left.Parent = node.Parent;
#endif
                    return node.Left;
                }
            }
            else
            {
                if (node.Right != null)
                {
                    newChild = delete(node.Right, arg, ref wasDeleted, ref wasSuccessful);
                    if (node.Right != newChild)
                        node.Right = newChild;

                    if (wasDeleted)
                        node.Balance--;
                }
            }

            if (wasDeleted)
            {
                switch (node.Balance)
                {
                    case -1:
                    case 1:
                        wasDeleted = false;
                        break;

                    case -2:
                        {
                            var nodeLeft = node.Left;

                            switch (nodeLeft.Balance)
                            {
                                case -1:
                                    node = rotateRight(node);
                                    node.Balance = 0;
                                    node.Right.Balance = 0;
                                    break;

                                case 0:
                                    node = rotateRight(node);
                                    node.Balance = 1;
                                    node.Right.Balance = -1;
                                    wasDeleted = false;
                                    break;

                                case 1:
                                    var leftRightBalance = nodeLeft.Right.Balance;

                                    node.Left = rotateLeft(nodeLeft);
                                    node = rotateRight(node);

                                    node.Balance = 0;
                                    node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                                    node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                                    break;
                            }
                        }
                        break;

                    case 2:
                        {
                            var nodeRight = node.Right;
                    
                            switch (nodeRight.Balance)
                            {
                                case 1:
                                    node = rotateLeft(node);
                                    node.Balance = 0;
                                    node.Left.Balance = 0;
                                    break;

                                case 0:
                                    node = rotateLeft(node);
                                    node.Balance = -1;
                                    node.Left.Balance = 1;
                                    wasDeleted = false;
                                    break;

                                case -1:
                                    var rightLeftBalance = nodeRight.Left.Balance;

                                    node.Right = rotateRight(nodeRight);
                                    node = rotateLeft(node);

                                    node.Balance = 0;
                                    node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                                    node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                                    break;
                            }
                        }
                        break;
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                node.Height = 1 + Math.Max(
                                        node.Left != null ? node.Left.Height : 0,
                                        node.Right != null ? node.Right.Height : 0);
#endif
            }

            return node;
        }

        /// <summary>
        /// Finds the min.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node<T> findMin(Node<T> node)
        {
            while (node != null && node.Left != null)
                node = node.Left;

            return node;
        }

        /// <summary>
        /// Finds the max.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node<T> findMax(Node<T> node)
        {
            while (node != null && node.Right != null)
                node = node.Right;

            return node;
        }

        /// <summary>
        /// Searches the specified subtree for the specified data.
        /// </summary>
        /// <param name="subtree">The subtree.</param>
        /// <param name="data">The data to search for.</param>
        /// <returns>null if not found, otherwise the node instance with the specified value</returns>
        private Node<T> search(Node<T> subtree, T data)
        {
            if (subtree != null)
            {
                var compare = _comparer.Compare(data, subtree.Data);
                if (compare < 0)
                    return search(subtree.Left, data);
                if (0 < compare)
                    return search(subtree.Right, data);
                return subtree;
            }

            return null;
        }

        /// <summary>
        /// Deletes the min element in the tree.
        /// Precondition: (node != null)
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="wasDeleted"></param>
        /// <param name="wasSuccessful"></param>
        /// <returns></returns>
        private static Node<T> deleteMin(Node<T> node, ref bool wasDeleted, ref bool wasSuccessful)
        {
            if (node.Left == null)
            {
                wasDeleted = true;
                wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                if (node.Right != null)
                    node.Right.Parent = node.Parent;
#endif
                return node.Right;
            }

            node.Left = deleteMin(node.Left, ref wasDeleted, ref wasSuccessful);
            if (wasDeleted)
                node.Balance++;

            if (wasDeleted)
            {
                switch (node.Balance)
                {
                    case -1:
                    case 1:
                        wasDeleted = false;
                        break;

                    case -2:
                        switch (node.Left.Balance)
                        {
                            case -1:
                                node = rotateRight(node);
                                node.Balance = 0;
                                node.Right.Balance = 0;
                                break;

                            case 0:
                                node = rotateRight(node);
                                node.Balance = 1;
                                node.Right.Balance = -1;
                                wasDeleted = false;
                                break;

                            case 1:
                                var leftRightBalance = node.Left.Right.Balance;

                                node.Left = rotateLeft(node.Left);
                                node = rotateRight(node);

                                node.Balance = 0;
                                node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                                node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                                break;
                        }
                        break;

                    case 2:
                        switch (node.Right.Balance)
                        {
                            case 1:
                                node = rotateLeft(node);
                                node.Balance = 0;
                                node.Left.Balance = 0;
                                break;

                            case 0:
                                node = rotateLeft(node);
                                node.Balance = -1;
                                node.Left.Balance = 1;
                                wasDeleted = false;
                                break;

                            case -1:
                                var rightLeftBalance = node.Right.Left.Balance;

                                node.Right = rotateRight(node.Right);
                                node = rotateLeft(node);

                                node.Balance = 0;
                                node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                                node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                                break;
                        }
                        break;
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                node.Height = 1 + Math.Max(
                                        node.Left != null ? node.Left.Height : 0,
                                        node.Right != null ? node.Right.Height : 0);
#endif
            }

            return node;
        }

        /// <summary>
        /// Deletes the max element in the tree.
        /// Precondition: (node != null)
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="wasDeleted"></param>
        /// <param name="wasSuccessful"></param>
        /// <returns></returns>
        private static Node<T> deleteMax(Node<T> node, ref bool wasDeleted, ref bool wasSuccessful)
        {
            if (node.Right == null)
            {
                wasDeleted = true;
                wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                if (node.Left != null)
                {
                    node.Left.Parent = node.Parent;
                }
#endif
                return node.Left;
            }

            node.Right = deleteMax(node.Right, ref wasDeleted, ref wasSuccessful);
            if (wasDeleted)
                node.Balance--;

            if (wasDeleted)
            {
                switch (node.Balance)
                {
                    case -1:
                    case 1:
                        wasDeleted = false;
                        break;

                    case -2:
                        switch (node.Left.Balance)
                        {
                            case -1:
                                node = rotateRight(node);
                                node.Balance = 0;
                                node.Right.Balance = 0;
                                break;
                            case 0:
                                node = rotateRight(node);
                                node.Balance = 1;
                                node.Right.Balance = -1;
                                wasDeleted = false;
                                break;
                            case 1:
                                var leftRightBalance = node.Left.Right.Balance;

                                node.Left = rotateLeft(node.Left);
                                node = rotateRight(node);

                                node.Balance = 0;
                                node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                                node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                                break;
                        }
                        break;

                    case 2:
                        {
                            var rightBalance = node.Right.Balance;
                            switch (rightBalance)
                            {
                                case 1:
                                    node = rotateLeft(node);
                                    node.Balance = 0;
                                    node.Left.Balance = 0;
                                    break;
                                case 0:
                                    node = rotateLeft(node);
                                    node.Balance = -1;
                                    node.Left.Balance = 1;
                                    wasDeleted = false;
                                    break;
                                case -1:
                                    var rightLeftBalance = node.Right.Left.Balance;

                                    node.Right = rotateRight(node.Right);
                                    node = rotateLeft(node);

                                    node.Balance = 0;
                                    node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                                    node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                                    break;
                            }
                        }
                        break;
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                node.Height = 1 + Math.Max(
                                        node.Left != null ? node.Left.Height : 0,
                                        node.Right != null ? node.Right.Height : 0);
#endif
            }

            return node;
        }

#if TREE_WITH_PARENT_POINTERS

        /// <summary>
        /// Returns the predecessor of the specified node.
        /// </summary>
        /// <returns></returns>
        private static Node<T> predecesor(Node<T> node)
        {
            if (node.Left != null)
                return findMax(node.Left);

            var p = node;
            while (p.Parent != null && p.Parent.Left == p)
                p = p.Parent;

            return p.Parent;
        }

        /// <summary>
        /// Returns the successor of the specified node.
        /// </summary>
        /// <returns></returns>
        private static Node<T> successor(Node<T> node)
        {
            if (node.Right != null)
                return findMin(node.Right);
            
            var p = node;
            while (p.Parent != null && p.Parent.Right == p)
                p = p.Parent;

            return p.Parent;
        }

#endif

        /// <summary>
        /// Visits the tree using the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        private void visit(VisitNodeHandler<Node<T>> visitor)
        {
            if (_root != null)
                _root.Visit(visitor, 0);
        }

        /// <summary>
        /// Rotates lefts this instance. 
        /// Precondition: (node != null && node.Right != null)
        /// </summary>
        /// <returns></returns>
        private static Node<T> rotateLeft(Node<T> node)
        {
            var right = node.Right;
            var nodeLeft = node.Left;
            var rightLeft = right.Left;

            node.Right = rightLeft;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            node.Height = 1 + Math.Max(
                                    nodeLeft != null ? nodeLeft.Height : 0,
                                    rightLeft != null ? rightLeft.Height : 0);
#endif

#if TREE_WITH_PARENT_POINTERS
            var parent = node.Parent;
            if (rightLeft != null)
            {
                rightLeft.Parent = node;
            }
#endif
            right.Left = node;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            right.Height = 1 + Math.Max(
                                    node.Height,
                                    right.Right != null ? right.Right.Height : 0);
#endif

#if TREE_WITH_PARENT_POINTERS
            node.Parent = right;
            if (parent != null)
            {
                if (parent.Left == node)
                {
                    parent.Left = right;
                }
                else
                {
                    parent.Right = right;
                }
            }

            right.Parent = parent;
#endif
            return right;
        }

        /// <summary>
        /// RotateRights this instance. 
        /// Precondition: (node != null && node.Left != null)
        /// </summary>
        /// <returns></returns>
        private static Node<T> rotateRight(Node<T> node)
        {
            var left = node.Left;
            var leftRight = left.Right;
            node.Left = leftRight;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            node.Height = 1 + Math.Max(
                                    leftRight != null ? leftRight.Height : 0,
                                    node.Right != null ? node.Right.Height : 0);
#endif

#if TREE_WITH_PARENT_POINTERS
            var parent = node.Parent;
            if (leftRight != null)
            {
                leftRight.Parent = node;
            }
#endif

            left.Right = node;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            left.Height = 1 + Math.Max(
                                    left.Left != null ? left.Left.Height : 0,
                                    node.Height);
#endif

#if TREE_WITH_PARENT_POINTERS
            node.Parent = left;
            if (parent != null)
            {
                if (parent.Left == node)
                {
                    parent.Left = left;
                }
                else
                {
                    parent.Right = left;
                }
            }

            left.Parent = parent;
#endif
            return left;
        }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS

        /// <summary>
        /// Concatenates the elements of the two trees. 
        /// Precondition: ALL elements of node2 must be LARGER than all elements of node1.
        /// </summary>
        /// <remarks>
        /// Complexity: 
        ///     Assuming height(node1) > height(node2), our procedure runs in height(node1) + height(node2) due to the two calls to findMin/deleteMin (or findMax, deleteMax respectively). 
        ///     Runs in O(height(node1)) if height(node1) == height(node2).
        /// Can be sped up.
        /// </remarks>
        private Node<T> concat(Node<T> node1, Node<T> node2)
        {
            if (node1 == null)
                return node2;
            if (node2 == null)
                return node1;
            bool wasAdded = false, wasDeleted = false, wasSuccessful = false;

            int height1 = node1.Height;
            int height2 = node2.Height;

            if (height1 == height2)
            {
                var result = new Node<T>
                    {
                        Data = default(T),
                        Left = node1,
                        Right = node2,
                        Balance = 0,
                        Height = 1 + height1
                    };

#if TREE_WITH_PARENT_POINTERS
                node1.Parent = result;
                node2.Parent = result;
#endif
                result = delete(result, default(T), ref wasDeleted, ref wasSuccessful);
                return result;
            }
            else if (height1 > height2)
            {
                var min = findMin(node2);
                node2 = deleteMin(node2, ref wasDeleted, ref wasSuccessful);

                if (node2 != null)
                {
                    node1 = ConcatImpl(node1, node2, min.Data, ref wasAdded);
                }
                else
                {
                    node1 = add(node1, min.Data, ref wasAdded, ref wasSuccessful);
                }

                return node1;
            }
            else
            {
                var max = findMax(node1);
                node1 = deleteMax(node1, ref wasDeleted, ref wasSuccessful);

                if (node1 != null)
                    node2 = ConcatImpl(node2, node1, max.Data, ref wasAdded);
                else
                    node2 = add(node2, max.Data, ref wasAdded, ref wasSuccessful);

                return node2;
            }
        }

        /// <summary>
        /// Concatenates the specified trees. 
        /// Precondition: height(elem2add) is less than height(elem)
        /// </summary>
        /// <param name="elem">The elem</param>
        /// <param name="elemHeight">Height of the elem.</param>
        /// <param name="elem2add">The elem2add.</param>
        /// <param name="elem2AddHeight">Height of the elem2 add.</param>
        /// <param name="newData">The new data.</param>
        /// <param name="wasAdded">if set to <c>true</c> [was added].</param>
        /// <returns></returns>
        private Node<T> ConcatImpl(Node<T> elem, Node<T> elem2add, T newData, ref bool wasAdded)
        {
            int heightDifference = elem.Height - elem2add.Height;

            if (elem == null)
            {
                if (heightDifference > 0)
                {
                    throw new ArgumentException("invalid input");
                }
            }
            else
            {
                int compareResult = _comparer.Compare(elem.Data, newData);
                if (compareResult < 0)
                {
                    if (heightDifference == 0 || (heightDifference == 1 && elem.Balance == -1))
                    {
                        int balance = elem2add.Height - elem.Height;

                        elem = new Node<T> { Data = newData, Left = elem, Right = elem2add, Balance = balance };
                        wasAdded = true;

#if TREE_WITH_PARENT_POINTERS
                        elem.Left.Parent = elem;
                        elem2add.Parent = elem;
#endif
                    }
                    else
                    {
                        elem.Right = ConcatImpl(elem.Right, elem2add, newData, ref wasAdded);

                        if (wasAdded)
                        {
                            elem.Balance++;
                            if (elem.Balance == 0)
                            {
                                wasAdded = false;
                            }
                        }

#if TREE_WITH_PARENT_POINTERS
                        elem.Right.Parent = elem;
#endif
                        if (elem.Balance == 2)
                        {
                            if (elem.Right.Balance == -1)
                            {
                                int elemRightLeftBalance = elem.Right.Left.Balance;

                                elem.Right = rotateRight(elem.Right);
                                elem = rotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemRightLeftBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemRightLeftBalance == -1 ? 1 : 0;

                                wasAdded = false;
                            }
                            else if (elem.Right.Balance == 1)
                            {
                                elem = rotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = 0;

                                wasAdded = false;
                            }
                            else if (elem.Right.Balance == 0)
                            {
                                ////special case for concat .. before adding the tree with smaller height to the tree with the bigger height, we find the correct insertion spot in the larger height tree.
                                ////because we balance the new subtree to be added which is normally done part of adding procedure, this situation isn't present in the adding procedure so we are catering for it here..

                                elem = rotateLeft(elem);

                                elem.Balance = -1;
                                elem.Left.Balance = 1;

                                wasAdded = true;
                            }
                        }
                    }
                }
                else if (compareResult > 0)
                {
                    if (heightDifference == 0 || (heightDifference == 1 && elem.Balance == 1))
                    {
                        int balance = elem.Height - elem2add.Height;

                        elem = new Node<T> { Data = newData, Left = elem2add, Right = elem, Balance = balance };
                        wasAdded = true;

#if TREE_WITH_PARENT_POINTERS
                        elem.Right.Parent = elem;
                        elem2add.Parent = elem;
#endif
                    }
                    else
                    {
                        elem.Left = ConcatImpl(elem.Left, elem2add, newData, ref wasAdded);

                        if (wasAdded)
                        {
                            elem.Balance--;
                            if (elem.Balance == 0)
                            {
                                wasAdded = false;
                            }
                        }

#if TREE_WITH_PARENT_POINTERS
                        elem.Left.Parent = elem;
#endif
                        if (elem.Balance == -2)
                        {
                            if (elem.Left.Balance == 1)
                            {
                                int elemLeftRightBalance = elem.Left.Right.Balance;

                                elem.Left = rotateLeft(elem.Left);
                                elem = rotateRight(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemLeftRightBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemLeftRightBalance == -1 ? 1 : 0;

                                wasAdded = false;
                            }
                            else if (elem.Left.Balance == -1)
                            {
                                elem = rotateRight(elem);
                                elem.Balance = 0;
                                elem.Right.Balance = 0;

                                wasAdded = false;
                            }
                            else if (elem.Left.Balance == 0)
                            {
                                ////special case for concat .. before adding the tree with smaller height to the tree with the bigger height, we find the correct insertion spot in the larger height tree.
                                ////because we balance the new subtree to be added which is normally done part of adding procedure, this situation isn't present in the adding procedure so we are catering for it here..

                                elem = rotateRight(elem);

                                elem.Balance = 1;
                                elem.Right.Balance = -1;

                                wasAdded = true;
                            }
                        }
                    }
                }

                elem.Height = 1 + Math.Max(
                                        elem.Left != null ? elem.Left.Height : 0,
                                        elem.Right != null ? elem.Right.Height : 0);
            }

            return elem;
        }

        /// <summary>
        /// This routine is used by the split procedure. Similar to concat except that the junction point is specified (i.e. the 'value' argument).
        /// ALL nodes in node1 tree have values less than the 'value' argument and ALL nodes in node2 tree have values greater than 'value'.
        /// Complexity: O(log N). 
        /// </summary>
        private Node<T> ConcatAtJunctionPoint(Node<T> node1, Node<T> node2, T value)
        {
            bool wasAdded = false, wasSuccessful = false;

            if (node1 == null)
            {
                if (node2 != null)
                    node2 = add(node2, value, ref wasAdded, ref wasSuccessful);
                else
                    node2 = new Node<T> {Data = value, Balance = 0, Left = null, Right = null, Height = 1};

                return node2;
            }
            else if (node2 == null)
            {
                node1 = add(node1, value, ref wasAdded, ref wasSuccessful);

                return node1;
            }
            else
            {
                var height1 = node1.Height;
                var height2 = node2.Height;

                if (height1 == height2)
                {
                    // construct a new tree with its left and right subtrees pointing to the trees to be concatenated
                    var newNode = new Node<T>() { Data = value, Left = node1, Right = node2, Balance = 0, Height = 1 + height1 };

#if TREE_WITH_PARENT_POINTERS
                    node1.Parent = newNode;
                    node2.Parent = newNode;
#endif
                    return newNode;

                }
                else if (height1 > height2)
                {
                    // walk on node1's rightmost edge until you find the right place to insert the subtree with the smaller height (i.e. node2)
                    return ConcatImpl(node1, node2, value, ref wasAdded);
                }
                else
                {
                    // walk on node2's leftmost edge until you find the right place to insert the subtree with the smaller height (i.e. node1)
                    return ConcatImpl(node2, node1, value, ref wasAdded);
                }
            }
        }

        /// <summary>
        /// Splits this AVL tree instance into 2 AVL subtrees by the specified value.
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="data"></param>
        /// <param name="splitLeftTree">The split left avl tree. All values of this subtree are less than the value argument.</param>
        /// <param name="splitRightTree">The split right avl tree. All values of this subtree are greater than the value argument.</param>
        /// <param name="mode">The mode specifying what to do with the value used for splitting. Options are not to include this value in either of the two resulting trees, include it in the left or include it in the right tree respectively</param>
        /// <param name="wasFound"></param>
        /// <returns></returns>
        private void split(Node<T> elem, T data, ref Node<T> splitLeftTree, ref Node<T> splitRightTree, SplitOperationMode mode, ref bool wasFound)
        {
            bool wasAdded = false, wasSuccessful = false;

            int compareResult = _comparer.Compare(data, elem.Data);
            if (compareResult < 0)
            {
                split(elem.Left, data, ref splitLeftTree, ref splitRightTree, mode, ref wasFound);
                if (wasFound)
                {
#if TREE_WITH_PARENT_POINTERS
                    if (elem.Right != null)
                    {
                        elem.Right.Parent = null;
                    }
#endif
                    splitRightTree = ConcatAtJunctionPoint(splitRightTree, elem.Right, elem.Data);
                }
            }
            else if (compareResult > 0)
            {
                split(elem.Right, data, ref splitLeftTree, ref splitRightTree, mode, ref wasFound);
                if (wasFound)
                {
#if TREE_WITH_PARENT_POINTERS
                    if (elem.Left != null)
                    {
                        elem.Left.Parent = null;
                    }
#endif
                    splitLeftTree = ConcatAtJunctionPoint(elem.Left, splitLeftTree, elem.Data);
                }
            }
            else
            {
                wasFound = true;
                splitLeftTree = elem.Left;
                splitRightTree = elem.Right;

#if TREE_WITH_PARENT_POINTERS
                if (splitLeftTree != null)
                {
                    splitLeftTree.Parent = null;
                }

                if (splitRightTree != null)
                {
                    splitRightTree.Parent = null;
                }
#endif

                switch (mode)
                {
                    case SplitOperationMode.IncludeSplitValueToLeftSubtree:
                        splitLeftTree = add(splitLeftTree, elem.Data, ref wasAdded, ref wasSuccessful);
                        break;
                    case SplitOperationMode.IncludeSplitValueToRightSubtree:
                        splitRightTree = add(splitRightTree, elem.Data, ref wasAdded, ref wasSuccessful);
                        break;
                }
            }
        }

#endif
        #endregion

        #region Nested Classes

        /// <summary>
        /// node class
        /// </summary>
        /// <typeparam name="TElem">The type of the elem.</typeparam>
        private class Node<TElem>
        {
            #region Properties

            public Node<TElem> Left { get; set; }

            public Node<TElem> Right { get; set; }

            public TElem Data { get; set; }

            public int Balance { get; set; }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            public int Height { get; set; }
#endif

#if TREE_WITH_PARENT_POINTERS
            public Node<TElem> Parent { get; set; }
#endif

            #endregion

            #region Methods

            /// <summary>
            /// Visits (in-order) this node with the specified visitor.
            /// </summary>
            /// <param name="visitor">The visitor.</param>
            /// <param name="level">The level.</param>
            public void Visit(VisitNodeHandler<Node<TElem>> visitor, int level)
            {
                if (visitor == null)
                    return;

                if (Left != null)
                    Left.Visit(visitor, level + 1);

                visitor(this, level);

                if (Right != null)
                    Right.Visit(visitor, level + 1);
            }

            #endregion
        }

        #endregion
    }
}
