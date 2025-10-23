namespace Ivy.Open.Raise.Apps.Settings.Views;

public class DealStateEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int dealStateId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var dealState = UseState(() => factory.CreateDbContext().DealStates.FirstOrDefault(e => e.Id == dealStateId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            db.DealStates.Update(dealState.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [dealState]);

        return dealState
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .ToSheet(isOpen, "Edit Deal State");
    }
}