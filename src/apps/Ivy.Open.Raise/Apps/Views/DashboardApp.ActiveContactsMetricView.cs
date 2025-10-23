namespace Ivy.Open.Raise.Apps.Views;

public class ActiveContactsMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateActiveContacts()
        {
            await using var db = factory.CreateDbContext();
            
            var currentPeriodContacts = await db.Interactions
                .Where(i => i.OccurredAt >= fromDate && i.OccurredAt <= toDate)
                .Select(i => i.ContactId)
                .Distinct()
                .CountAsync();
                
            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);
            
            var previousPeriodContacts = await db.Interactions
                .Where(i => i.OccurredAt >= previousFromDate && i.OccurredAt <= previousToDate)
                .Select(i => i.ContactId)
                .Distinct()
                .CountAsync();

            if (previousPeriodContacts == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodContacts.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }
            
            double? trend = ((double)currentPeriodContacts - previousPeriodContacts) / previousPeriodContacts;
            
            var goal = previousPeriodContacts * 1.1;
            double? goalAchievement = goal > 0 ? currentPeriodContacts / goal : null;
            
            return new MetricRecord(
                MetricFormatted: currentPeriodContacts.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Active Contacts",
            Icons.Users,
            CalculateActiveContacts
        );
    }
}