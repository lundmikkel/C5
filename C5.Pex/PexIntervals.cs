using System;
using System.Diagnostics.Contracts;
using C5.intervals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Using;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace C5.Pex
{
//    [PexClass(typeof(IntervalExtensions))]
//    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
//    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class PexIntervals
    {
        [PexMethod]
//        [PexUseType(typeof(IntervalBase<int>))]
        public bool OverlapTester(IntervalBase<int> target, IntervalBase<int> y)
        {
            return target.Overlaps(y);
        }
        
        [PexMethod]
        [PexUseType(typeof(IntervalBase<int>))]
        public bool Overlap2Tester(IInterval<int> x, IInterval<int> y)
        {
            return x.Overlaps(y);
        }

    }
}
