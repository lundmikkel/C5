using System;

namespace C5.Performance.Wpf.Benchmarks
{
    // Simple microbenchmark setups
    // sestoft@itu.dk * 2013-06-02, 2013-08-29
    public class SearchAndSort
    {
        private static readonly Random Rnd = new Random();

        public static int LinearSearch(int x, int[] arr)
        {
            int n = arr.Length, i = 0;
            while (i < n)
                if (arr[i] != x)
                    i++;
                else
                    return i;
            return -1;
        }

        public static int BinarySearch(int x, int[] arr)
        {
            int n = arr.Length, a = 0, b = n - 1;
            while (a <= b)
            {
                var i = (a + b)/2;
                if (x < arr[i])
                    b = i - 1;
                else if (arr[i] < x)
                    a = i + 1;
                else
                    return i;
            }
            return -1;
        }

        // Utility for sorting
        private static void Swap(int[] arr, int s, int t)
        {
            var tmp = arr[s];
            arr[s] = arr[t];
            arr[t] = tmp;
        }

        // Selection sort
        public static void Selsort(int[] arr)
        {
            var n = arr.Length;
            for (var i = 0; i < n; i++)
            {
                var least = i;
                for (var j = i + 1; j < n; j++)
                    if (arr[j] < arr[least])
                        least = j;
                Swap(arr, i, least);
            }
        }

        // Quicksort
        private static void Qsort(int[] arr, int a, int b)
        {
            // sort arr[a..b]
            if (a < b)
            {
                int i = a, j = b;
                var x = arr[(i + j)/2];
                do
                {
                    while (arr[i] < x) i++;
                    while (arr[j] > x) j--;
                    if (i <= j)
                    {
                        Swap(arr, i, j);
                        i++;
                        j--;
                    }
                } while (i <= j);
                Qsort(arr, a, j);
                Qsort(arr, i, b);
            }
        }

        public static void Quicksort(int[] arr)
        {
            Qsort(arr, 0, arr.Length - 1);
        }

        // Heapsort
        private static void Heapify(int[] arr, int i, int k)
        {
            // heapify node arr[i] in the tree arr[0..k]
            var j = 2*i + 1;
            if (j <= k)
            {
                if (j + 1 <= k && arr[j] < arr[j + 1])
                    j++;
                if (arr[i] < arr[j])
                {
                    Swap(arr, i, j);
                    Heapify(arr, j, k);
                }
            }
        }

        public static void Heapsort(int[] arr)
        {
            var n = arr.Length;
            for (var m = n/2; m >= 0; m--)
                Heapify(arr, m, n - 1);
            for (var m = n - 1; m >= 1; m--)
            {
                Swap(arr, 0, m);
                Heapify(arr, 0, m - 1);
            }
        }

        public static int[] FillIntArray(int n)
        {
            var arr = new int[n];
            for (var i = 0; i < n; i++)
                arr[i] = i;
            return arr;
        }

        public static int[] FillIntArrayRandomly(int n, int minValue = Int32.MinValue, int maxValue = Int32.MaxValue) {
            var arr = new int[n];
            for (var i = 0; i < n; i++)
                arr[i] = Rnd.Next(minValue, maxValue);
            return arr;
        }

        public static void Shuffle(int[] arr)
        {
            for (var i = arr.Length - 1; i > 0; i--)
                Swap(arr, i, Rnd.Next(i + 1));
        }
    }
}