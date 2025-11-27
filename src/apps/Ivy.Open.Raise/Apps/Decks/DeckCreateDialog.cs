using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record DeckCreateRequest
    {
        [Required]
        public string Title { get; init; } = "Deck";
        
        [Required]
        public FileUpload<BlobInfo>? File { get; init; }
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
            .Builder(e => e.File, FileUploadBuilder)
            .ToDialog(isOpen, title: "New Deck", submitTitle: "Create");
    }

    private Guid CreateDeck(DataContextFactory factory, DeckCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var deck = new Deck
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        db.Decks.Add(deck);
        
        var deckVersion = new DeckVersion
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            Name = "Version 1",
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true,
            BlobName = request.File.Content.BlobName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            FileName = request.File.FileName,
        };
        db.DeckVersions.Add(deckVersion);
        
        var deckLink = new DeckLink()
        {
            Id = Guid.NewGuid(),
            Secret = Utils.RandomKey(12),
            Reference = null!,
            ContactId = null,
            DeckId = deck.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.DeckLinks.Add(deckLink);
        
        db.SaveChanges();

        return deck.Id;
    }
}