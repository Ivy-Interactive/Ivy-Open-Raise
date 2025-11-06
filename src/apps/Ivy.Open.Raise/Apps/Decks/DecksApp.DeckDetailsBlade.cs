namespace Ivy.Open.Raise.Apps.Decks;

public class DeckDetailsBlade(Guid deckId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var client = this.UseService<IClientProvider>();
        var blobService = this.UseService<IBlobService>();
        
        var refreshToken = this.UseRefreshToken();
        var deck = this.UseState<Deck?>();
        var deckLinksCount = this.UseState<int>();
        var deckVersionCount = this.UseState<int>();
        var currentVersion = this.UseState<DeckVersion?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            currentVersion.Set(await db.DeckVersions
                .Where(e => e.DeckId == deckId && e.DeletedAt == null && e.IsPrimary)
                .FirstOrDefaultAsync());
            deck.Set(await db.Decks.SingleOrDefaultAsync(e => e.Id == deckId && e.DeletedAt == null));
            deckLinksCount.Set(await db.DeckLinks.CountAsync(e => e.DeckId == deckId && e.DeletedAt == null));
            deckVersionCount.Set(await db.DeckVersions.CountAsync(e => e.DeckId == deckId && e.DeletedAt == null));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (deck.Value == null) return null;

        var deckValue = deck.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this deck?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Deck");
        };

        async ValueTask OnDownloadVersion()
        {
            if (currentVersion.Value == null) return;
            var url = await blobService.GetDownloadUrlAsync(Constants.DeckBlobContainerName, currentVersion.Value.BlobName);
            if (!string.IsNullOrEmpty(url))
            {
                client.OpenUrl(url);        
            }
        }
        
        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete),
                MenuItem.Default("Edit").Icon(Icons.Pencil)
            );

        //todo ivy: need a way to put the "..." in the title bar
        
        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new DeckEditSheet(isOpen, refreshToken, deckId));
        
        var detailsCard = new Card(
            content: new
                {
                    deckValue.Title,
                    Current = (currentVersion.Value != null ? 
                        (object)(Layout.Vertical().Gap(0) | currentVersion.Value.Name //todo ivy: why is version name not aligned to the left?
                                                          | new Button(currentVersion.Value.FileName).Inline().HandleClick(OnDownloadVersion)
                                                          | Text.Muted(Utils.FormatBytes(currentVersion.Value.FileSize))
                            )
                        : Callout.Warning("There's no current version"))
                }
                .ToDetails()
                .RemoveEmpty(),
            
            footer: Layout.Horizontal().Gap(2).Align(Align.Right)
                    | dropDown
                    | editBtn
        )
            .Title("Deck Details")
            .Width(Size.Units(100));

        var relatedCard = new Card(
            new List(
                new ListItem("Links", onClick: _ =>
                {
                    blades.Push(this, new DeckLinksBlade(deckId), "Links");
                }, badge: deckLinksCount.Value.ToString("N0")),
                new ListItem("Versions", onClick: _ =>
                {
                    blades.Push(this, new DeckVersionsBlade(deckId), "Versions");
                }, badge: deckVersionCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView
               ;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var deck = db.Decks.FirstOrDefault(e => e.Id == deckId)!;
        deck.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}