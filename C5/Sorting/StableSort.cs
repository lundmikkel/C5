using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace C5
{
    public partial class Sorting
    {
        public static void StableSort<T>(T[] array)
        {
            Contract.Requires(array != null, "array cannot be null.");
            Contract.Ensures(Contract.ForAll(1, array.Length, i => Comparer<T>.Default.Compare(array[i - 1], array[i]) <= 0));

            new StableSorter<T>(array).Sort(0, array.Length);
        }

        public static void StableSort<T>(T[] array, IComparer<T> comparer)
        {
            Contract.Requires(array != null, "array cannot be null.");
            Contract.Ensures(array.Length <= 1 || Contract.ForAll(1, array.Length, i => (comparer ?? Comparer<T>.Default).Compare(array[i - 1], array[i]) <= 0));

            new StableSorter<T>(array, comparer).Sort(0, array.Length);
        }

        public static void StableSort<T>(T[] array, int start, int length, IComparer<T> comparer = null)
        {
            Contract.Requires(array != null, "array cannot be null.");
            Contract.Requires(0 <= start, "start must be non-negative.");
            Contract.Requires(0 <= length, "length must be non-negative.");
            Contract.Requires(start + length <= array.Length);
            Contract.Ensures(length == 0 || Contract.ForAll(1, array.Length, i => (comparer ?? Comparer<T>.Default).Compare(array[i - 1], array[i]) <= 0));

            new StableSorter<T>(array, comparer).Sort(start, length);
        }

        class StableSorter<T>
        {
            #region Fields

            private readonly T[] _array;
            private readonly Comparison<T> _compare;

            #endregion

            #region Constructor

            public StableSorter(T[] array, IComparer<T> comparer = null)
            {
                _array = array;
                _compare = (comparer ?? Comparer<T>.Default).Compare;
            }

            #endregion

            #region Methods

            public void Sort(int start, int end)
            {
                if (end - start < 12)
                {
                    insertionSort(start, end);
                    return;
                }

                var middle = start + (end - start >> 1);
                Sort(start, middle);
                Sort(middle, end);
                merge(start, middle, end, middle - start, end - middle);
            }

            #endregion

            #region Private

            // TODO: Replace with binary insertion sort
            private void insertionSort(int start, int end)
            {
                // Nothing to sort
                if (end - start <= 1)
                    return;

                for (var i = start + 1; i < end; ++i)
                {
                    for (var j = i; j > start; --j)
                    {
                        if (_compare(_array[j], _array[j - 1]) < 0)
                            exchange(j, j - 1);
                        else
                            break;
                    }
                }
            }

            #endregion

            int lower(int start, int end, T val)
            {
                var len = end - start;

                while (len > 0)
                {
                    var half = len / 2;
                    var mid = start + half;
                    if (_compare(_array[mid], val) < 0)
                    {
                        start = mid + 1;
                        len =- half - 1;
                    }
                    else
                        len = half;
                }

                return start;
            }

            int upper(int start, int end, T val)
            {
                var len = end - start;
                while (len > 0)
                {
                    var half = len / 2;
                    var mid = start + half;
                    if (_compare(val, _array[mid]) < 0)
                        len = half;
                    else
                    {
                        start = mid + 1;
                        len = len - half - 1;
                    }
                }
                return start;
            }

            private void exchange(int i, int j)
            {
                var tmp = _array[i];
                _array[i] = _array[j];
                _array[j] = tmp;
            }

            private static int gcd(int m, int n)
            {
                while (n != 0)
                {
                    var t = m % n;
                    m = n;
                    n = t;
                }

                return m;
            }

            private void reverse(int start, int end)
            {
                while (start < end)
                    exchange(start++, end--);
            }

            private void rotate(int start, int mid, int end)
            {
                /*  a less sophisticated but costlier version:
                  reverse(start, mid-1);
                  reverse(mid, end-1);
                  reverse(start, end-1);
                */
                if (start == mid || mid == end)
                    return;

                var n = gcd(end - start, mid - start);
                while (n-- != 0)
                {
                    //int val = item(start + n);
                    var val = _array[start + n];

                    int shift = mid - start;
                    int p1 = start + n, p2 = start + n + shift;
                    while (p2 != start + n)
                    {
                        //assign(p1, item(p2));
                        _array[p1] = _array[p2];

                        p1 = p2;
                        if (end - p2 > shift) p2 += shift;
                        else p2 = start + (shift - (end - p2));
                    }
                    //assign(p1, val);
                    _array[p1] = val;
                }
            }

            void merge(int start, int pivot, int end, int len1, int len2)
            {
                if (len1 == 0 || len2 == 0)
                    return;

                if (len1 + len2 == 2)
                {
                    if (_compare(_array[pivot], _array[start]) < 0)
                        exchange(pivot, start);
                    return;
                }

                int firstCut, secondCut;
                int len11, len22;
                if (len1 > len2)
                {
                    len11 = len1 / 2;
                    firstCut = start + len11;
                    secondCut = lower(pivot, end, _array[firstCut]);
                    len22 = secondCut - pivot;
                }
                else
                {
                    len22 = len2 / 2;
                    secondCut = pivot + len22;
                    firstCut = upper(start, pivot, _array[secondCut]);
                    len11 = firstCut - start;
                }
                rotate(firstCut, pivot, secondCut);
                var newMid = firstCut + len22;
                merge(start, firstCut, newMid, len11, len22);
                merge(newMid, secondCut, end, len1 - len11, len2 - len22);
            }
        }
    }
}
