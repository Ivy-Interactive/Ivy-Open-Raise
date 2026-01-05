using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsEditDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid versionId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var versionQuery = UseQuery(
            key: (nameof(DeckVersionsEditDialog), versionId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.DeckVersions.FirstOrDefaultAsync(e => e.Id == versionId, ct);
            },
            tags: [(typeof(DeckVersion), versionId)]
        );

        if (versionQuery.Loading || versionQuery.Value == null)
            return Skeleton.Form().ToDialog(isOpen, "Edit Version");

        return versionQuery.Value
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
            queryService.RevalidateByTag((typeof(DeckVersion), versionId));
            refreshToken.Refresh();
        }
    }
}
