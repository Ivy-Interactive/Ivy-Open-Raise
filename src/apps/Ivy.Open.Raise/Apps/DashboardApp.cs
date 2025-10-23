using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.LayoutDashboard, path: ["Apps"])]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        var fromDate = DateTime.UtcNow.Date.AddDays(-30);
        var toDate = DateTime.UtcNow.Date;
        
        var metrics =
                Layout.Grid().Columns(4)
                | new TotalDealsMetricView(fromDate, toDate).Key(fromDate, toDate) | new TotalInteractionsMetricView(fromDate, toDate).Key(fromDate, toDate) | new TotalDeckViewsMetricView(fromDate, toDate).Key(fromDate, toDate) | new ActiveContactsMetricView(fromDate, toDate).Key(fromDate, toDate) | new DealsInProgressMetricView(fromDate, toDate).Key(fromDate, toDate) | new AverageDealAmountMetricView(fromDate, toDate).Key(fromDate, toDate) | new TotalInvestorsMetricView(fromDate, toDate).Key(fromDate, toDate) | new TotalDeckLinksCreatedMetricView(fromDate, toDate).Key(fromDate, toDate) 
            ;
            
        var charts = 
                Layout.Grid().Columns(3)
                | new DailyInteractionsLineChartView(fromDate, toDate).Key(fromDate, toDate) | new DealsByStatePieChartView(fromDate, toDate).Key(fromDate, toDate) | new DailyDeckViewsLineChartView(fromDate, toDate).Key(fromDate, toDate) | new InvestorTypesDistributionPieChartView(fromDate, toDate).Key(fromDate, toDate) | new DailyCreatedDealsLineChartView(fromDate, toDate).Key(fromDate, toDate) | new InteractionsByTypePieChartView(fromDate, toDate).Key(fromDate, toDate) 
            ;

        return 
               Layout.Vertical().Width(Size.Full().Max(300))
                            | metrics
                            | charts;
    }
}