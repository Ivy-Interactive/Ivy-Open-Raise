using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckListBlade : ViewBase
{
    public record DeckListRecord(Guid Id, string Title);

    public override object? Build()
    {
        var blades = UseContext<IBladeService>();
        var factory = UseService<DataContextFactory>();
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
            if (refreshToken.ReturnValue is Guid deckId)
            {
                blades.Pop(this);
                blades.Push(this, new DeckDetailsBlade(deckId));
            }
        }, [refreshToken]);

        var decksQuery = UseDeckList(Context, throttledFilter.Value);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var deck = (DeckListRecord)e.Sender.Tag!;
            blades.Push(this, new DeckDetailsBlade(deck.Id), deck.Title);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Deck").ToTrigger((isOpen) => new DeckCreateDialog(isOpen, refreshToken));

        var items = (decksQuery.Value ?? [])
            .Select(record => new ListItem(
                title: record.Title,
                subtitle: null,
                onClick: onItemClicked,
                tag: record))
            .ToArray();

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (decksQuery.Loading ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<DeckListRecord[]> UseDeckList(IViewContext context, string filter)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseDeckList), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.Decks.Where(e => e.DeletedAt == null);

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var trimmed = filter.Trim();
                    linq = linq.Where(e => e.Title.Contains(trimmed));
                }

                return await linq
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(50)
                    .Select(e => new DeckListRecord(e.Id, e.Title))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(Deck[])],
            options: new QueryOptions { KeepPrevious = true }
        );
    }
}
