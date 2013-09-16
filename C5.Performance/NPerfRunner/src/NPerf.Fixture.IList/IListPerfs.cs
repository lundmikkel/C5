namespace NPerf.Fixture.IList
{
    using System;
    using System.Collections.Generic;
    using NPerf.Framework;

    /// <summary>
    /// Performance test for implementations of the .NET Base Class Library IList&lt;int&gt; interface.
    /// <see cref="http://msdn.microsoft.com/es-es/library/5y536ey6.aspx"/>
    /// </summary>
    [PerfTester(
        typeof(IDictionary<int,int>),
        20,
        Description = "IDictionary operations benchmark",
        FeatureDescription = "Collection size")]
    public class IDicPerfs
    {
        private readonly Random random = new Random();

        /// <summary>
        /// The number of elements of the tested list for the current test execution.
        /// </summary>
        private int count;

        /// <summary>
        /// Calculates the number of elements of the tested list from the test index number.
        /// </summary>
        /// <param name="testIndex">
        /// The test index number.
        /// </param>
        /// <returns>
        /// The number of elements of the collection to be tested..
        /// </returns>
        public int CollectionCount(int testIndex)
        {
            return testIndex * 50000;
        }

        /// <summary>
        /// The value that describes each execution of a test.
        /// In this case, the size of the list.
        /// </summary>
        /// <param name="testIndex">
        /// The test index number.
        /// </param>
        /// <returns>
        /// A double value that describes an execution of the test.
        /// </returns>
        [PerfRunDescriptor]
        public double RunDescription(int testIndex)
        {
            return this.CollectionCount(testIndex);
        }

        /// <summary>
        /// Set up the list with the appropriate number of elements for a test execution.
        /// </summary>
        /// <param name="testIndex">
        /// The test index number.
        /// </param>
        /// <param name="list">
        /// The list to be tested.
        /// </param>
        [PerfSetUp]
        public void SetUp(int testIndex, IDictionary<int, int> list)
        {
            this.count = this.CollectionCount(testIndex);

            for (var i = 0; i < this.count; i++)
            {
                list.Add(i,i);
            }
        }

        /// <summary>
        /// Performance test for adding elements to a list.
        /// </summary>
        /// <param name="list">
        /// The list to be tested.
        /// </param>
        [PerfTest]
        public void Add(IDictionary<int, int> list)
        {
            list.Add(this.random.Next(), this.random.Next());
        }

        /// <summary>
        /// Clears the tested list after the execution of a test.
        /// </summary>
        /// <param name="list">
        /// The list to be tested.
        /// </param>
        [PerfTearDown]
        public void TearDown(IDictionary<int,int> list)
        {
            list.Clear();
        }
    } // End of IListPerfs class
}
