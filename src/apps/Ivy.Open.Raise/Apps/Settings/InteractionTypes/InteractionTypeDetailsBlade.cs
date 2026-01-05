using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.InteractionTypes;

public class InteractionTypeDetailsBlade(int interactionTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();

        var query = UseQuery(
            key: (nameof(InteractionTypeDetailsBlade), interactionTypeId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                var interactionType = await db.InteractionTypes.SingleOrDefaultAsync(e => e.Id == interactionTypeId, ct);
                var interactionCount = await db.Interactions.CountAsync(e => e.InteractionType == interactionTypeId, ct);
                return (interactionType, interactionCount);
            },
            tags: [(typeof(InteractionType), interactionTypeId)]
        );

        if (query.Loading) return Skeleton.Card();
        if (query.Value.interactionType == null)
            return new Callout($"Interaction type '{interactionTypeId}' not found.").Variant(CalloutVariant.Warning);

        var interactionTypeValue = query.Value.interactionType;
        var interactionCount = query.Value.interactionCount;

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(interactionCount > 0).Icon(Icons.Trash).HandleSelect(async () =>
                {
                    await DeleteAsync(factory);
                    queryService.RevalidateByTag(typeof(InteractionType[]));
                    blades.Pop();
                })
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new InteractionTypeEditSheet(isOpen, refreshToken, interactionTypeId));

        var detailsCard = new Card(
            content: new
            {
                interactionTypeValue.Name,
                Interactions = interactionCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Interaction Type Details");

        return new Fragment()
               | new BladeHeader(Text.Literal(interactionTypeValue.Name))
               | (Layout.Vertical() | detailsCard);
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();

        var connectedInteractionsCount = await db.Interactions.CountAsync(e => e.InteractionType == interactionTypeId);

        if (connectedInteractionsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete interaction type with {connectedInteractionsCount} connected interaction(s).");
        }

        var interactionType = await db.InteractionTypes.FirstOrDefaultAsync(e => e.Id == interactionTypeId);
        if (interactionType != null)
        {
            db.InteractionTypes.Remove(interactionType);
            await db.SaveChangesAsync();
        }
    }
}
