using System;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    using IntervalCollection = IIntervalCollection<MyInterval, int>;

    [TestFixture]
    abstract class IIntervalCollectionTester
    {
        protected abstract Type GetCollectionType();

        // Override to add additional parameters to constructors
        protected object[] AdditionalParameters()
        {
            return new object[0];
        }

        protected IntervalCollection Factory(params MyInterval[] intervals)
        {
            var type = GetCollectionType();
            Type[] typeArgs = { typeof(MyInterval), typeof(int) };
            var genericType = type.MakeGenericType(typeArgs);

            var additionalParameters = AdditionalParameters();
            var parameters = new object[1 + additionalParameters.Length];
            parameters[0] = intervals;
            for (var i = 0; i < additionalParameters.Length; i++)
                parameters[i + 1] = additionalParameters[i];

            return (IntervalCollection)Activator.CreateInstance(genericType, parameters);
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

    class DynamicIntervalTreeTesterReferenceDuplicatesFalse : IIntervalCollectionTester
    {
        protected override Type GetCollectionType()
        {
            return typeof(DynamicIntervalTree<,>);
        }

        // DIT's standard behavior where we set the ReferenceDuplicates to false
        protected object[] AdditionalParameters()
        {
            return new object[] { false };
        }
    }

    class DynamicIntervalTreeTesterReferenceDuplicatesTrue : IIntervalCollectionTester
    {
        protected override Type GetCollectionType()
        {
            return typeof(DynamicIntervalTree<,>);
        }
        
        // DIT where we set the ReferenceDuplicates to true
        protected object[] AdditionalParameters()
        {
            return new object[] { true };
        }
    }

    class IntervalBinarySearchTreeTester : IIntervalCollectionTester
    {
        protected override Type GetCollectionType()
        {
            return typeof(IntervalBinarySearchTreeAvl<,>);
        }
    }

    internal class MyInterval : IntervalBase<int>
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
    }
}
