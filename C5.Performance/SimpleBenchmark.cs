using System;

namespace C5.Performance {
    class SimpleBenchmark : Benchmarkable
    {
        private int[] intArray;
        
        public static int BinarySearch(int x, int[] arr) {
            int n = arr.Length, a = 0, b = n - 1;
            while (a <= b) {
                int i = (a + b) / 2;
                if (x < arr[i])
                    b = i - 1;
                else if (arr[i] < x)
                    a = i + 1;
                else
                    return i;
            }
            return -1;
        }

        protected override void CollectionSetup()
        {
            var rnd = new Random();
            intArray = new int[size];
            for (var i = 0; i < size; i++)
                intArray[i] = rnd.Next();
        }

        protected override void Setup(){}

        protected override double Call(int i)
        {
            return BinarySearch(i, intArray);
        }

        protected override string Info()
        {
            return String.Format("{0,8:D} size", size);
        }

        protected override string BenchMarkName()
        {
            return "BinarySearch";
        }
    }
}
