namespace Ivy.Open.Raise.Apps.Views;

public class ContactDeckLinksBlade(Guid? contactId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var deckLinks = this.UseState<DeckLink[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            deckLinks.Set(await db.DeckLinks
                .Include(dl => dl.Deck)
                .Where(dl => dl.ContactId == contactId)
                .ToArrayAsync());
        }, [ EffectTrigger.AfterInit(), refreshToken ]);

        Action OnDelete(Guid id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this deck link?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Deck Link", AlertButtonSet.OkCancel);
            };
        }

        if (deckLinks.Value == null) return null;

        var table = deckLinks.Value.Select(dl => new
            {
                DeckTitle = dl.Deck.Title,
                FileType = dl.Deck.FileType,
                FileSize = dl.Deck.FileSize,
                LinkUrl = dl.LinkUrl,
                _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(dl.Id)))
                    | Icons.ChevronRight
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new ContactDeckLinksEditSheet(isOpen, refreshToken, dl.Id))
            })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Deck Link").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new ContactDeckLinksCreateDialog(isOpen, refreshToken, contactId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, Guid deckLinkId)
    {
        using var db = factory.CreateDbContext();
        db.DeckLinks.Remove(db.DeckLinks.Single(dl => dl.Id == deckLinkId));
        db.SaveChanges();
    }
}