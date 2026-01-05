namespace Ivy.Open.Raise.Apps.Dashboard;

public class DealsByStatePieChartView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object Build()
    {
        var factory = UseService<DataContextFactory>();
        var chart = UseState<object?>(() => null);
        var exception = UseState<Exception?>(() => null);

        UseEffect(async () =>
        {
            try
            {
                var db = factory.CreateDbContext();
                var data = await db.Deals
                    .Where(d => d.CreatedAt >= fromDate && d.CreatedAt <= toDate)
                    .GroupBy(d => d.DealState.Name)
                    .Select(g => new DealStateData { State = g.Key, DealCount = g.Count() })
                    .ToListAsync();

                var totalDeals = data.Sum(e => (double)e.DealCount);

                PieChartTotal total = new(Format.Number(@"[<1000]0;[<10000]0.0,""K"";0,""K""", totalDeals), "Deals");

                chart.Set(data.ToPieChart(
                    dimension: e => e.State,
                    measure: e => e.Sum(f => f.DealCount),
                    PieChartStyles.Dashboard,
                    total));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Deals by State").Height(Size.Units(80));

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

    private class DealStateData
    {
        public string State { get; set; } = null!;
        public int DealCount { get; set; }
    }
}