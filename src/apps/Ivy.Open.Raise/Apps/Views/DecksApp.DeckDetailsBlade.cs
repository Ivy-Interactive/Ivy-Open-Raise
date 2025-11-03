namespace Ivy.Open.Raise.Apps.Views;

public class DeckDetailsBlade(Guid deckId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var deck = this.UseState<Deck?>();
        var deckLinksCount = this.UseState<int>();
        var deckVersions = this.UseState<DeckVersion[]>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            deck.Set(await db.Decks.SingleOrDefaultAsync(e => e.Id == deckId));
            deckLinksCount.Set(await db.DeckLinks.CountAsync(e => e.DeckId == deckId));
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
            }, "Delete Deck", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new DeckEditSheet(isOpen, refreshToken, deckId));

        var detailsCard = new Card(
            content: new
            {
                deckValue.Id,
                deckValue.Title,
                deckValue.FileName,
                deckValue.FileType,
                deckValue.FileSize,
                deckValue.IsPrimary
            }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Gap(2).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Deck Details")
         .Width(Size.Units(140));

        var relatedCard = new Card(
            new List(
                new ListItem("Deck Links", onClick: _ =>
                {
                    blades.Push(this, new DeckDeckLinksBlade(deckId), "Deck Links");
                }, badge: deckLinksCount.Value.ToString("N0"))
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
        db.Decks.Remove(deck);
        db.SaveChanges();
    }
}