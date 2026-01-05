using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.DealApproaches;

public class DealApproachDetailsBlade(int dealApproachId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();

        var query = UseQuery(
            key: (nameof(DealApproachDetailsBlade), dealApproachId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                var dealApproach = await db.DealApproaches.SingleOrDefaultAsync(e => e.Id == dealApproachId, ct);
                var dealCount = await db.Deals.CountAsync(e => e.DealApproachId == dealApproachId, ct);
                return (dealApproach, dealCount);
            },
            tags: [(typeof(DealApproach), dealApproachId)]
        );

        if (query.Loading) return Skeleton.Card();
        if (query.Value.dealApproach == null)
            return new Callout($"Deal approach '{dealApproachId}' not found.").Variant(CalloutVariant.Warning);

        var dealApproachValue = query.Value.dealApproach;
        var dealCount = query.Value.dealCount;
        
        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(dealCount > 0).Icon(Icons.Trash).HandleSelect(async () =>
                {
                    await DeleteAsync(factory);
                    queryService.RevalidateByTag(typeof(DealApproach[]));
                    blades.Pop();
                })
            );
        
        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new DealApproachEditSheet(isOpen, refreshToken, dealApproachId));


        var detailsCard = new Card(
            content: new
            {
                dealApproachValue.Name,
                Deals = dealCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Deal Approach Details");

        return new Fragment()
               | new BladeHeader(Text.Literal(dealApproachValue.Name))
               | (Layout.Vertical() | detailsCard)
            ;
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();

        var connectedDealsCount = await db.Deals.CountAsync(e => e.DealApproachId == dealApproachId);

        if (connectedDealsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete deal approach with {connectedDealsCount} connected deal(s).");
        }

        var dealApproach = await db.DealApproaches.FirstOrDefaultAsync(e => e.Id == dealApproachId);
        if (dealApproach != null)
        {
            db.DealApproaches.Remove(dealApproach);
            await db.SaveChangesAsync();
        }
    }
}
