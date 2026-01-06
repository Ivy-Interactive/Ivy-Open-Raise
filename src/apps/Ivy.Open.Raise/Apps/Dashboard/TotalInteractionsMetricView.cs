namespace Ivy.Open.Raise.Apps.Dashboard;

public class TotalInteractionsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        return new MetricView(
            "Total Interactions",
            Icons.PersonStanding,
            UseMetricData
        );
    }

    private QueryResult<MetricRecord> UseMetricData(IViewContext context)
    {
        var factory = context.UseService<DataContextFactory>();

        return context.UseQuery(
            key: (nameof(TotalInteractionsMetricView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var interactions = db.Interactions.AsNoTracking().Where(e => e.DeletedAt == null);

                var current = await interactions.CountAsync(ct);

                var previousPeriod = await interactions.CountAsync(ct);

                if (previousPeriod == 0)
                {
                    return new MetricRecord(
                        MetricFormatted: current.ToString("N0")
                    );
                }

                double? trend = ((double)current - previousPeriod) / previousPeriod;

                return new MetricRecord(
                    MetricFormatted: current.ToString("N0"),
                    TrendComparedToPreviousPeriod: trend
                );
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );
    }
}
