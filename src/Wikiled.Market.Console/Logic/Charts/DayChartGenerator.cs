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

        private readonly TimeSeries dataset = new TimeSeries(new DayOfWeekSampling());

        private bool addedData;

        public DayChartGenerator(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            lineChart = new LineChart(500, 300);
            lineChart.SetTitle($"{name}", Colors.Black, 14);
        }

        public void AddSeriesByDay(string name, RatingRecord[] records, int days)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var today = DateTime.Today;
            var startDay = today.AddDays(-days);
            var data = records.Where(item => item.Rating.HasValue && item.Date < today && item.Date >= startDay)
                              .Select(
                                  item => new DataPoint
                                  {
                                      Date = item.Date,
                                      Value = (float)item.Rating.Value
                                  })
                              .ToList();
            if (data.Count > 0)
            {
                addedData = true;
                var byDate = data.ToLookup(item => item.Date.Date);
                for (int i = 0; i < days; i++)
                {
                    var day = today.AddDays(-1 - i);
                    if (!byDate.Contains(day))
                    {
                        data.Add(new DataPoint {Date = day, Value = 0});
                    }
                }

                dataset.AddSeries(name, data.ToArray());
            }
        }

        public Task<byte[]> GenerateGraph()
        {
            if (!addedData)
            {
                return Task.FromResult((byte[])null);
            }

            dataset.Generate();
            var max = dataset.Points.SelectMany(item => item).Max();
            var min = dataset.Points.SelectMany(item => item).Min();
            var scale = max > 0.5 || min < -0.5 ? 1 : 0.5f;
            var step = scale > 0.5 ? 0.2f : 0.1f;
            dataset.Rescale(point => (point * 50 / scale) + 50);
            lineChart.Populate(dataset).AdjustYScaleZero(scale, step);
            lineChart.AddLineStyleAll(new LineStyle(5, 0, 0));
            var request = new RequestManager();
            return request.GetImage(lineChart);
        }
    }
}
