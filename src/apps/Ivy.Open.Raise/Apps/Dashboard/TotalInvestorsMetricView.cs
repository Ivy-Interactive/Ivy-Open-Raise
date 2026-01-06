namespace Ivy.Open.Raise.Apps.Dashboard;

public class TotalInvestorsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        return new MetricView(
            "Total Investors",
            Icons.BriefcaseBusiness,
            UseMetricData
        );
    }

    private QueryResult<MetricRecord> UseMetricData(IViewContext context)
    {
        var factory = context.UseService<DataContextFactory>();

        return context.UseQuery(
            key: (nameof(TotalInvestorsMetricView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var investors = db.Investors.AsNoTracking().Where(e => e.DeletedAt == null);

                var current = await investors.CountAsync(ct);

                var previousPeriod = await investors.CountAsync(ct);

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
