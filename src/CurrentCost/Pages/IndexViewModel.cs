using System.Globalization;
using System.Text.Json.Serialization;
using Blazorise.Charts;
using Blazorise.LoadingIndicator;
using CurrentCost.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CurrentCost.Pages;

public class IndexViewModel
{
    private readonly Context _context;
    public LineChart<int>? LineChart { get; set; }
    public LoadingIndicator? LoadingIndicator { get; set; }
    public int SelectedNumberOfItems = 10;
    public GroupBy SelectedGroupBy = GroupBy.None;

    public IndexViewModel(Context context)
    {
        _context = context;
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Seq("http://seq:5341")
            .Enrich.WithAssemblyName()
            .CreateLogger();

    }

    public async Task HandleRedraw()
    {
        try
        {
            if (LoadingIndicator != null)
            {
                await LoadingIndicator.Show();
                await UpdateChart();
            }
        }
        finally
        {
            if (LoadingIndicator != null)
            {
                await LoadingIndicator.Hide();
            }
        }
    }
    [Flags]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GroupBy
    {
        None = 0,
        Hour = 1,
        Day = 2,
    }

    public async Task UpdateChart()
    {
        if (LineChart == null)
        {
           return;
        }

        await LineChart.Clear();
        if (SelectedGroupBy ==  GroupBy.None)
        {
            var messages = _context.Messages.OrderByDescending(x => x.CreatedTime)
                .Take(SelectedNumberOfItems)
                .OrderBy(x => x.CreatedTime);
            var labels = messages
                .Select(x => x.CreatedTime)
                .ToArray();
            var chartValues = messages.Select(x => x.TotalWatts).ToList();
            await LineChart.AddLabelsDatasetsAndUpdate(labels.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray(), GetLineChartDataset(chartValues));
        }
        if (SelectedGroupBy == GroupBy.Hour)
        {
            var fromDate = DateTime.SpecifyKind(DateTime.Now.AddHours(-SelectedNumberOfItems), DateTimeKind.Utc);

            try
            {
                var messages = _context.Messages
                  //  .Where(x=>x.CreatedTime > fromDate)
                    .GroupBy(x => new { x.CreatedTime.Year, x.CreatedTime.Month, x.CreatedTime.Day, x.CreatedTime.Hour })
                    .Select(x => new
                    {
                        Date = x.Key.Year + "/" + x.Key.Month + "/" + x.Key.Day + " " + x.Key.Hour + ":00:00",
                        TotalWatts = x.Sum(y => y.TotalWatts),
                        DateOrder = x.First().CreatedTime
                    })
                    .OrderByDescending(x => x.DateOrder)
                    .Take(SelectedNumberOfItems)
                    .ToList()
                    .OrderBy(x => x.Date)
                    .ToList();

                //var query = _context.Messages
                //    .Where(x => x.CreatedTime > fromDate)
                //    .GroupBy(
                //        x => new { x.CreatedTime.Year, x.CreatedTime.Month, x.CreatedTime.Day, x.CreatedTime.Hour })
                //    .Select(x => new
                //    {
                //        Date = x.Key.Year + "/" + x.Key.Month + "/" + x.Key.Day + " " + x.Key.Hour + ":00:00",
                //        TotalWatts = x.Sum(y => y.TotalWatts),
                //        DateOrder = x.First().CreatedTime
                //    })
                //    .OrderByDescending(x => x.DateOrder)
                //    .Take(SelectedNumberOfItems);
                //var sql = query.ToQueryString();
   
                var labels = messages
                    .Select(x => x.Date)
                    .ToArray();
                var chartValues = messages.Select(x => x.TotalWatts).ToList();
                await LineChart.AddLabelsDatasetsAndUpdate(labels, GetLineChartDataset(chartValues));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        if (SelectedGroupBy == GroupBy.Day)
        {
            var fromDate = DateTime.SpecifyKind(DateTime.Now.AddDays(-SelectedNumberOfItems), DateTimeKind.Utc);
            var messages = _context.Messages
                ////.Where(x => x.CreatedTime > fromDate)
                .GroupBy(x => new { x.CreatedTime.Year, x.CreatedTime.Month, x.CreatedTime.Day })
                .Select(x => new
                {
                    Date = x.Key.Year + "/" + x.Key.Month + "/" + x.Key.Day,
                    TotalWatts = x.Sum(y => y.TotalWatts),
                    DateOrder = x.First().CreatedTime
                })
                .OrderByDescending(x => x.DateOrder)
                .Take(SelectedNumberOfItems)
                .ToList()
                .OrderBy(x => x.Date)
                .ToList();

            var labels = messages
                    .Select(x => x.Date)
                    .ToArray();
            var chartValues = messages.Select(x => x.TotalWatts).ToList();
            await LineChart.AddLabelsDatasetsAndUpdate(labels, GetLineChartDataset(chartValues));
        }
    }

    private LineChartDataset<int> GetLineChartDataset(List<int> chartValues) => new()
    {
        Label = "Total Watts",
        Data = chartValues,
        BackgroundColor = _backgroundColors,
        BorderColor = _borderColors,
        Fill = true,
        PointRadius = 3,
        CubicInterpolationMode = "monotone",
    };

    private readonly List<string> _backgroundColors = new() { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    private readonly List<string> _borderColors = new() { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
}
