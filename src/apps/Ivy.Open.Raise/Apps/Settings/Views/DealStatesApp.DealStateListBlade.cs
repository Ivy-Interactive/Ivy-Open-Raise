namespace Ivy.Open.Raise.Apps.Settings.Views;

public class DealStateListBlade : ViewBase
{
    private record DealStateListRecord(int Id, string Name);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int dealStateId)
            {
                blades.Pop(this, true);
                blades.Push(this, new DealStateDetailsBlade(dealStateId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var dealState = (DealStateListRecord)e.Sender.Tag!;
            blades.Push(this, new DealStateDetailsBlade(dealState.Id), dealState.Name);
        });

        ListItem CreateItem(DealStateListRecord record) =>
            new(title: record.Name, subtitle: null, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Deal State").ToTrigger((isOpen) => new DealStateCreateDialog(isOpen, refreshToken));

        return new FilteredListView<DealStateListRecord>(
            fetchRecords: (filter) => FetchDealStates(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<DealStateListRecord[]> FetchDealStates(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.DealStates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Name.Contains(filter));
        }

        return await linq
            .OrderBy(e => e.Name)
            .Take(50)
            .Select(e => new DealStateListRecord(e.Id, e.Name))
            .ToArrayAsync();
    }
}