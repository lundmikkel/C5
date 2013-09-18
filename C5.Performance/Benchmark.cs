using System.Collections.Generic;

namespace C5.Performance
{
    public class Benchmark
    {
        public Benchmark()
        {
            BenchmarkName = "No Name Benchmark";
            MeanTimes = new List<double>();
            CollectionSizes = new List<double>();
            StandardDeviations = new List<double>();
        }

        public string BenchmarkName { get; set; }
        public List<double> CollectionSizes { get; set; }
        public List<double> StandardDeviations { get; set; }
        public List<double> MeanTimes { get; set; }

        public int NumberOfBenchmarks { get; private set; }

        public int IncreaseNumberOfBenchmarks()
        {
            return NumberOfBenchmarks++;
        }
    }
}