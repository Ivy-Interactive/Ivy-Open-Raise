namespace Ivy.Open.Raise.Apps.Views;

public class CurrencyDetailsBlade(string currencyId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var currency = UseState<Currency?>();
        var organizationSettingsCount = UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            using var db = factory.CreateDbContext();
            currency.Set(await db.Currencies.SingleOrDefaultAsync(e => e.Id == currencyId));
            organizationSettingsCount.Set(await db.OrganizationSettings.CountAsync(e => e.CurrencyId == currencyId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (currency.Value == null) return null;

        var currencyValue = currency.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this currency?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Currency", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new CurrencyEditSheet(isOpen, refreshToken, currencyId));

        var detailsCard = new Card(
            content: new
                {
                    currencyValue.Id,
                    currencyValue.Name,
                    currencyValue.Symbol
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Currency Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Organization Settings", onClick: _ =>
                {
                    blades.Push(this, new CurrencyOrganizationSettingsBlade(currencyId), "Organization Settings");
                }, badge: organizationSettingsCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var currency = db.Currencies.FirstOrDefault(e => e.Id == currencyId)!;
        db.Currencies.Remove(currency);
        db.SaveChanges();
    }
}