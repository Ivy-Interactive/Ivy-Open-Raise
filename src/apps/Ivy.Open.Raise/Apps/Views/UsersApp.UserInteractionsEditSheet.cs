namespace Ivy.Open.Raise.Apps.Views;

public class UserInteractionsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid interactionId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var interaction = UseState(() => factory.CreateDbContext().Interactions.FirstOrDefault(e => e.Id == interactionId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            interaction.Value.UpdatedAt = DateTime.UtcNow;
            db.Interactions.Update(interaction.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [interaction]);

        return interaction
            .ToForm()
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Builder(e => e.Subject, e => e.ToTextAreaInput())
            .Place(e => e.ContactId, e => e.InteractionType)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.UserId)
            .ToSheet(isOpen, "Edit Interaction");
    }
}