/*
Tracks the number of views on deck links each day.
SELECT DATE(ViewedAt) AS Date, COUNT(*) AS ViewCount FROM DeckLinkView WHERE ViewedAt BETWEEN @StartDate AND @EndDate GROUP BY DATE(ViewedAt) ORDER BY DATE(ViewedAt)
*/
namespace Ivy.Open.Raise.Apps.Views;

public class DailyDeckViewsLineChartView(DateTime startDate, DateTime endDate) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var chart = this.UseState<object?>((object?)null!);
        var exception = this.UseState<Exception?>((Exception?)null!);

        this.UseEffect(async () =>
        {
            try
            {
                var db = factory.CreateDbContext();
                var data = await db.DeckLinkViews
                    .Where(v => v.ViewedAt >= startDate && v.ViewedAt <= endDate)
                    .GroupBy(v => v.ViewedAt.Date)
                    .Select(g => new DailyDeckViewData
                    {
                        Date = g.Key.ToString("d MMM"),
                        ViewCount = g.Count()
                    })
                    .ToListAsync();

                chart.Set(data.ToLineChart(
                    e => e.Date, 
                    [e => e.Sum(f => f.ViewCount)], 
                    LineChartStyles.Dashboard));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Daily Deck Views").Height(Size.Units(80));

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

    private class DailyDeckViewData
    {
        public string Date { get; set; }
        public double ViewCount { get; set; }
    }
}