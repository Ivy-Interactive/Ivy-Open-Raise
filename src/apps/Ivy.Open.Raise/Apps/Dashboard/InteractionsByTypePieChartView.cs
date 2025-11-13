namespace Ivy.Open.Raise.Apps.Dashboard;

public class InteractionsByTypePieChartView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var chart = UseState<object?>((object?)null!);
        var exception = UseState<Exception?>((Exception?)null!);

        this.UseEffect(async () =>
        {
            try
            {
                var db = factory.CreateDbContext();
                var data = await db.Interactions
                    .Where(i => i.OccurredAt >= fromDate && i.OccurredAt <= toDate)
                    .GroupBy(i => i.InteractionTypeNavigation.Name)
                    .Select(g => new InteractionTypeData { Type = g.Key, InteractionCount = g.Count() })
                    .ToListAsync();

                var totalInteractions = data.Sum(e => (double)e.InteractionCount);

                PieChartTotal total = new(Format.Number(@"[<1000]0;[<10000]0.0,""K"";0,""K""", totalInteractions), "Interactions");

                chart.Set(data.ToPieChart(
                    dimension: e => e.Type,
                    measure: e => e.Sum(f => f.InteractionCount),
                    PieChartStyles.Dashboard,
                    total));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Interactions by Type").Height(Size.Units(80));

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

    private class InteractionTypeData
    {
        public string Type { get; set; } = null!;
        public int InteractionCount { get; set; }
    }
}