// Simple microbenchmark setups
// sestoft@itu.dk * 2013-06-02, 2013-08-29

using System;

class Benchmark {
  public static void MainObsolete(String[] args) {
    SystemInfo();
    // Mark0();
    // Mark1();
    // Mark2();
    // Mark3();
    // Mark4();
    // Mark5();
    // Mark6("multiply", Multiply);
    // Mark6("multiply", Multiply);
    // MathFunctionBenchmarks();
    Random rnd = new Random();
    int n = 1638400;
    Mark7("random_index", (int i) => (double)rnd.Next(n));
    // SearchBenchmarks();
    // SearchScalabilityBenchmarks1();
    // SearchScalabilityBenchmarks2();
    // GetPseudorandomItems();
    // SortingBenchmarks();
    // SortingScalabilityBenchmarks();

    // Use this method to compare to the WPF version of the Intervals project (in MainWindow.xaml.cs)
    IntervalsTestBenchmarks();
  }

  // ========== Example functions and benchmarks ==========

  private static double Multiply(int i) {
    double x = 1.1 * (double)(i & 0xFF);
     return x * x * x * x * x * x * x * x * x * x 
          * x * x * x * x * x * x * x * x * x * x;
  }

  private static void MathFunctionBenchmarks() {
    Mark6("pow",  (int i) => Math.Pow(10.0, 0.1 * (i & 0xFF)));
    Mark6("exp",  (int i) => Math.Exp(0.1 * (i & 0xFF)));
    Mark6("log",  (int i) => Math.Log(0.1 + 0.1 * (i & 0xFF)));
    Mark6("sin",  (int i) => Math.Sin(0.1 * (i & 0xFF)));
    Mark6("cos",  (int i) => Math.Cos(0.1 * (i & 0xFF)));
    Mark6("tan",  (int i) => Math.Tan(0.1 * (i & 0xFF)));
    Mark6("asin", (int i) => Math.Asin(1.0/256.0 * (i & 0xFF)));
    Mark6("acos", (int i) => Math.Acos(1.0/256.0 * (i & 0xFF)));
    Mark6("atan", (int i) => Math.Atan(1.0/256.0 * (i & 0xFF)));
  }

  private static void SearchBenchmarks() {
    int[] intArray = SearchAndSort.FillIntArray(10000);  // sorted [0,1,...]
    int successItem = 4900, failureItem = 14000;
    Mark6("linear_search_success", 
	  (int i) => SearchAndSort.LinearSearch(successItem, intArray));
    Mark6("binary_search_success", 
	  (int i) => SearchAndSort.BinarySearch(successItem, intArray));
  }

  private static void SearchScalabilityBenchmarks1() {
    for (int size = 100; size <= 10000000; size *= 2) {
      int[] intArray = SearchAndSort.FillIntArray(size);  // sorted [0,1,...]
      int successItem = (int)(0.49 * size);
      Mark7("binary_search_success", 
            String.Format("{0,8:D}", size),
            (int i) => SearchAndSort.BinarySearch(successItem, intArray));
    }
  }

  private static void IntervalsTestBenchmarks() {
    for (int size = 100; size <= 50000; size *= 2) {
      int[] intArray = SearchAndSort.FillIntArray(size);  // sorted [0,1,...]
      int[] items = SearchAndSort.FillIntArrayRandomly(size, 0, size*2);
      int n = size;
      Mark7("binary_search_success", 
            String.Format("{0,8:D}", size),
            (int i) => {
                int successItem = items[i % n];
                return SearchAndSort.BinarySearch(successItem, intArray); 
	    }); 
    }
  }

  private static void SearchScalabilityBenchmarks2() {
    for (int size = 100; size <= 10000000; size *= 2) {
      int[] intArray = SearchAndSort.FillIntArray(size);  // sorted [0,1,...]
      int[] items = SearchAndSort.FillIntArray(size); 
      int n = size;
      SearchAndSort.Shuffle(items);
      Mark7("binary_search_success", 
            String.Format("{0,8:D}", size),
            (int i) => {
                int successItem = items[i % n];
                return SearchAndSort.BinarySearch(successItem, intArray); 
	    });      
    }
  }

