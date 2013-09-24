using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Pdf;
using OxyPlot.Series;

namespace C5.Performance.Wpf
{
    public class Plotter
    {
        public Plotter()
        {
            PlotModel = new PlotModel();
            SetUpModel();
        }

        public PlotModel PlotModel { get; set; }

        public void ExportPdf(String path)
        {
            PdfExporter.Export(PlotModel, path, 4960, 7016);
        }

        private void SetUpModel()
        {
            PlotModel.Title = "Interval Plotter";
            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendPosition = LegendPosition.RightTop;
            PlotModel.LegendBackground = OxyColors.White;
            PlotModel.LegendBorder = OxyColors.Black;
            PlotModel.LegendPlacement = LegendPlacement.Outside;

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
        }

        public void AddDataPoint(int indexOfAreaSeries, Benchmark benchmark)
        {
            var areaSeries = PlotModel.Series[indexOfAreaSeries] as AreaSeries;
            if (areaSeries != null)
            {
                areaSeries.Points.Add(new DataPoint(benchmark.CollectionSize,
                    benchmark.MeanTime + benchmark.StandardDeviation));
                areaSeries.Points2.Add(new DataPoint(benchmark.CollectionSize,
                    benchmark.MeanTime - benchmark.StandardDeviation));
            }
            PlotModel.RefreshPlot(true);
        }

        public void AddAreaSeries(String name)
        {
            var areaSerie = new AreaSeries
            {
                StrokeThickness = 2,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Circle,
                Title = name
            };
            PlotModel.Series.Add(areaSerie);
        }
    }
}