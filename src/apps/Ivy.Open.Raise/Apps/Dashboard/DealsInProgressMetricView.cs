namespace Ivy.Open.Raise.Apps.Dashboard;

public class DealsInProgressMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateDealsInProgress()
        {
            await using var db = factory.CreateDbContext();
            
            var current = await db.Deals.Where(d => !d.DealState.IsFinal)
                .CountAsync();

            return new MetricRecord(
                MetricFormatted: current.ToString("N0")
            );
        }

        return new MetricView(
            "Deals in Progress",
            Icons.Clock,
            CalculateDealsInProgress
        );
    }
}