  private static void GetPseudorandomItems() {
    for (int size = 100; size <= 10000000; size *= 2) {
      int[] items = SearchAndSort.FillIntArray(size); 
      int n = size;
      SearchAndSort.Shuffle(items);
      Mark7("get_pseudorandom_items", 
            String.Format("{0,8:D}", size),
            (int i) => {
                int successItem = items[i % n];
                return successItem; 
	    }); 
    }
  }

  private static void SortingBenchmarks() {
    int[] intArray = SearchAndSort.FillIntArray(10000);
    Mark6("shuffle int", 
       (int i) => { SearchAndSort.Shuffle(intArray); 
		    return 0.0; });
    Mark7Setup("shuffle", 
       (int i) => { SearchAndSort.Shuffle(intArray); 
		    return 0.0; });
    Mark7Setup("selection_sort", 
       (int i) => { SearchAndSort.Selsort(intArray); 
		    return 0.0; },
       () => { SearchAndSort.Shuffle(intArray); });
    Mark7Setup("quicksort", 
       (int i) => { SearchAndSort.Quicksort(intArray); 
		    return 0.0; },
       () => { SearchAndSort.Shuffle(intArray); });
    Mark7Setup("heapsort", 
       (int i) => { SearchAndSort.Heapsort(intArray); 
		    return 0.0; },
       () => { SearchAndSort.Shuffle(intArray); });
  }

  private static void SortingScalabilityBenchmarks() {
    for (int size = 100; size <= 50000; size *= 2) {
      int[] intArray = SearchAndSort.FillIntArray(size);
      Mark7Setup("selection_sort", 
		 String.Format("{0,8:D}", size),
                 (int i) => { SearchAndSort.Selsort(intArray); 
			      return 0.0; },
                 () => { SearchAndSort.Shuffle(intArray); });
    }
    Console.Write("\n\n"); // data set divider
    for (int size = 100; size <= 2000000; size *= 2) {
      int[] intArray = SearchAndSort.FillIntArray(size);
      Mark7Setup("quicksort", 
		 String.Format("{0,8:D}", size),
                 (int i) => { SearchAndSort.Quicksort(intArray); 
			      return 0.0; },
                 () => { SearchAndSort.Shuffle(intArray); });
    }
    Console.Write("\n\n"); // data set divider
    for (int size = 100; size <= 2000000; size *= 2) {
      int[] intArray = SearchAndSort.FillIntArray(size);
      Mark7Setup("heapsort", 
		 String.Format("{0,8:D}", size),
                 (int i) => { SearchAndSort.Heapsort(intArray); 
			      return 0.0; },
                 () => { SearchAndSort.Shuffle(intArray); });
    }
  }

  // ========== Infrastructure code ==========

