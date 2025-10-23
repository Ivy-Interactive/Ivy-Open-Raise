namespace Ivy.Open.Raise.Apps.Settings.Views;

public class CurrencyOrganizationSettingsBlade(string currencyId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var organizationSettings = this.UseState<OrganizationSetting[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            organizationSettings.Set(await db.OrganizationSettings
                .Include(e => e.Country)
                .Include(e => e.StartupStageNavigation)
                .Where(e => e.CurrencyId == currencyId)
                .ToArrayAsync());
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(int id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this organization setting?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Organization Setting", AlertButtonSet.OkCancel);
            };
        }

        if (organizationSettings.Value == null) return null;

        var table = organizationSettings.Value.Select(e => new
        {
            Country = e.Country.Name,
            StartupStage = e.StartupStageNavigation.Name,
            ElevatorPitch = e.ElevatorPitch,
            Cofounders = e.Cofounders,
            RaiseTargetMin = e.RaiseTargetMin,
            RaiseTargetMax = e.RaiseTargetMax,
            RaiseTicketSize = e.RaiseTicketSize,
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(e.Id)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new CurrencyOrganizationSettingsEditSheet(isOpen, refreshToken, e.Id))
        })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Organization Setting").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new CurrencyOrganizationSettingsCreateDialog(isOpen, refreshToken, currencyId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, int organizationSettingId)
    {
        using var db = factory.CreateDbContext();
        db.OrganizationSettings.Remove(db.OrganizationSettings.Single(e => e.Id == organizationSettingId));
        db.SaveChanges();
    }
}