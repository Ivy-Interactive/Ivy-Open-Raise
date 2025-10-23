namespace Ivy.Open.Raise.Apps.Settings.Views;

public class StartupStageOrganizationSettingsBlade(int startup_stage) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var settings = this.UseState<OrganizationSetting[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            settings.Set(await db.OrganizationSettings
                .Include(e => e.Country)
                .Include(e => e.Currency)
                .Where(e => e.StartupStage == startup_stage)
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
        };

        if (settings.Value == null) return null;

        var table = settings.Value.Select(e => new
        {
            Country = e.Country.Name,
            Currency = e.Currency.Name,
            ElevatorPitch = e.ElevatorPitch,
            Cofounders = e.Cofounders,
            RaiseTargetMin = e.RaiseTargetMin,
            RaiseTargetMax = e.RaiseTargetMax,
            RaiseTicketSize = e.RaiseTicketSize,
            StartupWebsite = e.StartupWebsite,
            _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(e.Id)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new StartupStageOrganizationSettingsEditSheet(isOpen, refreshToken, e.Id))
        })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Organization Setting").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new StartupStageOrganizationSettingsCreateDialog(isOpen, refreshToken, startup_stage));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(DataContextFactory factory, int settingId)
    {
        using var db2 = factory.CreateDbContext();
        db2.OrganizationSettings.Remove(db2.OrganizationSettings.Single(e => e.Id == settingId));
        db2.SaveChanges();
    }
}