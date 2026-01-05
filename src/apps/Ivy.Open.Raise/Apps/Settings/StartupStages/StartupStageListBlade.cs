using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.StartupStages;

public class StartupStageListBlade : ViewBase
{
    public record StartupStageListRecord(int Id, string Name);

    public override object? Build()
    {
        var blades = UseContext<IBladeService>();
        var refreshToken = UseRefreshToken();

        var filter = UseState("");
        var throttledFilter = UseState("");

        UseEffect(() =>
        {
            throttledFilter.Set(filter.Value);
            blades.Pop(this);
        }, [filter.Throttle(TimeSpan.FromMilliseconds(250)).ToTrigger()]);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int startupStageId)
            {
                blades.Pop(this);
                blades.Push(this, new StartupStageDetailsBlade(startupStageId));
            }
        }, [refreshToken]);

        var startupStagesQuery = UseStartupStageList(Context, throttledFilter.Value);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var startupStage = (StartupStageListRecord)e.Sender.Tag!;
            blades.Push(this, new StartupStageDetailsBlade(startupStage.Id), startupStage.Name);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Startup Stage").ToTrigger((isOpen) => new StartupStageCreateDialog(isOpen, refreshToken));

        var items = (startupStagesQuery.Value ?? [])
            .Select(record => new ListItem(
                title: record.Name,
                onClick: onItemClicked,
                tag: record))
            .ToArray();

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (startupStagesQuery.Loading ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<StartupStageListRecord[]> UseStartupStageList(IViewContext context, string filter)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseStartupStageList), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.StartupStages.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var trimmed = filter.Trim();
                    linq = linq.Where(e => e.Name.Contains(trimmed));
                }

                return await linq
                    .OrderBy(e => e.Id)
                    .Take(50)
                    .Select(e => new StartupStageListRecord(e.Id, e.Name))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(StartupStage[])],
            options: new QueryOptions { KeepPrevious = true }
        );
    }
}
