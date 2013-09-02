using C5.intervals;
using C5.Tests.intervals;

namespace C5.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new IntervalBinarySearchTree<int>(BenchmarkTestCases.DataSetB(10000));
        }
    }
}