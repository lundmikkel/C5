using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace C5.intervals
{
    public class NestedContainmentList2<T> : CollectionValueBase<IInterval<T>>, IIntervalCollection<T> where T : IComparable<T>
    {
        private readonly Node[] _list;
        private readonly IInterval<T> _span;
        private readonly int _count;

        #region Node nested classes

        struct Node
        {
            internal IInterval<T> Interval { get; private set; }
            internal Node[] Sublist { get; private set; }

            internal Node(IInterval<T> interval, Node[] sublist)
                : this()
            {
                Interval = interval;
                Sublist = sublist;
            }

            public override string ToString()
            {
                return Interval.ToString();
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// A sorted list of IInterval&lt;T&gt; sorted with IntervalComparer&lt;T&gt;
        /// </summary>
        /// <param name="intervals">Sorted intervals</param>
        /// <returns>A list of nodes</returns>
        private static Node[] createList(IEnumerable<IInterval<T>> intervals)
        {
            // List to hold the nodes
            IList<Node> list = new ArrayList<Node>();

            // Iterate through the intervals to build the list
            var enumerator = intervals.GetEnumerator();
            if (enumerator.MoveNext())
            {
                // Remember the previous node so we can check if the next nodes are contained in it
                var previous = enumerator.Current;
                var sublist = new ArrayList<IInterval<T>>();

                // Loop through intervals
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;

                    if (previous.Contains(current))
                        // Add contained intervals to sublist for previous node
                        sublist.Add(current);
                    else
                    {
                        // The current interval wasn't contained in the prevoius node, so we can finally add the previous to the list
                        list.Add(new Node(previous, (sublist.IsEmpty ? null : createList(sublist))));

                        // Reset the looped values
                        if (!sublist.IsEmpty)
                            sublist = new ArrayList<IInterval<T>>();

                        previous = current;
                    }
                }

                // Add the last node to the list when we are done looping through them
                list.Add(new Node(previous, createList(sublist.ToArray())));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Create a Nested Containment List with a enumerable of intervals
        /// </summary>
        /// <param name="intervals">A collection of intervals in arbitrary order</param>
        public NestedContainmentList2(IEnumerable<IInterval<T>> intervals)
        {
            var intervalsArray = intervals as IInterval<T>[] ?? intervals.ToArray();

            if (intervalsArray.IsEmpty())
                return;

            _count = intervalsArray.Count();

            Sorting.IntroSort(intervalsArray, 0, _count, ComparerFactory<IInterval<T>>.CreateComparer(IntervalExtensions.CompareTo));

            // Build nested containment list recursively and save the upper-most list in the class
            _list = createList(intervalsArray);

            // Save span to allow for constant speeds on later requests
            IInterval<T> i = _list[0].Interval, j = _list[_list.Length - 1].Interval;
            _span = new IntervalBase<T>(i, j);
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // TODO: Test the order is still the same as when sorted with IntervalComparer. This should be that case!
        /// <summary>
        /// Create an enumerator, enumerating the intervals in sorted order - sorted on low endpoint with shortest intervals first
        /// </summary>
        /// <returns>Enumerator</returns>
        // TODO: Test the order is still the same as when sorted with IntervalComparer. This should be that case!
        public override IEnumerator<IInterval<T>> GetEnumerator()
        {
            return getEnumerator(_list);
        }

        private IEnumerator<IInterval<T>> getEnumerator(IEnumerable<Node> list)
        {
            // Just for good measures
            if (list == null)
                yield break;

            foreach (var node in list)
            {
                // Yield the interval itself before the sublist to maintain sorting order
                yield return node.Interval;

                if (node.Sublist != null)
                {
                    var child = getEnumerator(node.Sublist);

                    while (child.MoveNext())
                        yield return child.Current;
                }
            }
        }

        #endregion


        #region ICollectionValue


        public override bool IsEmpty { get { return Count == 0; } }
        public override int Count { get { return _count; } }
        public override Speed CountSpeed { get { return Speed.Constant; } }

        public override IInterval<T> Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _list[0].Interval;
        }

        #endregion

        #region IIntervaled

        public IInterval<T> Span
        {
            get
            {
                // TODO: Use a better exception? Return null for empty collection?
                if (IsEmpty)
                    throw new InvalidOperationException("An empty collection has no span");

                return _span;
            }
        }

        public int MaximumOverlap
        {
            get { throw new NotSupportedException(); }
        }

        public IEnumerable<IInterval<T>> FindOverlaps(T query)
        {
            if (ReferenceEquals(query, null))
                throw new NullReferenceException("Query can't be null");

            return overlap(_list, new IntervalBase<T>(query));
        }

        // TODO: Test speed difference between version that takes overlap-loop and upper and low bound loop
        private static IEnumerable<IInterval<T>> overlap(Node[] list, IInterval<T> query)
        {
            if (list == null)
                yield break;

            int first, last;

            // Find first overlapping interval
            first = findFirst(list, query);

            // If index is out of bound, or interval doesn't overlap, we can just stop our search
            if (first < 0 || list.Length - 1 < first || !list[first].Interval.Overlaps(query))
                yield break;

            last = findLast(list, query);

            while (first <= last)
            {
                var node = list[first++];

                yield return node.Interval;

                if (node.Sublist != null)
                {
                    foreach (var interval in overlap(node.Sublist, query))
                        yield return interval;
                }
            }
        }

        /// <summary>
        /// Get the index of the first node with an interval the overlaps the query
        /// </summary>
        private static int findFirst(Node[] list, IInterval<T> query)
        {
            if (query == null || list.Length == 0)
                return -1;

            int min = -1, max = list.Length;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = list[middle].Interval;

                var compare = query.Low.CompareTo(interval.High);

                if (compare < 0 || compare == 0 && query.LowIncluded && interval.HighIncluded)
                    max = middle;
                else
                    min = middle;
            }

            // We return min so we know if the query was lower or higher than the list
            return max;
        }

        /// <summary>
        /// Get the index of the first node with an interval the overlaps the query
        /// </summary>
        private static int findLast(Node[] list, IInterval<T> query)
        {
            if (query == null || list.Length == 0)
                return -1;

            int min = -1, max = list.Length;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = list[middle].Interval;

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return min;
        }

        public IEnumerable<IInterval<T>> FindOverlaps(IInterval<T> query)
        {
            if (query == null)
                return Enumerable.Empty<IInterval<T>>();

            return overlap(_list, query);
        }

        public IInterval<T> FindAnyOverlap(IInterval<T> query)
        {
            if (query == null)
                return null;

            // Check if query overlaps the collection at all
            if (_list.IsEmpty() || !query.Overlaps(Span))
                return null;

            // Find first overlap
            var i = findFirst(_list, query);

            // Check if index is in bound and if the interval overlaps the query
            return 0 <= i && i < _list.Length && _list[i].Interval.Overlaps(query) ? _list[i].Interval : null;
        }

        public IInterval<T> FindAnyOverlap(T query)
        {
            return FindAnyOverlap(new IntervalBase<T>(query));
        }

        public bool OverlapExists(IInterval<T> query)
        {
            if (query == null)
                return false;

            // Check if query overlaps the collection at all
            if (_list.IsEmpty() || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(_list, query);

            // Check if index is in bound and if the interval overlaps the query
            return 0 <= i && i < _list.Length && _list[i].Interval.Overlaps(query);
        }

        public int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }

        public bool Add(IInterval<T> interval)
        {
            throw new NotSupportedException();
        }

        public bool Remove(IInterval<T> interval)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
