namespace Ivy.Open.Raise.Apps.Views;

public class DealApproachEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int dealApproachId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var dealApproach = UseState(() => factory.CreateDbContext().DealApproaches.FirstOrDefault(e => e.Id == dealApproachId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.DealApproaches.Update(dealApproach.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [dealApproach]);

        return dealApproach
            .ToForm()
            .Builder(e => e.Name, e => e.ToTextAreaInput())
            .ToSheet(isOpen, "Edit Deal Approach");
    }
}