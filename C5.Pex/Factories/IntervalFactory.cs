using System.Collections;
using C5.intervals;
using Microsoft.Pex.Framework;

namespace C5.Pex.Factories
{
    /// <summary>A factory for C5.intervals</summary>
    public static partial class IntervalFactory
    {
        /// <summary>A factory for C5.intervals.IntervalBase`1[System.Int32] instances</summary>
        [PexFactoryMethod(typeof(IntervalBase<int>))]
        public static IntervalBase<int> Create(int low, int high, bool lowIncluded, bool highIncluded)
        {
            return new IntervalBase<int>(low, high, lowIncluded, highIncluded);
        }
//
//        [PexFactoryMethod(typeof(IntervalBinarySearchTreeAvl<IInterval<int>, int>))]
//        public static IntervalBinarySearchTreeAvl<IntervalBase<int>, int> Create(int numberOfIntervals)
//        {
//            //PexAssume.IsTrue(intervals.Length > 5);
//            var collection = new ArrayList<IntervalBase<int>>(numberOfIntervals);
//            for (int i = 0; i < numberOfIntervals; i++)
//            {
//                collection.Add(PexChoose.Value<IntervalBase<int>>("random interval"));
//            }
//            return new IntervalBinarySearchTreeAvl<IntervalBase<int>, int>(collection);
//        } 
    }
}
