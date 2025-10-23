namespace Ivy.Open.Raise.Apps.Views;

public class DeckLinkDetailsBlade(Guid deckLinkId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var deckLink = this.UseState<DeckLink?>();
        var deckLinkViewCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            deckLink.Set(await db.DeckLinks.Include(e => e.Deck).SingleOrDefaultAsync(e => e.Id == deckLinkId));
            deckLinkViewCount.Set(await db.DeckLinkViews.CountAsync(e => e.DeckLinkId == deckLinkId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (deckLink.Value == null) return null;

        var deckLinkValue = deckLink.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this deck link?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Deck Link", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new DeckLinkEditSheet(isOpen, refreshToken, deckLinkId));

        var detailsCard = new Card(
            content: new
            {
                deckLinkValue.Id,
                LinkUrl = deckLinkValue.LinkUrl,
                DeckTitle = deckLinkValue.Deck.Title
            }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Deck Link Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Deck Link Views", onClick: _ =>
                {
                    blades.Push(this, new DeckLinkDeckLinkViewsBlade(deckLinkId), "Deck Link Views");
                }, badge: deckLinkViewCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var deckLink = db.DeckLinks.FirstOrDefault(e => e.Id == deckLinkId)!;
        db.DeckLinks.Remove(deckLink);
        db.SaveChanges();
    }
}