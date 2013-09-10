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
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <param name="wasAdded">States if a new node was added. If true we need to check the tree balance and the way back, to see if we need to do a rotation</param>
        /// <param name="wasSuccessful"></param>
        /// <returns></returns>
        private Node<T> add(Node<T> node, T data, ref bool wasAdded, ref bool wasSuccessful)
        {
            // Insert new node
            if (node == null)
            {
                node = new Node<T> { Data = data };
                wasAdded = true;
                wasSuccessful = true;
                return node;
            }

            var compare = _comparer.Compare(data, node.Data);
            if (compare < 0)
            {

                var newLeft = add(node.Left, data, ref wasAdded, ref wasSuccessful);
                if (node.Left != newLeft)
                    node.Left = newLeft;

                // A new node was added
                if (wasAdded)
                {
                    switch (--node.Balance)
                    {
                        case 0:
                            wasAdded = false;
                            break;

                        case -2:
                            switch (newLeft.Balance)
                            {
                                case 1:
                                    {
                                        node.Left = rotateLeft(newLeft);
                                        node = rotateRight(node);

                                        node.Balance = 0;
                                        node.Left.Balance = newLeft.Right.Balance == 1 ? -1 : 0;
                                        node.Right.Balance = newLeft.Right.Balance == -1 ? 1 : 0;
                                    }
                                    break;
                                case -1:
                                    node = rotateRight(node);
                                    node.Balance = 0;
                                    node.Right.Balance = 0;
                                    break;
                            }
                            wasAdded = false;
                            break;
                    }
                }
            }
            else if (compare > 0)
            {
                var newRight = add(node.Right, data, ref wasAdded, ref wasSuccessful);
                if (node.Right != newRight)
                    node.Right = newRight;

                if (wasAdded)
                {
                    switch (++node.Balance)
                    {
                        case 0:
                            wasAdded = false;
                            break;

                        case 2:
                            {
                                switch (newRight.Balance)
                                {
                                    case -1:
                                        node.Right = rotateRight(newRight);
                                        node = rotateLeft(node);

                                        node.Balance = 0;
                                        node.Left.Balance = newRight.Left.Balance == 1 ? -1 : 0;
                                        node.Right.Balance = newRight.Left.Balance == -1 ? 1 : 0;
                                        break;

                                    case 1:
                                        node = rotateLeft(node);
                                        node.Balance = node.Left.Balance = 0;
                                        break;
                                }

                                wasAdded = false;
                            }
                            break;
                    }
                }
            }

            return node;
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

                    return node.Right;
                }
                else
                {
                    wasSuccessful = true;

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

            }

            return node;
        }

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
            node.Right = right.Left;
            right.Left = node;

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
            node.Left = left.Right;
            left.Right = node;

            return left;
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// node class
        /// </summary>
        /// <typeparam name="TElem">The type of the node.</typeparam>
        private class Node<TElem>
        {
            #region Properties

            public Node<TElem> Left { get; set; }

            public Node<TElem> Right { get; set; }

            public TElem Data { get; set; }

            public int Balance { get; set; }

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
