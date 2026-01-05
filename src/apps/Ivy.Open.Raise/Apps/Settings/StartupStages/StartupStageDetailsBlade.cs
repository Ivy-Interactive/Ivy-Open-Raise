using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.StartupStages;

public class StartupStageDetailsBlade(int startupStageId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();

        var query = UseQuery(
            key: (nameof(StartupStageDetailsBlade), startupStageId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                var startupStage = await db.StartupStages.SingleOrDefaultAsync(e => e.Id == startupStageId, ct);
                var organizationSettingsCount = await db.OrganizationSettings.CountAsync(e => e.StartupStageId == startupStageId, ct);
                return (startupStage, organizationSettingsCount);
            },
            tags: [(typeof(StartupStage), startupStageId)]
        );

        if (query.Loading) return Skeleton.Card();
        if (query.Value.startupStage == null)
            return new Callout($"Startup stage '{startupStageId}' not found.").Variant(CalloutVariant.Warning);

        var startupStageValue = query.Value.startupStage;
        var organizationSettingsCount = query.Value.organizationSettingsCount;

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(organizationSettingsCount > 0).Icon(Icons.Trash).HandleSelect(async () =>
                {
                    await DeleteAsync(factory);
                    queryService.RevalidateByTag(typeof(StartupStage[]));
                    blades.Pop();
                })
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
               | new BladeHeader(Text.Literal(startupStageValue.Name))
               | (Layout.Vertical() | detailsCard);
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();

        var connectedOrganizationSettingsCount = await db.OrganizationSettings.CountAsync(e => e.StartupStageId == startupStageId);

        if (connectedOrganizationSettingsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete startup stage with {connectedOrganizationSettingsCount} connected organization setting(s).");
        }

        var startupStage = await db.StartupStages.FirstOrDefaultAsync(e => e.Id == startupStageId);
        if (startupStage != null)
        {
            db.StartupStages.Remove(startupStage);
            await db.SaveChangesAsync();
        }
    }
}
