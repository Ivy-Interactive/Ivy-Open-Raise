namespace Ivy.Open.Raise.Apps.Settings.StartupStages;

public class StartupStageEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int startupStageId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<StartupStage?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.StartupStages.FirstOrDefaultAsync(e => e.Id == startupStageId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Startup Stage");

        async Task OnSubmit(StartupStage? startupStage)
        {
            if (startupStage == null) return;
            await using var db = factory.CreateDbContext();
            db.StartupStages.Update(startupStage);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}
