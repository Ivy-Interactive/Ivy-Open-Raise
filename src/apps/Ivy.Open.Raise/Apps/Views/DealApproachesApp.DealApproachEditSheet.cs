namespace Ivy.Open.Raise.Apps.Views;

public class DealApproachEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int dealApproachId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var dealApproach = UseState(() => factory.CreateDbContext().DealApproaches.FirstOrDefault(e => e.Id == dealApproachId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            db.DealApproaches.Update(dealApproach.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [dealApproach]);

        return dealApproach
            .ToForm()
            .Builder(e => e.Name, e => e.ToTextAreaInput())
            .ToSheet(isOpen, "Edit Deal Approach");
    }
}