using System;

namespace C5.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var jit = new LCListEnumeratorSorted();
            jit.StartBenchmark();
            var simple = new SimpleBenchmark();
            simple.StartBenchmark(100000);

            //var jit = new JITCheater();
            //jit.StartBenchmark();
            //var simple = new SimpleBenchmark();
            //simple.StartBenchmark(maxCollectionSize: 10000000);

            //var scalability = new SearchScalability();
            //scalability.StartBenchmark(maxCollectionSize: 10000000);
            //Console.Out.WriteLine("Done");
            //WaitForKey();
        }

        private static void WaitForKey()
        {
            Console.ReadLine();
        }
    }
}