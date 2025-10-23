namespace Ivy.Open.Raise.Apps.Settings.Views;

public class InvestorTypeInvestorsBlade(int investorTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var investors = this.UseState<Investor[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            investors.Set(await db.Investors
                .Where(i => i.InvestorTypeId == investorTypeId)
                .Include(i => i.AddressCountry)
                .ToArrayAsync());
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(Guid id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this investor?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Investor", AlertButtonSet.OkCancel);
            };
        }

        if (investors.Value == null) return null;

        var table = investors.Value.Select(i => new
        {
            Name = i.Name,
            Website = i.WebsiteUrl,
            Address = $"{i.AddressStreet}, {i.AddressCity}, {i.AddressCountry?.Name}",
            Thesis = i.Thesis,
            CheckSize = $"{i.CheckSizeMin} - {i.CheckSizeMax}",
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(i.Id)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new InvestorTypeInvestorsEditSheet(isOpen, refreshToken, i.Id))
        })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Investor").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new InvestorTypeInvestorsCreateDialog(isOpen, refreshToken, investorTypeId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, Guid investorId)
    {
        using var db = factory.CreateDbContext();
        db.Investors.Remove(db.Investors.Single(i => i.Id == investorId));
        db.SaveChanges();
    }
}