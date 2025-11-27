using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckId) : ViewBase
{
    private record DeckVersionCreateRequest
    {
        [Required]
        public string Name { get; private init; } = "";

        [Required]
        public FileUpload<BlobInfo>? File { get; init; }

        public bool MakeCurrent { get; init; } = true;

        public static DeckVersionCreateRequest Create(DataContextFactory factory, Guid deckId)
        {
            var db = factory.CreateDbContext();
            var decks = db.DeckVersions.Count(e => e.DeckId == deckId);
            return new DeckVersionCreateRequest
            {
                Name = $"Version {decks + 1}"
            };
        }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var versionRequest = UseState(() => DeckVersionCreateRequest.Create(factory, deckId));

        UseEffect(() =>
        {
            CreateDeckVersion(factory, versionRequest.Value);
            refreshToken.Refresh(deckId);
        }, [versionRequest]);

        return versionRequest
            .ToForm()
            .Builder(e => e.File, FileUploadBuilder)
            .ToDialog(isOpen, title: "New Version", submitTitle: "Create");
    }

    private void CreateDeckVersion(DataContextFactory factory, DeckVersionCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        if (request.MakeCurrent)
        {
            var currentVersions = db.DeckVersions.Where(e => e.DeckId == deckId && e.IsPrimary).ToList();
            foreach (var version in currentVersions)
            {
                version.IsPrimary = false;
                db.DeckVersions.Update(version);
            }
        }
        
        var deckVersion = new DeckVersion
        {
            Id = Guid.NewGuid(),
            DeckId = deckId,
            Name = request.Name,
            IsPrimary = request.MakeCurrent,
            BlobName = request.File.Content.BlobName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            FileName = request.File.FileName,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        db.DeckVersions.Add(deckVersion);
        db.SaveChanges();
    }
}
