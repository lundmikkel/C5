using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.intervaled
{
    class LayeredContainmentList<T> : CollectionValueBase<IInterval<T>>, IStaticIntervaled<T> where T : IComparable<T>
    {
        private readonly int _count;
        private readonly Node[][] _lists;
        private readonly IInterval<T> _span;

        #region Node nested classes

        struct Node
        {
            internal IInterval<T> Interval { get; private set; }
            internal int Containment { get; private set; }

            internal Node(IInterval<T> interval, int nextList)
                : this()
            {
                Interval = interval;
                Containment = nextList;
            }

            public override string ToString()
            {
                return Interval != null ? String.Format("{0} - {1}", Interval, Containment) : "(empty)";
            }
        }

        #endregion

        #region Constructor

        public LayeredContainmentList(IEnumerable<IInterval<T>> intervals)
        {
            // Make intervals to array to allow fast sorting and counting
            var intervalArray = intervals as IInterval<T>[] ?? intervals.ToArray();

            // Only do the work if we have something to work with
            if (!intervalArray.IsEmpty())
            {
                // Count intervals so we can use it later on
                _count = intervalArray.Count();

                // Sort intervals
                var comparer = ComparerFactory<IInterval<T>>.CreateComparer(IntervalExtensions.CompareTo);
                Sorting.IntroSort(intervalArray, 0, _count, comparer);

                // Analyze intervals to figure out how many layered containment lists we need, and their sizes
                var listCounts = analyzeContainment(intervalArray);

                // Create the list that contains the containment lists
                _lists = new Node[listCounts.Count()][];
                // Create each containment list
                for (var i = 0; i < listCounts.Count(); i++)
                    _lists[i] = new Node[listCounts[i]];

                // Put intervals in the arrays
                createLists(intervalArray);

                // Save the span once
                _span = new IntervalBase<T>(_lists.First().First().Interval, _lists.First().Last().Interval);
            }

            // TODO: Should we do anything if the collection is empty?
        }

        private void createLists(IEnumerable<IInterval<T>> intervalArray)
        {
            // Keep track of the next index for each containment list
            // We keep an extra space to avoid checking on the most contained layer
            var indexes = new int[_lists.Count() + 1];

            // Use a stack to keep track of current containment
            IStack<IInterval<T>> stack = new LinkedList<IInterval<T>>();

            foreach (var interval in intervalArray)
            {
                // Track containment
                while (!stack.IsEmpty)
                {
                    var popped = stack.Pop();

                    // If the interval is contained in the top of the stack, leave it...
                    // if (popped.Contains(interval)) // We just need to compare the highs
                    if (interval.CompareHigh(popped) < 0)
                    {
                        //...by pushing it back
                        stack.Push(popped);
                        break;
                    }
                }
                stack.Push(interval);


                var list = stack.Count - 1;
                var i = indexes[list]++;
                var j = indexes[list + 1];

                _lists[list][i] = new Node(interval, j);
            }
        }

        /// <summary>
        /// Analyze the intervals to find out how many layers, we need and how big each is
        /// </summary>
        /// <param name="intervalArray">Sorted intervals that should be analyzed</param>
        private static int[] analyzeContainment(IEnumerable<IInterval<T>> intervalArray)
        {
            // Use a stack to keep track of current containment
            IStack<IInterval<T>> stack = new LinkedList<IInterval<T>>();
            var listCounts = new ArrayList<int>();

            foreach (var interval in intervalArray)
            {
                // Track containment
                while (!stack.IsEmpty)
                {
                    var popped = stack.Pop();

                    // If the interval is contained in the top of the stack, leave it...
                    // if (popped.Contains(interval)) // We just need to compare the highs
                    if (interval.CompareHigh(popped) < 0)
                    {
                        //...by pushing it back
                        stack.Push(popped);
                        break;
                    }
                }
                stack.Push(interval);

                if (listCounts.Count < stack.Count)
                    listCounts.Add(0);

                // Increament the count for the containment list on the right layer
                listCounts[stack.Count - 1]++;
            }

            return listCounts.ToArray();
        }

        #endregion

        #region CollectionValue

        public override bool IsEmpty
        {
            get { return Count == 0; }
        }

        public override int Count { get { return _count; } }

        public override Speed CountSpeed
        {
            get { return Speed.Constant; }
        }

        public override IInterval<T> Choose()
        {
            if (Count == 0)
                throw new NoSuchItemException();

            return _lists.First().First().Interval;
        }

        #endregion

        public int OverlapCount(IInterval<T> query)
        {
            // No overlaps
            if (query == null)
                return 0;

            var before = 0;

            var low = 0;
            for (var i = 0; i < _lists.Count(); i++)
            {
                before += low;

                if (low < _lists[i].Count())
                {
                    var node = _lists[i][low];
                    low = node.Containment;
                }
            }

            var notAfter = 0;

            return notAfter - before;
        }

        private int searchLowInHighs(Node[] list, int lower, int upper, IInterval<T> query)
        {
            if (query == null || list.Count() == 0)
                return -1;

            int min = lower, max = upper;

            while (min <= max)
            {
                var middle = (max + min) / 2;

                if (query.Overlaps(list[middle].Interval))
                {
                    // Only if the next interval to the overlapping interval does not overlap, we have found the right one
                    if (middle == upper || !query.Overlaps(list[middle + 1].Interval))
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

        public override IEnumerator<IInterval<T>> GetEnumerator()
        {
            return getEnumerator(0, 0, _lists[0].Count());
        }

        private IEnumerator<IInterval<T>> getEnumerator(int level, int start, int end)
        {
            while (start < end)
            {
                var node = _lists[level][start];

                yield return node.Interval;

                // Check if we are at the last node
                if (start + 1 == end)
                {
                    // Make sure there is another level
                    if (level + 1 < _lists.Count())
                    {
                        var child = getEnumerator(level + 1, node.Containment, _lists[level + 1].Count());

                        while (child.MoveNext())
                            yield return child.Current;
                    }
                }
                else if (node.Containment < _lists[level][start + 1].Containment)
                {
                    var child = getEnumerator(level + 1, node.Containment, _lists[level][start + 1].Containment);

                    while (child.MoveNext())
                        yield return child.Current;
                }

                start++;
            }
        }

        public IInterval<T> Span
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("An empty collection has no span");

                return _span;
            }
        }

        public IEnumerable<IInterval<T>> FindOverlaps(T query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IInterval<T>> FindOverlaps(IInterval<T> query)
        {
            throw new NotImplementedException();
        }

        public bool OverlapExists(IInterval<T> query)
        {
            throw new NotImplementedException();
        }
    }
}
