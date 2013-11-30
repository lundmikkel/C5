using System;

namespace C5.Performance.Wpf.Benchmarks
{
    public class SimpleBenchmark : Benchmarkable
    {
        private int[] _intArray;

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

        public override void CollectionSetup()
        {
            // Setup an int array with sorted integers from 0 to CollectionSize
            _intArray = SearchAndSort.FillIntArray(CollectionSize);

            /*
             * Setup an items array with things to look for. Fill in random numbers from 0 to twice the value of the collection size.
             * This should make roughly half the searched succesful.
             */
            ItemsArray = SearchAndSort.FillIntArrayRandomly(CollectionSize, 0, CollectionSize*2);
        }

        public override void Setup()
        {
        }

        public override double Call(int i)
        {
            return BinarySearch(i, _intArray);
        }

        public override string BenchMarkName()
        {
            return "TrainSearch";
        }
    }
}