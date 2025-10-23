namespace Ivy.Open.Raise.Apps.Views;

public class DeckLinkDeckLinkViewsBlade(Guid deckLinkId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var deckLinkViews = this.UseState<DeckLinkView[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            deckLinkViews.Set(await db.DeckLinkViews.Where(e => e.DeckLinkId == deckLinkId).ToArrayAsync());
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(Guid id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this view?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete View", AlertButtonSet.OkCancel);
            };
        };

        if (deckLinkViews.Value == null) return null;

        var table = deckLinkViews.Value.Select(e => new
        {
            e.ViewedAt,
            e.IpAddress,
            e.UserAgent,
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(e.Id)))
                    | Icons.ChevronRight
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new DeckLinkDeckLinkViewsEditSheet(isOpen, refreshToken, e.Id))
        })
            .ToTable()
            .RemoveEmptyColumns()
        ;

        var addBtn = new Button("Add View").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DeckLinkDeckLinkViewsCreateDialog(isOpen, refreshToken, deckLinkId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, Guid viewId)
    {
        using var db = factory.CreateDbContext();
        db.DeckLinkViews.Remove(db.DeckLinkViews.Single(e => e.Id == viewId));
        db.SaveChanges();
    }
}