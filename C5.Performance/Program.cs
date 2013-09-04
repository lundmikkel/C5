using C5.intervals;
using C5.Tests.intervals;

namespace C5.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            for (var i = 0; i < 100; i++) {
                var ibs = new IntervalBinarySearchTree<int>(BenchmarkTestCases.DataSetB(30000));
            }
        }
    }
}