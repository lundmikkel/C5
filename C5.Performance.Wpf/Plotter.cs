using System;
using System.IO;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace C5.Performance.Wpf
{
    public class Plotter
    {
        public PlotModel PlotModel { get; set; }

        public static Plotter CreatePlotter()
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
        public void ExportPdf(String path, double width, double height)
        {
            PdfExporter.Export(PlotModel, new StreamWriter(path).BaseStream, width, height);
        }

        /// <summary>
        /// Prepare the plotter
        /// </summary>
        private void setUpModel(bool logarithmicXAxis = false)
        {
            //PlotModel.Title = "Interval Plotter";
            //PlotModel.LegendTitle = "Legend";
            PlotModel.LegendPosition = LegendPosition.TopLeft;
            PlotModel.LegendBackground = OxyColors.White;
            PlotModel.LegendBorder = OxyColors.Black;
            PlotModel.LegendPlacement = LegendPlacement.Inside;

            var sizeAxis = new LinearAxis(AxisPosition.Bottom)
            {
                AxisTitleDistance = 10,
                Title = "Collection Size",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };

            // Comment in the line with the axis you want
            var valueAxis = logarithmicXAxis ? (Axis)new LogarithmicAxis() : new LinearAxis();
            valueAxis.Position = AxisPosition.Left;
            valueAxis.AxisTitleDistance = 10;
            valueAxis.Title = "Execution Time in nanoseconds";
            valueAxis.MajorGridlineStyle = LineStyle.Solid;
            valueAxis.MinorGridlineStyle = LineStyle.Dot;
            
            sizeAxis.AbsoluteMinimum = 0;
            valueAxis.AbsoluteMinimum = 0;

            PlotModel.Axes.Add(sizeAxis);
            PlotModel.Axes.Add(valueAxis);
        }

        public void ToggleLogarithmicAxis(bool logarithmicXAxis)
        {
            PlotModel.Axes.Clear();
            setUpModel(logarithmicXAxis);
            PlotModel.RefreshPlot(true);
        }

        /// <summary>
        /// Add a benchmark data point to the graph being drawn
        /// </summary>
        /// <param name="indexOfAreaSeries">Index of the graph you wish to add data to</param>
        /// <param name="benchmark">Benchmark containing the data to be added</param>
        public void AddDataPoint(int indexOfAreaSeries, Benchmark benchmark)
        {
            var areaSeries = PlotModel.Series[indexOfAreaSeries] as AreaSeries;
            if (areaSeries != null)
            {
                areaSeries.Points.Add(new DataPoint(benchmark.CollectionSize,
                    (benchmark.MeanTime + benchmark.StandardDeviation)));
                areaSeries.Points2.Add(new DataPoint(benchmark.CollectionSize,
                    (benchmark.MeanTime - benchmark.StandardDeviation)));
            }
            PlotModel.RefreshPlot(true);

        }

        /// <summary>
        /// Add a plot to the graph showing the benchmark you are running
        /// </summary>
        /// <param name="name">Name of the benchmark you wish to plot</param>
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