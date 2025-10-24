namespace Ivy.Open.Raise.Apps.Views;

public class TotalInvestorsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateTotalInvestors()
        {
            await using var db = factory.CreateDbContext();
            
            var currentPeriodInvestors = await db.Investors
                .Where(i => i.CreatedAt >= fromDate && i.CreatedAt <= toDate)
                .CountAsync();
                
            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);
            
            var previousPeriodInvestors = await db.Investors
                .Where(i => i.CreatedAt >= previousFromDate && i.CreatedAt <= previousToDate)
                .CountAsync();

            if (previousPeriodInvestors == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodInvestors.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }
            
            double? trend = ((double)currentPeriodInvestors - previousPeriodInvestors) / previousPeriodInvestors;
            
            var goal = previousPeriodInvestors * 1.1;
            double? goalAchievement = goal > 0 ? currentPeriodInvestors / goal : null;
            
            return new MetricRecord(
                MetricFormatted: currentPeriodInvestors.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Total Investors",
            Icons.Building,
            CalculateTotalInvestors
        );
    }
}