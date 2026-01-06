namespace Ivy.Open.Raise.Apps.Dashboard;

public class TotalDeckViewsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        return new MetricView(
            "Total Deck Views",
            Icons.Eye,
            UseMetricData
        );
    }

    private QueryResult<MetricRecord> UseMetricData(IViewContext context)
    {
        var factory = context.UseService<DataContextFactory>();

        return context.UseQuery(
            key: (nameof(TotalDeckViewsMetricView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var currentViews = await db.DeckLinkViews.CountAsync(ct);

                var previousViews = await db.DeckLinkViews
                    .Where(v => v.ViewedAt <= fromDate)
                    .CountAsync(ct);

                if (previousViews == 0)
                {
                    return new MetricRecord(
                        MetricFormatted: currentViews.ToString("N0")
                    );
                }

                double? trend = ((double)currentViews - previousViews) / previousViews;

                return new MetricRecord(
                    MetricFormatted: currentViews.ToString("N0"),
                    TrendComparedToPreviousPeriod: trend
                );
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );
    }
}
