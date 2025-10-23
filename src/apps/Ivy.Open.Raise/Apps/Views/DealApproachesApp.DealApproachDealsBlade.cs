namespace Ivy.Open.Raise.Apps.Views;

public class DealApproachDealsBlade(int? dealApproachId) : ViewBase
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
            deals.Set(await db.Deals.Where(e => e.DealApproachId == dealApproachId).ToArrayAsync());
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
        };

        if (deals.Value == null) return null;

        var table = deals.Value.Select(e => new
        {
            ContactId = e.ContactId,
            DealState = e.DealState.Name,
            AmountFrom = e.AmountFrom,
            AmountTo = e.AmountTo,
            Priority = e.Priority,
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(e.Id)))
                    | Icons.ChevronRight
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new DealApproachDealsEditSheet(isOpen, refreshToken, e.Id))
        })
            .ToTable()
            .RemoveEmptyColumns()
        ;

        var addBtn = new Button("Add Deal").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new DealApproachDealsCreateDialog(isOpen, refreshToken, dealApproachId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, Guid dealId)
    {
        using var db = factory.CreateDbContext();
        db.Deals.Remove(db.Deals.Single(e => e.Id == dealId));
        db.SaveChanges();
    }
}