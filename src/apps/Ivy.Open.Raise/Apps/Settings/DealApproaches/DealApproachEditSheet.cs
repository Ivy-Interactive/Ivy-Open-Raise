using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.DealApproaches;

public class DealApproachEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int dealApproachId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var dealApproachQuery = UseQuery(
            key: (nameof(DealApproachEditSheet), dealApproachId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.DealApproaches.FirstOrDefaultAsync(e => e.Id == dealApproachId, ct);
            },
            tags: [(typeof(DealApproach), dealApproachId)]
        );

        if (dealApproachQuery.Loading || dealApproachQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Deal Approach");

        return dealApproachQuery.Value
            .ToForm()
            .Remove(e => e.Id)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Deal Approach");

        async Task OnSubmit(DealApproach? modifiedDealApproach)
        {
            if (modifiedDealApproach == null) return;
            await using var db = factory.CreateDbContext();
            db.DealApproaches.Update(modifiedDealApproach);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(DealApproach), dealApproachId));
            refreshToken.Refresh();
        }
    }
}
