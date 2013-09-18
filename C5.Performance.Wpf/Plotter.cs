using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Pdf;
using OxyPlot.Series;

namespace C5.Performance.Wpf
{
    public class Plotter
    {
        public PlotModel PlotModel { get; set; }

        public Plotter(Benchmark benchmark)
        {
            PlotModel = new PlotModel();
            SetUpModel(benchmark);
            ExportPdf();
        }

        private void ExportPdf(String path = "plot.pdf")
        {
            PdfExporter.Export(PlotModel, path, 800, 600);
        }


        private void SetUpModel(Benchmark benchmark)
        {
            PlotModel.Title = "Interval Plotter";
            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendPosition = LegendPosition.BottomRight;
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
                Title = "Execution Time in nano seconds",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            PlotModel.Axes.Add(sizeAxis);
            PlotModel.Axes.Add(valueAxis);

            AddBenchmarks(benchmark);
        }

        private void AddBenchmarks(Benchmark benchmark)
        {
            var lineSerie = new LineSeries {
                StrokeThickness = 2,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = false,
                Title = benchmark.BenchmarkName,
                Smooth = false,
            };
            for (var i = 0; i < benchmark.NumberOfBenchmarks; i++)
            {
                lineSerie.Points.Add(new DataPoint(benchmark.CollectionSizes[i],benchmark.MeanTimes[i]));
            }
            PlotModel.Series.Add(lineSerie);
        }
    }
}