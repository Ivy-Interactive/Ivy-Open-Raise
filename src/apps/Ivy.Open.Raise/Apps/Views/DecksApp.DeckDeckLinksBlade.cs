namespace Ivy.Open.Raise.Apps.Views;

public class DeckDeckLinksBlade(Guid deckId) : ViewBase
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
            deckLinks.Set(await db.DeckLinks.Include(dl => dl.Contact).Where(dl => dl.DeckId == deckId).ToArrayAsync());
        }, [EffectTrigger.AfterInit(), refreshToken]);

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
            ContactName = dl.Contact != null ? $"{dl.Contact.FirstName} {dl.Contact.LastName}" : "No Contact",
            LinkUrl = dl.LinkUrl,
            CreatedDate = dl.CreatedAt.ToString("yyyy-MM-dd"),
            UpdatedDate = dl.UpdatedAt.ToString("yyyy-MM-dd"),
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(dl.Id)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new DeckDeckLinksEditSheet(isOpen, refreshToken, dl.Id))
        })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Deck Link").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DeckDeckLinksCreateDialog(isOpen, refreshToken, deckId));

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