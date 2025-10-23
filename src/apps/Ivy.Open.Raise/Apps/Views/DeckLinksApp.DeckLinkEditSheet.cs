namespace Ivy.Open.Raise.Apps.Views;

public class DeckLinkEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid deckLinkId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckLink = UseState(() => factory.CreateDbContext().DeckLinks.FirstOrDefault(e => e.Id == deckLinkId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                deckLink.Value.UpdatedAt = DateTime.UtcNow;
                db.DeckLinks.Update(deckLink.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [deckLink]);

        return deckLink
            .ToForm()
            .Builder(e => e.LinkUrl, e => e.ToUrlInput())
            .Place(e => e.DeckId, e => e.ContactId)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .ToSheet(isOpen, "Edit Deck Link");
    }
}