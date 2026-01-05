using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.DealStates;

public class DealStateDetailsBlade(int dealStateId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();

        var dealStateQuery = UseQuery(
            key: (nameof(DealStateDetailsBlade), dealStateId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                var dealState = await db.DealStates.SingleOrDefaultAsync(e => e.Id == dealStateId, ct);
                var dealCount = await db.Deals.CountAsync(e => e.DealStateId == dealStateId, ct);
                return (dealState, dealCount);
            },
            tags: [(typeof(DealState), dealStateId)]
        );

        if (dealStateQuery.Loading) return Skeleton.Card();
        if (dealStateQuery.Value.dealState == null)
            return new Callout($"Deal state '{dealStateId}' not found.").Variant(CalloutVariant.Warning);

        var dealStateValue = dealStateQuery.Value.dealState;
        var dealCount = dealStateQuery.Value.dealCount;

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(dealCount > 0).Icon(Icons.Trash).HandleSelect(async () =>
                {
                    await DeleteAsync(factory);
                    queryService.RevalidateByTag(typeof(DealState[]));
                    blades.Pop();
                })
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new DealStateEditSheet(isOpen, refreshToken, dealStateId));

        var detailsCard = new Card(
            content: new
            {
                dealStateValue.Name,
                dealStateValue.Order,
                Deals = dealCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Deal State Details");

        return new Fragment()
               | new BladeHeader(Text.Literal(dealStateValue.Name))
               | (Layout.Vertical() | detailsCard);
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();

        var connectedDealsCount = await db.Deals.CountAsync(e => e.DealStateId == dealStateId);

        if (connectedDealsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete deal state with {connectedDealsCount} connected deal(s).");
        }

        var dealState = await db.DealStates.FirstOrDefaultAsync(e => e.Id == dealStateId);
        if (dealState != null)
        {
            db.DealStates.Remove(dealState);
            await db.SaveChangesAsync();
        }
    }
}
