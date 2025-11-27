namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsEditDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid versionId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<DeckVersion?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.DeckVersions.FirstOrDefaultAsync(e => e.Id == versionId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
            .ToForm()
            .Remove(e => e.Id, e => e.DeckId, e => e.BlobName, e => e.ContentType, e => e.FileSize, e => e.FileName, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, "Edit Version");

        async Task OnSubmit(DeckVersion? deckVersion)
        {
            if (deckVersion == null) return;
            await using var db = factory.CreateDbContext();
            deckVersion.UpdatedAt = DateTime.UtcNow;
            db.DeckVersions.Update(deckVersion);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}
