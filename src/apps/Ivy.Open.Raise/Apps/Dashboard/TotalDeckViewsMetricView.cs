namespace Ivy.Open.Raise.Apps.Dashboard;

public class TotalDeckViewsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateTotalDeckViews()
        {
            await using var db = factory.CreateDbContext();
            
            var currentViews = await db.DeckLinkViews.CountAsync();
            
            var previousViews = await db.DeckLinkViews
                .Where(v => v.ViewedAt <= fromDate)
                .CountAsync();

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
        }

        return new MetricView(
            "Total Deck Views",
            Icons.Eye,
            CalculateTotalDeckViews
        );
    }
}