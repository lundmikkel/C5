using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Pdf;
using OxyPlot.Series;

namespace C5.Performance.Wpf
{
    public class Plotter
    {
        public PlotModel PlotModel { get; set; }
        private readonly List<Point[]> _benchmarks = new List<Point[]>();

        public Plotter(Benchmark benchmarks)
        {
            PlotModel = new PlotModel();
            SetUpModel(benchmarks);
            ExportPdf();
        }

        private void ExportPdf(String path = "plot.pdf")
        {
            PdfExporter.Export(PlotModel, path, 800, 600);
        }


        private void SetUpModel(Benchmark benchmarks)
        {
            PlotModel.Title = "Interval Plotter";
            // Remove the padding on the left side of the plot
            PlotModel.Padding = new OxyThickness(-10, 0, 0, 0);

            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendBackground = OxyColors.White;
            PlotModel.LegendBorder = OxyColors.Black;

            var sizeAxis = new LinearAxis(AxisPosition.Bottom)
            {
                AxisTitleDistance = 10,
                Title = "Collection Size",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };

            var valueAxis = new LinearAxis(AxisPosition.Left)
            {
                AxisTitleDistance = 10,
                Title = "Execution Time",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            PlotModel.Axes.Add(sizeAxis);
            PlotModel.Axes.Add(valueAxis);

            AddBenchmarks(benchmarks);
        }

        private void AddBenchmarks(Benchmark benchmark)
        {
            var testData1 = new[] {new Point(0, 1), new Point(4, 5), new Point(100, 6)};
            var testData2 = new[] {new Point(1, 15), new Point(4, 25), new Point(100, 8)};
            var testData3 = new[] {new Point(2, 30), new Point(4, 45), new Point(100, 9)};
            _benchmarks.Add(testData1);
            var lineSerie = new LineSeries {
                StrokeThickness = 2,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                Title = benchmark.BenchmarkName,
                Smooth = false,
            };
            for (var i = 0; i < benchmark.NumberOfBenchmarks-1; i++)
            {
                lineSerie.Points.Add(new DataPoint(benchmark.CollectionSizes[i],benchmark.MeanTimes[i]));
            }
            PlotModel.Series.Add(lineSerie);
        }
    }
}