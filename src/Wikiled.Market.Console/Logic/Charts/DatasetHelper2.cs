using System;
using System.Collections.Generic;
using System.Linq;
using Deedle;
using Wikiled.Google.Chart;
using Wikiled.Google.Chart.Helpers;

namespace Wikiled.Market.Console.Logic.Charts
{
    public class DatasetHelper2
    {
        private readonly FrameBuilder.Columns<DateTime, string> frameBuilder = new FrameBuilder.Columns<DateTime, string>();

        private readonly ISampling sampling;

        public DatasetHelper2(ISampling sampling)
        {
            this.sampling = sampling ?? throw new ArgumentNullException(nameof(sampling));
        }

        public void AddSeries(string name, DataPoint[] points)
        {
            Series<DateTime, float> newColumn = GetSeries(points);
            frameBuilder.Add(name, newColumn);
        }

        public void Populate(IChart chart, Func<DateTime, int, bool> labelSampling = null)
        {
            Frame<DateTime, string> frame = frameBuilder.Frame.FillMissing(0f);
            Series<DateTime, float> emptySeries = frame.GetColumnAt<float>(0).Observations.Select(item => new KeyValuePair<DateTime, float>(item.Key, 0)).ToSeries();

            emptySeries = emptySeries.Sample(sampling.GetStep());
            emptySeries = emptySeries.EndAt(frame.RowIndex.KeyAt(frame.RowIndex.KeyCount - 1));
            frameBuilder.Add("NULL", emptySeries);
            frame = frameBuilder.Frame.FillMissing(0f);
            frame.DropColumn("NULL");
            List<float[]> values = new List<float[]>();
            foreach (KeyValuePair<string, Series<DateTime, float>> columns in frame.GetAllColumns<float>())
            {
                values.Add(columns.Value.Values.ToArray());
            }

            List<string> days = new List<string>();
            for (int i = 0; i < frame.RowIndex.Keys.Count; i++)
            {
                if (labelSampling == null ||
                    labelSampling(frame.RowIndex.Keys[i], i))
                {
                    days.Add(sampling.GetName(frame.RowIndex.Keys[i]));
                }
            }

            chart.AddAxis(new ChartAxis(ChartAxisType.Bottom, days.ToArray()));
            chart.SetData(values);
            chart.SetAutoColors();
        }

        private Series<DateTime, float> GetSeries(DataPoint[] points)
        {
            SeriesBuilder<DateTime, float> seriesBuilder = new SeriesBuilder<DateTime, float>();
            foreach (IGrouping<DateTime, DataPoint> point in points.GroupBy(item => item.Date.Date))
            {
                seriesBuilder.Add(point.Key, point.Average(item => item.Value));
            }

            return seriesBuilder.Series.SortByKey();
        }
    }
}
