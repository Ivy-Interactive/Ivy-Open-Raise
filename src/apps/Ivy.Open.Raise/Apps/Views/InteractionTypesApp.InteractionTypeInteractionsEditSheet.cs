namespace Ivy.Open.Raise.Apps.Views;

public class InteractionTypeInteractionsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid interactionId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var interaction = UseState(() => factory.CreateDbContext().Interactions.FirstOrDefault(e => e.Id == interactionId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                interaction.Value.UpdatedAt = DateTime.UtcNow;
                db.Interactions.Update(interaction.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [interaction]);

        return interaction
            .ToForm()
            .Builder(e => e.Subject, e => e.ToTextAreaInput())
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Builder(e => e.OccurredAt, e => e.ToDateTimeInput())
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.InteractionType)
            .ToSheet(isOpen, "Edit Interaction");
    }
}