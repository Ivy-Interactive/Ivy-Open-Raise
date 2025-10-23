/*
Tracks the number of interactions logged each day.
SELECT DATE(OccurredAt) AS Date, COUNT(*) AS InteractionCount FROM Interaction WHERE OccurredAt BETWEEN @StartDate AND @EndDate GROUP BY DATE(OccurredAt) ORDER BY DATE(OccurredAt)
*/
namespace Ivy.Open.Raise.Apps.Views;

public class DailyInteractionsLineChartView(DateTime startDate, DateTime endDate) : ViewBase
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
                    .Where(i => i.OccurredAt >= startDate && i.OccurredAt <= endDate)
                    .GroupBy(i => i.OccurredAt.Date)
                    .Select(g => new InteractionData
                    {
                        Date = g.Key.ToString("d MMM"),
                        InteractionCount = g.Count()
                    })
                    .ToListAsync();

                chart.Set(data.ToLineChart(
                    e => e.Date, 
                    [e => e.Sum(f => f.InteractionCount)], 
                    LineChartStyles.Dashboard));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Daily Interactions").Height(Size.Units(80));

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

    private class InteractionData
    {
        public string Date { get; set; }
        public int InteractionCount { get; set; }
    }
}