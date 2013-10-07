using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using C5.Performance.Wpf.Benchmarks;

namespace C5.Performance.Wpf
{
    public partial class MainWindow
    {
        // Parameters for running the benchmarks
        private const int MinCollectionSize = 100;
        private const int MaxCollectionSize = 50000;
        private const int CollectionMultiplier = 2;
        private const int MaxCount = Int32.MaxValue/10;
        private const int Repeats = 10;
        private const double MaxExecutionTimeInSeconds = 0.25;
        // Path of the exported pdf file containing the benchmark
        private const String PdfPath = "pdfplot.pdf";
        private readonly Plotter _plotter;
        // Every time we benchmark we count this up in order to get a new color for every benchmark
        private int _lineSeriesIndex;

        public MainWindow()
        {
            _plotter = Plotter.CreatePlotter();
            DataContext = _plotter;
            InitializeComponent();
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            // This benchmark is the one we use to compare with Sestoft's cmd line version of the tool
            var intervalsTestBenchmarks = new SimpleBenchmark();
            var b2 = new IbsAvlAddBenchmarker();
            var b3 = new IbsAddBenchmarker();
            var thread = new Thread(() => runBenchmarks(intervalsTestBenchmarks));
            thread.Start();
//            _plotter.ExportPdf(PdfPath);
        }

        private void runBenchmarks(params Benchmarkable[] benchmarks)
        {
            foreach (var b in benchmarks)
            {
                _plotter.AddAreaSeries(b.BenchMarkName());
                for (b.CollectionSize = MinCollectionSize;
                    b.CollectionSize < MaxCollectionSize;
                    b.CollectionSize *= CollectionMultiplier)
                {
                    updateStatusLabel("Running " + b.BenchMarkName() + " with collection size " + b.CollectionSize);
                    var benchmark = b.Benchmark(MaxCount, Repeats, MaxExecutionTimeInSeconds, this);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        _plotter.AddDataPoint(_lineSeriesIndex, benchmark)));
                    Thread.Sleep(100);
                }
                _lineSeriesIndex++;
            }
            UpdateRunningLabel("");
            updateStatusLabel("Finished");
            Thread.Sleep(1000);
            updateStatusLabel("");
        }

        private void updateStatusLabel(String s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => StatusLabel.Content = s));
        }

        public void UpdateRunningLabel(String s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => RunningLabel.Content = s));
        }
    }
}