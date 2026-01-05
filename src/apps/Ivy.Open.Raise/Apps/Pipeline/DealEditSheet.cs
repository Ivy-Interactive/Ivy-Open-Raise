using Ivy.Hooks;
using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Pipeline;

public class DealEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid dealId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var query = UseQuery(
            key: (nameof(DealEditSheet), dealId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Deals.FirstOrDefaultAsync(e => e.Id == dealId, ct);
            },
            tags: [(typeof(Deal), dealId)]
        );

        if (query.Loading || query.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Deal");

        return query.Value
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(UseContactSearch, UseContactLookup, placeholder: "Select Contact"))
            .Builder(e => e.DealStateId, e => e.ToAsyncSelectInput(UseDealStateSearch, UseDealStateLookup, placeholder: "Select Deal State"))
            .Builder(e => e.DealApproachId, e => e.ToAsyncSelectInput(UseDealApproachSearch, UseDealApproachLookup, placeholder: "Select Deal Approach"))
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
            queryService.RevalidateByTag((typeof(Deal), dealId));
            refreshToken.Refresh();
        }
    }
}