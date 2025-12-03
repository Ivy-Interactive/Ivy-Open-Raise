namespace Ivy.Open.Raise.Apps.Dashboard;

public class AverageDealAmountMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateAverageDealAmount()
        {
            await using var db = factory.CreateDbContext();

            var currency = (await db.OrganizationSettings.FirstOrDefaultAsync()).CurrencyId;
            
            var deals = await db.Deals
                .Where(e => e.AmountFrom != null || e.AmountTo != null) 
                .Select(e => new
                {
                    e.AmountFrom,
                    e.AmountTo
                })
                .ToListAsync();
            
            var value = deals.Select(e => FromToFix(e.AmountFrom, e.AmountTo)).Average();
            
            return new MetricRecord(
                MetricFormatted: value.ToString("N0") + " " + currency
            );
        }
        
        return new MetricView(
            "Average Deal Amount",
            Icons.DollarSign,
            CalculateAverageDealAmount
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