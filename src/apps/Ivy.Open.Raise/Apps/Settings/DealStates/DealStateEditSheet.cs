using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.DealStates;

public class DealStateEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int dealStateId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var dealStateQuery = UseQuery(
            key: (nameof(DealStateEditSheet), dealStateId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.DealStates.FirstOrDefaultAsync(e => e.Id == dealStateId, ct);
            },
            tags: [(typeof(DealState), dealStateId)]
        );

        if (dealStateQuery.Loading || dealStateQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Deal State");

        return dealStateQuery.Value
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Deal State");

        async Task OnSubmit(DealState? modifiedDealState)
        {
            if (modifiedDealState == null) return;
            await using var db = factory.CreateDbContext();
            db.DealStates.Update(modifiedDealState);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(DealState), dealStateId));
            refreshToken.Refresh();
        }
    }
}
