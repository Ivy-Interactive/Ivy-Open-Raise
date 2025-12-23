namespace Ivy.Open.Raise.Apps.Settings.StartupStages;

public class StartupStageEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int startupStageId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var startupStage = UseState<StartupStage?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            startupStage.Set(await context.StartupStages.FirstOrDefaultAsync(e => e.Id == startupStageId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return startupStage
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Startup Stage");

        async Task OnSubmit(StartupStage? modifiedStartupStage)
        {
            if (modifiedStartupStage == null) return;
            await using var db = factory.CreateDbContext();
            db.StartupStages.Update(modifiedStartupStage);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}
