using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.StartupVerticals;

public class StartupVerticalListBlade : ViewBase
{
    public record StartupVerticalListRecord(int Id, string Name);

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
            if (refreshToken.ReturnValue is int startupVerticalId)
            {
                blades.Pop(this);
                blades.Push(this, new StartupVerticalDetailsBlade(startupVerticalId));
            }
        }, [refreshToken]);

        var startupVerticalsQuery = UseStartupVerticalList(Context, throttledFilter.Value);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var startupVertical = (StartupVerticalListRecord)e.Sender.Tag!;
            blades.Push(this, new StartupVerticalDetailsBlade(startupVertical.Id), startupVertical.Name);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Startup Vertical").ToTrigger((isOpen) => new StartupVerticalCreateDialog(isOpen, refreshToken));

        var items = (startupVerticalsQuery.Value ?? [])
            .Select(record => new ListItem(
                title: record.Name,
                subtitle: null,
                onClick: onItemClicked,
                tag: record))
            .ToArray();

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (startupVerticalsQuery.Loading ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<StartupVerticalListRecord[]> UseStartupVerticalList(IViewContext context, string filter)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseStartupVerticalList), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.StartupVerticals.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var trimmed = filter.Trim();
                    linq = linq.Where(e => e.Name.Contains(trimmed));
                }

                return await linq
                    .OrderBy(e => e.Name)
                    .Take(50)
                    .Select(e => new StartupVerticalListRecord(e.Id, e.Name))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(StartupVertical[])],
            options: new QueryOptions { KeepPrevious = true }
        );
    }
}
