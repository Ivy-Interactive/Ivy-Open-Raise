namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsEditDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid versionId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckVersion = UseState<DeckVersion?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            deckVersion.Set(await context.DeckVersions.FirstOrDefaultAsync(e => e.Id == versionId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return deckVersion
            .ToForm()
            .Remove(e => e.Id, e => e.DeckId, e => e.BlobName, e => e.ContentType, e => e.FileSize, e => e.FileName, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, "Edit Version");

        async Task OnSubmit(DeckVersion? modifiedDeckVersion)
        {
            if (modifiedDeckVersion == null) return;
            await using var db = factory.CreateDbContext();
            modifiedDeckVersion.UpdatedAt = DateTime.UtcNow;
            db.DeckVersions.Update(modifiedDeckVersion);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}
