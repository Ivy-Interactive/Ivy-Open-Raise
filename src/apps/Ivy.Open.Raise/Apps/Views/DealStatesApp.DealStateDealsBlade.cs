namespace Ivy.Open.Raise.Apps.Views;

public class DealStateDealsBlade(int dealStateId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var deals = this.UseState<Deal[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            deals.Set(await db.Deals
                .Include(d => d.Contact)
                .Include(d => d.Owner)
                .Where(d => d.DealStateId == dealStateId)
                .ToArrayAsync());
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(Guid id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this deal?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Deal", AlertButtonSet.OkCancel);
            };
        }

        if (deals.Value == null) return null;

        var table = deals.Value.Select(d => new
        {
            ContactName = $"{d.Contact.FirstName} {d.Contact.LastName}",
            OwnerName = $"{d.Owner.FirstName} {d.Owner.LastName}",
            AmountRange = $"{d.AmountFrom?.ToString() ?? "N/A"} - {d.AmountTo?.ToString() ?? "N/A"}",
            Priority = d.Priority?.ToString() ?? "N/A",
            Notes = d.Notes,
            NextAction = d.NextAction?.ToString("yyyy-MM-dd") ?? "N/A",
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(d.Id)))
                    | Icons.ChevronRight
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new DealStateDealsEditSheet(isOpen, refreshToken, d.Id))
        })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Deal").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DealStateDealsCreateDialog(isOpen, refreshToken, dealStateId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, Guid dealId)
    {
        using var db = factory.CreateDbContext();
        db.Deals.Remove(db.Deals.Single(d => d.Id == dealId));
        db.SaveChanges();
    }
}