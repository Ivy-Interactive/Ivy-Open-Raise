namespace Ivy.Open.Raise.Apps.Views;

public class StartupStageEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int startupStageId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var startupStage = UseState(() => factory.CreateDbContext().StartupStages.FirstOrDefault(e => e.Id == startupStageId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            db.StartupStages.Update(startupStage.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [startupStage]);

        return startupStage
            .ToForm()
            .Place(e => e.Name)
            .ToSheet(isOpen, "Edit Startup Stage");
    }
}