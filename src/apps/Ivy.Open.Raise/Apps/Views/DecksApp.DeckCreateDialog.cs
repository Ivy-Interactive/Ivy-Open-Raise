namespace Ivy.Open.Raise.Apps.Views;

public class DeckCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record DeckCreateRequest
    {
        [Required]
        public string Title { get; init; } = "";
        
        [Required]
        public FileUpload<BlobInfo> File { get; init; } = new();
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
            .Builder(e => e.File, (s, v) =>
            {
                var blobService = v.UseService<IBlobService>();
                var upload = v.UseUpload(BlobUploadHandler.Create(s, blobService, Constants.DeckBlobContainerName, CalculateBlobName))
                    .MaxFileSize(FileSize.FromMegabytes(50))
                    .Accept(FileTypes.Pdf);
                return s.ToFileInput(upload);
                string CalculateBlobName(FileUpload f) => f.Id + System.IO.Path.GetExtension(f.FileName);
            })
            .ToDialog(isOpen, title: "Create Deck", submitTitle: "Create");
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
        
        db.SaveChanges();

        return deck.Id;
    }
}