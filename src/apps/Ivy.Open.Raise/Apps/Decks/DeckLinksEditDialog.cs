namespace Ivy.Open.Raise.Apps.Decks;

public class DeckLinksEditDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckLinkId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckLink = UseState(() => factory.CreateDbContext().DeckLinks.FirstOrDefault(e => e.Id == deckLinkId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            deckLink.Value.UpdatedAt = DateTime.UtcNow;
            db.DeckLinks.Update(deckLink.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [deckLink]);

        return deckLink
            .ToForm()
            //.Builder(e => e.Secret, e => e.ToReadOnlyInput())
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(Shared.QueryContacts(factory), Shared.LookupContact(factory), placeholder: "Select Contact"))
            .Remove(e => e.Id, e => e.DeckId, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.Secret)
            .ToDialog(isOpen, "Edit Deck Link");
    }
}