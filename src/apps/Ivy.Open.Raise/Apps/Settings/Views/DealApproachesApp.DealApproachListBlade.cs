namespace Ivy.Open.Raise.Apps.Settings.Views;

public class DealApproachListBlade : ViewBase
{
    private record DealApproachListRecord(int Id, string Name);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int dealApproachId)
            {
                blades.Pop(this, true);
                blades.Push(this, new DealApproachDetailsBlade(dealApproachId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var dealApproach = (DealApproachListRecord)e.Sender.Tag!;
            blades.Push(this, new DealApproachDetailsBlade(dealApproach.Id), dealApproach.Name);
        });

        ListItem CreateItem(DealApproachListRecord record) =>
            new(title: record.Name, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Deal Approach").ToTrigger((isOpen) => new DealApproachCreateDialog(isOpen, refreshToken));

        return new FilteredListView<DealApproachListRecord>(
            fetchRecords: (filter) => FetchDealApproaches(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<DealApproachListRecord[]> FetchDealApproaches(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.DealApproaches.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Name.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.Id)
            .Take(50)
            .Select(e => new DealApproachListRecord(e.Id, e.Name))
            .ToArrayAsync();
    }
}