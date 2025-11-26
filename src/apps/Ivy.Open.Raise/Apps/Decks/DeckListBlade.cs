namespace Ivy.Open.Raise.Apps.Decks;

public class DeckListBlade : ViewBase
{
    private record DeckListRecord(Guid Id, string Title);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid deckId)
            {
                blades.Pop(this, true);
                blades.Push(this, new DeckDetailsBlade(deckId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var deck = (DeckListRecord)e.Sender.Tag!;
            blades.Push(this, new DeckDetailsBlade(deck.Id), deck.Title);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Deck").ToTrigger((isOpen) => new DeckCreateDialog(isOpen, refreshToken));

        return new FilteredListView<DeckListRecord>(
            fetchRecords: (filter) => FetchDecks(factory, filter),
            createItem: CalculateCreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );

        ListItem CalculateCreateItem(DeckListRecord record) =>
            new(title: record.Title, subtitle: null, onClick: onItemClicked, tag: record);
    }

    private async Task<DeckListRecord[]> FetchDecks(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Decks.Where(e => e.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Title.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .Select(e => new DeckListRecord(e.Id, e.Title))
            .ToArrayAsync();
    }
}