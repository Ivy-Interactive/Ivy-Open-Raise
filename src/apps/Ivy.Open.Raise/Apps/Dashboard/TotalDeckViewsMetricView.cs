namespace Ivy.Open.Raise.Apps.Dashboard;

public class TotalDeckViewsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateTotalDeckViews()
        {
            await using var db = factory.CreateDbContext();
            
            var currentPeriodViews = await db.DeckLinkViews
                .Where(v => v.ViewedAt >= fromDate && v.ViewedAt <= toDate)
                .CountAsync();
                
            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);
            
            var previousPeriodViews = await db.DeckLinkViews
                .Where(v => v.ViewedAt >= previousFromDate && v.ViewedAt <= previousToDate)
                .CountAsync();

            if (previousPeriodViews == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodViews.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }
            
            double? trend = ((double)currentPeriodViews - previousPeriodViews) / previousPeriodViews;
            
            var goal = previousPeriodViews * 1.1;
            double? goalAchievement = goal > 0 ? currentPeriodViews / goal : null;
            
            return new MetricRecord(
                MetricFormatted: currentPeriodViews.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Total Deck Views",
            Icons.Eye,
            CalculateTotalDeckViews
        );
    }
}