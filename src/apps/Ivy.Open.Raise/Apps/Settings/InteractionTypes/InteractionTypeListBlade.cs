using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.InteractionTypes;

public class InteractionTypeListBlade : ViewBase
{
    public record InteractionTypeListRecord(int Id, string Name);

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
            if (refreshToken.ReturnValue is int interactionTypeId)
            {
                blades.Pop(this);
                blades.Push(this, new InteractionTypeDetailsBlade(interactionTypeId));
            }
        }, [refreshToken]);

        var interactionTypesQuery = UseInteractionTypeList(Context, throttledFilter.Value);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var interactionType = (InteractionTypeListRecord)e.Sender.Tag!;
            blades.Push(this, new InteractionTypeDetailsBlade(interactionType.Id), interactionType.Name);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Interaction Type").ToTrigger((isOpen) => new InteractionTypeCreateDialog(isOpen, refreshToken));

        var items = (interactionTypesQuery.Value ?? [])
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
               | (interactionTypesQuery.Loading ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<InteractionTypeListRecord[]> UseInteractionTypeList(IViewContext context, string filter)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseInteractionTypeList), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.InteractionTypes.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var trimmed = filter.Trim();
                    linq = linq.Where(e => e.Name.Contains(trimmed));
                }

                return await linq
                    .OrderBy(e => e.Name)
                    .Take(50)
                    .Select(e => new InteractionTypeListRecord(e.Id, e.Name))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(InteractionType[])],
            options: new QueryOptions { KeepPrevious = true }
        );
    }
}
