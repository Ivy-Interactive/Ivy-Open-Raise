namespace Ivy.Open.Raise.Apps.Views;

public class DeckLinkListBlade : ViewBase
{
    private record DeckLinkListRecord(Guid Id, string LinkUrl, string DeckTitle);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid deckLinkId)
            {
                blades.Pop(this, true);
                blades.Push(this, new DeckLinkDetailsBlade(deckLinkId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var deckLink = (DeckLinkListRecord)e.Sender.Tag!;
            blades.Push(this, new DeckLinkDetailsBlade(deckLink.Id), deckLink.LinkUrl);
        });

        ListItem CreateItem(DeckLinkListRecord record) =>
            new(title: record.LinkUrl, subtitle: record.DeckTitle, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create DeckLink").ToTrigger((isOpen) => new DeckLinkCreateDialog(isOpen, refreshToken));

        return new FilteredListView<DeckLinkListRecord>(
            fetchRecords: (filter) => FetchDeckLinks(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<DeckLinkListRecord[]> FetchDeckLinks(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.DeckLinks.Include(dl => dl.Deck).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(dl => dl.LinkUrl.Contains(filter) || dl.Deck.Title.Contains(filter));
        }

        return await linq
            .OrderByDescending(dl => dl.CreatedAt)
            .Take(50)
            .Select(dl => new DeckLinkListRecord(dl.Id, dl.LinkUrl, dl.Deck.Title))
            .ToArrayAsync();
    }
}