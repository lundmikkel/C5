using System;

namespace C5.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var jit = new LCListEnumeratorSorted();
            jit.GetBenchmark();
            var simple = new SimpleBenchmark();
            simple.GetBenchmark(100000);

            //var jit = new JITCheater();
            //jit.GetBenchmark();
            //var simple = new SimpleBenchmark();
            //simple.GetBenchmark(maxCollectionSize: 10000000);

            //var scalability = new SearchScalability();
            //scalability.GetBenchmark(maxCollectionSize: 10000000);
            //Console.Out.WriteLine("Done");
            //WaitForKey();
        }

        private static void WaitForKey()
        {
            Console.ReadLine();
        }
    }
}