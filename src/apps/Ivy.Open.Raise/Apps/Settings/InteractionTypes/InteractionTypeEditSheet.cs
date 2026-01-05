using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.InteractionTypes;

public class InteractionTypeEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int interactionTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var interactionTypeQuery = UseQuery(
            key: (nameof(InteractionTypeEditSheet), interactionTypeId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.InteractionTypes.FirstOrDefaultAsync(e => e.Id == interactionTypeId, ct);
            },
            tags: [(typeof(InteractionType), interactionTypeId)]
        );

        if (interactionTypeQuery.Loading || interactionTypeQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Interaction Type");

        return interactionTypeQuery.Value
            .ToForm()
            .Remove(e => e.Id)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Interaction Type");

        async Task OnSubmit(InteractionType? modifiedInteractionType)
        {
            if (modifiedInteractionType == null) return;
            await using var db = factory.CreateDbContext();
            db.InteractionTypes.Update(modifiedInteractionType);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(InteractionType), interactionTypeId));
            refreshToken.Refresh();
        }
    }
}
