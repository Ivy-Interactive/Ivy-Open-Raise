namespace Ivy.Open.Raise.Apps.Settings.Views;

public class InteractionTypeEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int interactionTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var interactionType = UseState(() => factory.CreateDbContext().InteractionTypes.FirstOrDefault(e => e.Id == interactionTypeId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            db.InteractionTypes.Update(interactionType.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [interactionType]);

        return interactionType
            .ToForm()
            .Remove(e => e.Id)
            .ToSheet(isOpen, "Edit Interaction Type");
    }
}