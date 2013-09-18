using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C5.Tests.intervals;
using C5.intervals;

namespace C5.Performance
{
    class LCListEnumeratorSorted : Benchmarkable
    {
        private LayeredContainmentList2<int> _lclist;

        protected override string BenchMarkName()
        {
            return "LCList Enumerable Sorted";
        }

        protected override void CollectionSetup()
        {
            _lclist = new LayeredContainmentList2<int>(BenchmarkTestCases.DataSetA(CollectionSize));
        }

        protected override void Setup()
        {
        }

        protected override double Call(int i)
        {
            var sum = 0.0;
            foreach (var interval in _lclist.Sorted)
                sum += interval.Low;
            return sum;
        }
    }
}
