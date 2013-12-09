using System;

namespace C5.Performance.Wpf.Benchmarks
{
    public abstract class Benchmarkable
    {
        public int CollectionSize;
        internal int[] ItemsArray;

        public abstract String BenchMarkName();

        // Prepare the collections used for the benchmark
        public abstract void CollectionSetup();

        // Do some setup before each benchmark run
        public abstract void Setup();

        public abstract double Call(int i);

        public Benchmark Benchmark(int maxCount, int repeats, double maxExecutionTimeInSeconds, Benchmarker caller, bool runWarmup = true)
        {
            CollectionSetup();
            var count = 1;
            double dummy = 0.0, runningTimeInSeconds = 0.0, elapsedTime, elapsedSquaredTime;
            if (runWarmup)
            {
                const int warmups = 1000;
                // Warmup the JIT-compiler
                var warmupTimer = new Timer();
                warmupTimer.Play();
                var i1 = 0;
                while (i1 < warmups && warmupTimer.Check() < 3) // Don't warmup for more than 3 seconds
                {
                    Setup();
                    caller.UpdateRunningLabel("Warmup run " + i1);
                    dummy += Call(ItemsArray[i1 % CollectionSize]);
                    i1++;
                }
                warmupTimer.Pause();
            }
            dummy = 0.0;
            do
            {
                // Step up the count by a factor
                count *= 10;
                elapsedTime = elapsedSquaredTime = 0.0;
                for (var j = 0; j < repeats; j++)
                {
                    caller.UpdateRunningLabel(String.Format("Benchmarking {0} calls {1} of {2} times",count, (j + 1),repeats));
                    
                    var t = new Timer();
                    for (var i = 0; i < count; i++)
                    {
                        Setup();
                        t.Play();
                        dummy += Call(ItemsArray[i%CollectionSize]);
                        t.Pause();
                    }
                    runningTimeInSeconds = t.Check();
                    // Convert runningTime to nanoseconds and divide by the number of count
                    var time = runningTimeInSeconds*1e9/count;
                    elapsedTime += time;
                    elapsedSquaredTime += time*time;
                }
            } while (runningTimeInSeconds < maxExecutionTimeInSeconds && count < maxCount);
            var meanTime = elapsedTime/repeats;
            var standardDeviation = Math.Sqrt(elapsedSquaredTime/repeats - meanTime*meanTime)/meanTime*100;
            caller.UpdateRunningLabel("");
            return new Benchmark(BenchMarkName(), CollectionSize, meanTime, standardDeviation,count);
        }
    }
}