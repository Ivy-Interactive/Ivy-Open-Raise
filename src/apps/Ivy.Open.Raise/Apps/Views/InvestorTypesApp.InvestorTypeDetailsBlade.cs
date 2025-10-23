namespace Ivy.Open.Raise.Apps.Views;

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
            }, "Delete Investor Type", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new InvestorTypeEditSheet(isOpen, refreshToken, investorTypeId));

        var detailsCard = new Card(
            content: new
            {
                investorTypeValue.Id,
                investorTypeValue.Name
            }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Investor Type Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Investors", onClick: _ =>
                {
                    blades.Push(this, new InvestorTypeInvestorsBlade(investorTypeId), "Investors");
                }, badge: investorCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var investorType = db.InvestorTypes.FirstOrDefault(e => e.Id == investorTypeId)!;
        db.InvestorTypes.Remove(investorType);
        db.SaveChanges();
    }
}