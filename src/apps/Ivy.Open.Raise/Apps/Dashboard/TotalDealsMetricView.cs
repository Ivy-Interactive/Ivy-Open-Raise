namespace Ivy.Open.Raise.Apps.Dashboard;

public class TotalDealsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateTotalInvestors()
        {
            await using var db = factory.CreateDbContext();

            var deals = db.Deals.AsNoTracking().Where(e => e.DeletedAt == null);
            
            var current = await deals.CountAsync();
            
            var previousPeriod = await deals.CountAsync();

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
        }

        return new MetricView(
            "Total Deals",
            Icons.PersonStanding,
            CalculateTotalInvestors
        );
    }
}