namespace C5.Profiling
{
    class Program
    {
        public static void Main(string[] args)
        {
            var test = new Intervals.Tests.IntervalBinarySearchTree.RandomRemove();
            test.SetUp();
            test.AddAndRemove();
        }
    }
}
