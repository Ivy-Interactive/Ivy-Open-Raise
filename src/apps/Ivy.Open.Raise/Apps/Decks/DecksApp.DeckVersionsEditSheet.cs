namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid versionId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckVersion = UseState(() => factory.CreateDbContext().DeckVersions.FirstOrDefault(e => e.Id == versionId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            deckVersion.Value.UpdatedAt = DateTime.UtcNow;
            db.DeckVersions.Update(deckVersion.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [deckVersion]);

        return deckVersion
            .ToForm()
            .Remove(e => e.Id, e => e.DeckId, e => e.BlobName, e => e.ContentType, e => e.FileSize, e => e.FileName, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .ToSheet(isOpen, "Edit Version");
    }
}
