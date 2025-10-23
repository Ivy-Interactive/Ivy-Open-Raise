namespace Ivy.Open.Raise.Apps.Views;

public class DeckListBlade : ViewBase
{
    private record DeckListRecord(Guid Id, string Title, string FileType, long FileSize);

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

        ListItem CreateItem(DeckListRecord record) =>
            new(title: record.Title, subtitle: record.FileType, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Deck").ToTrigger((isOpen) => new DeckCreateDialog(isOpen, refreshToken));

        return new FilteredListView<DeckListRecord>(
            fetchRecords: (filter) => FetchDecks(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<DeckListRecord[]> FetchDecks(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Decks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Title.Contains(filter) || e.FileType.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .Select(e => new DeckListRecord(e.Id, e.Title, e.FileType, e.FileSize))
            .ToArrayAsync();
    }
}