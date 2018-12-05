using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deedle;
using Wikiled.Google.Chart;
using Wikiled.Google.Chart.Api;
using Wikiled.Google.Chart.Helpers;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Market.Console.Logic.Charts
{
    public class DayChartGenerator : IDayChartGenerator
    {
        private readonly LineChart lineChart;

        private readonly DatasetHelper dataset = new DatasetHelper(new DayOfWeekSampling());

        public DayChartGenerator(string name)
        {
            lineChart = new LineChart(500, 300);
            lineChart.SetTitle($"Sentiment Data: {name}", Colors.Black, 14);
            lineChart.AddLineStyleAll(new LineStyle(5, 0, 0));
            lineChart.AddRangeMarker(new RangeMarker(RangeMarkerType.Horizontal, Colors.Black, 0.499, 0.501));
            lineChart.AddAxis(new ChartAxis(ChartAxisType.Left).SetRange(-1, 1));
            lineChart.SetAutoColors();
        }

        public void AddSeriesByDay(string name, RatingRecord[] records)
        {
            dataset.AddSeries(name,
                              records
                                  .Where(item => item.Rating.HasValue)
                                  .Select(item => new DataPoint
                                  {
                                      Date = item.Date, Value = (float)item.Rating.Value
                                  })
                                  .ToArray());
        }

        public Task<byte[]> GenerateGraph()
        {
            dataset.Populate(lineChart);
            var request = new RequestManager();
            return request.GetImage(lineChart);
        }
    }
}
