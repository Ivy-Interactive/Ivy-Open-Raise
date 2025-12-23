namespace Ivy.Open.Raise.Apps.Settings.InteractionTypes;

public class InteractionTypeEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int interactionTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<InteractionType?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.InteractionTypes.FirstOrDefaultAsync(e => e.Id == interactionTypeId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
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
            refreshToken.Refresh();
        }
    }
}
