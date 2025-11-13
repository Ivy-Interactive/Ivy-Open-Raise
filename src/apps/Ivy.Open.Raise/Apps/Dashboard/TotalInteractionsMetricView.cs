namespace Ivy.Open.Raise.Apps.Dashboard;

public class TotalInteractionsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateTotalInteractions()
        {
            await using var db = factory.CreateDbContext();
            
            var currentPeriodInteractions = await db.Interactions
                .Where(i => i.OccurredAt >= fromDate && i.OccurredAt <= toDate)
                .CountAsync();
                
            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);
            
            var previousPeriodInteractions = await db.Interactions
                .Where(i => i.OccurredAt >= previousFromDate && i.OccurredAt <= previousToDate)
                .CountAsync();

            if (previousPeriodInteractions == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodInteractions.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }
            
            double? trend = ((double)currentPeriodInteractions - previousPeriodInteractions) / previousPeriodInteractions;
            
            var goal = previousPeriodInteractions * 1.1;
            double? goalAchievement = goal > 0 ? currentPeriodInteractions / goal : null;
            
            return new MetricRecord(
                MetricFormatted: currentPeriodInteractions.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Total Interactions",
            Icons.MessageSquare,
            CalculateTotalInteractions
        );
    }
}