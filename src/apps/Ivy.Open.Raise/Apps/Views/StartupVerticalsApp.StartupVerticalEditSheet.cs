namespace Ivy.Open.Raise.Apps.Views;

public class StartupVerticalEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int startupVerticalId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var startupVertical = UseState(() => factory.CreateDbContext().StartupVerticals.FirstOrDefault(e => e.Id == startupVerticalId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.StartupVerticals.Update(startupVertical.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [startupVertical]);

        return startupVertical
            .ToForm()
            .Place(e => e.Name)
            .ToSheet(isOpen, "Edit Startup Vertical");
    }
}