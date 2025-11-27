namespace Ivy.Open.Raise.Apps.Settings.DealStates;

public class DealStateEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int dealStateId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<DealState?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.DealStates.FirstOrDefaultAsync(e => e.Id == dealStateId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Deal State");

        async Task OnSubmit(DealState? dealState)
        {
            if (dealState == null) return;
            await using var db = factory.CreateDbContext();
            db.DealStates.Update(dealState);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}
