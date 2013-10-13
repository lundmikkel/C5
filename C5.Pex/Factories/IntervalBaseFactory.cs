using C5.intervals;
using Microsoft.Pex.Framework;

namespace C5.Pex.Factories
{
    /// <summary>A factory for C5.intervals.IntervalBase`1[System.Int32] instances</summary>
    public static partial class IntervalBaseFactory
    {
        /// <summary>A factory for C5.intervals.IntervalBase`1[System.Int32] instances</summary>
        [PexFactoryMethod(typeof(IntervalBase<int>))]
        public static IntervalBase<int> Create(int low, int high, bool lowIncluded, bool highIncluded)
        {
            return new IntervalBase<int>(low, high, lowIncluded, highIncluded);
        }
    }
}
