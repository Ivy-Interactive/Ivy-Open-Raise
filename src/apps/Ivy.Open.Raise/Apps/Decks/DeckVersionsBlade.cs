using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsBlade(Guid deckId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();
        var (editView, showEdit) = UseTrigger((IState<bool> isOpen, Guid versionId)
            => new DeckVersionsEditDialog(isOpen, refreshToken, versionId));

        var versionsQuery = UseQuery(
            key: (nameof(DeckVersionsBlade), deckId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.DeckVersions
                    .Where(dv => dv.DeckId == deckId && dv.DeletedAt == null)
                    .OrderByDescending(dv => dv.CreatedAt)
                    .ToArrayAsync(ct);
            },
            tags: [(typeof(DeckVersion[]), deckId)]
        );

        if (versionsQuery.Loading) return Text.Muted("Loading...");

        var deckVersions = versionsQuery.Value ?? [];

        MenuItem CreateDeleteBtn(Guid id) => MenuItem.Default("Delete")
            .Icon(Icons.Trash)
            .HandleSelect(async () =>
            {
                await DeleteAsync(factory, id);
                versionsQuery.Mutator.Revalidate();
            });

        async ValueTask OnMakeCurrent(Guid id)
        {
            await using var db = factory.CreateDbContext();
            var versions = await db.DeckVersions
                .Where(dv => dv.DeckId == deckId && dv.DeletedAt == null)
                .ToListAsync();
            foreach (var version in versions)
            {
                version.IsPrimary = version.Id == id;
            }
            await db.SaveChangesAsync();
            versionsQuery.Mutator.Revalidate();
            queryService.RevalidateByTag((typeof(Deck), deckId));
        }

        var table = deckVersions.Select(dv => new
            {
                __ = dv.IsPrimary ? Icons.Crown : Icons.None,
                dv.Name,
                FileName = (Layout.Vertical().Gap(0)
                            | new Button(dv.FileName).Variant(ButtonVariant.Inline)
                            | Text.Muted(Ivy.Utils.FormatBytes(dv.FileSize))),
                _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(
                            CreateDeleteBtn(dv.Id),
                            MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(() => showEdit(dv.Id)),
                            MenuItem.Default("Make Current").Icon(Icons.Crown).HandleSelect(() => OnMakeCurrent(dv.Id))
                        )
            })
            .ToTable()
            .ColumnWidth(e => e._, Size.Fit())
            .ColumnWidth(e => e.__, Size.Fit());

        var addBtn = new Button("New Version").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DeckVersionsCreateDialog(isOpen, refreshToken, deckId));

        return new Fragment()
               | new BladeHeader(addBtn)
               | table
               | editView;
    }

    private async Task DeleteAsync(DataContextFactory factory, Guid versionId)
    {
        await using var db = factory.CreateDbContext();
        var deckVersion = await db.DeckVersions.SingleAsync(dv => dv.Id == versionId);
        deckVersion.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
