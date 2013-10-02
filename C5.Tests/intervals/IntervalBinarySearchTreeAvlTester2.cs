using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    [TestFixture]
    class IntervalBinarySearchTreeAvlTester2
    {
        private IntervalBinarySearchTreeAVL<IInterval<int>, int> intervals;

        [SetUp]
        public void SetUp()
        {
            intervals = new IntervalBinarySearchTreeAVL<IInterval<int>, int>();

            intervals.CollectionChanged
              += delegate
                  {
                      //Console.WriteLine("Collection changed");
                  };

            intervals.ItemsAdded += delegate(object sender, ItemCountEventArgs<IInterval<int>> eventArgs)
                {
                    var intervalCollection = (IIntervalCollection<IInterval<int>, int>) sender;
                    //Console.WriteLine(eventArgs.Item);
                };
        }

        [Test]
        public void AddElement(/*[Range(1, 10)]int i*/)
        {
            var rnd = new Random(1);
            var numbers = Enumerable.Range(0, 100).OrderBy(r => rnd.Next());

            foreach (var number in numbers/*.Take(i)*/)
                intervals.Add(new IntervalBase<int>(number));

            /*var ints = new IInterval<int>[numbers.Count()];

            foreach (var i in numbers)
                ints[i] = new IntervalBase<int>(i);

            ints.Shuffle();

            foreach (var interval in ints)
                Console.WriteLine(intervals.Add(interval) ? "Added" : "Not added");

            Console.WriteLine("Second loop");

            foreach (var interval in ints)
                Console.WriteLine(intervals.Add(interval) ? "Added" : "Not added");*/
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine(intervals.QuickGraph());

            File.WriteAllText(@"../../intervals/data/avl/avl-" + intervals.Count + ".gv", intervals.QuickGraph());
        }
    }
}
