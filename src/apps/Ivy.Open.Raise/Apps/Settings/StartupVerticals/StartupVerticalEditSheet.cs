namespace Ivy.Open.Raise.Apps.Settings.StartupVerticals;

public class StartupVerticalEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int startupVerticalId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<StartupVertical?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.StartupVerticals.FirstOrDefaultAsync(e => e.Id == startupVerticalId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Startup Vertical");

        async Task OnSubmit(StartupVertical? startupVertical)
        {
            if (startupVertical == null) return;
            await using var db = factory.CreateDbContext();
            db.StartupVerticals.Update(startupVertical);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}
