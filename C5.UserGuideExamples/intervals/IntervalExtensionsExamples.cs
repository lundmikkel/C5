using System;
using C5.intervals;

namespace C5.UserGuideExamples.intervals
{
    public class IntervalExtensionsExamples
    {
        public static void Main(string[] args)
        {
            var x = new IntervalBase<int>(1, 5, IntervalType.LowIncluded);
            var y = new IntervalBase<int>(2, 3, IntervalType.Closed);
            var z = new IntervalBase<int>(3, 5, IntervalType.Open);

            Console.Out.WriteLine("x.Overlaps(y): {0}", x.Overlaps(y)); // true
            Console.Out.WriteLine("y.Overlaps(z): {0}", y.Overlaps(z)); // false

            Console.Out.WriteLine("x.Contains(y): {0}", x.Contains(y)); // true
            Console.Out.WriteLine("x.Contains(z): {0}", x.Contains(z)); // true

            Console.Out.WriteLine("x.StrictlyContains(y): {0}", x.StrictlyContains(y)); // true
            Console.Out.WriteLine("x.StrictlyContains(z): {0}", x.StrictlyContains(z)); // false

            Console.Out.WriteLine("x.CompareTo(y): {0}", x.CompareTo(y)); // -1
            Console.Out.WriteLine("y.CompareTo(x): {0}", y.CompareTo(x)); // 1

            var x2 = new IntervalBase<int>(1, 5, IntervalType.LowIncluded);
            Console.Out.WriteLine("x.IntervalEquals(y): {0}", x.IntervalEquals(y)); // false
            Console.Out.WriteLine("x.IntervalEquals(x2): {0}", x.IntervalEquals(x2)); // true

            Console.Out.WriteLine("x.GetHashCode(): {0}", x.GetHashCode()); // 15730764
            Console.Out.WriteLine("y.GetHashCode(): {0}", y.GetHashCode()); // 15760494

            Console.Out.WriteLine("x.ToIntervalString(): {0}", x.ToString()); // [1:5)
            Console.Out.WriteLine("x.ToIntervalString(): {0}", y.ToString()); // [2:3]
            Console.Out.WriteLine("x.ToIntervalString(): {0}", z.ToString()); // (3:5)

            Console.Read();
        }
    }
}
