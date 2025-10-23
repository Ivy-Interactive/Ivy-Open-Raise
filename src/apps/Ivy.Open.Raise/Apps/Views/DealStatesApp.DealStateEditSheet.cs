namespace Ivy.Open.Raise.Apps.Views;

public class DealStateEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int dealStateId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var dealState = UseState(() => factory.CreateDbContext().DealStates.FirstOrDefault(e => e.Id == dealStateId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.DealStates.Update(dealState.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [dealState]);

        return dealState
            .ToForm()
            .Place(e => e.Name)
            .ToSheet(isOpen, "Edit Deal State");
    }
}