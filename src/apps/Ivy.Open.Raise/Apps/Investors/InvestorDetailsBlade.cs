namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorDetailsBlade(Guid investorId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var investor = UseState<Investor?>();
        var contactCount = UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            investor.Set(await db.Investors.Include(e => e.AddressCountry).Include(e => e.InvestorType).SingleOrDefaultAsync(e => e.Id == investorId && e.DeletedAt == null));
            contactCount.Set(await db.Contacts.CountAsync(e => e.InvestorId == investorId && e.DeletedAt == null));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (investor.Value == null) return null;

        var investorValue = investor.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this investor?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Investor", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new InvestorEditSheet(isOpen, refreshToken, investorId));

        var detailsCard = new Card(
            content: new
            {
                investorValue.Name,
                InvestorType = investorValue.InvestorType.Name,
                investorValue.WebsiteUrl,
                investorValue.LinkedinUrl,
                investorValue.XUrl,
                investorValue.AddressStreet,
                investorValue.AddressZip,
                investorValue.AddressCity,
                AddressCountry = investorValue.AddressCountry?.Name,
                investorValue.Thesis,
                CheckSizeRange = $"{investorValue.CheckSizeMin} - {investorValue.CheckSizeMax}"
            }
                .ToDetails()
                .RemoveEmpty(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Investor Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Contacts", onClick: _ =>
                {
                    blades.Push(this, new InvestorContactsBlade(investorId), "Contacts");
                }, badge: contactCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var investor = db.Investors.FirstOrDefault(e => e.Id == investorId)!;
        investor.DeletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }
}