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
        var details = UseState(() => new DeckLinkCreateRequest());

        return details
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(Shared.QueryContacts(factory), Shared.LookupContact(factory), placeholder: "Select Contact"))
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Link", submitTitle: "Create");

        async Task OnSubmit(DeckLinkCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var deckLink = new DeckLink
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
            await db.SaveChangesAsync();
            refreshToken.Refresh(deckId);
        }
    }
}