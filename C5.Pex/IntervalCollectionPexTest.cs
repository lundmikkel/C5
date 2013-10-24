using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using C5.intervals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Using;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace C5.Pex
{
    [PexClass(typeUnderTest: typeof(IIntervalCollection<IInterval<int>, int>))]
    [TestClass]
    public partial class IntervalCollectionPexTest
    {
        [PexMethod(Timeout = 10)]
        [PexUseType(typeof(IntervalBinarySearchTreeAvl<IInterval<int>, int>))]
        [PexUseType(typeof(IntervalBase<int>))]
        public bool FindOverlapPexTest([PexAssumeNotNull] IntervalBinarySearchTreeAvl<IInterval<int>, int> intervals, [PexAssumeNotNull] IInterval<int> interval)
        {
            Console.Out.WriteLine(intervals);
            Console.Out.WriteLine(interval);
            IInterval<int> overlap = null;
            return intervals.FindOverlap(interval,ref overlap);
        }
    }
}
