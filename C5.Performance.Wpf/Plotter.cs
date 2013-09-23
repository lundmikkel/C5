using System;
using C5.Performance.Wpf.Benchmarks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Pdf;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;

namespace C5.Performance.Wpf
{
    public class Plotter
    {
        public Plotter(params Benchmarkable[] benchmarks)
        {
            PlotModel = new PlotModel();
            SetUpModel();
//            AddBenchmarks(benchmarks);
//            ExportPdf();
        }

        public PlotModel PlotModel { get; set; }

        private void ExportPdf(String path = "plot.pdf")
        {
            PdfExporter.Export(PlotModel, path, 800, 600);
        }

        private void SetUpModel()
        {
            PlotModel.Title = "Interval Plotter";
            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendPosition = LegendPosition.LeftTop;
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
        }

//        public void UpdateModel() {
//            List<Measurement> measurements = Data.GetUpdateData(lastUpdate);
//            var dataPerDetector = measurements.GroupBy(m => m.DetectorId).OrderBy(m => m.Key).ToList();
//
//            foreach (var benchmark in dataPerDetector) {
//                var lineSerie = PlotModel.Series[0] as LineSeries;
//                if (lineSerie != null) {
//                    benchmark.ToList()
//                        .ForEach(d => lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(d.DateTime), d.Value)));
//                }
//            }
//            lastUpdate = DateTime.Now;
//        }

        public void AddDataPoint(int indexOfLineSeries, Benchmark benchmark)
        {
            var lineSeries = PlotModel.Series[indexOfLineSeries] as LineSeries;
            if (lineSeries != null)
                lineSeries.Points.Add(new DataPoint(benchmark.CollectionSize, benchmark.MeanTime));
            PlotModel.RefreshPlot(true);
        }

        public void AddLineSeries(String name)
        {
            var lineSerie = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.Black,
                    MarkerType = MarkerType.Circle,
                    CanTrackerInterpolatePoints = false,
                    Title = name,
                    Smooth = false,
                };
            PlotModel.Series.Add(lineSerie);
        }
    }
}