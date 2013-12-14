namespace C5.Profiling
{
    class Program
    {
        public static void Main(string[] args)
        {
            var test = new Tests.intervals.IntervalBinarySearchTree.RandomRemove();
            test.SetUp();
            test.AddAndRemove();
        }
    }
}
