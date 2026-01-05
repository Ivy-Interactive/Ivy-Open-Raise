using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.DealApproaches;

public class DealApproachListBlade : ViewBase
{
    public record DealApproachListRecord(int Id, string Name);

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
            if (refreshToken.ReturnValue is int dealApproachId)
            {
                blades.Pop(this);
                blades.Push(this, new DealApproachDetailsBlade(dealApproachId));
            }
        }, [refreshToken]);

        var dealApproachesQuery = UseDealApproachList(Context, throttledFilter.Value);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var dealApproach = (DealApproachListRecord)e.Sender.Tag!;
            blades.Push(this, new DealApproachDetailsBlade(dealApproach.Id), dealApproach.Name);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Deal Approach").ToTrigger((isOpen) => new DealApproachCreateDialog(isOpen, refreshToken));

        var items = (dealApproachesQuery.Value ?? [])
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
               | (dealApproachesQuery.Loading ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<DealApproachListRecord[]> UseDealApproachList(IViewContext context, string filter)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDealApproachList), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.DealApproaches.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var trimmed = filter.Trim();
                    linq = linq.Where(e => e.Name.Contains(trimmed));
                }

                return await linq
                    .OrderByDescending(e => e.Id)
                    .Take(50)
                    .Select(e => new DealApproachListRecord(e.Id, e.Name))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(DealApproach[])],
            options: new QueryOptions { KeepPrevious = true }
        );
    }
}
