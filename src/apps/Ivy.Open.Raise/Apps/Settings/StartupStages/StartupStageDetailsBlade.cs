namespace Ivy.Open.Raise.Apps.Settings.StartupStages;

public class StartupStageDetailsBlade(int startupStageId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<DataContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var startupStage = this.UseState<StartupStage?>();
        var organizationSettingsCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            startupStage.Set(await db.StartupStages.SingleOrDefaultAsync(e => e.Id == startupStageId));
            organizationSettingsCount.Set(await db.OrganizationSettings.CountAsync(e => e.StartupStage == startupStageId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (startupStage.Value == null) return null;

        var startupStageValue = startupStage.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this startup stage?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Startup Stage");
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(organizationSettingsCount.Value>0).Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new StartupStageEditSheet(isOpen, refreshToken, startupStageId));

        var detailsCard = new Card(
            content: new
            {
                startupStageValue.Name,
                OrganizationSettings = organizationSettingsCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Startup Stage Details");

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();

        var connectedOrganizationSettingsCount = db.OrganizationSettings.Count(e => e.StartupStage == startupStageId);

        if (connectedOrganizationSettingsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete startup stage with {connectedOrganizationSettingsCount} connected organization setting(s).");
        }

        var startupStage = db.StartupStages.FirstOrDefault(e => e.Id == startupStageId)!;
        db.StartupStages.Remove(startupStage);
        db.SaveChanges();
    }
}
