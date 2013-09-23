﻿using System;

namespace C5.Performance
{
    public abstract class Benchmarkable
    {
        public int CollectionSize;
        internal int[] ItemsArray;

        public abstract String BenchMarkName();

        public abstract void CollectionSetup();

        public abstract void Setup();

        public abstract double Call(int i);

        public Benchmark Benchmark(int maxCount, int repeats, double maxExecutionTimeInSeconds)
        {
            CollectionSetup();
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
            Console.Out.WriteLine("Running benchmark with collection size " + CollectionSize);
            return new Benchmark(BenchMarkName(), CollectionSize, meanTime, standardDeviation);
        }

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
    }
}