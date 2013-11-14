using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    using IntervalCollection = IIntervalCollection<MyInterval, int>;

    [TestFixture]
    abstract class IIntervalCollectionTester
    {
        protected abstract Type GetCollectionType();

        protected IntervalCollection Factory(params MyInterval[] intervals)
        {
            var type = GetCollectionType();
            Type[] typeArgs = { typeof(MyInterval), typeof(int) };
            var genericType = type.MakeGenericType(typeArgs);
            return (IntervalCollection) Activator.CreateInstance(genericType, new object[] { intervals });
        }


        [Test]
        public void Test()
        {
            var collection = Factory(
                new MyInterval(1, 2),
                new MyInterval(2, 3)
            );

            Assert.AreEqual(collection.Count, 2);
        }
    }

    class DynamicIntervalTreeTester : IIntervalCollectionTester
    {
        protected override Type GetCollectionType()
        {
            return typeof(DynamicIntervalTree<,>);
        }
    }

    class IntervalBinarySearchTreeTester : IIntervalCollectionTester
    {
        protected override Type GetCollectionType()
        {
            return typeof(IntervalBinarySearchTreeAvl<,>);
        }
    }

    internal class MyInterval : IntervalBase<int>, IInterval<int>
    {
        public MyInterval(int query)
            : base(query)
        {
        }

        public MyInterval(int low, int high, bool lowIncluded = true, bool highIncluded = false)
            : base(low, high, lowIncluded, highIncluded)
        {
        }

        public MyInterval(int low, int high, IntervalType type)
            : base(low, high, type)
        {
        }

        public MyInterval(IInterval<int> i)
            : base(i)
        {
        }

        public MyInterval(IInterval<int> low, IInterval<int> high)
            : base(low, high)
        {
        }

        public int Low { get; private set; }
        public int High { get; private set; }
        public bool LowIncluded { get; private set; }
        public bool HighIncluded { get; private set; }
    }
}
