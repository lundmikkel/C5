using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace C5.Intervals
{
    class MultiWayMergeQueue<T>
    {
        #region Fields

        private readonly Section[] _queue;
        private readonly T[] _array;
        private readonly IComparer<T> _comparer;

        private int _listCount;

        #endregion

        #region Inner Class

        [DebuggerDisplay("Section: [{First} : {Last}). Key: {Key}")]
        private struct Section
        {
            public int First;
            public int Last;
            public T Key;
        }

        #endregion

        #region Constructor

        public MultiWayMergeQueue(int capacity, T[] array, IComparer<T> comparer)
        {
            _queue = new Section[capacity + 1];
            _array = array;
            _comparer = comparer;
            _listCount = 0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if this queue is empty.
        /// </summary>
        /// <returns>True, if queue is empty.</returns>
        public bool IsEmpty
        {
            get
            {
                return _listCount == 0;
            }
        }

        public void Insert(int first, int last)
        {

            // double size of array if necessary
            if (_listCount >= _queue.Length - 1)
                throw new InvalidOperationException("Queue is full!");

            if (first < last)
                _queue[++_listCount] = new Section { First = first, Last = last, Key = _array[first] };
        }

        public T Pop()
        {
            if (IsEmpty)
                throw new NoSuchItemException();

            var key = _queue[1].Key;

            // Move pointer to subsequent interval
            _queue[1].Key = _array[++_queue[1].First];

            // If section is non-empty, swap with last item
            if (_queue[1].First >= _queue[1].Last)
            {
                swap(1, _listCount--);
                _queue[_listCount + 1] = default(Section);
            }

            sink(1);

            return key;
        }

        #endregion

        #region Helper

        private void swap(int i, int j)
        {
            var swap = _queue[i];
            _queue[i] = _queue[j];
            _queue[j] = swap;
        }

        private void swim(int k)
        {
            while (k > 1 && greater(k / 2, k))
            {
                swap(k, k / 2);
                k = k / 2;
            }
        }

        private void sink(int k)
        {
            while (2 * k <= _listCount)
            {
                int j = 2 * k;
                if (j < _listCount && greater(j, j + 1)) j++;
                if (!greater(k, j)) break;
                swap(k, j);
                k = j;
            }
        }

        private bool greater(int i, int j)
        {
            return _comparer.Compare(_queue[i].Key, _queue[j].Key) > 0;
        }

        #endregion
    }
}