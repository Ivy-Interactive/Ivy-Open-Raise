namespace Ivy.Open.Raise.Apps.Dashboard;

public class TotalInvestorsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateTotalInvestors()
        {
            await using var db = factory.CreateDbContext();

            var contacts = db.Contacts.AsNoTracking().Where(e => e.DeletedAt == null);
            
            var current = await contacts.CountAsync();
            
            var previousPeriod = await contacts.CountAsync();

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
            "Total Contacts",
            Icons.PersonStanding,
            CalculateTotalInvestors
        );
    }
}