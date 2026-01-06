namespace Ivy.Open.Raise.Apps.Dashboard;

public class DealsInProgressMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        return new MetricView(
            "Deals in Progress",
            Icons.Clock,
            UseMetricData
        );
    }

    private QueryResult<MetricRecord> UseMetricData(IViewContext context)
    {
        var factory = context.UseService<DataContextFactory>();

        return context.UseQuery(
            key: (nameof(DealsInProgressMetricView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var current = await db.Deals.Where(d => !d.DealState.IsFinal)
                    .CountAsync(ct);

                return new MetricRecord(
                    MetricFormatted: current.ToString("N0")
                );
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );
    }
}
