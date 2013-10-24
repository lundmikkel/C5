using System;
using C5.intervals;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Using;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace C5.Pex
{
    [PexClass(typeof(IInterval<>))]
    [TestClass]
    public partial class PexIntervals
    {
        [PexMethod(Timeout = 10)]
//        [PexAllowedException(typeof(ContractException))]
        public bool OverlapTester([PexAssumeNotNull] IntervalBase<int> target, [PexAssumeNotNull] IntervalBase<int> y)
        {
            Console.Out.WriteLine(target);
            Console.Out.WriteLine(y);
            return target.Overlaps(y);
        }
        
        [PexMethod]
        // Add the type to look for when Pex can't figure it out.
        [PexUseType(typeof(IntervalBase<int>))]
        public bool Overlap2Tester(IInterval<int> x, IInterval<int> y)
        {
            var result = x.Overlaps(y);
            PexAssert.AreEqual(true,result);
            return result;
        }

    }
}
