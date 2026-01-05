using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.StartupVerticals;

public class StartupVerticalEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int startupVerticalId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var startupVerticalQuery = UseQuery(
            key: (nameof(StartupVerticalEditSheet), startupVerticalId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.StartupVerticals.FirstOrDefaultAsync(e => e.Id == startupVerticalId, ct);
            },
            tags: [(typeof(StartupVertical), startupVerticalId)]
        );

        if (startupVerticalQuery.Loading || startupVerticalQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Startup Vertical");

        return startupVerticalQuery.Value
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Startup Vertical");

        async Task OnSubmit(StartupVertical? modifiedStartupVertical)
        {
            if (modifiedStartupVertical == null) return;
            await using var db = factory.CreateDbContext();
            db.StartupVerticals.Update(modifiedStartupVertical);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(StartupVertical), startupVerticalId));
            refreshToken.Refresh();
        }
    }
}
