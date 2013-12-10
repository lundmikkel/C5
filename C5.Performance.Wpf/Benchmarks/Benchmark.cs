using System;
using System.Collections.Generic;

namespace C5.Performance
{
    public class Benchmark
    {
        public string BenchmarkName { get; set; }
        public int CollectionSize { get; set; }
        public int NumberOfRuns { get; set; }
        public double MeanTime { get; set; }
        public double StandardDeviation { get; set; }

        public Benchmark()
        {}

        public Benchmark(String benchmarkName, int collectionSize, double meanTime, double standardDeviation, int numberOfRuns)
        {
            BenchmarkName = benchmarkName;
            CollectionSize = collectionSize;
            NumberOfRuns = numberOfRuns;
            MeanTime = meanTime;
            StandardDeviation = standardDeviation;
        }
    }
}