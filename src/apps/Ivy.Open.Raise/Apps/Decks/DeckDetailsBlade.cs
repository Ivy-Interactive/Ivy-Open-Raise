using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckDetailsBlade(Guid deckId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeService>();
        var client = UseService<IClientProvider>();
        var blobService = UseService<IBlobService>();
        var queryService = UseService<IQueryService>();

        var refreshToken = UseRefreshToken();
        var (deckEditView, showDeckEdit) = UseTrigger(isOpen => new DeckEditDialog(isOpen, refreshToken, deckId));

        var deckQuery = UseQuery(
            key: (nameof(DeckDetailsBlade), deckId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                var deck = await db.Decks.SingleOrDefaultAsync(e => e.Id == deckId && e.DeletedAt == null, ct);
                var currentVersion = await db.DeckVersions
                    .Where(e => e.DeckId == deckId && e.DeletedAt == null && e.IsPrimary)
                    .FirstOrDefaultAsync(ct);
                var deckLinksCount = await db.DeckLinks.CountAsync(e => e.DeckId == deckId && e.DeletedAt == null, ct);
                var deckVersionCount = await db.DeckVersions.CountAsync(e => e.DeckId == deckId && e.DeletedAt == null, ct);
                return (deck, currentVersion, deckLinksCount, deckVersionCount);
            },
            tags: [(typeof(Deck), deckId)]
        );

        if (deckQuery.Loading) return Skeleton.Card();
        if (deckQuery.Value.deck == null)
            return new Callout($"Deck '{deckId}' not found.").Variant(CalloutVariant.Warning);

        var deckValue = deckQuery.Value.deck;
        var currentVersion = deckQuery.Value.currentVersion;
        var deckLinksCount = deckQuery.Value.deckLinksCount;
        var deckVersionCount = deckQuery.Value.deckVersionCount;

        async ValueTask OnDownloadVersion()
        {
            if (currentVersion == null) return;
            var url = await blobService.GetDownloadUrlAsync(Constants.DeckBlobContainerName, currentVersion.BlobName);
            if (!string.IsNullOrEmpty(url))
            {
                client.OpenUrl(url);
            }
        }

        var actions = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(async () =>
                {
                    await DeleteAsync(factory);
                    queryService.RevalidateByTag(typeof(Deck[]));
                    blades.Pop();
                }),
                MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(showDeckEdit)
            );

        var detailsCard = new Card(
            content: new
                {
                    deckValue.Title,
                    Current = (currentVersion != null ?
                        (object)(Layout.Vertical().Gap(0)
                            | Text.Inline(currentVersion.Name)
                            | new Button(currentVersion.FileName).Inline().HandleClick(OnDownloadVersion).Width(Size.Fit().Max(Size.Units(50)))
                            | Text.Muted(Ivy.Utils.FormatBytes(currentVersion.FileSize))
                            )
                        : Callout.Warning("There's no current version"))
                }
                .ToDetails()
                .RemoveEmpty()
        )
            .Title("Deck Details")
            .Icon(actions)
            .Width(Size.Units(100));

        var relatedCard = new Card(
            new List(
                new ListItem(
                    "Links",
                    onClick: _ => blades.Push(this, new DeckLinksBlade(deckId), "Links"),
                    badge: deckLinksCount.ToString("N0")),
                new ListItem(
                    "Versions",
                    onClick: _ => blades.Push(this, new DeckVersionsBlade(deckId), "Versions"),
                    badge: deckVersionCount.ToString("N0"))
            ));

        return new Fragment()
               | new BladeHeader(Text.Literal(deckValue.Title))
               | (Layout.Vertical() | detailsCard | relatedCard)
               | deckEditView
               ;
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var deck = await db.Decks.FirstOrDefaultAsync(e => e.Id == deckId);
        if (deck != null)
        {
            deck.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}
