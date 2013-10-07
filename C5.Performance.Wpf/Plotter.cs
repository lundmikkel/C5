using System;
using OxyPlot;
using OxyPlot.Axes;
//using OxyPlot.Pdf;
using OxyPlot.Pdf;
using OxyPlot.Series;

namespace C5.Performance.Wpf
{
    public class Plotter
    {
        public PlotModel PlotModel { get; set; }

        public static Plotter createPlotter()
        {
            return new Plotter();
        }

        private Plotter()
        {
            PlotModel = new PlotModel();
            setUpModel();
        }

        /// <summary>
        /// Export the plot as a pdf file
        /// </summary>
        /// <param name="path">The file path where the pdf should be created</param>
        /// <param name="width">Width in pixels of the generated pfd</param>
        /// <param name="height">Height in pixels of the generated pfd</param>
        public void ExportPdf(String path = "plot.pdf", int width = 4960, int height = 7016)
        {
            PdfExporter.Export(PlotModel, path, width, height);
        }

        /// <summary>
        /// Prepare the plotter
        /// </summary>
        private void setUpModel()
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

        /// <summary>
        /// Add a benchmark data point to the graph being drawn
        /// </summary>
        /// <param name="indexOfAreaSeries">Index of the graph you wish to add data to</param>
        /// <param name="benchmark">Benchmark containing the data to be added</param>
        public void addDataPoint(int indexOfAreaSeries, Benchmark benchmark)
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

        /// <summary>
        /// Add a plot to the graph showing the benchmark you are running
        /// </summary>
        /// <param name="name">Name of the benchmark you wish to plot</param>
        public void addAreaSeries(String name)
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