using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C5.intervals
{
    class NestedContainmentListArticle<I, T> : CollectionValueBase<I>, IIntervalCollection<I, T>
        where I : IInterval<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly Item[] _list;
        private readonly Sublist[] _header;
        private readonly int _count;

        #endregion

        #region Inner Classes

        private class Item
        {
            public Item(I interval)
            {
                Interval = interval;
            }

            public I Interval { get; set; }
            public int Sublist { get; set; }

            public override string ToString()
            {
                return String.Format("{0} / {1}", Interval, Sublist);
            }
        }

        private class Sublist
        {
            public int Start { get; set; }
            public int Length { get; set; }

            public Sublist()
            {

            }

            public Sublist(int start, int length)
            {
                Start = start;
                Length = length;
            }

            public override string ToString()
            {
                return String.Format("{0} / {1}", Start, Length);
            }
        }

        #endregion

        #region Constructor

        public NestedContainmentListArticle(IEnumerable<I> intervals)
        {
            var intervalArray = intervals as I[] ?? intervals.ToArray();

            if (!intervalArray.Any())
                return;

            _count = intervalArray.Length;

            // Sort
            Sorting.IntroSort(intervalArray, 0, _count, IntervalExtensions.CreateComparer<I, T>());

            _list = new Item[_count];
            _header = new Sublist[_count + 1];
            for (var i = 0; i < _count; i++)
            {
                _list[i] = new Item(intervalArray[i]);
                _header[i] = new Sublist();
            }

            // Count sublists
            var sublists = countSublists(ref intervalArray);

            labelSublists();

            computeSubStart(sublists);

            computeAbsolutePosition();

            var sublistComparer = ComparerFactory<Item>.CreateComparer((i, j) => i.Sublist.CompareTo(j.Sublist));
            Sorting.InsertionSort(_list, 0, _count, sublistComparer);

            sublistInvert();
        }

        private void sublistInvert()
        {
            var isub = 0;
            for (int i = 0; i < _count; i++)
            {
                if (_list[i].Sublist > isub)
                {
                    isub = _list[i].Sublist;
                    var parent = _header[isub].Start;
                    _list[parent].Sublist = isub;
                    _header[isub].Start = i;
                    _header[isub].Length = 0;
                }

                _header[isub].Length++;
                _list[i].Sublist = -1;
            }
        }

        private void computeAbsolutePosition()
        {
            for (var i = 1; i < _count; i++)
                if (_list[i].Sublist > _list[i - 1].Sublist)
                    _header[_list[i].Sublist].Start += _header[_list[i - 1].Sublist].Length;
        }

        private void computeSubStart(int sublistCount)
        {
            var total = 0;
            for (var i = 0; i < sublistCount; i++)
            {
                var tmp = _header[i].Length;
                _header[i].Length = total;
                total += tmp;
            }
        }

        private void labelSublists2()
        {
            var header = _header;
            var list = _list;

            // Index of the parent interval
            var parent = 0;
            // Index of the current interval
            var i = 1;
            // Temperary holder for isub value used when incrementing isub
            var tsub = 1;
            // The counter for sublist numbers
            var isub = 1;

            // Init the sublist of the first interval
            _header[0] = new Sublist(-1, 1);

            while (i <= _count)
            {
                if (i < _count && (_list[parent].Interval.StrictlyContains(_list[i].Interval) || isub == 0))
                {
                    // Increment the length of the sublist interval's sublist
                    _header[parent].Length++;

                    // Assign the interval to a sublist
                    _list[i].Sublist = isub;

                    // increment isub somehow
                    isub++;

                    // Update the parent to be the current interval
                    parent = i;

                    // Increment the counter so we can look at the next interval
                    i++;
                }
                else
                {
                    // Decrement sublists length, as we didn't actually add the interval
                    _header[_list[parent].Sublist].Length--;

                    // Continue on the sublist of the parent
                    isub = _list[parent].Sublist;

                    // Update parent
                    parent = _header[isub].Start;
                }
            }
        }

        private void labelSublists()
        {
            var header = _header;
            var list = _list;

            int parent = 0, i = 1, tsub = 1, isub = 1;

            _header[0] = new Sublist(-1, 1);

            while (i <= _count)
            {
                if (i < _count && (_list[parent].Interval.StrictlyContains(_list[i].Interval) || isub == 0))
                {
                    if (_header[isub].Length == 0)
                        tsub++;

                    _header[isub].Length++;
                    _list[i].Sublist = isub;
                    isub = tsub;
                    _header[isub].Start = parent;
                    parent++;
                    i++;
                }
                else
                {
                    _header[isub].Start = i - _header[_list[parent].Sublist].Length;
                    isub = _list[parent].Sublist;
                    parent = _header[isub].Start;
                }
            }
        }

        private int countSublists(ref I[] intervals)
        {
            var n = 1;

            for (var i = 1; i < _count; i++)
                if (intervals[i - 1].Contains(intervals[i]))
                    n++;

            return n;
        }


        #endregion

        #region Collection Value

        public override bool IsEmpty { get { return _count == 0; } }

        public override int Count { get { return _count; } }

        public override Speed CountSpeed { get { return Speed.Constant; } }

        public override I Choose()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            return _list.First().Interval;
        }

        #endregion

        #region Enumerable

        public override IEnumerator<I> GetEnumerator()
        {
            return _list.Select(item => item.Interval).GetEnumerator();
        }

        #endregion

        #region Interval Collection

        #region Properties

        public IInterval<T> Span { get; private set; }
        public int MaximumOverlap { get; private set; }
        public bool AllowsReferenceDuplicates { get { return true; } }

        #endregion

        #region Find Overlaps

        public IEnumerable<I> FindOverlaps(T query)
        {
            return findOverlaps(new IntervalBase<T>(query), _header[0]);
        }

        public IEnumerable<I> FindOverlaps(IInterval<T> query)
        {
            return findOverlaps(new IntervalBase<T>(query), _header[0]);
        }

        private IEnumerable<I> findOverlaps(IInterval<T> query, Sublist sublist)
        {
            var subend = sublist.Start + sublist.Length;
            var i = findFirst(query, sublist);

            while (i < subend && _list[i].Interval.Overlaps(query))
            {
                yield return _list[i].Interval;

                foreach (var interval in findOverlaps(query, _header[_list[i].Sublist]))
                    yield return interval;

                i++;
            }
        }

        private int findFirst(IInterval<T> query, Sublist sublist)
        {
            int min = sublist.Start - 1,
                max = sublist.Start + sublist.Length;

            while (max - min > 1)
            {
                var middle = min + ((max - min) >> 1);

                if (query.CompareLowHigh(_list[middle].Interval) <= 0)
                    max = middle;
                else
                    min = middle;
            }

            return max;
        }

        #endregion

        #region Find Overlap

        public bool FindOverlap(T query, ref I overlap)
        {
            return FindOverlap(new IntervalBase<T>(query), ref overlap);
        }

        public bool FindOverlap(IInterval<T> query, ref I overlap)
        {
            // Find first overlap
            var i = findFirst(query, _header[0]);

            // Check if index is in bound and if the interval overlaps the query
            var result = 0 <= i && i < _header[0].Length && _list[i].Interval.Overlaps(query);

            if (result)
                overlap = _list[i].Interval;

            return result;
        }

        #endregion

        #region Count Overlaps

        public int CountOverlaps(T query)
        {
            return FindOverlaps(query).Count();
        }

        public int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public void AddAll(IEnumerable<I> intervals)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            throw new ReadOnlyCollectionException();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            throw new ReadOnlyCollectionException();
        }

        #endregion

        #endregion
    }
}
