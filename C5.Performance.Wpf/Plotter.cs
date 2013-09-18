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

        public Plotter()
        {
            PlotModel = new PlotModel();
            SetUpModel();
            ExportPdf();
        }

        private void ExportPdf(String path = "plot.pdf")
        {
            PdfExporter.Export(PlotModel, path, 800, 600);
        }


        private void SetUpModel()
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

            AddBenchmarks();
        }

        private void AddBenchmarks()
        {
            var testData1 = new[] {new Point(0, 1), new Point(4, 5), new Point(100, 6)};
            var testData2 = new[] {new Point(1, 15), new Point(4, 25), new Point(100, 8)};
            var testData3 = new[] {new Point(2, 30), new Point(4, 45), new Point(100, 9)};
            _benchmarks.Add(testData1);

            foreach (var benchmark in _benchmarks)
            {
                var lineSerie = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.Black,
                    MarkerType = MarkerType.Circle,
                    CanTrackerInterpolatePoints = false,
                    Title = string.Format("Test {0}", benchmark.First().X),
                    Smooth = false,
                };
                benchmark.ToList().ForEach(point => lineSerie.Points.Add(new DataPoint(point.X, point.Y)));
                PlotModel.Series.Add(lineSerie);
            }
        }
    }
}