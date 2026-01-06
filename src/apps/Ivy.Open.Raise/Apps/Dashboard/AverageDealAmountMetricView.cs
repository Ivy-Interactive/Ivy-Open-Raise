namespace Ivy.Open.Raise.Apps.Dashboard;

public class AverageDealAmountMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        return new MetricView(
            "Average Deal Amount",
            Icons.DollarSign,
            UseMetricData
        );
    }

    private QueryResult<MetricRecord> UseMetricData(IViewContext context)
    {
        var factory = context.UseService<DataContextFactory>();

        return context.UseQuery(
            key: (nameof(AverageDealAmountMetricView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var currency = (await db.OrganizationSettings.FirstOrDefaultAsync(ct))?.CurrencyId;

                var deals = await db.Deals
                    .Where(e => e.AmountFrom != null || e.AmountTo != null)
                    .Select(e => new
                    {
                        e.AmountFrom,
                        e.AmountTo
                    })
                    .ToListAsync(ct);

                var value = deals.Count > 0
                    ? deals.Select(e => FromToFix(e.AmountFrom, e.AmountTo)).Average()
                    : 0;

                return new MetricRecord(
                    MetricFormatted: value.ToString("N0") + " " + currency
                );
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );

        double FromToFix(double? from, double? to)
        {
            if (from == null && to != null) return to.Value;
            if (to == null && from != null) return from.Value;
            if (from != null && to != null) return (from.Value + to.Value) / 2;
            return 0;
        }
    }
}
