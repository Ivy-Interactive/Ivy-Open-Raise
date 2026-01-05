using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.DealStates;

public class DealStateListBlade : ViewBase
{
    public record DealStateListRecord(int Id, string Name, int Order);

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
            if (refreshToken.ReturnValue is int dealStateId)
            {
                blades.Pop(this);
                blades.Push(this, new DealStateDetailsBlade(dealStateId));
            }
        }, [refreshToken]);

        var dealStatesQuery = UseDealStateList(Context, throttledFilter.Value);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var dealState = (DealStateListRecord)e.Sender.Tag!;
            blades.Push(this, new DealStateDetailsBlade(dealState.Id), dealState.Name);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Deal State").ToTrigger((isOpen) => new DealStateCreateDialog(isOpen, refreshToken));

        var items = (dealStatesQuery.Value ?? [])
            .Select(record => new ListItem(
                title: record.Name,
                subtitle: record.Order.ToString(),
                onClick: onItemClicked,
                tag: record))
            .ToArray();

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (dealStatesQuery.Loading ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<DealStateListRecord[]> UseDealStateList(IViewContext context, string filter)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDealStateList), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.DealStates.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var trimmed = filter.Trim();
                    linq = linq.Where(e => e.Name.Contains(trimmed));
                }

                return await linq
                    .OrderBy(e => e.Order)
                    .Take(50)
                    .Select(e => new DealStateListRecord(e.Id, e.Name, e.Order))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(DealState[])],
            options: new QueryOptions { KeepPrevious = true }
        );
    }
}
