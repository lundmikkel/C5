using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.intervaled
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Check if an enumerable is null or empty
        /// </summary>
        /// <param name="enumerable">An enumerable</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if collection is either null or empty, otherwise false</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }

    public class IntervaledHelper<T> where T : IComparable<T>
    {
        private const bool RED = true;
        private const bool BLACK = false;

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
        }

        #endregion

        public static int MaximumOverlap(System.Collections.Generic.IEnumerable<IInterval<T>> intervals)
        {
            Node root = null;

            foreach (var interval in intervals)
                root = add(root, interval);

            return root != null ? root.Max : 0;
        }

        private static Node addLow(Node root, IInterval<T> interval)
        {
            if (root == null)
                root = new Node(interval.Low);

            var compareTo = root.Key.CompareTo(interval.Low);

            if (compareTo < 0)
                root.Right = addLow(root.Right, interval);
            else if (compareTo > 0)
                root.Left = addLow(root.Left, interval);
            else
                // Update delta
                if (interval.LowIncluded)
                    root.Delta++;
                else
                    root.DeltaAfter++;

            // Red Black tree rotations
            root = rotate(root);

            // Update PMO
            root.UpdateMaximumOverlap();

            return root;
        }

        private static Node addHigh(Node root, IInterval<T> interval)
        {
            if (root == null)
                root = new Node(interval.High);

            var compareTo = root.Key.CompareTo(interval.High);

            if (compareTo > 0)
                root.Left = addHigh(root.Left, interval);
            else if (compareTo < 0)
                root.Right = addHigh(root.Right, interval);
            else
                if (!interval.HighIncluded)
                    root.Delta--;
                else
                    root.DeltaAfter--;

            // Red Black tree rotations
            root = rotate(root);

            // Update PMO
            root.UpdateMaximumOverlap();

            return root;
        }

        private static Node add(Node root, IInterval<T> interval)
        {
            root = addLow(root, interval);
            root = addHigh(root, interval);

            root.Color = BLACK;

            return root;
        }
    }
}
