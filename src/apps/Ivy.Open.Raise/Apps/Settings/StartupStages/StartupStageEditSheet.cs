using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.StartupStages;

public class StartupStageEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int startupStageId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var startupStageQuery = UseQuery(
            key: (nameof(StartupStageEditSheet), startupStageId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.StartupStages.FirstOrDefaultAsync(e => e.Id == startupStageId, ct);
            },
            tags: [(typeof(StartupStage), startupStageId)]
        );

        if (startupStageQuery.Loading || startupStageQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Startup Stage");

        return startupStageQuery.Value
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Startup Stage");

        async Task OnSubmit(StartupStage? modifiedStartupStage)
        {
            if (modifiedStartupStage == null) return;
            await using var db = factory.CreateDbContext();
            db.StartupStages.Update(modifiedStartupStage);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(StartupStage), startupStageId));
            refreshToken.Refresh();
        }
    }
}
