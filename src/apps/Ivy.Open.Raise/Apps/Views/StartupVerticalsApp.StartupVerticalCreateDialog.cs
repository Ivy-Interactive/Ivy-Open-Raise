namespace Ivy.Open.Raise.Apps.Views;

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
        var startupVertical = UseState(() => new StartupVerticalCreateRequest());

        UseEffect(() =>
        {
            CreateStartupVertical(factory, startupVertical.Value);
            refreshToken.Refresh();
        }, [startupVertical]);

        return startupVertical
            .ToForm()
            .ToDialog(isOpen, title: "Create Startup Vertical", submitTitle: "Create");
    }

    private void CreateStartupVertical(DataContextFactory factory, StartupVerticalCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var startupVertical = new StartupVertical
        {
            Name = request.Name
        };

        db.StartupVerticals.Add(startupVertical);
        db.SaveChanges();
    }
}