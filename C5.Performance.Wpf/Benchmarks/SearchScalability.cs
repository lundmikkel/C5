using System;

namespace C5.Performance.Wpf.Benchmarks
{
    internal class SearchScalability : Benchmarkable
    {
        private int n;
        private int[] intArray, items;

        public override string BenchMarkName()
        {
            return "binary_search_success";
        }

        public override void CollectionSetup()
        {
            intArray = SearchAndSort.FillIntArray(CollectionSize); // sorted [0,1,...]
            items = SearchAndSort.FillIntArray(CollectionSize);
            n = CollectionSize;
            SearchAndSort.Shuffle(items);
        }

        public override void Setup(){}

        public override double Call(int i)
        {
            var successItem = items[i%n];
            return SearchAndSort.BinarySearch(successItem, intArray);
        }

        public class SearchAndSort
        {
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
                    int i = (a + b)/2;
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
                int tmp = arr[s];
                arr[s] = arr[t];
                arr[t] = tmp;
            }

            // Selection sort
            public static void Selsort(int[] arr)
            {
                int n = arr.Length;
                for (int i = 0; i < n; i++)
                {
                    int least = i;
                    for (int j = i + 1; j < n; j++)
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
                    int x = arr[(i + j)/2];
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
                int j = 2*i + 1;
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
                int n = arr.Length;
                for (int m = n/2; m >= 0; m--)
                    Heapify(arr, m, n - 1);
                for (int m = n - 1; m >= 1; m--)
                {
                    Swap(arr, 0, m);
                    Heapify(arr, 0, m - 1);
                }
            }

            public static int[] FillIntArray(int n)
            {
                int[] arr = new int[n];
                for (int i = 0; i < n; i++)
                    arr[i] = i;
                return arr;
            }

            private static readonly Random rnd = new Random();

            public static void Shuffle(int[] arr)
            {
                for (int i = arr.Length - 1; i > 0; i--)
                    Swap(arr, i, rnd.Next(i + 1));
            }
        }
    }
}