  private static void SystemInfo() {
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

  public static void Mark0() {         // USELESS
    Timer t = new Timer();
    double dummy = Multiply(10);
    double time = t.Check() * 1e9;
    Console.WriteLine("{0,6:F1} ns", time);
  }

  public static void Mark1() {         // NEARLY USELESS
    Timer t = new Timer();
    int count = 1000000;
    for (int i=0; i<count; i++) {
      double dummy = Multiply(i);
    }
    double time = t.Check() * 1e9 / count;
    Console.WriteLine("{0,6:F1} ns", time);
  }

  public static double Mark2() {
    Timer t = new Timer();
    int count = 100000000;
    double dummy = 0.0;
    for (int i=0; i<count; i++) 
      dummy += Multiply(i);
    double time = t.Check() * 1e9 / count;
    Console.WriteLine("{0,6:F1} ns", time);
    return dummy;
  }

  public static double Mark3() {
    int repeats = 10;
    int count = 100000000;
    double dummy = 0.0;
    for (int j=0; j<repeats; j++) {
      Timer t = new Timer();
      for (int i=0; i<count; i++) 
        dummy += Multiply(i);
      double time = t.Check() * 1e9 / count;
      Console.WriteLine("{0,6:F1} ns", time);
    }
    return dummy;
  }

  public static double Mark4() {
    int repeats = 10;
    int count = 100000000;
    double dummy = 0.0;
    double st = 0.0, sst = 0.0;
    for (int j=0; j<repeats; j++) {
      Timer t = new Timer();
      for (int i=0; i<count; i++) 
        dummy += Multiply(i);
      double time = t.Check() * 1e9 / count;
      st += time; 
      sst += time * time;
    }
    double mean = st/repeats, sdev = Math.Sqrt(sst/repeats - mean*mean);
    Console.WriteLine("{0,6:F1} +/- {1,6:F3} ns", mean, sdev);
    return dummy;
  }

  public static double Mark5() {
    int repeats = 10, count = 1;
    double dummy = 0.0, runningTime = 0.0;
    do {
      count *= 10;
      double st = 0.0, sst = 0.0;
      for (int j=0; j<repeats; j++) {
        Timer t = new Timer();
        for (int i=0; i<count; i++) 
          dummy += Multiply(i);
        runningTime = t.Check();
        double time = runningTime * 1e9 / count;
        st += time; 
        sst += time * time;
      }
      double mean = st/repeats, sdev = Math.Sqrt(sst/repeats - mean*mean);
      Console.WriteLine("{0,6:F1} +/- {1,8:F2} ns {2,10:D}", mean, sdev, count);
    } while (runningTime < 0.25 && count < Int32.MaxValue/10);
    return dummy;
  }

  public static double Mark6(String msg, Func<int,double> f) {
    int repeats = 10, count = 1;
    double dummy = 0.0, runningTime = 0.0, st = 0.0, sst = 0.0;
    do { 
      count *= 10;
      st = sst = 0.0;
      for (int j=0; j<repeats; j++) {
        Timer t = new Timer();
        for (int i=0; i<count; i++) 
          dummy += f(i);
        runningTime = t.Check();
        double time = runningTime * 1e9 / count;
        st += time; 
        sst += time * time;
      }
    } while (runningTime < 0.25 && count < Int32.MaxValue/10);
    double mean = st/repeats, sdev = Math.Sqrt(sst/repeats - mean*mean);
    Console.WriteLine("{0,-25} {1,15:F1} {2,10:F2} {3,10:D}", msg, mean, sdev, count);
    return dummy;
  }

  public static double Mark7(String msg, String info, Func<int,double> f, 
                             int repeats = 10, double minTime = 0.25) {
    int count = 1;
    double dummy = 0.0, runningTime = 0.0, st = 0.0, sst = 0.0;
    do { 
      count *= 10;
      st = sst = 0.0;
      for (int j=0; j<repeats; j++) {
        Timer t = new Timer();
        for (int i=0; i<count; i++) 
          dummy += f(i);
        runningTime = t.Check();
        double time = runningTime * 1e9 / count;
        st += time; 
        sst += time * time;
      }
    } while (runningTime < minTime && count < Int32.MaxValue/10);
    double mean = st/repeats, sdev = Math.Sqrt(sst/repeats - mean*mean);
    Console.WriteLine("{0,-25} {1}{2,15:F1} {3,10:F2} {4,10:D}", msg, info, mean, sdev, count);
    return dummy;
  }

  public static double Mark7(String msg, Func<int,double> f, 
                             int repeats = 10, double minTime = 0.25) {
    return Mark7(msg, "", f, repeats, minTime);
  }

  public static double Mark7Setup(String msg, String info, Func<int,double> f, 
				  Action setup = null, int repeats = 10, double minTime = 0.25) {
    int count = 1;
    double dummy = 0.0, runningTime = 0.0, st = 0.0, sst = 0.0;
    do { 
      count *= 10;
      st = sst = 0.0;
      for (int j=0; j<repeats; j++) {
        Timer t = new Timer();
        for (int i=0; i<count; i++) {
          t.Pause();
          if (setup != null)
	    setup();
          t.Play();
          dummy += f(i);
        }
        runningTime = t.Check();
        double time = runningTime * 1e9 / count;
        st += time; 
        sst += time * time;
      }
    } while (runningTime < minTime && count < Int32.MaxValue/10);
    double mean = st/repeats, sdev = Math.Sqrt(sst/repeats - mean*mean);
    Console.WriteLine("{0,-25} {1}{2,15:F1} {3,10:F2} {4,10:D}", msg, info, mean, sdev, count);
    return dummy;
  }

  public static double Mark7Setup(String msg, Func<int,double> f, 
				  Action setup = null, int repeats = 10, double minTime = 0.25) {
    return Mark7Setup(msg, "", f, setup, repeats, minTime);
  }
}

// Crude timing utility ----------------------------------------

public class Timer {
  private readonly System.Diagnostics.Stopwatch stopwatch
    = new System.Diagnostics.Stopwatch();
  public Timer() { Play(); }
  public double Check() { return stopwatch.ElapsedMilliseconds / 1000.0; }
  public void Pause() { stopwatch.Stop(); }
  public void Play() { stopwatch.Start(); }
}

// ========== The searching and sorting examples ==========

public class SearchAndSort {
  public static int LinearSearch(int x, int[] arr) {
    int n = arr.Length, i = 0;                             
    while (i < n) 
      if (arr[i] != x) 
        i++;
      else 
        return i;                     
    return -1;
  }
  
