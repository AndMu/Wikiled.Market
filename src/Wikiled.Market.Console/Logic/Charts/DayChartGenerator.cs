using System;
using System.Linq;
using System.Threading.Tasks;
using Wikiled.Google.Chart;
using Wikiled.Google.Chart.Api;
using Wikiled.Google.Chart.Helpers;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Market.Console.Logic.Charts
{
    public class DayChartGenerator : IDayChartGenerator
    {
        private readonly LineChart lineChart;

        private readonly DatasetHelper2 dataset = new DatasetHelper2(new DayOfWeekSampling());

        public DayChartGenerator(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            lineChart = new LineChart(500, 300);
            lineChart.SetTitle($"Sentiment Data: {name}", Colors.Black, 14);
            lineChart.AddLineStyleAll(new LineStyle(5, 0, 0));
            lineChart.AddRangeMarker(new RangeMarker(RangeMarkerType.Horizontal, Colors.Black, 0.499, 0.501));
            lineChart.AddAxis(new ChartAxis(ChartAxisType.Left).SetRange(-1, 1));
            lineChart.SetLegend(new[] { "AMD" });
        }

        public void AddSeriesByDay(string name, RatingRecord[] records)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            DataPoint[] data = records.Where(item => item.Rating.HasValue)
                              .Select(
                                  item => new DataPoint
                                  {
                                      Date = item.Date,
                                      Value = ((float)item.Rating.Value * 50) + 50
                                  })
                              .ToArray();
            dataset.AddSeries(name, data);
        }

        public Task<byte[]> GenerateGraph()
        {
            dataset.Populate(lineChart);
            RequestManager request = new RequestManager();
            return request.GetImage(lineChart);
        }
    }
}
