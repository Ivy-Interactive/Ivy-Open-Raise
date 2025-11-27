namespace Ivy.Open.Raise.Apps.Settings.DealApproaches;

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
            .Remove(e => e.Id)
            .ToSheet(isOpen, "Edit Deal Approach");
    }
}
