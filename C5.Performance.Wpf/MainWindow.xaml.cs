using System.Windows;

namespace C5.Performance.Wpf {
    public partial class MainWindow : Window {
        public MainWindow() {
            // Create the plotter with the benchmarks
            var viewModel = new Plotter(new SimpleBenchmark().GetBenchmark());
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
