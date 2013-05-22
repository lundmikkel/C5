using System;
using System.Collections;
using SCG = System.Collections.Generic;
using System.Linq;

namespace C5.intervaled
{
    public class NestedContainmentList<T> : CollectionValueBase<IInterval<T>>, IStaticIntervaled<T> where T : IComparable<T>
    {
        private readonly Node[] _list;
        private readonly IInterval<T> _span;
        private readonly Section _section;
        private readonly int _count;

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

            internal Node(IInterval<T> interval, Section section)
                : this()
            {
                Interval = interval;
                Sublist = section;
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

            for (var s = source.Offset; s < source.Offset + source.Length; s++)
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

                _list[t++] = new Node(interval, new Section(end, length));
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
                _count = intervalsArray.Count();

                Sorting.IntroSort(intervalsArray, 0, _count, ComparerFactory<IInterval<T>>.CreateComparer(IntervalExtensions.CompareTo));

                // TODO: Figure out how Orcomp does it
                //MaximumOverlap = IntervaledHelper<T>.MaximumOverlap(intervalsArray);

                var totalSection = new Section(0, intervalsArray.Count());
                _list = new Node[totalSection.Length];

                // Build nested containment list recursively and save the upper-most list in the class
                _section = new Section(0, createList(intervalsArray, totalSection, totalSection));

                // Save span to allow for constant speeds on later requests
                _span = new IntervalBase<T>(_list[_section.Offset].Interval, _list[_section.Length + _section.Offset - 1].Interval);
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // TODO: Test the order is still the same as when sorted with IntervalComparer. This should be that case!

        private SCG.IEnumerator<IInterval<T>> getEnumerator(Section section)
        {
            // Just for good measures
            if (_list == null || section.Length == 0)
                yield break;

            for (var i = section.Offset; i < section.Offset + section.Length; i++)
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


        /// <summary>
        /// Create an enumerator, enumerating the intervals in sorted order - sorted on low endpoint with shortest intervals first
        /// </summary>
        /// <returns>Enumerator</returns>
        // TODO: Test the order is still the same as when sorted with IntervalComparer. This should be that case!
        public override SCG.IEnumerator<IInterval<T>> GetEnumerator()
        {
            return getEnumerator(_section);
        }

        public bool Show(System.Text.StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region ICollectionValue

        public override bool IsEmpty { get { return Count == 0; } }
        public override int Count { get { return _count; } }
        public override Speed CountSpeed { get { return Speed.Constant; } }

        public override IInterval<T> Choose()
        {
            if (Count > 0)
                return _list[_section.Offset].Interval;

            throw new NoSuchItemException();
        }

        #endregion

        #region IIntervaled

        public IInterval<T> Span
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("An empty collection has no span");

                return _span;
            }
        }

        public SCG.IEnumerable<IInterval<T>> FindOverlaps(T query)
        {
            if (ReferenceEquals(query, null))
                return Enumerable.Empty<IInterval<T>>();

            return findOverlap(_section, new IntervalBase<T>(query));
        }

        private SCG.IEnumerable<IInterval<T>> findOverlap(Section section, IInterval<T> query)
        {
            if (_list == null || section.Length == 0)
                yield break;

            // Find first overlapping interval
            var first = findFirst(section, query);

            // If index is out of bound, or interval doesn't overlap, we can just stop our search
            if (first < section.Offset || section.Offset + section.Length - 1 < first || !_list[first].Interval.Overlaps(query))
                yield break;

            var last = searchLowInHighs(section, query);

            while (first <= last)
            {
                var node = _list[first++];

                yield return node.Interval;

                if (node.Sublist.Length > 0)
                    // If the interval is contained in the query, all intervals in the sublist must overlap the query
                    foreach (var interval in findOverlap(node.Sublist, query))
                        yield return interval;
            }
        }

        /// <summary>
        /// Get the index of the first node with an interval the overlaps the query
        /// </summary>
        private int findFirst(Section section, IInterval<T> query)
        {
            if (query == null || section.Length == 0)
                return -1;

            int min = section.Offset - 1, max = section.Offset + section.Length;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = _list[middle].Interval;

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
        /// Get the index of the last node with an interval the overlaps the query
        /// </summary>
        private int searchLowInHighs(Section section, IInterval<T> query)
        {
            if (query == null || section.Length == 0)
                return -1;

            int min = section.Offset - 1, max = section.Offset + section.Length;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1); // Shift one is the same as dividing by 2

                var interval = _list[middle].Interval;

                var compare = interval.Low.CompareTo(query.High);

                if (compare < 0 || compare == 0 && interval.LowIncluded && query.HighIncluded)
                    min = middle;
                else
                    max = middle;
            }

            return min;
        }

        public SCG.IEnumerable<IInterval<T>> FindOverlaps(IInterval<T> query)
        {
            if (query == null)
                return Enumerable.Empty<IInterval<T>>();

            return findOverlap(_section, query);
        }

        public bool OverlapExists(IInterval<T> query)
        {
            // Check if query overlaps the collection at all
            if (query == null || _list == null || !query.Overlaps(Span))
                return false;

            // Find first overlap
            var i = findFirst(_section, query);

            // Check if index is in bound and if the interval overlaps the query
            return _section.Offset <= i && i < _section.Offset + _section.Length && _list[i].Interval.Overlaps(query);
        }

        public int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }


        #endregion
    }
}
