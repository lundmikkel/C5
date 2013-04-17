using System;
using System.Collections;
using SCG = System.Collections.Generic;
using System.Linq;

namespace C5.intervaled
{
    public class NestedContainmentList<T> : IStaticIntervaled<T> where T : IComparable<T>
    {
        private readonly IList<Node> _list;
        private IInterval<T> _span;
        private Section _section;

        struct Section
        {
            public Section(int offset, int length)
                : this()
            {
                Offset = offset;
                Length = length;
            }

            public int Offset { get; private set; }
            public int Length { get; private set; }
        }

        #region Node nested classes

        struct Node
        {
            internal IInterval<T> Interval { get; private set; }
            internal IList<Node> SublistList { get; private set; }
            internal Section Sublist { get; private set; }

            internal int NodesBefore { get; private set; }
            internal int NodesInSublist { get; private set; }

            internal Node(IInterval<T> interval, IList<Node> list, Section section, int nodesBefore, int nodesInSublist)
                : this()
            {
                Interval = interval;

                if (!list.IsEmpty())
                    SublistList = list;
                else
                    SublistList = new ArrayList<Node>();

                Sublist = section;

                NodesBefore = nodesBefore;
                NodesInSublist = nodesInSublist;
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
        private static IList<Node> createList(IInterval<T>[] intervals)
        {
            // List to hold the nodes
            IList<Node> list = new ArrayList<Node>(); // TODO: Null and then init in if?
            // Remember the number of nodes before the current node to allow fast count operation
            var nodesBefore = 0;

            // Iterate through the intervals to build the list
            var enumerator = intervals.Cast<IInterval<T>>().GetEnumerator();
            if (enumerator.MoveNext())
            {
                // Remember the previous node so we can check if the next nodes are contained in it
                var previous = enumerator.Current;
                var sublist = new ArrayList<IInterval<T>>();
                IList<Node> sublistList;

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
                        sublistList = createList(sublist.ToArray());
                        list.Add(new Node(previous, sublistList, new Section(0, sublistList.Count), nodesBefore, sublist.Count));
                        // Add the sublist count and the current node to the nodes before
                        nodesBefore += sublist.Count + 1;

                        // Reset the looped values
                        sublist = new ArrayList<IInterval<T>>();
                        previous = current;
                    }
                }

                // Add the last node to the list when we are done looping through them
                sublistList = createList(sublist.ToArray());
                list.Add(new Node(previous, sublistList, new Section(0, sublistList.Count), nodesBefore, sublist.Count));
            }

            return list;
        }

