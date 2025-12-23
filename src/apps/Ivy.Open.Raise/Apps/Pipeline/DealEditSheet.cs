using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Pipeline;

public class DealEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid dealId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<Deal?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.Deals.FirstOrDefaultAsync(e => e.Id == dealId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
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
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Deal");

        async Task OnSubmit(Deal? modifiedDeal)
        {
            if (modifiedDeal == null) return;
            await using var db = factory.CreateDbContext();
            modifiedDeal.UpdatedAt = DateTime.UtcNow;
            db.Deals.Update(modifiedDeal);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}