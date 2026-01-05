namespace Ivy.Open.Raise.Apps.Dashboard;

public class InvestorTypesDistributionPieChartView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var chart = UseState<object?>();
        var exception = UseState<Exception?>();

        UseEffect(async () =>
        {
            try
            {
                var db = factory.CreateDbContext();
                var data = await db.Investors
                    .Where(i => i.CreatedAt >= fromDate && i.CreatedAt <= toDate)
                    .GroupBy(i => i.InvestorType.Name)
                    .Select(g => new InvestorTypeData { Type = g.Key, Count = g.Count() })
                    .ToListAsync();

                var totalInvestors = data.Sum(e => (double)e.Count);

                PieChartTotal total = new(Format.Number(@"[<1000]0;[<10000]0.0,""K"";0,""K""", totalInvestors), "Investors");

                chart.Set(data.ToPieChart(
                    dimension: e => e.Type,
                    measure: e => e.Sum(f => f.Count),
                    PieChartStyles.Dashboard,
                    total));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Investor Types Distribution").Height(Size.Units(80));

        if (exception.Value != null)
        {
            return card | new ErrorTeaserView(exception.Value);
        }

        if (chart.Value == null)
        {
            return card | new Skeleton();
        }

        return card | chart.Value;
    }

    private class InvestorTypeData
    {
        public string Type { get; set; } = null!;
        public int Count { get; set; }
    }
}