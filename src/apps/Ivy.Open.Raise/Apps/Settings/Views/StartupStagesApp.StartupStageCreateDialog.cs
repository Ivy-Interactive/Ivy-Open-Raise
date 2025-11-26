namespace Ivy.Open.Raise.Apps.Settings.Views;

public class StartupStageCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record StartupStageCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var startupStage = UseState(() => new StartupStageCreateRequest());

        UseEffect(() =>
        {
            var startupStageId = CreateStartupStage(factory, startupStage.Value);
            refreshToken.Refresh(startupStageId);
        }, [startupStage]);

        return startupStage
            .ToForm()
            .ToDialog(isOpen, title: "New Startup Stage", submitTitle: "Create");
    }

    private int CreateStartupStage(DataContextFactory factory, StartupStageCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var startupStage = new StartupStage()
        {
            Name = request.Name
        };

        db.StartupStages.Add(startupStage);
        db.SaveChanges();

        return startupStage.Id;
    }
}