namespace Ivy.Open.Raise.Apps.Decks;

public class DeckVersionsBlade(Guid deckId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var deckVersions = this.UseState<DeckVersion[]?>();
        var (alertView, showAlert) = this.UseAlert();
        var (editView, showEdit) = this.UseTrigger((IState<bool> isOpen, Guid versionId) 
            => new DeckVersionsEditDialog(isOpen, refreshToken, versionId));

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            deckVersions.Set(await db.DeckVersions
                .Where(dv => dv.DeckId == deckId && dv.DeletedAt == null)
                .OrderByDescending(dv => dv.CreatedAt).ToArrayAsync());
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
                }, "Delete Version");
            };
        }
        
        Action OnMakeCurrent(Guid id)
        {
            return () =>
            {
                var db = factory.CreateDbContext();
                var versions = db.DeckVersions.Where(dv => dv.DeckId == deckId && dv.DeletedAt == null).ToList();
                foreach (var version in versions)
                {
                    version.IsPrimary = version.Id == id;
                }

                db.SaveChanges();
                refreshToken.Refresh();
            };
        }

        if (deckVersions.Value == null) return null;

        var table = deckVersions.Value.Select(dv => new
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
                            MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(dv.Id)),
                            MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(() => showEdit(dv.Id)),
                            MenuItem.Default("Make Current").Icon(Icons.Crown).HandleSelect(OnMakeCurrent(dv.Id))
                        )
        })
            .ToTable()
            // .Width(e => e._, Size.Fit())
            // .Width(e => e.__, Size.Fit())
            // .Width(e => e.FileName, Size.Fraction(1))
            .RemoveEmptyColumns();

        var addBtn = new Button("New Version").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DeckVersionsCreateDialog(isOpen, refreshToken, deckId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView
               | editView;
    }

    public void Delete(DataContextFactory factory, Guid versionId)
    {
        using var db = factory.CreateDbContext();
        var deckVersion = db.DeckVersions.Single(dv => dv.Id == versionId);
        deckVersion.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}
