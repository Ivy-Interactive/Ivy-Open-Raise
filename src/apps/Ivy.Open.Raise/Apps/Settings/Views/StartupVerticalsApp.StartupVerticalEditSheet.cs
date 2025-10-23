namespace Ivy.Open.Raise.Apps.Settings.Views;

public class StartupVerticalEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int startupVerticalId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var startupVertical = UseState(() => factory.CreateDbContext().StartupVerticals.FirstOrDefault(e => e.Id == startupVerticalId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            db.StartupVerticals.Update(startupVertical.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [startupVertical]);

        return startupVertical
            .ToForm()
            .Place(e => e.Name)
            .ToSheet(isOpen, "Edit Startup Vertical");
    }
}