namespace Ivy.Open.Raise.Apps.Views;

public class DealListBlade : ViewBase
{
    private record DealListRecord(Guid Id, string ContactName, string DealStateName);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid dealId)
            {
                blades.Pop(this, true);
                blades.Push(this, new DealDetailsBlade(dealId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var deal = (DealListRecord)e.Sender.Tag!;
            blades.Push(this, new DealDetailsBlade(deal.Id), deal.ContactName);
        });

        ListItem CreateItem(DealListRecord record) =>
            new(title: record.ContactName, subtitle: record.DealStateName, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create Deal").ToTrigger((isOpen) => new DealCreateDialog(isOpen, refreshToken));

        return new FilteredListView<DealListRecord>(
            fetchRecords: (filter) => FetchDeals(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<DealListRecord[]> FetchDeals(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Deals.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Contact.FirstName.Contains(filter) || e.Contact.LastName.Contains(filter) || e.DealState.Name.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .Select(e => new DealListRecord(
                e.Id,
                $"{e.Contact.FirstName} {e.Contact.LastName}",
                e.DealState.Name
            ))
            .ToArrayAsync();
    }
}