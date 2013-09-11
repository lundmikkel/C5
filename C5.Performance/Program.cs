using System;

namespace C5.Performance
{
    public class Program
    {
        public static void Main(string[] args) {
            var jit = new JITCheater();
            jit.StartBenchmark();
            Console.Out.WriteLine("Done");
            WaitForKey();
        }

        private static void WaitForKey() {
            Console.ReadLine();
        }
    }
}