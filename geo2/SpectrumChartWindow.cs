using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Linq;
using System.Windows;

namespace AeroSpectroApp
{
    public partial class SpectrumChartWindow : Window
    {
        public PlotModel PlotModel { get; private set; }

        public SpectrumChartWindow(Measurement measurement)
        {
            InitializeComponent();
            Title = $"Спектрограмма - {measurement.Timestamp:g}";

            PlotModel = new PlotModel
            {
                Title = "Спектрограмма",
                Subtitle = $"Измерение {measurement.MeasurementID}",
                
           
            };

            if (!string.IsNullOrEmpty(measurement.SpectrumData))
            {
                var values = measurement.SpectrumData.Split(',')
                    .Select((v, i) => new DataPoint(
                        measurement.SpectrumEnergyMin + (i * (measurement.SpectrumEnergyMax - measurement.SpectrumEnergyMin) / measurement.SpectrumChannels),
                        double.Parse(v)))
                    .ToList();

                var series = new LineSeries
                {
                    Title = "Интенсивность",
                    ItemsSource = values,
                    Color = OxyColors.Blue,
                    StrokeThickness = 1,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerFill = OxyColors.Blue
                };

                PlotModel.Series.Add(series);

                PlotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Энергия, кэВ",
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot
                });

                PlotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Интенсивность",
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot
                });
            }

            DataContext = this;
        }
    }
}