using System;
using System.Collections.Generic;

namespace C5.Performance
{
    public abstract class Benchmarkable
    {
        internal int CollectionSize;
        // Normally this is Int32.MaxValue / 10 - change 10 to a higher value to have the tests execute fewer times
        private const int MaxCount = Int32.MaxValue/100000;

        protected abstract String BenchMarkName();

        public Benchmark GetBenchmark(int maxCollectionSize = 50000, int minCollectionSize = 100)
        {
            var results = new List<double[]>();
            var benchmark = new Benchmark(BenchMarkName());
            for (CollectionSize = minCollectionSize; CollectionSize < maxCollectionSize; CollectionSize *= 2)
            {
                CollectionSetup();
                results.Add(Benchmark(BenchMarkName(), Call, Setup));
                benchmark.IncreaseNumberOfBenchmarks();
            }
            foreach (var result in results)
            {
                benchmark.MeanTimes.Add(result[0]);
                benchmark.CollectionSizes.Add(result[1]);
                benchmark.StandardDeviations.Add(result[2]);
            }
            return benchmark;
        }

        protected abstract void CollectionSetup();

        protected abstract void Setup();

        protected abstract double Call(int i);

        protected void SystemInfo()
        {
            Console.WriteLine("# OS          {0}",
              Environment.OSVersion.VersionString);
            Console.WriteLine("# .NET vers.  {0}",
              Environment.Version);
            Console.WriteLine("# 64-bit OS   {0}",
              Environment.Is64BitOperatingSystem);
            Console.WriteLine("# 64-bit proc {0}",
              Environment.Is64BitProcess);
            Console.WriteLine("# CPU         {0}",
              Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"));
            Console.WriteLine("# Date        {0:s}",
              DateTime.Now);
        }

        protected double[] Benchmark(String benchmarkName, Func<int, double> f, Action setup = null, int repeats = 10, double maxExecutionTimeInSeconds = 0.25)
        {
            var count = 1;
            double dummy = 0.0, runningTimeInSeconds = 0.0, elapsedTime, elapsedSquaredTime;
            do
            {
                // Step up the count by a factor
                count *= 10;
                elapsedTime = elapsedSquaredTime = 0.0;
                for (var j = 0; j < repeats; j++)
                {
                    var t = new Timer();
                    for (var i = 0; i < count; i++)
                    {
                        if (setup != null)
                            setup();

                        t.Play();
                        dummy += f(i);
                        t.Pause();
                    }
                    runningTimeInSeconds = t.Check();
                    // Convert runningTime to nanoseconds and divide by the number of count
                    var time = runningTimeInSeconds * 1e9 / count;
                    elapsedTime += time;
                    elapsedSquaredTime += time * time;
                }
            } while (runningTimeInSeconds < maxExecutionTimeInSeconds && count < MaxCount);
            var meanTime = elapsedTime / repeats;
            var standardDeviation = Math.Sqrt(elapsedSquaredTime / repeats - meanTime * meanTime) / meanTime * 100;
            return new[]{meanTime,CollectionSize,standardDeviation};
        }
    }
}
