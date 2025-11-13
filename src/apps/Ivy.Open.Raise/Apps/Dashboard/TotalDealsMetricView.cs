namespace Ivy.Open.Raise.Apps.Dashboard;

public class TotalDealsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateTotalDeals()
        {
            await using var db = factory.CreateDbContext();
            
            var currentPeriodDeals = await db.Deals
                .Where(d => d.CreatedAt >= fromDate && d.CreatedAt <= toDate)
                .CountAsync();
                
            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);
            
            var previousPeriodDeals = await db.Deals
                .Where(d => d.CreatedAt >= previousFromDate && d.CreatedAt <= previousToDate)
                .CountAsync();

            if (previousPeriodDeals == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodDeals.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }
            
            double? trend = ((double)currentPeriodDeals - previousPeriodDeals) / previousPeriodDeals;
            
            var goal = previousPeriodDeals * 1.1;
            double? goalAchievement = goal > 0 ? currentPeriodDeals / goal : null;
            
            return new MetricRecord(
                MetricFormatted: currentPeriodDeals.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Total Deals",
            Icons.Briefcase,
            CalculateTotalDeals
        );
    }
}