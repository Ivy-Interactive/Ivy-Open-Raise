namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckId) : ViewBase
{
    private record DeckVersionCreateRequest
    {
        [Required]
        public string Name { get; private init; } = "";

        [Required]
        public FileUpload<BlobInfo>? File { get; init; } = new();

        public bool MakePrimary { get; init; } = true;

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
            .Builder(e => e.File, (s, v) =>
            {
                var blobService = v.UseService<IBlobService>();
                var upload = v.UseUpload(BlobUploadHandler.Create(s, blobService, Constants.DeckBlobContainerName, CalculateBlobName))
                    .MaxFileSize(Constants.MaxUploadFileSize)
                    .Accept(FileTypes.Pdf);
                return s.ToFileInput(upload);
                string CalculateBlobName(FileUpload f) => f.Id + System.IO.Path.GetExtension(f.FileName);
            })
            .ToDialog(isOpen, title: "Create Version", submitTitle: "Create");
    }

    private void CreateDeckVersion(DataContextFactory factory, DeckVersionCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        //todo: if MakePrimary is true, set all other versions IsPrimary to false
        
        var deckVersion = new DeckVersion
        {
            Id = Guid.NewGuid(),
            DeckId = deckId,
            Name = request.Name,
            IsPrimary = request.MakePrimary,
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