  public static int BinarySearch(int x, int[] arr) {
    int n = arr.Length, a = 0, b = n-1;                 
    while (a <= b) {                                 
      int i = (a+b) / 2;
      if (x < arr[i]) 
        b = i-1;
      else if (arr[i] < x) 
        a = i+1;
      else 
        return i;                  
    }                                 
    return -1;
  }

  // Utility for sorting
  private static void Swap(int[] arr, int s, int t) {
    int tmp = arr[s];  arr[s] = arr[t];  arr[t] = tmp;
  }

  // Selection sort
  public static void Selsort(int[] arr) { 
    int n = arr.Length;
    for (int i = 0; i < n; i++) {
      int least = i;                                      
      for (int j = i+1; j < n; j++) 
        if (arr[j] < arr[least])
          least = j;
      Swap(arr, i, least);
    }
  }

  // Quicksort
  private static void Qsort(int[] arr, int a, int b) { 
    // sort arr[a..b]
    if (a < b) { 
      int i = a, j = b;
      int x = arr[(i+j) / 2];                
      do {                                   
        while (arr[i] < x) i++;              
        while (arr[j] > x) j--; 
        if (i <= j) {
          Swap(arr, i, j);
          i++; j--;
        }                                    
      } while (i <= j);                      
      Qsort(arr, a, j);                      
      Qsort(arr, i, b);                      
    }                                        
  }

  public static void Quicksort(int[] arr) {
    Qsort(arr, 0, arr.Length-1);
  }

  // Heapsort
  private static void Heapify(int[] arr, int i, int k) {
    // heapify node arr[i] in the tree arr[0..k]
    int j = 2 * i + 1;                          
    if (j <= k) {
      if (j+1 <= k && arr[j] < arr[j+1])
        j++;                                  
      if (arr[i] < arr[j]) {
        Swap(arr, i, j);                    
        Heapify(arr, j, k);                 
      }           
    }                                         
  }

  public static void Heapsort(int[] arr) {
    int n = arr.Length;
    for (int m=n/2; m >= 0; m--) 
      Heapify(arr, m, n-1);
    for (int m=n-1; m >= 1; m--) { 
      Swap(arr, 0, m);           
      Heapify(arr, 0, m-1);      
    }                            
  }

  public static int[] FillIntArray(int n) {
    int [] arr = new int[n];
    for (int i = 0; i < n; i++)
      arr[i] = i;
    return arr;
  }

  private static readonly Random rnd = new Random();

  public static void Shuffle(int[] arr) {
    for (int i = arr.Length-1; i > 0; i--)
      Swap(arr, i, rnd.Next(i+1));
  }

  public static int[] FillIntArrayRandomly(int n, int minValue = Int32.MinValue, int maxValue = Int32.MaxValue) {
            var arr = new int[n];
            for (var i = 0; i < n; i++)
                arr[i] = rnd.Next(minValue, maxValue);
            return arr;
        }
}
