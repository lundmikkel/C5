using System;
using System.Collections.Generic;
using C5.intervals;
using NUnit.Framework;

namespace C5.Tests.intervals
{
    using Interval = IntervalBase<int>;

    [TestFixture]
    abstract class IntervalCollectionTester
    {
        #region Meta

        protected abstract Type GetCollectionType();

        /// <summary>
        /// Override to add additional parameters to constructors.
        /// </summary>
        /// <returns>An object array of the extra parameters.</returns>
        protected object[] AdditionalParameters()
        {
            return new object[0];
        }

        protected IIntervalCollection<IntervalBase<T>, T> Factory<T>(params IntervalBase<T>[] intervals) where T : IComparable<T>
        {
            var type = GetCollectionType();
            Type[] typeArgs = { typeof(IntervalBase<T>), typeof(T) };
            var genericType = type.MakeGenericType(typeArgs);

            var additionalParameters = AdditionalParameters();
            var parameters = new object[1 + additionalParameters.Length];
            parameters[0] = intervals;
            for (var i = 0; i < additionalParameters.Length; i++)
                parameters[i + 1] = additionalParameters[i];

            return (IIntervalCollection<IntervalBase<T>, T>) Activator.CreateInstance(genericType, parameters);
        }

        class IntervalBinarySearchTreeTester : IntervalCollectionTester
        {
            protected override Type GetCollectionType()
            {
                return typeof(IntervalBinarySearchTreeAvl<,>);
            }
        }

        #endregion

        #region Test Methods

        [Test]
        public void Test()
        {
            var collection = Factory(
                new Interval(1, 2),
                new Interval(2, 3)
            );
            Assert.AreEqual(collection.Count, 2);

            var collection2 = Factory(new IntervalBase<bool>(true));
            Assert.AreEqual(collection2.Count, 1);
        }

        #region Contructors
        #endregion

        #region Properties

        #region Span

        #endregion

        #region Maximum Overlap
        #endregion

        #region Allows Reference Duplicates
        #endregion

        #endregion

        #region Find Overlaps

        #region Stabbing
        #endregion

        #region Range
        #endregion

        #endregion

        #region Find Overlap

        #region Stabbing
        #endregion

        #region Range
        #endregion

        #endregion

        #region Count Overlaps

        #region Stabbing
        #endregion

        #region Range
        #endregion

        #endregion

        #region Extensible

        #region Is Read Only
        #endregion

        #region Add

        #region Events
        #endregion

        #endregion

        #region Add All

        #region Events
        #endregion

        #endregion

        #region Remove

        #region Events
        #endregion

        #endregion

        #region Clear

        #region Events
        #endregion

        #endregion

        #endregion

        #endregion
    }
}
