namespace Ivy.Open.Raise.Apps.Views;

public class DeckCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record DeckCreateRequest
    {
        [Required]
        public string Title { get; init; } = "";

        [Required]
        public long? FileSize { get; init; } = null;

        [Required]
        public string FileType { get; init; } = "";

        [Required]
        public string FileName { get; init; } = "";

        [Required]
        public string StorageWriteUrl { get; init; } = "";

        [Required]
        public string StorageReadUrl { get; init; } = "";

        [Required]
        public bool IsPrimary { get; init; } = false;
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckState = UseState(() => new DeckCreateRequest());

        UseEffect(() =>
        {
            var deckId = CreateDeck(factory, deckState.Value);
            refreshToken.Refresh(deckId);
        }, [deckState]);

        return deckState
            .ToForm()
            .ToDialog(isOpen, title: "Create Deck", submitTitle: "Create");
    }

    private Guid CreateDeck(DataContextFactory factory, DeckCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var deck = new Deck
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            FileSize = request.FileSize!.Value,
            FileType = request.FileType,
            FileName = request.FileName,
            StorageWriteUrl = request.StorageWriteUrl,
            StorageReadUrl = request.StorageReadUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPrimary = request.IsPrimary
        };

        db.Decks.Add(deck);
        db.SaveChanges();

        return deck.Id;
    }
}