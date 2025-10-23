namespace Ivy.Open.Raise.Apps.Views;

public class ContactDeckLinksEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid deckLinkId) : ViewBase
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
            .Builder(e => e.LinkUrl, e => e.ToUrlInput())
            .Builder(e => e.DeckId, e => e.ToAsyncSelectInput(QueryDecks(factory), LookupDeck(factory), placeholder: "Select Deck"))
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.ContactId)
            .ToSheet(isOpen, "Edit Deck Link");
    }

    private static AsyncSelectQueryDelegate<Guid?> QueryDecks(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Decks
                    .Where(e => e.Title.Contains(query))
                    .Select(e => new { e.Id, e.Title })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.Title, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<Guid?> LookupDeck(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var deck = await db.Decks.FirstOrDefaultAsync(e => e.Id == id);
            if (deck == null) return null;
            return new Option<Guid?>(deck.Title, deck.Id);
        };
    }
}