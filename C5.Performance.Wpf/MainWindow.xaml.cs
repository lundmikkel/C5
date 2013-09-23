using System;
using System.Data.Odbc;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using C5.Performance.Wpf.Benchmarks;

namespace C5.Performance.Wpf
{
    public partial class MainWindow : Window
    {
        // Parameters for running the benchmarks
        private const int MinCollectionSize = 100;
        private const int MaxCollectionSize = 50000;
        private const int CollectionMultiplier = 2;
        private const int MaxCount = Int32.MaxValue/100000;
        private const int Repeats = 10;
        private const double MaxExecutionTimeInSeconds = 0.25;


        private readonly Plotter _viewModel;
        // Every time we benchmark we count this up in order to get a new color for every benchmark
        private int _lineSeriesIndex = 0;

        public MainWindow()
        {
            _viewModel = new Plotter();
            DataContext = _viewModel;
            InitializeComponent();
        }

        private void button1_Click_1(object sender, RoutedEventArgs e) {
            var b = new SimpleBenchmark();
            var b2 = new IbsAddBenchmarker();
            var thread = new Thread(() => RunBenchmarks(b,b2));
            thread.Start();
        }

        private void updateStatusLabel(String s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this.StatusLabel.Content = s));
        }

        public void updateRunningLabel(String s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this.RunningLabel.Content = s));
        }

        private void RunBenchmarks(params Benchmarkable[] benchmarks)
        {
            foreach (var b in benchmarks)
            {
                _viewModel.AddLineSeries(b.BenchMarkName());
                for (b.CollectionSize = MinCollectionSize; b.CollectionSize < MaxCollectionSize; b.CollectionSize *= CollectionMultiplier)
                {
                    updateStatusLabel("Running " + b.BenchMarkName() + " with collection size " + b.CollectionSize);
                    var benchmark = b.Benchmark(MaxCount, Repeats, MaxExecutionTimeInSeconds, this);
                    var index = _lineSeriesIndex;
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        _viewModel.AddDataPoint(index, benchmark)));
                    //Thread.Sleep(100);
                }
                _lineSeriesIndex++;
            }
            updateRunningLabel("");
            updateStatusLabel("Finished");
            Thread.Sleep(1000);
            updateStatusLabel("");
        }
    }
}