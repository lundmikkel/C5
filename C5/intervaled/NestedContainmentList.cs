using System;
using System.Collections;
using SCG = System.Collections.Generic;
using System.Linq;

namespace C5.intervaled
{
    public class NestedContainmentList<T> : IStaticIntervaled<T> where T : IComparable<T>
    {
        private readonly Node[] _list;
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
            internal Section Sublist { get; private set; }

            internal int NodesBefore { get; private set; }
            internal int NodesInSublist { get; private set; }

            internal Node(IInterval<T> interval, Section section, int nodesBefore, int nodesInSublist)
                : this()
            {
                Interval = interval;
                Sublist = section;

                NodesBefore = nodesBefore;
                NodesInSublist = nodesInSublist;
            }

            public override string ToString()
            {
                return String.Format("{0} - {1}/{2}", Interval, Sublist.Length, Sublist.Offset);
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// A sorted list of IInterval&lt;T&gt; sorted with IntervalComparer&lt;T&gt;
        /// </summary>
        /// <param name="intervals">Sorted intervals</param>
        /// <returns>A list of nodes</returns>
        private int createList(IInterval<T>[] intervals, Section source, Section target)
        {
            var end = target.Offset + target.Length;
            var t = target.Offset;

            // Remember the number of nodes before the current node to allow fast count operation
            var nodesBefore = 0;

            for (int s = source.Offset; s < source.Offset + source.Length; s++)
            {
                var interval = intervals[s];
                var contained = 0;
                var length = 0;

                // Continue as long as we have more intervals
                while (s + 1 < source.Offset + source.Length)
                {
                    var nextInterval = intervals[s + 1];

                    if (!interval.Contains(nextInterval))
                        break;

                    contained++;
                    s++;
                }

                if (contained > 0)
                {
                    end -= contained;
                    length = createList(intervals, new Section(s - contained + 1, contained), new Section(end, contained));
                }

                _list[t++] = new Node(interval, new Section(end, length), nodesBefore, contained);

                nodesBefore += contained + 1;
            }

            return t - target.Offset;
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

                Sorting.IntroSort(intervalsArray, 0, Count, ComparerFactory<IInterval<T>>.CreateComparer(IntervalExtensions.CompareTo));

                // TODO: Figure out how Orcomp does it
                //MaximumOverlap = IntervaledHelper<T>.MaximumOverlap(intervalsArray);

                var totalSection = new Section(0, intervalsArray.Count());
                _list = new Node[totalSection.Length];

                // Build nested containment list recursively and save the upper-most list in the class
                _section = new Section(0, createList(intervalsArray, totalSection, totalSection));

                // Save span to allow for constant speeds on later requests
                IInterval<T> i = _list[_section.Offset].Interval, j = _list[_section.Length + _section.Offset - 1].Interval;
                Span = new IntervalBase<T>(i.Low, j.High, i.LowIncluded, j.HighIncluded);
            }
            else
            {
                _list = new Node[0];
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
            return getEnumerator(_section);
        }

        private SCG.IEnumerator<IInterval<T>> getEnumerator(Section section)
        {
            // Just for good measures
            if (_list == null || section.Length == 0)
                yield break;

            for (int i = section.Offset; i < section.Offset + section.Length; i++)
            {
                var node = _list[i];
                // Yield the interval itself before the sublist to maintain sorting order
                yield return node.Interval;

                if (node.Sublist.Length > 0)
                {
                    var child = getEnumerator(node.Sublist);

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
                return _list[_section.Offset].Interval;

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

            return findOverlap(_section, new IntervalBase<T>(query));
        }

        // TODO: Test speed difference between version that takes overlap-loop and upper and low bound loop
        private SCG.IEnumerable<IInterval<T>> findOverlap(Section section, IInterval<T> query, bool contained = false)
        {
            if (_list == null || section.Length == 0)
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
                first = searchHighInLows(section, query);

                // If index is out of bound, or interval doesn't overlap, we can just stop our search
                if (first < section.Offset || section.Offset + section.Length - 1 < first || !_list[first].Interval.Overlaps(query))
                    yield break;

                last = searchLowInHighs(section, query);
            }

            while (first <= last)
            {
                var node = _list[first++];

                yield return node.Interval;

                if (node.Sublist.Length > 0)
                {
                    // If the interval is contained in the query, all intervals in the sublist must overlap the query
                    if (!contained)
                        contained = query.Contains(node.Interval);

                    foreach (var interval in findOverlap(node.Sublist, query, contained))
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
        private int searchHighInLows(Section section, IInterval<T> query)
        {
            if (query == null || section.Length == 0)
                return -1;

            int min = section.Offset, max = section.Offset + section.Length - 1;

            while (min <= max)
            {
                var middle = (max + min) / 2;

                if (query.Overlaps(_list[middle].Interval))
                {
                    // Only if the previous interval to the overlapping interval does not overlap, we have found the right one
                    if (middle == section.Offset || !query.Overlaps(_list[middle - 1].Interval))
                        // The right interval is found
                        return middle;

                    // The previous interval overlap as well, move left
                    max = middle - 1;
                }
                else
                {
                    // The interval does not overlap, found out whether query is lower or higher
                    if (query.CompareTo(_list[middle].Interval) < 0)
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
        private int searchLowInHighs(Section section, IInterval<T> query)
        {
            if (query == null || section.Length == 0)
                return -1;

            int min = section.Offset, max = section.Offset + section.Length - 1;

            while (min <= max)
            {
                var middle = (max + min) / 2;

                if (query.Overlaps(_list[middle].Interval))
                {
                    // Only if the next interval to the overlapping interval does not overlap, we have found the right one
                    if (middle == (section.Offset + section.Length - 1) || !query.Overlaps(_list[middle + 1].Interval))
                        // The right interval is found
                        return middle;

                    // The previous interval overlap as well, move right
                    min = middle + 1;
                }
                else
                {
                    // The interval does not overlap, found out whether query is lower or higher
                    if (query.CompareTo(_list[middle].Interval) < 0)
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

            return findOverlap(_section, query);
        }

        public bool OverlapExists(IInterval<T> query)
        {
            if (query == null)
                return false;

            // Check if query overlaps the collection at all
            if (_list.IsEmpty() || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = searchHighInLows(_section, query);

            // Check if index is in bound and if the interval overlaps the query
            return _section.Offset <= i && i < _section.Offset + _section.Length && _list[i].Interval.Overlaps(query);
        }

        public int MaximumOverlap { get; private set; }

        #region Static Intervaled

        public int CountOverlaps(IInterval<T> query)
        {
            if (query == null)
                return 0;

            // The number of overlaps is the difference between the number of nodes not after the last overlap
            // and the number of nodes before the first overlap
            return countNotAfter(_section, query) - countBefore(_section, query);
        }

        private int countBefore(Section section, IInterval<T> query)
        {
            // Return 0 if list is empty
            if (_list == null || section.Length == 0)
                return 0;

            var i = searchHighInLows(section, query);

            // query is before the list's span
            if (i == section.Offset && !query.Overlaps(_list[section.Offset].Interval))
                return 0;

            // query is after the list's span
            if (i >= section.Offset + section.Length)
                return nodesInList(section);

            // The interval overlaps so all intervals before don't
            // We still need to check the sublist though
            if (query.Overlaps(_list[i].Interval))
                return _list[i].NodesBefore + countBefore(_list[i].Sublist, query);

            return _list[i].NodesBefore;
        }

        private int countNotAfter(Section section, IInterval<T> query)
        {
            // Return 0 if list is empty
            if (_list == null || section.Length == 0)
                return 0;

            var i = searchLowInHighs(section, query);

            // query is after the list's span
            if (i == section.Offset + section.Length - 1 && !query.Overlaps(_list[section.Offset].Interval))
                return nodesInList(section);

            // query is before the list's span
            if (i < section.Offset)
                return 0;

            // If the interval doesn't overlap
            if (!query.Overlaps(_list[i].Interval))
                return nodesBeforeNext(_list[i]);

            return _list[i].NodesBefore + 1 + countNotAfter(_list[i].Sublist, query);
        }

        private int nodesInList(Section section)
        {
            return nodesBeforeNext(_list[section.Offset + section.Length - 1]);
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
