namespace Ivy.Open.Raise.Apps.Views;

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
            }, "Delete Startup Stage", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new StartupStageEditSheet(isOpen, refreshToken, startupStageId));

        var detailsCard = new Card(
            content: new
                {
                    startupStageValue.Id,
                    startupStageValue.Name
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Startup Stage Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Organization Settings", onClick: _ =>
                {
                    blades.Push(this, new StartupStageOrganizationSettingsBlade(startupStageId), "Organization Settings");
                }, badge: organizationSettingsCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(DataContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var startupStage = db.StartupStages.FirstOrDefault(e => e.Id == startupStageId)!;
        db.StartupStages.Remove(startupStage);
        db.SaveChanges();
    }
}