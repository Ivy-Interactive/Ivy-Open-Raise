namespace Ivy.Open.Raise.Apps.Decks;

public class DeckLinksCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckId) : ViewBase
{
    private record DeckLinkCreateRequest
    {
        public string? Reference { get; init; } = null!;

        public Guid? ContactId { get; init; } = null;
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckLinkRequest = UseState(() => new DeckLinkCreateRequest());

        UseEffect(() =>
        {
            CreateDeckLink(factory, deckLinkRequest.Value);
            refreshToken.Refresh(deckId);
        }, [deckLinkRequest]);

        return deckLinkRequest
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(QueryContacts(factory), LookupContact(factory), placeholder: "Select Contact"))
            .ToDialog(isOpen, title: "New Link", submitTitle: "Create");
    }

    private void CreateDeckLink(DataContextFactory factory, DeckLinkCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var deckLink = new DeckLink()
        {
            Id = Guid.NewGuid(),
            Secret = Utils.RandomKey(12),
            Reference = request.Reference,
            ContactId = request.ContactId,
            DeckId = deckId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.DeckLinks.Add(deckLink);
        db.SaveChanges();
    }

    private static AsyncSelectQueryDelegate<Guid?> QueryContacts(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Contacts
                    .Where(e => (e.FirstName.Contains(query) || e.LastName.Contains(query)) && e.DeletedAt == null)
                    .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.FullName, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<Guid?> LookupContact(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var contact = await db.Contacts.FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
            if (contact == null) return null;
            return new Option<Guid?>(contact.FirstName + " " + contact.LastName, contact.Id);
        };
    }
}