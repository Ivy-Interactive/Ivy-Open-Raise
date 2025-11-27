namespace Ivy.Open.Raise.Apps.Settings.DealApproaches;

public class DealApproachEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int dealApproachId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<DealApproach?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.DealApproaches.FirstOrDefaultAsync(e => e.Id == dealApproachId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
            .ToForm()
            .Remove(e => e.Id)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Deal Approach");

        async Task OnSubmit(DealApproach? dealApproach)
        {
            if (dealApproach == null) return;
            await using var db = factory.CreateDbContext();
            db.DealApproaches.Update(dealApproach);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}
