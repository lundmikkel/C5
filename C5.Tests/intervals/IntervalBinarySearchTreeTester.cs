using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using C5.intervals;
using C5.Tests.intervals.Generic;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    namespace IntervalBinarySearchTree
    {
        [TestFixture]
        public class RandomRemove
        {
            [SetUp]
            public void SetUp()
            {
                const int count = 100;
                _intervals = new ArrayList<IInterval<int>>(count);
                _intervals.AddAll(BenchmarkTestCases.DataSetB(count));
                _intervals.Shuffle(_random);
            }

            private IList<IInterval<int>> _intervals;
            private readonly Random _random = new Random(0);

            [Test]
            public void AddAndRemove()
            {
                IInterval<int> eventInterval = null;
                var intervalCollection = new IntervalBinarySearchTree<IInterval<int>, int>();
                intervalCollection.ItemsAdded += (c, args) => eventInterval = args.Item;
                intervalCollection.ItemsRemoved += (c, args) => eventInterval = args.Item;

                foreach (var interval in _intervals)
                {
                    intervalCollection.Add(interval);

                    Assert.AreSame(interval, eventInterval);
                    eventInterval = null;
                }

                foreach (var interval in _intervals)
                {
                    Assert.True(intervalCollection.Remove(interval));
                    Assert.AreSame(interval, eventInterval);
                    eventInterval = null;

                    Assert.False(intervalCollection.Remove(interval));
                    Assert.IsNull(eventInterval);
                }
            }

            [Test]
            public void Clear()
            {
                var eventThrown = false;
                var intervalCollection = new IntervalBinarySearchTree<IInterval<int>, int>(_intervals);
                intervalCollection.CollectionCleared += (sender, args) => eventThrown = true;
                intervalCollection.Clear();
                Assert.True(eventThrown);
                Assert.True(intervalCollection.IsEmpty);
            }
        }
    }
}