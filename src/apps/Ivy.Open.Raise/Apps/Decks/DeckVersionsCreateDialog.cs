using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckId) : ViewBase
{
    private record DeckVersionCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        [Required]
        public FileUpload<BlobInfo>? File { get; init; }

        public bool MakeCurrent { get; init; } = true;
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<DeckVersionCreateRequest?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            var count = await db.DeckVersions.CountAsync(e => e.DeckId == deckId);
            details.Set(new DeckVersionCreateRequest { Name = $"Version {count + 1}" });
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
            .ToForm()
            .Builder(e => e.File, FileUploadBuilder)
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Version", submitTitle: "Create");

        async Task OnSubmit(DeckVersionCreateRequest? request)
        {
            if (request?.File == null) return;
            await using var db = factory.CreateDbContext();

            if (request.MakeCurrent)
            {
                var currentVersions = await db.DeckVersions.Where(e => e.DeckId == deckId && e.IsPrimary).ToListAsync();
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
            await db.SaveChangesAsync();
            refreshToken.Refresh(deckId);
        }
    }
}
