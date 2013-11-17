using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using C5.Tests.intervals.Generic;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    namespace DynamicIntervalTree
    {
        [TestFixture]
        public class RandomRemove
        {
            private IList<IInterval<int>> _intervals;
            private readonly Random _random = new Random(0);

            [SetUp]
            public void SetUp()
            {
                const int count = 100;
                _intervals = new ArrayList<IInterval<int>>(count);
                _intervals.AddAll(BenchmarkTestCases.DataSetB(count));
                _intervals.Shuffle(_random);
            }

            [Test]
            public void AddAndRemove()
            {
                var intervalCollection = new DynamicIntervalTree<IInterval<int>, int>();

                foreach (var interval in _intervals)
                    intervalCollection.Add(interval);

                foreach (var interval in _intervals)
                {
                    Assert.True(intervalCollection.Remove(interval));
                    Assert.False(intervalCollection.Remove(interval));
                }
            }
        }

        [TestFixture]
        public class DuplicateIntervals
        {
            private DynamicIntervalTree<IntervalBase<int>, int> intervalCollection;

            [SetUp]
            public void SetUp()
            {
                intervalCollection = new DynamicIntervalTree<IntervalBase<int>, int>();
            }

            [Test]
            public void AllSameLow()
            {
                var random = new Random(0);
                const int count = 15;

                for (var i = 0; i < count; i++)
                    intervalCollection.Add(new IntervalBase<int>(0, random.Next(10) + 1));
            }

            [Test]
            public void AddAll()
            {
                var random = new Random(0);
                const int count = 15;

                var intervals = new ArrayList<IntervalBase<int>>();
                for (var i = 0; i < count; i++)
                {
                    var interval = new IntervalBase<int>(0, random.Next(10) + 1);
                    intervals.Add(interval);
                }
                intervalCollection.AddAll(intervals);
            }
        }
    }
}
