namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsBlade(Guid deckId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var deckVersions = this.UseState<DeckVersion[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            deckVersions.Set(await db.DeckVersions.Where(dv => dv.DeckId == deckId && dv.DeletedAt == null).OrderByDescending(dv => dv.CreatedAt).ToArrayAsync());
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(Guid id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this version?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Version", AlertButtonSet.OkCancel);
            };
        }

        if (deckVersions.Value == null) return null;

        var table = deckVersions.Value.Select(dv => new
        {
            dv.Name,
            Primary = dv.IsPrimary ? "Yes" : "No",
            dv.FileName,
            Size = $"{dv.FileSize / 1024.0 / 1024.0:N2} MB",
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(dv.Id)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new DeckVersionsEditSheet(isOpen, refreshToken, dv.Id))
        })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Version").Icon(Icons.Plus).Ghost()
            .ToTrigger((isOpen) => new DeckVersionsCreateDialog(isOpen, refreshToken, deckId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, Guid versionId)
    {
        using var db = factory.CreateDbContext();
        var deckVersion = db.DeckVersions.Single(dv => dv.Id == versionId);
        deckVersion.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}
