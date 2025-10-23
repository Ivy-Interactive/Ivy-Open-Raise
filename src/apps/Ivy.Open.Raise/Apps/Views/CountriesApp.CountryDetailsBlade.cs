namespace Ivy.Open.Raise.Apps.Views;

public class CountryDetailsBlade(int countryId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var country = this.UseState<Country?>();
        var investorCount = this.UseState<int>();
        var organizationSettingCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            country.Set(await db.Countries.SingleOrDefaultAsync(e => e.Id == countryId));
            investorCount.Set(await db.Investors.CountAsync(e => e.AddressCountryId == countryId));
            organizationSettingCount.Set(await db.OrganizationSettings.CountAsync(e => e.CountryId == countryId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (country.Value == null) return null;

        var countryValue = country.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this country?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Country", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new CountryEditSheet(isOpen, refreshToken, countryId));

        var detailsCard = new Card(
            content: new
                {
                    countryValue.Id,
                    countryValue.Name,
                    countryValue.Iso
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Country Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Investors", onClick: _ =>
                {
                    blades.Push(this, new CountryInvestorsBlade(countryId), "Investors");
                }, badge: investorCount.Value.ToString("N0")),
                new ListItem("Organization Settings", onClick: _ =>
                {
                    blades.Push(this, new CountryOrganizationSettingsBlade(countryId), "Organization Settings");
                }, badge: organizationSettingCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var country = db.Countries.FirstOrDefault(e => e.Id == countryId)!;
        db.Countries.Remove(country);
        db.SaveChanges();
    }
}