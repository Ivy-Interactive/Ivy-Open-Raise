namespace Ivy.Open.Raise.Apps.Settings.StartupVerticals;

public class StartupVerticalCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record StartupVerticalCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState(() => new StartupVerticalCreateRequest());

        return details
            .ToForm()
            .OnSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Startup Vertical", submitTitle: "Create");

        async Task OnSubmit(StartupVerticalCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var startupVertical = new StartupVertical
            {
                Name = request.Name
            };

            db.StartupVerticals.Add(startupVertical);
            await db.SaveChangesAsync();
            refreshToken.Refresh(startupVertical.Id);
        }
    }
}
