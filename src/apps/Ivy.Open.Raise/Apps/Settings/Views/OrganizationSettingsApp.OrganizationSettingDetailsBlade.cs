namespace Ivy.Open.Raise.Apps.Settings.Views;

public class OrganizationSettingDetailsBlade(int organizationSettingId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var organizationSetting = UseState<OrganizationSetting?>(() => null!);
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            organizationSetting.Set(await db.OrganizationSettings
                .Include(e => e.Country)
                .Include(e => e.Currency)
                .Include(e => e.StartupStageNavigation)
                .SingleOrDefaultAsync(e => e.Id == organizationSettingId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (organizationSetting.Value == null) return null;

        var organizationSettingValue = organizationSetting.Value;

        var onDelete = () =>
        {
            showAlert("Are you sure you want to delete this organization setting?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Organization Setting", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(onDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new OrganizationSettingEditSheet(isOpen, refreshToken, organizationSettingId));

        var detailsCard = new Card(
            content: new
            {
                Id = organizationSettingValue.Id,
                Currency = organizationSettingValue.Currency.Name,
                Country = organizationSettingValue.Country.Name,
                StartupStage = organizationSettingValue.StartupStageNavigation.Name,
                OutreachSubject = organizationSettingValue.OutreachSubject,
                OutreachBody = organizationSettingValue.OutreachBody,
                StartupWebsite = organizationSettingValue.StartupWebsite,
                StartupLinkedinUrl = organizationSettingValue.StartupLinkedinUrl,
                ElevatorPitch = organizationSettingValue.ElevatorPitch,
                Cofounders = organizationSettingValue.Cofounders,
                RaiseTargetMin = organizationSettingValue.RaiseTargetMin,
                RaiseTargetMax = organizationSettingValue.RaiseTargetMax,
                RaiseTicketSize = organizationSettingValue.RaiseTicketSize
            }
            .ToDetails()
            .MultiLine(e => e.OutreachBody, e => e.ElevatorPitch)
            .RemoveEmpty()
            .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                | dropDown
                | editBtn
        ).Title("Organization Setting Details");

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var organizationSetting = db.OrganizationSettings.FirstOrDefault(e => e.Id == organizationSettingId)!;
        db.OrganizationSettings.Remove(organizationSetting);
        db.SaveChanges();
    }
}