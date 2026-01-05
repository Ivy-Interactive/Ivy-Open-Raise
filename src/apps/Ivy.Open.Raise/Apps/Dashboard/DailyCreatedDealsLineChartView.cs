namespace Ivy.Open.Raise.Apps.Dashboard;

public class DailyCreatedDealsLineChartView(DateTime startDate, DateTime endDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var chart = UseState<object?>((object?)null!);
        var exception = UseState<Exception?>((Exception?)null!);

        UseEffect(async () =>
        {
            try
            {
                var db = factory.CreateDbContext();
                
                var data = await db.Deals
                    .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                    .GroupBy(d => d.CreatedAt.Date)
                    .Select(g => new DailyDealData { Date = g.Key.ToString("d MMM"), DealCount = g.Count() })
                    .ToListAsync();

                chart.Set(data.ToLineChart(
                    e => e.Date, 
                    [e => e.Sum(f => f.DealCount)], 
                    LineChartStyles.Dashboard));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Daily Created Deals").Height(Size.Units(80));

        if (exception.Value != null)
        {
            return card | new ErrorTeaserView(exception.Value);
        }

        if (chart.Value == null)
        {
            return card | new Skeleton();
        }

        return card | chart.Value;
    }

    private class DailyDealData
    {
        public string Date { get; set; }
        public int DealCount { get; set; }
    }
}