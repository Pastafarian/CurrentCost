using Blazorise.Charts;
using Blazorise.LoadingIndicator;

namespace CurrentCost.Pages;

public class IndexViewModel
{
    public LineChart<double> LineChart { get; set; }
    public LoadingIndicator LoadingIndicator { get; set; }

    public async Task HandleRedraw()
    {
        try
        {
            await LoadingIndicator.Show();

            // Simulate server call ...
            await Task.Delay(3000); 

            await LineChart.Clear();
            await LineChart.AddLabelsDatasetsAndUpdate(Labels, GetLineChartDataset());
        }
        finally
        {
            await LoadingIndicator.Hide();
        }
    }

    private LineChartDataset<double> GetLineChartDataset()
    {
        return new LineChartDataset<double>
        {
            Label = "# of randoms",
            Data = RandomizeData(),
            BackgroundColor = backgroundColors,
            BorderColor = borderColors,
            Fill = true,
            PointRadius = 3,
            CubicInterpolationMode = "monotone",
        };
    }

    private string[] Labels = { "Red", "Blue", "Yellow", "Green", "Purple", "Orange" };
    private List<string> backgroundColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    private List<string> borderColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };

    private List<double> RandomizeData()
    {
        var r = new Random(DateTime.Now.Millisecond);

        return new List<double> {
            r.Next( 3, 50 ) * r.NextDouble(),
            r.Next( 3, 50 ) * r.NextDouble(),
            r.Next( 3, 50 ) * r.NextDouble(),
            r.Next( 3, 50 ) * r.NextDouble(),
            r.Next( 3, 50 ) * r.NextDouble(),
            r.Next( 3, 50 ) * r.NextDouble() };
    }
}
