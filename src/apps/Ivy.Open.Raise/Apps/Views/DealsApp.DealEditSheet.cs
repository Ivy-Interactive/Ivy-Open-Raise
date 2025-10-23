namespace Ivy.Open.Raise.Apps.Views;

public class DealEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid dealId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deal = UseState(() => factory.CreateDbContext().Deals.FirstOrDefault(e => e.Id == dealId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            deal.Value.UpdatedAt = DateTime.UtcNow;
            db.Deals.Update(deal.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [deal]);

        return deal
            .ToForm()
            .Builder(e => e.AmountFrom, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.AmountTo, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.Priority, e => e.ToFeedbackInput())
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Builder(e => e.NextAction, e => e.ToTextAreaInput())
            .Builder(e => e.NextActionNotes, e => e.ToTextAreaInput())
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .ToSheet(isOpen, "Edit Deal");
    }
}