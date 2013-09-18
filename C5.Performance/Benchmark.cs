namespace C5.Performance {
    public class Benchmark
    {
        public Benchmark(string benchmarkName)
        {
            BenchmarkName = benchmarkName;
        }

        public double[] CollectionSizes { get; set; }
        public double[] StandardDeviations { get; set; }
        public string BenchmarkName { get; set; }
        public double[] MeanTimes { get; set; }

        public int NumberOfBenchmarks { get; private set; }

        public int IncreaseNumberOfBenchmarks()
        {
            return NumberOfBenchmarks++;
        }
    }
}
