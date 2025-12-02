namespace Ivy.Open.Raise.Apps.Settings.InvestorTypes;

public class InvestorTypeDetailsBlade(int investorTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var investorType = this.UseState<InvestorType?>();
        var investorCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            investorType.Set(await db.InvestorTypes.SingleOrDefaultAsync(e => e.Id == investorTypeId));
            investorCount.Set(await db.Investors.CountAsync(e => e.InvestorTypeId == investorTypeId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (investorType.Value == null) return null;

        var investorTypeValue = investorType.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this investor type?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Investor Type");
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(investorCount.Value>0).Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new InvestorTypeEditSheet(isOpen, refreshToken, investorTypeId));

        var detailsCard = new Card(
            content: new
            {
                investorTypeValue.Name,
                Investors = investorCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Investor Type Details");

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();

        var connectedInvestorsCount = db.Investors.Count(e => e.InvestorTypeId == investorTypeId);

        if (connectedInvestorsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete investor type with {connectedInvestorsCount} connected investor(s).");
        }

        var investorType = db.InvestorTypes.FirstOrDefault(e => e.Id == investorTypeId)!;
        db.InvestorTypes.Remove(investorType);
        db.SaveChanges();
    }
}
