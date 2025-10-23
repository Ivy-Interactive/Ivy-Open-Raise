namespace Ivy.Open.Raise.Apps.Views;

public class ContactDeckLinksCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid? contactId) : ViewBase
{
    private record DeckLinkCreateRequest
    {
        [Required]
        public string LinkUrl { get; init; } = "";

        [Required]
        public Guid DeckId { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckLink = UseState(() => new DeckLinkCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var deckLinkId = CreateDeckLink(factory, deckLink.Value);
                refreshToken.Refresh(deckLinkId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [deckLink]);

        return deckLink
            .ToForm()
            .Builder(e => e.DeckId, e => e.ToAsyncSelectInput(QueryDecks(factory), LookupDeck(factory), placeholder: "Select Deck"))
            .Builder(e => e.LinkUrl, e => e.ToUrlInput())
            .ToDialog(isOpen, title: "Create Deck Link", submitTitle: "Create");
    }

    private Guid CreateDeckLink(DataContextFactory factory, DeckLinkCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var deckLink = new DeckLink
        {
            Id = Guid.NewGuid(),
            LinkUrl = request.LinkUrl,
            DeckId = request.DeckId,
            ContactId = contactId!.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.DeckLinks.Add(deckLink);
        db.SaveChanges();

        return deckLink.Id;
    }

    private static AsyncSelectQueryDelegate<Guid> QueryDecks(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Decks
                    .Where(e => e.Title.Contains(query))
                    .Select(e => new { e.Id, e.Title })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid>(e.Title, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<Guid> LookupDeck(DataContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var deck = await db.Decks.FirstOrDefaultAsync(e => e.Id == id);
            if (deck == null) return null;
            return new Option<Guid>(deck.Title, deck.Id);
        };
    }
}