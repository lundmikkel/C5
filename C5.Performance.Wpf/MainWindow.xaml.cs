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
        private double _currentCollectionSize = MinCollectionSize;
        private const int MaxCollectionSize = 500000;
        internal int MaxIterations;
        private const int CollectionMultiplier = 2;
        private int _maxCount = Int32.MaxValue/10;
        private const int StandardRepeats = 10;
        private int _repeats = StandardRepeats;
        private bool _runWarmups = true;
        private const double MaxExecutionTimeInSeconds = 0.25;
        private readonly Plotter _plotter;
        // Every time we benchmark we count this up in order to get a new color for every benchmark
        private int _lineSeriesIndex;

        public MainWindow()
        {
            MaxIterations = Convert.ToInt32(Math.Round(Math.Log(MaxCollectionSize)));
            _plotter = Plotter.CreatePlotter();
            DataContext = _plotter;
            InitializeComponent();
        }

        private static Benchmarkable[] benchmarks()
        {
            return new Benchmarkable[]
            {
                new IbsAvlRandomRemoveBenchmarker(),
                new DynamicTreeRandomRemoveBenchmarker()
            };
        }

        private void benchmarkStart(object sender, RoutedEventArgs e)
        {
            // This benchmark is the one we use to compare with Sestoft's cmd line version of the tool
            var thread = new Thread(() => runBenchmarksParallel(benchmarks()));
            thread.Start();
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
                    var benchmark = b.Benchmark(_maxCount, _repeats, MaxExecutionTimeInSeconds, this);
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

        private void runBenchmarksParallel(params Benchmarkable[] benchmarks)
        {
            foreach (var benchmarkable in benchmarks)
                _plotter.AddAreaSeries(benchmarkable.BenchMarkName());
            var collectionSize = MinCollectionSize;
            while (collectionSize < MaxCollectionSize)
            {
                _lineSeriesIndex = 0;
                foreach (var b in benchmarks)
                {
                    b.CollectionSize = collectionSize;
                    updateStatusLabel("Running " + b.BenchMarkName() + " with collection size " + collectionSize);
                    var benchmark = b.Benchmark(_maxCount, _repeats, MaxExecutionTimeInSeconds, this, _runWarmups);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        _plotter.AddDataPoint(_lineSeriesIndex, benchmark)));
                    Thread.Sleep(100);
                    _lineSeriesIndex++;
                    _currentCollectionSize = (collectionSize*1.0) / (MaxCollectionSize*1.0);
                    updateProgressBar(benchmarks.Length);
                }
                collectionSize *= CollectionMultiplier;

            }
            UpdateRunningLabel("");
            updateStatusLabel("Finished");
            Thread.Sleep(1000);
            updateStatusLabel("");
        }

        private void updateProgressBar(int numberOfBenchmarks)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => progress.Value += (100.0 / MaxIterations)/numberOfBenchmarks));
        }

        private void updateStatusLabel(String s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => StatusLabel.Content = s));
        }

        public void UpdateRunningLabel(String s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => RunningLabel.Content = s));
        }

        private void savePdf(object sender, RoutedEventArgs routedEventArgs)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = benchmarks()[0].BenchMarkName(),
                DefaultExt = ".pdf",
                Filter = "PDF documents (.pdf)|*.pdf"
            };

            // Show save file dialog box
            var result = dlg.ShowDialog();
            if (result != true) return;
            
            // Save document
            var path = dlg.FileName;
            _plotter.ExportPdf(path,ActualWidth,ActualHeight);
        }

        private void CheckBox_Checked_RunWarmups(object sender, RoutedEventArgs e)
        {
            _runWarmups = true;
        }
        
        private void CheckBox_Unchecked_RunWarmups(object sender, RoutedEventArgs e)
        {
            _runWarmups = false;
        }

        private void CheckBox_Checked_RunQuick(object sender, RoutedEventArgs e)
        {
            _repeats = 1;
            _maxCount = Int32.MaxValue/1000;
        }

        private void CheckBox_Unchecked_RunQuick(object sender, RoutedEventArgs e)
        {
            _repeats = StandardRepeats;
            _maxCount = Int32.MaxValue/10;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }
    }
}