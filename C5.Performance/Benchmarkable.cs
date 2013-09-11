using System;

namespace C5.Performance {
    public abstract class Benchmarkable
    {
        public abstract void StartBenchmark(int minCollectionSize = 100, int maxCollectionSize = 50000);

        protected abstract void Setup();

        protected abstract double Call(int i);

        protected abstract String Info();

        protected void SystemInfo() {
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

        protected double Benchmark(String msg, String info, Func<int, double> f, Action setup = null, int repeats = 10, double maxExecutionTimeInSeconds = 0.25) {
            var count = 1;
            var totalCount = count;
            double dummy = 0.0, runningTimeInSeconds = 0.0, elapsedTime = 0.0, elapsedSquaredTime = 0.0;
            do {
                // Step up the count by a factor
                count *= 10;
                elapsedTime = elapsedSquaredTime = 0.0;
                for (var j = 0; j < repeats; j++) {
                    var t = new Timer();
                    for (var i = 0; i < count; i++) {
                        // Pause timer during setup before profiling
                        t.Pause();
                        if (setup != null)
                            setup();
                        t.Play();
                        dummy += f(i);
                    }
                    runningTimeInSeconds = t.Check();
                    // Convert runningTime to nanoseconds and divide by the number of count
                    var time = runningTimeInSeconds * 1e9 / count;
                    elapsedTime += time;
                    elapsedSquaredTime += time * time;
                }
                totalCount += count;
            } while (runningTimeInSeconds < maxExecutionTimeInSeconds && count < Int32.MaxValue / 10);
            double meanTime = elapsedTime / repeats, standardDeviation = Math.Sqrt(elapsedSquaredTime / repeats - meanTime * meanTime);
            const int maxMsgLength = 17;
            var trimmedName = msg.Substring(0, Math.Min(msg.Length, maxMsgLength));
            Console.WriteLine("{0,-18} {1} size {2,15:F1}ns {3,9:F1}+/- {4,10:D} runs", trimmedName, info, meanTime, standardDeviation, totalCount);
            return dummy;
        }
    }
}
