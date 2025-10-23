/*
The total number of deck links created within the selected date range, representing the core value delivered to customers.
SELECT COUNT(*) FROM DeckLink WHERE CreatedAt BETWEEN StartDate AND EndDate
*/
namespace Ivy.Open.Raise.Apps.Views;

public class TotalDeckLinksCreatedMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateTotalDeckLinksCreated()
        {
            await using var db = factory.CreateDbContext();
            
            var currentPeriodDeckLinks = await db.DeckLinks
                .Where(dl => dl.CreatedAt >= fromDate && dl.CreatedAt <= toDate)
                .CountAsync();
                
            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);
            
            var previousPeriodDeckLinks = await db.DeckLinks
                .Where(dl => dl.CreatedAt >= previousFromDate && dl.CreatedAt <= previousToDate)
                .CountAsync();

            if (previousPeriodDeckLinks == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodDeckLinks.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }
            
            double? trend = ((double)currentPeriodDeckLinks - previousPeriodDeckLinks) / previousPeriodDeckLinks;
            
            var goal = previousPeriodDeckLinks * 1.1;
            double? goalAchievement = goal > 0 ? currentPeriodDeckLinks / goal : null;
            
            return new MetricRecord(
                MetricFormatted: currentPeriodDeckLinks.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Total Deck Links Created",
            Icons.Link,
            CalculateTotalDeckLinksCreated
        );
    }
}