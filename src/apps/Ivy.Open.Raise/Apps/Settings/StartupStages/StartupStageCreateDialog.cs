namespace Ivy.Open.Raise.Apps.Settings.StartupStages;

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
        var details = UseState(() => new StartupStageCreateRequest());

        return details
            .ToForm()
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Startup Stage", submitTitle: "Create");

        async Task OnSubmit(StartupStageCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var startupStage = new StartupStage
            {
                Name = request.Name
            };

            db.StartupStages.Add(startupStage);
            await db.SaveChangesAsync();
            refreshToken.Refresh(startupStage.Id);
        }
    }
}