        /// <summary>
        /// Create a Nested Containment List with a enumerable of intervals
        /// </summary>
        /// <param name="intervals">A collection of intervals in arbitrary order</param>
        public NestedContainmentList(SCG.IEnumerable<IInterval<T>> intervals)
        {
            var intervalsArray = intervals as IInterval<T>[] ?? intervals.ToArray();

            if (!intervalsArray.IsEmpty())
            {
                Count = intervalsArray.Count();

                Sorting.IntroSort(intervalsArray, 0, intervalsArray.Count(),
                                  ComparerFactory<IInterval<T>>.CreateComparer(IntervalExtensions.CompareTo));

                // TODO: Figure out how Orcomp does it
                //MaximumOverlap = IntervaledHelper<T>.MaximumOverlap(intervalsArray);

                // Build nested containment list recursively and save the upper-most list in the class
                _list = createList(intervalsArray);

                _section = new Section(0, _list.Count);

                // Save span to allow for constant speeds on later requests
                IInterval<T> i = _list.First.Interval, j = _list.Last.Interval;
                Span = new IntervalBase<T>(i.Low, j.High, i.LowIncluded, j.HighIncluded);
            }
            else
            {
                _list = new ArrayList<Node>();
                _section = new Section(0, 0);
            }
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
        public SCG.IEnumerator<IInterval<T>> GetEnumerator()
        {
            return getEnumerator(_list);
        }

        private SCG.IEnumerator<IInterval<T>> getEnumerator(SCG.IEnumerable<Node> list)
        {
            // Just for good measures
            if (list == null)
                yield break;

            foreach (var node in list)
            {
                // Yield the interval itself before the sublist to maintain sorting order
                yield return node.Interval;

                if (node.SublistList != null)
                {
                    var child = getEnumerator(node.SublistList);

                    while (child.MoveNext())
                        yield return child.Current;
                }
            }
        }

        #endregion

        #region Formatting

        public string ToString(string format, IFormatProvider formatProvider)
        {
            // TODO: Correct implementation?
            return _list.ToString();
        }

        #region IShowable

        public bool Show(System.Text.StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region ICollectionValue

        #region Events

        // The structure is static and has therefore no meaningful events
        public EventTypeEnum ListenableEvents { get { return EventTypeEnum.None; } }
        public EventTypeEnum ActiveEvents { get { return EventTypeEnum.None; } }

        public event CollectionChangedHandler<IInterval<T>> CollectionChanged;
        public event CollectionClearedHandler<IInterval<T>> CollectionCleared;
        public event ItemsAddedHandler<IInterval<T>> ItemsAdded;
        public event ItemInsertedHandler<IInterval<T>> ItemInserted;
        public event ItemsRemovedHandler<IInterval<T>> ItemsRemoved;
        public event ItemRemovedAtHandler<IInterval<T>> ItemRemovedAt;

        #endregion

        public bool IsEmpty { get { return Count == 0; } }
        public int Count { get; private set; }
        public Speed CountSpeed { get { return Speed.Constant; } }

        public void CopyTo(IInterval<T>[] array, int index)
        {
            if (index < 0 || index + Count > array.Length)
                throw new ArgumentOutOfRangeException();

            foreach (var item in this)
                array[index++] = item;
        }

        public IInterval<T>[] ToArray()
        {
            var res = new IInterval<T>[Count];
            var i = 0;

            foreach (var item in this)
                res[i++] = item;

            return res;
        }

        public void Apply(Action<IInterval<T>> action)
        {
            foreach (var item in this)
                action(item);
        }

        public bool Exists(Func<IInterval<T>, bool> predicate)
        {
            return this.Any(predicate);
        }

        public bool Find(Func<IInterval<T>, bool> predicate, out IInterval<T> item)
        {
            foreach (var jtem in this.Where(predicate))
            {
                item = jtem;
                return true;
            }
            item = default(IInterval<T>);
            return false;
        }

        public bool All(Func<IInterval<T>, bool> predicate)
        {
            return Enumerable.All(this, predicate);
        }

        public IInterval<T> Choose()
        {
            if (Count > 0)
                return _list.First.Interval;

            throw new NoSuchItemException();
        }

        public SCG.IEnumerable<IInterval<T>> Filter(Func<IInterval<T>, bool> filter)
        {
            return this.Where(filter);
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

            private set { _span = value; }
        }

        public Speed SpanSpeed { get { return Speed.Constant; } }

        public SCG.IEnumerable<IInterval<T>> FindOverlaps(T query)
        {
            if (ReferenceEquals(query, null))
                return Enumerable.Empty<IInterval<T>>();

            return findOverlap(_list.ToArray(), _section, new IntervalBase<T>(query));
        }

        // TODO: Test speed difference between version that takes overlap-loop and upper and low bound loop
        private static SCG.IEnumerable<IInterval<T>> findOverlap(Node[] list, Section section, IInterval<T> query, bool contained = false)
        {
            if (list == null || section.Length == 0)
                yield break;

            int first, last;

            // If all intervals are contained in the query, just loop through all nodes
            if (contained)
            {
                first = section.Offset;
                last = section.Offset + section.Length - 1;
            }
            // If not, we need to search for the bounds
            else
            {
                // Find first overlapping interval
                first = searchHighInLows(list, section, query);

                // If index is out of bound, or interval doesn't overlap, we can just stop our search
                if (first < section.Offset || section.Offset + section.Length - 1 < first || !list[first].Interval.Overlaps(query))
                    yield break;

                last = searchLowInHighs(list, section, query);
            }

            while (first <= last)
            {
                var node = list[first++];

                yield return node.Interval;

                if (node.SublistList != null)
                {
                    // If the interval is contained in the query, all intervals in the sublist must overlap the query
                    if (!contained)
                        contained = query.Contains(node.Interval);

                    foreach (var interval in findOverlap(node.SublistList.ToArray(), node.Sublist, query, contained))
                        yield return interval;
                }
            }
        }

        /// <summary>
        /// Get the index of the first node with an interval the overlaps the query
        /// </summary>
        /// <param name="list"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static int searchHighInLows(Node[] list, Section section, IInterval<T> query)
        {
            if (query == null || section.Length == 0)
                return -1;

            int min = section.Offset, max = section.Offset + section.Length - 1;

            while (min <= max)
            {
                var middle = (max + min) / 2;

                if (query.Overlaps(list[middle].Interval))
                {
                    // Only if the previous interval to the overlapping interval does not overlap, we have found the right one
                    if (middle == section.Offset || !query.Overlaps(list[middle - 1].Interval))
                        // The right interval is found
                        return middle;

                    // The previous interval overlap as well, move left
                    max = middle - 1;
                }
                else
                {
                    // The interval does not overlap, found out whether query is lower or higher
                    if (query.CompareTo(list[middle].Interval) < 0)
                        // The query is lower than the interval, move left
                        max = middle - 1;
                    else
                        // The query is higher than the interval, move right
                        min = middle + 1;
                }
            }

            // We return min so we know if the query was lower or higher than the list
            return min;
        }

        /// <summary>
        /// Get the index of the last node with an interval the overlaps the query
        /// </summary>
        /// <param name="list"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static int searchLowInHighs(Node[] list, Section section, IInterval<T> query)
        {
            if (query == null || section.Length == 0)
                return -1;

            int min = section.Offset, max = section.Offset + section.Length - 1;

            while (min <= max)
            {
                var middle = (max + min) / 2;

                if (query.Overlaps(list[middle].Interval))
                {
                    // Only if the next interval to the overlapping interval does not overlap, we have found the right one
                    if (middle == (section.Offset + section.Length - 1) || !query.Overlaps(list[middle + 1].Interval))
                        // The right interval is found
                        return middle;

                    // The previous interval overlap as well, move right
                    min = middle + 1;
                }
                else
                {
                    // The interval does not overlap, found out whether query is lower or higher
                    if (query.CompareTo(list[middle].Interval) < 0)
                        // The query is lower than the interval, move left
                        max = middle - 1;
                    else
                        // The query is higher than the interval, move right
                        min = middle + 1;
                }
            }

            return max;
        }

        public SCG.IEnumerable<IInterval<T>> FindOverlaps(IInterval<T> query)
        {
            if (query == null)
                return Enumerable.Empty<IInterval<T>>();

            return findOverlap(_list.ToArray(), _section, query);
        }

        public bool OverlapExists(IInterval<T> query)
        {
            if (query == null)
                return false;

            // Check if query overlaps the collection at all
            if (_list.IsEmpty() || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = searchHighInLows(_list.ToArray(), _section, query);

            // Check if index is in bound and if the interval overlaps the query
            return 0 <= i && i < _list.Count && _list[i].Interval.Overlaps(query);
        }

        public int MaximumOverlap { get; private set; }

        #region Static Intervaled

        public int OverlapCount(IInterval<T> query)
        {
            // TODO: Exception?
            if (query == null)
                return -1;

            // The number of overlaps is the difference between the number of nodes not after the last overlap
            // and the number of nodes before the first overlap
            return countNotAfter(_list.ToArray(), _section, query) - countBefore(_list.ToArray(), _section, query);
        }

        private static int countBefore(Node[] list, Section section, IInterval<T> query)
        {
            // Return 0 if list is empty
            if (list == null || section.Length == 0)
                return 0;

            var i = searchHighInLows(list, section, query);

            // query is before the list's span
            if (i == section.Offset && !query.Overlaps(list[section.Offset].Interval))
                return 0;

            // query is after the list's span
            if (i >= section.Offset + section.Length)
                return nodesInList(list, section);

            // The interval overlaps so all intervals before don't
            // We still need to check the sublist though
            if (query.Overlaps(list[i].Interval))
                return list[i].NodesBefore + countBefore(list[i].SublistList.ToArray(), list[i].Sublist, query);

            return list[i].NodesBefore;
        }

        private static int countNotAfter(Node[] list, Section section, IInterval<T> query)
        {
            // Return 0 if list is empty
            if (list == null || section.Length == 0)
                return 0;

            var i = searchLowInHighs(list, section, query);

            // query is after the list's span
            if (i == section.Offset + section.Length - 1 && !query.Overlaps(list[section.Offset].Interval))
                return nodesInList(list, section);

            // query is before the list's span
            if (i < section.Offset)
                return 0;

            // If the interval doesn't overlap
            if (!query.Overlaps(list[i].Interval))
                return nodesBeforeNext(list[i]);

            return list[i].NodesBefore + 1 + countNotAfter(list[i].SublistList.ToArray(), list[i].Sublist, query);
        }

        private static int nodesInList(Node[] list, Section section)
        {
            return nodesBeforeNext(list[section.Offset + section.Length - 1]);
        }

        private static int nodesBeforeNext(Node node)
        {
            // Nodes before the node, one for the node itself and the number of nodes in the sublist
            return node.NodesBefore + 1 + node.NodesInSublist;
        }

        #endregion

        #endregion
    }
}
