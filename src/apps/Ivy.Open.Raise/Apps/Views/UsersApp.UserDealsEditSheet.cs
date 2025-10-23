namespace Ivy.Open.Raise.Apps.Views;

public class UserDealsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid dealId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deal = UseState(() => factory.CreateDbContext().Deals.FirstOrDefault(e => e.Id == dealId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                deal.Value.UpdatedAt = DateTime.UtcNow;
                db.Deals.Update(deal.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [deal]);

        return deal
            .ToForm()
            .Builder(e => e.AmountFrom, e => e.ToFeedbackInput())
            .Builder(e => e.AmountTo, e => e.ToFeedbackInput())
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Builder(e => e.NextActionNotes, e => e.ToTextAreaInput())
            .Builder(e => e.NextAction, e => e.ToDateInput())
            .Place(e => e.AmountFrom, e => e.AmountTo)
            .Group("Details", e => e.Notes, e => e.NextAction, e => e.NextActionNotes)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.OwnerId)
            .ToSheet(isOpen, "Edit Deal");
    }
}