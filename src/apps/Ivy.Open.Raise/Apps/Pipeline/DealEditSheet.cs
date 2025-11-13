using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Pipeline;

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
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(QueryContacts(factory), LookupContact(factory), placeholder: "Select Contact"))
            .Builder(e => e.DealStateId, e => e.ToAsyncSelectInput(QueryDealStates(factory), LookupDealState(factory), placeholder: "Select Deal State"))
            .Builder(e => e.DealApproachId, e => e.ToAsyncSelectInput(QueryDealApproaches(factory), LookupDealApproach(factory), placeholder: "Select Deal Approach"))
            .Remove(e => e.OwnerId)
            .Builder(e => e.AmountFrom, e => e.ToMoneyInput().Currency("USD")) //todo: should be from settings
            .Builder(e => e.AmountTo, e => e.ToMoneyInput().Currency("USD"))
            .Place(e => e.Contact)
            .Label(e => e.DealStateId, "State")
            .Label(e => e.DealApproachId, "Approach")
            .Place(e => e.Contact) //todo ivy: why isn't Contact placed correctly 
            .PlaceHorizontal(e => e.AmountFrom, e => e.AmountTo)
            .Builder(e => e.Priority, e => e.ToFeedbackInput())
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Builder(e => e.NextActionNotes, e => e.ToTextAreaInput())
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.Order)
            .Builder(e => e.NextAction, e => e.ToDateInput())
            .ToSheet(isOpen, "Edit Deal");
    }
}