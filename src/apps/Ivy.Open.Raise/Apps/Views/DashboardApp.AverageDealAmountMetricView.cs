/*
The average amount of deals created within the selected date range.
SELECT AVG((AmountFrom + AmountTo) / 2) FROM Deal WHERE CreatedAt BETWEEN StartDate AND EndDate
*/
namespace Ivy.Open.Raise.Apps.Views;

public class AverageDealAmountMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        
        async Task<MetricRecord> CalculateAverageDealAmount()
        {
            await using var db = factory.CreateDbContext();
            
            var currentPeriodDeals = await db.Deals
                .Where(d => d.CreatedAt >= fromDate && d.CreatedAt <= toDate)
                .ToListAsync();
                
            var currentAverageDealAmount = currentPeriodDeals
                .Where(d => d.AmountFrom.HasValue && d.AmountTo.HasValue)
                .Average(d => (double)((d.AmountFrom!.Value + d.AmountTo!.Value) / 2));
            
            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);
            
            var previousPeriodDeals = await db.Deals
                .Where(d => d.CreatedAt >= previousFromDate && d.CreatedAt <= previousToDate)
                .ToListAsync();
                
            double? previousAverageDealAmount = null;
            if (previousPeriodDeals.Any(d => d.AmountFrom.HasValue && d.AmountTo.HasValue))
            {
                previousAverageDealAmount = previousPeriodDeals
                    .Where(d => d.AmountFrom.HasValue && d.AmountTo.HasValue)
                    .Average(d => (double)((d.AmountFrom!.Value + d.AmountTo!.Value) / 2));
            }

            if (previousAverageDealAmount == null || previousAverageDealAmount == 0) 
            {
                return new MetricRecord(
                    MetricFormatted: currentAverageDealAmount.ToString("C0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }
            
            double? trend = (currentAverageDealAmount - previousAverageDealAmount.Value) / previousAverageDealAmount.Value;

            var goal = previousAverageDealAmount.Value * 1.1;
            double? goalAchievement = goal > 0 ? currentAverageDealAmount / goal : null;
            
            return new MetricRecord(
                MetricFormatted: currentAverageDealAmount.ToString("C0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("C0")
            );
        }

        return new MetricView(
            "Average Deal Amount",
            Icons.DollarSign,
            CalculateAverageDealAmount
        );
    }
}